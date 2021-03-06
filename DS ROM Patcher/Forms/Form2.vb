﻿Imports System.ComponentModel
Imports System.IO
Imports System.IO.Compression
Imports System.Reflection
Imports SkyEditor.Core.Utilities

Public Class Form2

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.Text = String.Format("{0} Patcher v{1}", "DS", Assembly.GetExecutingAssembly.GetName.Version.ToString)
        core = New NDSand3DSCore
    End Sub
    Private WithEvents core As PatcherCore
    Private Property Mods As List(Of ModJson)
    Private Property Modpack As ModpackInfo
    Private Property Patchers As List(Of FilePatcher)

    Private Property IsLoading As Boolean
        Get
            Return _isLoading
        End Get
        Set(value As Boolean)
            _isLoading = value

            btnBrowse.Enabled = Not value
            btnPatch.Enabled = Not value
            menuMain.Enabled = Not value
        End Set
    End Property
    Dim _isLoading As Boolean

    'Filenames
    Dim tempDirectory = Path.Combine(Path.GetTempPath, "DS-ROM-Patcher-" & Guid.NewGuid.ToString)
    Dim currentDirectory = Environment.CurrentDirectory
    Dim modTempDirectory = Path.Combine(tempDirectory, "modstemp")
    Dim unpackTempDirectory = Path.Combine(tempDirectory, "dstemp")
    Dim modsDirectory = Path.Combine(currentDirectory, "Mods")

    Private Async Sub Form2_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim completed As Integer = 0

        'Create directories
        If Not Directory.Exists(modsDirectory) Then
            Directory.CreateDirectory(modsDirectory)
        End If
        If Not Directory.Exists(modTempDirectory) Then
            Directory.CreateDirectory(modTempDirectory)
        End If

        'Delete old directories (in case the program crashed earlier)
        If Directory.Exists(unpackTempDirectory) Then
            Directory.Delete(unpackTempDirectory, True)
        End If

        'Load modpack info
        Modpack = ModBuilder.GetModpackInfo(currentDirectory)

        'Unpack Mods
        If Directory.Exists(modsDirectory) Then
            Dim modFiles = Directory.GetFiles(modsDirectory, "*.mod", SearchOption.TopDirectoryOnly)

            lblStatus.Text = "Unpacking Mods..."
            For Each item In modFiles
                pbProgress.Value = completed / modFiles.Count

                If Not Directory.Exists(Path.Combine(modTempDirectory, Path.GetFileNameWithoutExtension(item))) Then
                    Directory.CreateDirectory(Path.Combine(modTempDirectory, Path.GetFileNameWithoutExtension(item)))
                End If
                ZipFile.ExtractToDirectory(item, Path.Combine(modTempDirectory, Path.GetFileNameWithoutExtension(item)))

                completed += 1
            Next
        End If

        'Load Mods
        Dim modDirs = Directory.GetDirectories(modTempDirectory, "*", SearchOption.TopDirectoryOnly)
        Dim total As Integer = modDirs.Count
        Mods = New List(Of ModJson)

        lblStatus.Text = "Opening Mods..."
        completed = 0

        For Each item In modDirs
            pbProgress.Value = completed / total

            Dim jsonFile = IO.Path.Combine(item, "mod.json")
            If IO.File.Exists(jsonFile) Then
                Dim m = Json.Deserialize(Of ModJson)(File.ReadAllText(jsonFile))
                m.Filename = jsonFile
                Mods.Add(m)
                completed += 1
            Else
                total -= 1
            End If
        Next

        lblStatus.Text = "Ready"

        'Load patchers
        Dim patchersDir = Path.Combine(currentDirectory, "Tools", "Patchers")
        Dim patchersPath = Path.Combine(patchersDir, "patchers.json")
        If File.Exists(patchersPath) Then
            Patchers = FilePatcher.DeserializePatcherList(patchersPath, patchersDir)
        Else
            Patchers = New List(Of FilePatcher)
        End If

        'Auto-patch, if applicable
        Dim args = Environment.GetCommandLineArgs
        If args.Count > 2 Then
            btnBrowse.Enabled = False
            btnPatch.Enabled = False
            core.SelectedFilename = args(1)
            For Each item In Mods
                If Await core.SupportsMod(Modpack, item) Then
                    chbMods.Items.Add(item, True)
                End If
            Next

            Dim items As New List(Of ModFile)
            For Each item As ModJson In chbMods.CheckedItems
                items.Add(New ModFile(item.Filename))
            Next
            Await core.RunPatch(currentDirectory, tempDirectory, Patchers, Modpack, items, args(2))

            Me.Close()
        End If
    End Sub

#Region "Menu Event Handlers"
    Private Sub ImportPatcherPackToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImportPatcherPackToolStripMenuItem.Click
        Dim o As New OpenFileDialog
        o.Filter = $"{My.Resources.Language.PatcherPack}|*.dsrppp;*.zip|{My.Resources.Language.AllFiles}|*.*"
        If o.ShowDialog = DialogResult.OK Then
            FilePatcher.ImportCurrentPatcherPack(currentDirectory, o.FileName)
        End If
    End Sub

    Private Sub ExportPatcherPackToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportPatcherPackToolStripMenuItem.Click
        Dim s As New SaveFileDialog
        s.Filter = $"{My.Resources.Language.PatcherPack}|*.dsrppp;*.zip|{My.Resources.Language.AllFiles}|*.*"
        If s.ShowDialog = DialogResult.OK Then
            FilePatcher.ExportCurrentPatcherPack(currentDirectory, s.FileName)
        End If
    End Sub

    Private Async Sub CreateModToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CreateModToolStripMenuItem.Click
        Dim c As New CreateModWindow(Patchers, currentDirectory)
        If c.ShowDialog = DialogResult.OK Then
            Dim item = c.CreatedModFilename

            'If a mod was created, extract it
            Dim extractedModDirectory = Path.Combine(modTempDirectory, Path.GetFileNameWithoutExtension(item))
            If Not Directory.Exists(extractedModDirectory) Then
                Directory.CreateDirectory(extractedModDirectory)
            End If
            ZipFile.ExtractToDirectory(item, extractedModDirectory)

            'Then open the JSON and add it
            Dim jsonFile = Path.Combine(extractedModDirectory, "mod.json")
            If File.Exists(jsonFile) Then
                Dim m = Json.Deserialize(Of ModJson)(File.ReadAllText(jsonFile))
                m.Filename = jsonFile
                Mods.Add(m)

                'Then refresh the display
                If Not String.IsNullOrEmpty(core.SelectedFilename) Then
                    Await RefreshModList()
                End If
            End If
        End If
    End Sub

    Private Sub EditMetadataToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EditMetadataToolStripMenuItem.Click
        Dim metaEdit As New ModpackMetadataWindow(Modpack)
        If metaEdit.ShowDialog = DialogResult.OK Then
            Modpack = metaEdit.ModpackInfo
            ModBuilder.SaveModpackInfo(currentDirectory, metaEdit.ModpackInfo)
        End If
    End Sub

    Private Sub ExportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportToolStripMenuItem.Click
        Dim s As New SaveFileDialog
        s.Filter = $"{My.Resources.Language.ModpackZip}|*.zip|{My.Resources.Language.AllFiles}|*.*"
        If s.ShowDialog = DialogResult.OK Then
            ModBuilder.ZipModpack(currentDirectory, s.FileName)
        End If
    End Sub
#End Region

    Private Async Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        core.PromptFilePath()
        txtInput.Text = core.SelectedFilename

        If Not String.IsNullOrEmpty(txtInput.Text) Then
            Await RefreshModList()
        End If
    End Sub

    Private Async Function RefreshModList() As Task
        'Display supported mods
        chbMods.Items.Clear()
        For Each item In Mods
            If Await core.SupportsMod(Modpack, item) Then
                chbMods.Items.Add(item, True)
            End If
        Next
    End Function

    Private Sub chbMods_SelectedIndexChanged(sender As Object, e As EventArgs) Handles chbMods.SelectedIndexChanged
        If chbMods.SelectedIndex > -1 Then
            txtDescription.Text = DirectCast(chbMods.SelectedItem, ModJson).GetDescription
        End If
    End Sub

    Private Async Sub btnPatch_Click(sender As Object, e As EventArgs) Handles btnPatch.Click
        IsLoading = True

        Dim items As New List(Of ModFile)
        For Each item As ModJson In chbMods.CheckedItems
            items.Add(New ModFile(item.Filename))
        Next
        Await core.RunPatch(currentDirectory, tempDirectory, Patchers, Modpack, items)

        IsLoading = False
    End Sub

    Private Sub Form2_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Directory.Exists(tempDirectory) Then
            Directory.Delete(tempDirectory, True)
        End If
    End Sub

    Private Sub core_ProgressChanged(sender As Object, e As PatcherCore.ProgressChangedEventArgs) Handles core.ProgressChanged
        If InvokeRequired Then
            Invoke(Sub()
                       lblStatus.Text = e.Message
                       pbProgress.Value = e.Progress
                   End Sub)
        Else
            lblStatus.Text = e.Message
            pbProgress.Value = e.Progress * 100
        End If
    End Sub

End Class