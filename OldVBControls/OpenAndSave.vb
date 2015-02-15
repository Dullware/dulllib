Imports System.Windows.Forms
Imports System.Collections

Public Class OpenAndSave
    Inherits System.Windows.Forms.UserControl
    Event NewData()
    Event ReadData(ByVal Filename As String)
    Event WriteData(ByVal Filename As String)
    Event RevertData(ByVal Filename As String)
    Event FileNameChanged(ByVal oldName As String, ByVal newName As String)
    Event FolderNameChanged(ByVal oldName As String, ByVal newName As String)

    Private _Filename As String
    Private _untitled As Boolean = True
    Private _changed As Boolean = False
    Private _UntitledPrefix As String = "Untitled"
    Private _DefaultExt As String = ""
    Private _Filter As String = ""

    Private _multiselect As Boolean = False
    Public Property Multiselect() As Boolean
        Get
            Multiselect = _multiselect
        End Get
        Set(ByVal Value As Boolean)
            _multiselect = Value
        End Set
    End Property

    Public fileQ As New MyQueue(4)

    Private Shared Function GetNewUntitledName() As String
        Static Dim UntitledCounter As Integer = 1
        GetNewUntitledName = "Untitled" + UntitledCounter.ToString
        UntitledCounter += 1
    End Function

    Property DefaultExt() As String
        Get
            DefaultExt = _DefaultExt
        End Get
        Set(ByVal Value As String)
            If Value.StartsWith(".") Then
                Value = Value.Substring(1)
            End If
            _DefaultExt = Value
        End Set
    End Property

    Property Filter() As String
        Get
            Filter = _Filter
        End Get
        Set(ByVal Value As String)
            _Filter = Value
        End Set
    End Property

    ReadOnly Property Filename() As String
        Get
            If _Filename Is Nothing Then _Filename = GetNewUntitledName()
            Filename = _Filename
        End Get
    End Property

    ReadOnly Property Untitled() As Boolean
        Get
            Untitled = _untitled
        End Get
    End Property

    Property UntitledPrefix() As String
        Get
            UntitledPrefix = _UntitledPrefix
        End Get
        Set(ByVal Value As String)
            _UntitledPrefix = Value
        End Set
    End Property

    Overridable Sub OnNewData()
        RaiseEvent NewData()
    End Sub

    Overridable Sub OnReadData(ByVal Filename As String)
        RaiseEvent ReadData(Filename)
    End Sub

    Overridable Sub OnWriteData(ByVal Filename As String)
        RaiseEvent WriteData(Filename)
    End Sub

    Overridable Sub OnRevertData(ByVal Filename As String)
        RaiseEvent RevertData(Filename)
    End Sub

    Overridable Sub OnFileNameChanged(ByVal oldName As String, ByVal newName As String)
        RaiseEvent FileNameChanged(oldName, newName)
    End Sub

    Overridable Sub OnFolderNameChanged(ByVal oldName As String, ByVal newName As String)
        RaiseEvent FolderNameChanged(oldName, newName)
    End Sub

    Private Sub _NewData()
        Try
            _Filename = GetNewUntitledName()
            OnNewData()
        Catch e As Exception
            MessageBox.Show(e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub _OpenData()
        Try
            _untitled = False
            OnReadData(Filename)
            fileQ.Add(Filename)
            _changed = False
        Catch e As Exception
            MessageBox.Show(e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try
    End Sub

    Private Sub _SaveData()
        Try
            _untitled = False
            OnWriteData(Filename)
            fileQ.Add(Filename)
            _changed = False
        Catch e As Exception
            MessageBox.Show(e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub _RevertData()
        Try
            OnRevertData(Filename)
            _changed = False
        Catch e As Exception
            MessageBox.Show(e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try
    End Sub

    Private Function SaveChangesDialog() As DialogResult
        SaveChangesDialog = MessageBox.Show("The document " + Filename + " has changed." + Chr(13) + Chr(13) + "Do you want to save the changes?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation)
    End Function

    Private Function SaveAsDialog(Optional ByVal Title As String = "Save As") As DialogResult
        Dim name, folder As String

        With New IO.FileInfo(Filename)
            name = .Name
            folder = .DirectoryName
            if not folder.EndsWith("\") then folder = folder + "\"
        End With

        With New SaveFileDialog
            .Title = Title
            .FileName = folder + name 'Added May 23 2004
            '.DefaultExt = DefaultExt
            .Filter = Filter

            SaveAsDialog = .ShowDialog
            If SaveAsDialog = DialogResult.OK Then
                _Filename = .FileName

                With New IO.FileInfo(Filename)
                    If name <> .Name Then OnFileNameChanged(name, .Name)
                    If folder <> .DirectoryName Then OnFolderNameChanged(folder, .DirectoryName)
                End With
            End If
            .Dispose()
        End With
    End Function

	Private Function _OKToOpenOrNewFile() As Boolean
		_OKToOpenOrNewFile = False
        If _changed Then
            Select Case SaveChangesDialog()
                Case DialogResult.Yes
                    If Untitled Then
                        Select Case SaveAsDialog()
                            Case DialogResult.OK
                            Case DialogResult.Cancel
                                Exit Function
                        End Select
                    End If
                    _SaveData()
                Case DialogResult.No
                Case DialogResult.Cancel
                    Exit Function
            End Select
        End If
        _OKToOpenOrNewFile = True
    End Function

    Public Sub NewFile(ByVal changed As Boolean)
        _changed = changed
        If _OKToOpenOrNewFile() Then _NewData()
    End Sub

	Public Function OpenFile(ByVal changed As Boolean) As Boolean
		OpenFile = False
	    'Return true when opened.
        _changed = changed
        If _OKToOpenOrNewFile() Then
            With New OpenFileDialog
                .Multiselect = Multiselect
                If _DefaultExt <> "" Then
                    .FileName = "*." + DefaultExt
                    .DefaultExt = DefaultExt
                End If
                .Filter = Filter

                If .ShowDialog() = DialogResult.OK Then
                    _Filename = .FileName
                    _OpenData()
                    OpenFile = True
                End If
                .Dispose()
            End With
        End If
    End Function

    Public Function OpenFile(ByVal file As String, ByVal changed As Boolean) As Boolean
    	OpenFile = False
    	_changed = changed
        If file = _Filename And _changed Then
            If MessageBox.Show("This file is already opened." + Chr(13) + Chr(13) + "Discard all changes?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                _OpenData()
                OpenFile = True
            Else
                Exit Function
            End If
        End If
        If _OKToOpenOrNewFile() Then
            _Filename = file
            _OpenData()
            OpenFile = True
        End If
    End Function

    Public Function SaveFile(ByVal changed As Boolean) As Boolean
        'Returns false if the action is cancelled by the user.
        'So 'true' does not mean the file is actually saved when this function completes.
        SaveFile = False
        _changed = changed
        If changed Or Untitled Then
            If Untitled Then
                Select Case SaveAsDialog()
                    Case DialogResult.OK
                    Case DialogResult.Cancel
                        Exit Function
                End Select
            End If
            _SaveData()
        End If
        SaveFile = True
    End Function

    Public Function CloseWindow(ByVal changed As Boolean) As Boolean
        _changed = changed
        Return _OKToOpenOrNewFile()
    End Function

    Public Function SaveFileAs(Optional ByVal Title As String = "Save As") As Boolean
    	SaveFileAs = False
    	If SaveAsDialog(Title) = DialogResult.OK Then
            _SaveData()
            SaveFileAs = True
        End If
    End Function

    Public Sub OpenFileCommandLine(ByVal changed As Boolean)
        Dim file As String = ""
        _changed = changed
        Try
            file = Microsoft.VisualBasic.Command()
            If file.StartsWith("""") And file.EndsWith("""") Then
                file = file.Substring(1, file.Length - 2)
            End If
        Catch exc As Exception
            MessageBox.Show(exc.Message + Chr(13) + Microsoft.VisualBasic.Command(), Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        If file <> "" AndAlso _OKToOpenOrNewFile() Then
            _Filename = file
            _OpenData()
        End If
    End Sub

    Public Sub Revert(ByVal changed As Boolean)
        If changed AndAlso MessageBox.Show("Do you want to revert to the last saved " + Filename + "?", "Revert", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) = DialogResult.OK Then
            OnRevertData(_Filename)
        End If
    End Sub

    Public Sub New()
        MyBase.New()

        Visible = False
    End Sub

End Class

Public Class MyQueue
    Inherits ArrayList
    Public Size As Integer

    Sub New(ByVal size As Integer)
        Me.Size = size
    End Sub

    Public Overrides Function Add(ByVal value As Object) As Integer
    	Add = 0
    	If Contains(value) Then Remove(value)
        Insert(0, value)
        If Count > Size Then RemoveAt(Size)
    End Function
End Class

