' Copyright (c) Laserfiche.

Imports Laserfiche.ClientAutomation
Imports Laserfiche.RepositoryAccess


NotInheritable Class Util

    Public Shared Function GetRASession(repoconn As RepositoryConnection) As ISession
        Dim strSerializedConnection As String = repoconn.GetConnectionString()
        Return Session.CreateFromSerializedLFConnectionString(strSerializedConnection)
    End Function

    Private Shared Sub OpenFindRefresh(args As String())
        If args.Count() <> 4 Then
            Return
        End If

        ' Find a client that is logged into AutoUpdate. If one isn't running,
        ' launch the client and log in. Then refresh all open windows.
        'using ClientAutomation;
        'using LFSO100Lib;

        Dim server As String = "v-qa-autoupdate"
        Dim repository As String = "AutoUpdate"
        Dim pLFServer As Server = New Server(server)
        Dim pLFDatabase As RepositoryRegistration = New RepositoryRegistration(server, repository)

        Using lfclient As New ClientManager()
            ' Find an existing client instance that is logged in to the repository
            Dim clients As IEnumerable(Of ClientInstance) = lfclient.GetAllClientInstances()
            Dim client As ClientInstance = Nothing
            For Each _client As ClientInstance In clients
                Dim repos As IEnumerable(Of RepositoryConnection) = _client.RepositoryConnections
                For Each repo As RepositoryConnection In repos
                    If repo.RepositoryName.ToString().ToLower() = pLFDatabase.Name.ToLower() AndAlso
                        repo.ServerName.ToString().ToLower() = pLFDatabase.ServerName.ToLower() Then
                        client = _client
                        Exit For
                    End If
                Next
                If client IsNot Nothing Then
                    Exit For
                End If
            Next

            ' No matching client found, launch a new one
            If client Is Nothing Then
                Dim options As New LaunchOptions()
                options.ServerName = server
                options.RepositoryName = repository
                options.ShowSplashScreen = False
                options.UserName = "admin"
                ' Leave username blank for windows authentication
                client = lfclient.LaunchClient(options)
            End If

            ' Get all of the open windows and refresh them
            Dim windows As IEnumerable(Of ClientWindow) = client.GetAllClientWindows()
            For Each window As ClientWindow In windows
                If window.GetWindowType() = ClientWindowType.Main Then
                    Dim mainwindow As MainWindow = DirectCast(window, MainWindow)
                    mainwindow.Refresh()
                ElseIf window.GetWindowType() = ClientWindowType.DocumentViewer Then
                    Dim docwindow As DocumentViewer = DirectCast(window, DocumentViewer)
                    docwindow.Refresh()
                End If
            Next
        End Using
    End Sub

    Private Shared Sub ButtonClickHandler(args As String())
        If args.Count() <> 4 Then
            Return
        End If

        Dim pid As Integer = 0
        ' The LF.exe process ID that the button was clicked from
        Dim hwnd As Integer = 0
        ' The window that the button was clicked from
        If args(0) = "-pid" Then
            pid = Integer.Parse(args(1))
        End If
        If args(2) = "-hwnd" Then
            hwnd = Integer.Parse(args(3))
        End If

        Using lfclient As New ClientManager()
            ' Find an existing client instance that is logged in to the repository
            Dim clients As IEnumerable(Of ClientInstance) = lfclient.GetAllClientInstances()
            For Each client As ClientInstance In clients
                If client.ProcessID = pid Then
                    Dim windows As IEnumerable(Of ClientWindow) = client.GetAllClientWindows()
                    For Each window As ClientWindow In windows
                        If window.Hwnd = CType(hwnd, IntPtr) Then
                            ' Found the window, now get the selected context hits
                            If window.GetWindowType() = ClientWindowType.Main Then
                                Dim mainwindow As MainWindow = DirectCast(window, MainWindow)
                                Dim contexthits As IList(Of ContextHitInfo) = mainwindow.GetSelectedContextHits()
                                Dim strdetails As String = ""
                                For i As Integer = 0 To contexthits.Count - 1
                                    If i > 0 Then
                                        strdetails += vbCr & vbLf & vbCr & vbLf
                                    End If
                                    Dim info As ContextHitInfo = contexthits(i)
                                    strdetails += (("EntryID: " & info.EntryId.ToString() & vbCr & vbLf & "PageNum: " & info.PageNumber.ToString() & vbCr & vbLf & "Info: ") + info.HitType & vbCr & vbLf & "Text: ") + info.HitText
                                Next
                                Console.Write(strdetails)
                            End If
                        End If
                    Next
                End If
            Next
        End Using
    End Sub

    Private Shared Function GetRootFolder()
        Using lfclient As New ClientManager()
            ' Find an existing client instance that is logged in to the repository
            Dim clients As IEnumerable(Of ClientInstance) = lfclient.GetAllClientInstances()
            For Each _client As ClientInstance In clients
                Dim repos As IEnumerable(Of RepositoryConnection) = _client.RepositoryConnections
                For Each repo As RepositoryConnection In repos
                    ' Retrieve the serialized connection string and use it to initialize the LFSO connection object
                    Dim strSerializedConnection As String = repo.GetConnectionString()
                    Dim session = RepositoryAccess.Session.CreateFromSerializedLFConnectionString(strSerializedConnection)
                    Dim folder = RepositoryAccess.Folder.GetFolderInfo(1, session)
                    Return folder
                Next
            Next
        End Using

        Return Nothing
    End Function

End Class