' Copyright (c) Laserfiche.

Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes
Imports System.Windows.Forms
Imports Laserfiche.ClientAutomation
Imports Laserfiche.RepositoryAccess
Imports Microsoft.Win32
Imports PageSet = Laserfiche.ClientAutomation.PageSet
Imports SortDirection = Laserfiche.ClientAutomation.SortDirection


Class CustomButtonManagerApp
    Shared Sub Main(ByVal args As String())
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Dim strCommands As String = ""
        For i As Integer = 0 To args.Length - 1
            If i > 0 Then
                strCommands += " "
            End If
            strCommands += args(i)
        Next

        While True
            Try
                If MainHandler(args) Then
                    Exit While
                End If
            Catch e As Exception
                Dim result As DialogResult = MessageBox.Show(e.Message & ", Retry?", "Error", MessageBoxButtons.YesNo)
                If result = DialogResult.No Then
                    Exit While
                End If
            End Try
        End While
    End Sub

    Private Shared Function MainHandler(_args As String()) As Boolean
        Dim buttonid As Integer = 0
        Dim pid As Integer = 0
        Dim hwnd As Integer = 0
        Dim silent As Boolean = False
        Dim command As String = ""
        Dim selectedpages As PageSet = Nothing
        Dim selectedentries As List(Of Integer) = Nothing

        Dim args As New List(Of String)()

        For i As Integer = 0 To _args.Length - 1
            args.Add(_args(i))
        Next

        For i As Integer = 0 To args.Count - 1
            Try
                If args(i) = "-pid" AndAlso args.Count > i + 1 Then
                    pid = Integer.Parse(args(i + 1))
                    i += 1
                ElseIf args(i) = "-buttonid" AndAlso args.Count > i + 1 Then
                    buttonid = Integer.Parse(args(i + 1))
                    i += 1
                ElseIf args(i) = "-hwnd" AndAlso args.Count > i + 1 Then
                    hwnd = Integer.Parse(args(i + 1))
                    i += 1
                ElseIf args(i) = "-command" AndAlso args.Count > i + 1 Then
                    command = args(i + 1)
                    i += 1
                ElseIf args(i) = "-SelectedPages" AndAlso args.Count > i + 1 Then
                    selectedpages = New PageSet()
                    If args(i + 1).Length > 0 Then
                        Dim pagenumArray As String() = args(i + 1).Split(","c)
                        For Each strEntryId As String In pagenumArray
                            selectedpages.AddPage(Integer.Parse(strEntryId))
                        Next
                    End If
                    i += 1
                ElseIf args(i) = "-SelectedEntries" AndAlso args.Count > i + 1 Then
                    selectedentries = New List(Of Integer)()
                    If args(i + 1).Length > 0 Then
                        Dim pagenumArray As String() = args(i + 1).Split(","c)
                        For Each strEntryId As String In pagenumArray
                            selectedentries.Add(Integer.Parse(strEntryId))
                        Next
                    End If
                    i += 1
                ElseIf args(i) = "-silent" Then
                    silent = True
                End If
            Catch e As Exception
                MessageBox.Show(e.Message)
            End Try
        Next

        If args.Count > 0 Then
            If args.Count > 0 AndAlso args(0) = "-buttonclick" Then
                ButtonClick(buttonid, pid, hwnd, command, selectedentries,
                 selectedpages)
                Return True
            End If

            Dim strCommands As String = "Unknown parameters:" & vbCr & vbLf
            For i As Integer = 0 To args.Count - 1
                If i > 0 Then
                    strCommands += " "
                End If
                strCommands += args(i)
            Next
            MessageBox.Show(strCommands)
        Else
            If silent Then
                SetupToolbar(True)
            Else
                Application.Run(New CustomButtonManagerDialog())
            End If
            Return True
        End If
        Return True
    End Function

    Private Class MyCustomButtonInfo
        Public Sub New(windowtype As ClientWindowType, caption As String, args As String)
            m_windowtype = windowtype
            m_args = args
            m_caption = caption
        End Sub

        Public m_windowtype As ClientWindowType = ClientWindowType.Unknown
        Public m_args As String = ""
        Public m_caption As String = ""
    End Class

    Private Shared Function GetClientPath() As String
        Dim clientInfoKey As RegistryKey = Registry.LocalMachine.OpenSubKey("Software\Laserfiche\Client", False)
        If clientInfoKey Is Nothing Then
            Throw New Exception("Error reading Laserfiche client version")
        End If
        Dim strCurrentVersion As String = TryCast(clientInfoKey.GetValue("CurrentVersion"), String)
        If strCurrentVersion Is Nothing Then
            Throw New Exception("Error reading Laserfiche client version")
        End If
        Dim dblCurrentVersion As Double = Double.Parse(strCurrentVersion)
        If dblCurrentVersion < 8.4 OrElse dblCurrentVersion > 20.0 Then
            Throw New Exception("Incompatible Laserfiche client version")
        End If
        Dim clientKey As RegistryKey = clientInfoKey.OpenSubKey(strCurrentVersion)
        If clientKey Is Nothing Then
            Throw New Exception("Error reading Laserfiche client version. " & strCurrentVersion & " key not found")
        End If
        Dim strInstallPath As String = TryCast(clientKey.GetValue("InstallPath"), String)
        If Not strInstallPath.EndsWith("\") Then
            strInstallPath += "\"
        End If

        clientInfoKey.Close()
        clientKey.Close()

        Dim clientpath As String = strInstallPath & "LF.exe"
        Return clientpath
    End Function

    ' Create a toolbar and add a variety of sample buttons
    Public Shared Sub SetupToolbar(silent As Boolean)
        RemoveToolbar(True)

        Dim argsbase As String = " -buttonclick -connguid ""%(ConnectionGUID)"" -hwnd ""%(hwnd)"" -pid ""%(PID)"" "
        Dim mainargsbase As String = argsbase & " -SelectedEntries ""%(SelectedEntries)"" "
        Dim docviewerargbase As String = argsbase & " -DocumentID ""%(DocumentID)"" "
        Dim strProcessPath As String = Application.ExecutablePath

        Dim toolbarPosition__1 As ToolbarPosition = ToolbarPosition.Top

        Dim buttons As New List(Of MyCustomButtonInfo)()


        ' Main window buttons
        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "OpenMetadata", mainargsbase & "-command openmetadata"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "SearchByName", mainargsbase & "-command searchbyname"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "UpOneLevel", mainargsbase & "-command uponelevel"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "OpenDocumentViewer", mainargsbase & "-command opendocviewer"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "Refresh", mainargsbase & "-command refresh"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "Print", mainargsbase & "-command print"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "ContextHits", mainargsbase & "-command contexthits"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "Export", mainargsbase & "-command export"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "SetColumns", mainargsbase & "-command setcolumns"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "SetFieldColumns", mainargsbase & "-command setfieldcolumns"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.Main, "CloseAll", mainargsbase & "-command closeall"))

        ' Doc viewer buttons
        buttons.Add(New MyCustomButtonInfo(ClientWindowType.DocumentViewer, "Preview", docviewerargbase & "-command preview"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.DocumentViewer, "FirstPage", docviewerargbase & "-command firstpage"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.DocumentViewer, "LastPage", docviewerargbase & "-command lastpage"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.DocumentViewer, "Refresh", docviewerargbase & "-command refresh"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.DocumentViewer, "Print", docviewerargbase & "-command print"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.DocumentViewer, "Export", docviewerargbase & "-command export"))

        buttons.Add(New MyCustomButtonInfo(ClientWindowType.DocumentViewer, "CloseAll", docviewerargbase & "-command closeall"))

        Using lfclient As New ClientManager()
            Dim windowTypes As New List(Of ClientWindowType)()
            windowTypes.Add(ClientWindowType.Main)
            windowTypes.Add(ClientWindowType.DocumentViewer)

            For Each windowtype As ClientWindowType In windowTypes
                Using toolbarmgr As ToolbarManager = lfclient.GetToolbarManager(windowtype)
                    Dim strToolbarName As String = strProcessPath
                    Dim nSlashPos As Integer = strToolbarName.LastIndexOf("\")
                    If nSlashPos >= 0 Then
                        strToolbarName = strToolbarName.Substring(nSlashPos + 1)
                    End If

                    Dim nToolbarCount As Integer = toolbarmgr.GetToolbarCount()
                    For i As Integer = 0 To nToolbarCount - 1
                        Dim strToolbar As String = toolbarmgr.GetToolbarName(i)
                        If strToolbar = strToolbarName Then
                            toolbarmgr.DeleteToolbar(strToolbarName)
                            Exit For
                        End If
                    Next

                    toolbarmgr.AddToolbar(strToolbarName, toolbarPosition__1)

                    For Each buttonInfo As MyCustomButtonInfo In buttons
                        If buttonInfo.m_windowtype <> windowtype Then
                            Continue For
                        End If

                        Dim nButtonID As Integer = -1
                        Dim nCustomButtons As Integer = toolbarmgr.GetCustomToolbarButtonCount()
                        For i As Integer = 0 To nCustomButtons - 1
                            Dim existingButtonInfo As CustomButtonInfo = toolbarmgr.GetCustomToolbarButton(i)
                            If existingButtonInfo.Description.Equals(buttonInfo.m_caption, StringComparison.CurrentCultureIgnoreCase) AndAlso existingButtonInfo.Command.Contains(buttonInfo.m_args) Then
                                nButtonID = existingButtonInfo.Id
                                Exit For
                            End If
                        Next

                        If nButtonID = -1 Then
                            Dim newButtonInfo__2 As New CustomButtonInfo()
                            newButtonInfo__2.Description = buttonInfo.m_caption

                            newButtonInfo__2.Command = """" & strProcessPath & """" & buttonInfo.m_args

                            Dim nPathSlashPos As Integer = strProcessPath.LastIndexOf("\")
                            Dim strIconDir As String = strProcessPath.Substring(0, nPathSlashPos) & "\Resources\"

                            newButtonInfo__2.IconPath = strIconDir & buttonInfo.m_caption & ".ico"
                            nButtonID = toolbarmgr.AddCustomToolbarButton(newButtonInfo__2)
                        End If

                        Dim newbuttonInfo__3 As New ToolbarButtonInfo()
                        newbuttonInfo__3.Id = nButtonID
                        newbuttonInfo__3.IsSeparator = True

                        toolbarmgr.AddButton(strToolbarName, newbuttonInfo__3, -1)
                    Next
                End Using
            Next
        End Using

        If Not silent Then
            MessageBox.Show("Successfully added toolbar")
        End If
    End Sub

    ' Remove the custom toolbar and all custom buttons
    Public Shared Sub RemoveToolbar(silent As Boolean)
        Dim bRemovedAnything As Boolean = False

        Dim strProcessName As String = Application.ExecutablePath
        Dim nSlashPos As Integer = strProcessName.LastIndexOf("\")
        If nSlashPos >= 0 Then
            strProcessName = strProcessName.Substring(nSlashPos + 1)
        End If

        Using lfclient As New ClientManager()
            Using maintoolbarmgr As ToolbarManager = lfclient.GetToolbarManager(ClientWindowType.Main)
                Using doctoolbarmgr As ToolbarManager = lfclient.GetToolbarManager(ClientWindowType.DocumentViewer)
                    Dim toolbarmgrs As ToolbarManager() = New ToolbarManager(1) {}
                    toolbarmgrs(0) = maintoolbarmgr
                    toolbarmgrs(1) = doctoolbarmgr

                    For Each toolbarmgr As ToolbarManager In toolbarmgrs
                        Dim nToolbarCount As Integer = toolbarmgr.GetToolbarCount()
                        For i As Integer = 0 To nToolbarCount - 1
                            Dim strToolbar As String = toolbarmgr.GetToolbarName(i)
                            If strToolbar.Equals(strProcessName, StringComparison.CurrentCultureIgnoreCase) Then
                                toolbarmgr.DeleteToolbar(strToolbar)
                                bRemovedAnything = True
                            End If
                        Next
                    Next
                End Using

                Dim nCustomButtons As Integer = maintoolbarmgr.GetCustomToolbarButtonCount()
                For i As Integer = 0 To nCustomButtons - 1
                    maintoolbarmgr.RemoveCustomToolbarButton(0)
                    bRemovedAnything = True
                Next
            End Using
        End Using

        If Not silent Then
            If bRemovedAnything Then
                MessageBox.Show("Successfully removed toolbar")
            Else
                MessageBox.Show("Toolbar not found")
            End If
        End If
    End Sub

    Public Shared Sub LaunchClient()
        Using lfclient As New ClientManager()
            Dim options As New LaunchOptions()
            lfclient.LaunchClient(options)
        End Using
    End Sub

    <DllImport("ole32.dll", CharSet:=CharSet.Auto, ExactSpelling:=True)>
    Public Shared Function CreateStreamOnHGlobal(hGlobal As IntPtr, fDeleteOnRelease As Boolean, ByRef istream As IStream) As Integer
    End Function

    ' Custom button click handler (when -buttonclick is specified on the command line)
    Private Shared Function ButtonClick(buttonid As Integer, pid As Integer, hwnd As Integer, command As String, selectedentries As List(Of Integer),
     selectedpages As PageSet) As Boolean
        Using lfclient As New ClientManager()
            Dim clients As IEnumerable(Of ClientInstance) = lfclient.GetAllClientInstances()
            For Each client As ClientInstance In clients
                If client.ProcessID = pid Then
                    Dim windows As IEnumerable(Of ClientWindow) = client.GetAllClientWindows()
                    For Each window As ClientWindow In windows
                        If window.Hwnd = CType(hwnd, IntPtr) Then
                            Dim repoconn As RepositoryConnection = window.GetCurrentRepository()

                            If window.GetWindowType() = ClientWindowType.Main Then
                                Dim mainwindow As MainWindow = DirectCast(window, MainWindow)

                                Dim listEntryIDs As IList(Of Integer) = mainwindow.GetSelectedEntries()
                                If listEntryIDs.Count = 1 Then
                                    If command = "openmetadata" Then
                                        ' Open the metadata dialog for the currently selected entry, with the position and tabs preset.
                                        Dim options As New OpenOptions()
                                        options.OpenStyle = DocumentOpenType.Metadata
                                        options.MetadataVisibleTabs = MetadataTab.Signatures Or MetadataTab.Fields
                                        options.MetadataStartTab = MetadataTab.Signatures

                                        Dim screenrect As New System.Drawing.Rectangle()
                                        screenrect = System.Windows.Forms.Screen.GetBounds(screenrect)

                                        options.Position = New WindowPosition(screenrect.Right - 600, screenrect.Bottom - 400, screenrect.Right, screenrect.Bottom, False)
                                        mainwindow.OpenDocumentById(listEntryIDs(0), options)
                                    ElseIf command = "print" Then
                                        ' Print silently
                                        Dim printoptions As New PrintOptions()
                                        printoptions.PageNumbers = New PageSet()
                                        printoptions.PageNumbers.AddPage(1)
                                        printoptions.PageNumbers.AddPage(6)
                                        printoptions.DoNotPrompt = False
                                        printoptions.DocumentPart = PrintType.Images
                                        printoptions.PrinterName = "Microsoft Print to PDF"
                                        'printoptions.printername = "Send To OneNote 2010";
                                        'printoptions.printername = @"\\v-services\HP LaserJet CP3525 - Dev";

                                        mainwindow.PrintById(listEntryIDs(0), printoptions)
                                    ElseIf command = "scan" Then
                                        ' Launch scanning
                                        Dim options As New ScanOptions()
                                        options.EntryId = listEntryIDs(0)
                                        options.ScanMode = ScanMode.Standard
                                        options.InsertPagesAt = CInt(InsertAt.[End])

                                        mainwindow.LaunchScanningFromClient(options)
                                    ElseIf command = "searchbyname" Then
                                        ' Run a search for all entries with the same name as the currently selected entry
                                        Dim pRASession As ISession = Util.GetRASession(repoconn)
                                        Dim pLFCurrentEntry As EntryInfo = Entry.GetEntryInfo(listEntryIDs(0), pRASession)

                                        Dim options As New SearchOptions()
                                        options.Query = "({Lf:Name=""" & pLFCurrentEntry.Name & """, Type=""DBFS""}) & {LF:ID<>" & pLFCurrentEntry.Id & "}"
                                        options.NewWindow = False
                                        mainwindow.LaunchSearch(options)
                                    End If
                                End If

                                If command = "refresh" Then
                                    ' Refresh the current window
                                    mainwindow.Refresh()
                                ElseIf command = "uponelevel" Then
                                    ' Move up one level to the parent folder
                                    Dim nCurrentFolderID As Integer = mainwindow.GetCurrentFolderId()

                                    If repoconn IsNot Nothing AndAlso nCurrentFolderID <> 0 Then
                                        Dim pRASession As ISession = Util.GetRASession(repoconn)
                                        Dim pLFCurrentFolder As FolderInfo = Folder.GetFolderInfo(nCurrentFolderID, pRASession)
                                        Dim pLFParentFolderId As Integer = pLFCurrentFolder.ParentId
                                        If pLFParentFolderId > -1 Then
                                            mainwindow.SetCurrentFolder(pLFParentFolderId)
                                        End If
                                    End If
                                ElseIf command = "contexthits" Then
                                    ' Display a message box showing the context hits for the currently selected search result
                                    Dim entryids As IList(Of Integer) = mainwindow.GetSelectedEntries()
                                    Dim contexthits As IList(Of ContextHitInfo) = mainwindow.GetSelectedContextHits()
                                    If contexthits.Count = 0 Then
                                        MessageBox.Show("No context hits selected")
                                    Else
                                        Dim strdetails As String = ""
                                        For i As Integer = 0 To contexthits.Count - 1
                                            If i > 0 Then
                                                strdetails += vbCr & vbLf & vbCr & vbLf
                                            End If
                                            Dim info As ContextHitInfo = contexthits(i)
                                            strdetails += (("EntryID: " & info.EntryId.ToString() & vbCr & vbLf & "PageNum: " & info.PageNumber.ToString() & vbCr & vbLf & "Info: ") + info.Context & vbCr & vbLf & "Text: ") + info.HitText
                                        Next
                                        MessageBox.Show(strdetails)
                                    End If
                                ElseIf command = "opendocviewer" Then
                                    ' Open the currently selected document in the document viewer, with the position and layout preset.
                                    Dim entryids As IList(Of Integer) = mainwindow.GetSelectedEntries()
                                    If entryids.Count = 1 Then
                                        Dim entryid__1 As Integer = entryids(0)
                                        Dim options As New OpenOptions()
                                        options.OpenStyle = DocumentOpenType.DocumentViewer
                                        options.VisiblePanes = DocViewerPane.MetadataPane Or DocViewerPane.ThumbnailPane
                                        options.MetadataVisibleTabs = MetadataTab.Signatures Or MetadataTab.Fields
                                        options.MetadataStartTab = MetadataTab.Signatures
                                        options.Position = New WindowPosition(0, 0, 800, 800, False)
                                        mainwindow.OpenDocumentById(entryid__1, options)
                                    End If
                                ElseIf command = "setcolumns" Then
                                    ' Set the current column layout to a preset list
                                    Dim options As New SetColumnsOptions()
                                    options.Columns.Add(New ClientColumn(CInt(SystemColumn.Tags), 100))
                                    options.Columns.Add(New ClientColumn(CInt(SystemColumn.ModifierName), 100))
                                    options.Columns.Add(New ClientColumn("Document", 100))
                                    options.SortFieldName = "Document"

                                    options.SortDirection = SortDirection.Ascending
                                    mainwindow.SetColumns(options)
                                ElseIf command = "setfieldcolumns" AndAlso listEntryIDs.Count > 0 Then
                                    ' Set the current column layout to the template and fields for the currently selected entries.
                                    Dim pRASession As ISession = Util.GetRASession(repoconn)

                                    Dim options As New SetColumnsOptions()
                                    options.Columns.Add(New ClientColumn(CInt(SystemColumn.TemplateName), 100))

                                    For Each entryID__2 As Integer In listEntryIDs
                                        Dim pLFCurrentEntry As EntryInfo = Entry.GetEntryInfo(entryID__2, pRASession)
                                        If pLFCurrentEntry.EntryType = EntryType.Shortcut Then
                                            Dim targetEntryId = DirectCast(pLFCurrentEntry, IShortcutInfo).TargetId
                                            pLFCurrentEntry = Entry.GetEntryInfo(targetEntryId, pRASession)
                                        End If

                                        Dim pLFFielddata As FieldValueCollection = pLFCurrentEntry.GetFieldValues()
                                        Dim pLFTemplateName As String = pLFCurrentEntry.TemplateName
                                        If Not String.IsNullOrEmpty(pLFTemplateName) Then
                                            Dim pLFTemplate = Template.GetInfo(pLFTemplateName, pRASession)
                                            If pLFTemplate IsNot Nothing AndAlso pLFTemplate.FieldCount > 0 Then
                                                Dim fields = pLFTemplate.Fields
                                                For Each pLFField In fields
                                                    Dim strfieldname As String = pLFField.Name
                                                    Dim column As New ClientColumn(strfieldname, 100)
                                                    options.Columns.Add(column)
                                                Next
                                            End If
                                        End If

                                        For Each field In pLFFielddata
                                            Dim column As New ClientColumn(field.Key, 100)
                                            options.Columns.Add(column)
                                        Next

                                    Next

                                    options.AreColumnsPersistent = False
                                    mainwindow.SetColumns(options)
                                ElseIf command = "export" Then
                                    ' Export the current entries
                                    Dim options As New ExportOptions()
                                    If selectedpages IsNot Nothing Then
                                        options.PageNumbers = selectedpages
                                    End If
                                    options.DestinationPath = "c:\test"
                                    options.DocumentPart = ExportType.Edoc
                                    options.ImageFormat = ImageType.TiffG4
                                    options.DoNotPrompt = True
                                    options.UseMultiPageFile = True

                                    mainwindow.ExportById(listEntryIDs, options)
                                End If
                            ElseIf window.GetWindowType() = ClientWindowType.DocumentViewer Then
                                Dim docwindow As DocumentViewer = DirectCast(window, DocumentViewer)
                                If command = "nextpage" Then
                                    ' Move the doc viewer to the next page
                                    Dim nCurrentPage As Integer = docwindow.GetCurrentPageNumber()
                                    Dim nPageCount As Integer = docwindow.GetPageCount()
                                    If nCurrentPage < nPageCount Then
                                        docwindow.GoToPage(nCurrentPage + 1)
                                    Else
                                        docwindow.GoToPage(1)
                                    End If
                                End If
                                If command = "firstpage" Then
                                    ' Jump to the first page in the document
                                    docwindow.GoToPage(1)
                                End If
                                If command = "lastpage" Then
                                    ' Jump to the last page in the document
                                    Dim nPageCount As Integer = docwindow.GetPageCount()
                                    docwindow.GoToPage(nPageCount)
                                ElseIf command = "print" Then
                                    ' Print the current document silently
                                    Dim printoptions As New PrintOptions()
                                    printoptions.PrinterName = "Microsoft Print to PDF"
                                    printoptions.DoNotPrompt = True
                                    Dim nPageCount As Integer = docwindow.GetPageCount()
                                    If nPageCount > 0 Then
                                        printoptions.PageNumbers = New PageSet()
                                        printoptions.PageNumbers.AddPage(nPageCount)
                                        printoptions.DocumentPart = PrintType.Images
                                    End If

                                    docwindow.Print(printoptions)
                                ElseIf command = "refresh" Then
                                    ' Refresh the doc viewer
                                    docwindow.Refresh()
                                ElseIf command = "scan" Then
                                    ' Launch scanning
                                    Dim options As New ScanOptions()
                                    options.EntryId = docwindow.GetDocumentId()
                                    options.ScanMode = ScanMode.Standard
                                    options.InsertPagesAt = CInt(InsertAt.[End])

                                    docwindow.LaunchScanningFromClient(options)
                                ElseIf command = "export" Then
                                    ' Export the currently selected pages
                                    Dim options As New ExportOptions()
                                    If selectedpages IsNot Nothing Then
                                        options.PageNumbers = selectedpages
                                    End If
                                    options.DestinationPath = "c:\test"
                                    options.DocumentPart = ExportType.Images
                                    options.ImageFormat = ImageType.Jpeg

                                    docwindow.Export(options)
                                ElseIf command = "preview" Then
                                    ' Open the current document in the preview pane
                                    For Each _window As ClientWindow In windows
                                        If _window.GetWindowType() = ClientWindowType.Main Then
                                            Dim mainwindow As MainWindow = DirectCast(_window, MainWindow)
                                            Dim options As New OpenOptions()
                                            options.OpenStyle = DocumentOpenType.Preview
                                            options.VisiblePanes = DocViewerPane.ThumbnailPane Or DocViewerPane.MetadataPane
                                            mainwindow.OpenDocumentById(docwindow.GetDocumentId(), options)
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    Next
                End If
            Next
            If command = "closeall" Then
                For Each client As ClientInstance In clients
                    client.Close(False)
                Next
            End If

            Return False
        End Using
    End Function
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Class