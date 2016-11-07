﻿Imports System.IO
Imports SkyEditor.Core.Utilities

Public Class ModFile
    Public Sub New(Filename As String)
        Dim provider As New WindowsIOProvider
        Me.ModDetails = Json.DeserializeFromFile(Of ModJson)(Filename, provider)
        Me.ID = Me.ModDetails.ID
        Me.Name = Me.ModDetails.Name
        Me.Patched = False
        Me.Filename = Filename

        'Load patchers.json
        Dim toolsDir = Path.Combine(Path.GetDirectoryName(Filename), "Tools")
        Dim patchersPath = IO.Path.Combine(toolsDir, "patchers.json")
        If IO.File.Exists(patchersPath) Then
            ModLevelPatchers = FilePatcher.DeserializePatcherList(patchersPath, toolsDir)
        End If
    End Sub

    Public Property ModDetails As ModJson
    Public Property ID As String
    Public Property Name As String
    Public Property Patched As Boolean
    Public Property Filename As String
    Public Property ModLevelPatchers As List(Of FilePatcher)

    Public Async Function ApplyPatch(currentDirectory As String, ROMDirectory As String, patchers As List(Of FilePatcher)) As Task
        Dim renameTemp = IO.Path.Combine(currentDirectory, "Tools", "renametemp")
        If ModDetails.ToAdd IsNot Nothing Then
            For Each file In ModDetails.ToAdd
                IO.File.Copy(IO.Path.Combine(IO.Path.GetDirectoryName(Filename), "Files", file.Trim("\")), Path.Combine(ROMDirectory, file.Trim("\")), True)
            Next
        End If

        If ModDetails.ToUpdate IsNot Nothing Then
            For Each file In ModDetails.ToUpdate
                If IO.File.Exists(Path.Combine(ROMDirectory, file.TrimStart("\"))) Then
                    Dim patches = Directory.GetFiles(Path.GetDirectoryName(Path.Combine(Path.GetDirectoryName(Filename), "Files", file.Trim("\"))), Path.GetFileName(file.Trim("\")) & "*")
                    'Hopefully we only have 1 patch, but if there's more than 1 patch, apply them all.
                    For Each patchFile In patches
                        Dim possiblePatchers As New List(Of FilePatcher) ' = (From p In patchers Where p.PatchExtension = IO.Path.GetExtension(patchFile) Select p).ToList
                        'Load pack level patchers
                        For Each p In patchers
                            If p.SerializableInfo.PatchExtension = Path.GetExtension(patchFile).Trim(".") Then
                                possiblePatchers.Add(p)
                            End If
                        Next

                        'Load mod level patchers
                        For Each p In ModLevelPatchers
                            If "." & p.SerializableInfo.PatchExtension = Path.GetExtension(patchFile) Then
                                possiblePatchers.Add(p)
                            End If
                        Next

                        'If possiblePatchers.Count = 0 Then
                        '   Do nothing, we don't have the tools to deal with this patch
                        If possiblePatchers.Count >= 1 Then
                            Dim tempFilename As String = IO.Path.Combine(currentDirectory, "Tools", "tempFile")
                            'If there's 1 possible patcher, great.  If there's more than one, then multiple programs have the same extension, which is their fault.  Only using the first one because we don't need to apply the same patch multiple times.
                            Dim path As String
                            If ModLevelPatchers.Contains(possiblePatchers(0)) Then
                                path = IO.Path.Combine(IO.Path.GetDirectoryName(Filename), "Tools")
                            Else
                                path = IO.Path.Combine(currentDirectory, "Tools", "Patchers")
                            End If

                            Await ProcessHelper.RunProgram(possiblePatchers(0).GetApplyPatchProgramPath, String.Format(possiblePatchers(0).SerializableInfo.ApplyPatchArguments, IO.Path.Combine(ROMDirectory, file.TrimStart("\")), patchFile, tempFilename))

                            If Not IO.File.Exists(tempFilename) Then
                                MessageBox.Show("Unable to patch file """ & file & """.  Please ensure you're using a supported ROM.  If you sure you are, report this to the mod author.")
                            Else
                                IO.File.Copy(tempFilename, IO.Path.Combine(ROMDirectory, file.TrimStart("\")), True)
                                IO.File.Delete(tempFilename)
                            End If
                        End If
                    Next
                End If
            Next
        End If

        If ModDetails.ToRename IsNot Nothing Then
            'Create temporary directory
            If Not IO.Directory.Exists(renameTemp) Then
                IO.Directory.CreateDirectory(renameTemp)
            End If

            'Move to a temporary directory (so swapping files works)
            For Each file In ModDetails.ToRename
                CopyFile(IO.Path.Combine(ROMDirectory, file.Key.Trim("\")), IO.Path.Combine(renameTemp, file.Key.Trim("\")), True)
            Next

            'Rename the things
            For Each file In ModDetails.ToRename
                CopyFile(IO.Path.Combine(renameTemp, file.Key.Trim("\")), IO.Path.Combine(ROMDirectory, file.Value.Trim("\")), True)
            Next
        End If

        If ModDetails.ToDelete IsNot Nothing Then
            For Each file In ModDetails.ToDelete
                If IO.File.Exists(IO.Path.Combine(ROMDirectory, file.Trim("\"))) Then
                    IO.File.Delete(IO.Path.Combine(ROMDirectory, file.Trim("\")))
                End If
            Next
        End If

        If IO.Directory.Exists(renameTemp) Then IO.Directory.Delete(renameTemp, True)

        Patched = True
    End Function
    Public Shared Async Function ApplyPatch(Mods As List(Of ModFile), ModFile As ModFile, currentDirectory As String, ROMDirectory As String, patchers As List(Of FilePatcher)) As Task
        If Not ModFile.Patched Then
            'Patch depencencies
            If ModFile.ModDetails.DependenciesBefore IsNot Nothing Then
                For Each item In ModFile.ModDetails.DependenciesBefore
                    Dim q = From m In Mods Where m.id = item AndAlso Not String.IsNullOrEmpty(m.id)

                    For Each d In q
                        Await ApplyPatch(Mods, d, currentDirectory, ROMDirectory, patchers)
                    Next
                Next
            End If
            Await ModFile.ApplyPatch(currentDirectory, ROMDirectory, patchers)
            'Patch dependencies
            If ModFile.ModDetails.DependenciesBefore IsNot Nothing Then
                For Each item In ModFile.ModDetails.DependenciesAfter
                    Dim q = From m In Mods Where m.id = item AndAlso Not String.IsNullOrEmpty(m.id)

                    For Each d In q
                        Await ApplyPatch(Mods, d, currentDirectory, ROMDirectory, patchers)
                    Next
                Next
            End If
        End If
    End Function

    Public Shared Sub CopyFile(OriginalFilename As String, NewFilename As String, Overwrite As Boolean)
        If Not Directory.Exists(IO.Path.GetDirectoryName(NewFilename)) Then
            Directory.CreateDirectory(IO.Path.GetDirectoryName(NewFilename))
        End If
        File.Copy(OriginalFilename, NewFilename, Overwrite)
    End Sub
End Class