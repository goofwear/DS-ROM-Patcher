﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Class Language
        
        Private Shared resourceMan As Global.System.Resources.ResourceManager
        
        Private Shared resourceCulture As Global.System.Globalization.CultureInfo
        
        <Global.System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>  _
        Friend Sub New()
            MyBase.New
        End Sub
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("DS_ROM_Patcher.Language", GetType(Language).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to All Files.
        '''</summary>
        Friend Shared ReadOnly Property AllFiles() As String
            Get
                Return ResourceManager.GetString("AllFiles", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Complete.
        '''</summary>
        Friend Shared ReadOnly Property Complete() As String
            Get
                Return ResourceManager.GetString("Complete", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Unable to detect the source ROM type.  Please use one of the following command-line arguments when using a directory as the source: -output-nds, -output-3ds, -output-cia, or -output-hans.
        '''</summary>
        Friend Shared ReadOnly Property ErrorUnknownInputType() As String
            Get
                Return ResourceManager.GetString("ErrorUnknownInputType", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Analyzing files....
        '''</summary>
        Friend Shared ReadOnly Property LoadingAnalzingFiles() As String
            Get
                Return ResourceManager.GetString("LoadingAnalzingFiles", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Comparing files....
        '''</summary>
        Friend Shared ReadOnly Property LoadingComparingFiles() As String
            Get
                Return ResourceManager.GetString("LoadingComparingFiles", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Computing hashes....
        '''</summary>
        Friend Shared ReadOnly Property LoadingComputingHashes() As String
            Get
                Return ResourceManager.GetString("LoadingComputingHashes", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Generating patch....
        '''</summary>
        Friend Shared ReadOnly Property LoadingGeneratingPatch() As String
            Get
                Return ResourceManager.GetString("LoadingGeneratingPatch", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Modpack Zip Files.
        '''</summary>
        Friend Shared ReadOnly Property ModpackZip() As String
            Get
                Return ResourceManager.GetString("ModpackZip", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Patcher Pack.
        '''</summary>
        Friend Shared ReadOnly Property PatcherPack() As String
            Get
                Return ResourceManager.GetString("PatcherPack", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Supported ROMs.
        '''</summary>
        Friend Shared ReadOnly Property SupportedROMs() As String
            Get
                Return ResourceManager.GetString("SupportedROMs", resourceCulture)
            End Get
        End Property
    End Class
End Namespace
