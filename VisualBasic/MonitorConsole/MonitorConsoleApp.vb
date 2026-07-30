' Copyright (c) Laserfiche.

Imports Laserfiche.ClientAutomation

Class MonitorConsoleApp
    Shared Sub Main(ByVal args As String())
        ' The main loop.
        While True
            Try
                If MainHandler(args) Then
                    Exit While
                End If
            Catch e As Exception
                Console.WriteLine(e.Message)
                Console.ReadLine()
            End Try

            System.Threading.Thread.Sleep(500)
        End While
        Console.Write("Done")
    End Sub

    ' Sit in a loop and report any new client windows.
    Private Shared Function MainHandler(args As String()) As Boolean
        Dim lastlines As New List(Of String)()

        While True
            Try
                Dim curlines As List(Of String) = ReportClientInstances()
                If Not ListsEqual(lastlines, curlines) Then
                    Dim strNow As String = DateTime.Now.ToString("hh:mm:ss.fff tt")

                    For Each line As String In curlines
                        Dim bNew As Boolean = True
                        For i As Integer = 0 To lastlines.Count - 1
                            If lastlines(i) = line Then
                                bNew = False
                            End If
                        Next

                        If bNew Then
                            Console.WriteLine(strNow & " " & line)
                        End If
                    Next

                    lastlines = curlines
                End If
            Catch e As ClientAutomationException
                System.Console.Out.WriteLine(e.Message)
                System.Threading.Thread.Sleep(500)
            Catch e As Exception
                System.Console.Out.WriteLine(e.Message)
                System.Threading.Thread.Sleep(500)
            End Try

            System.Threading.Thread.Sleep(200)
        End While
        Return True
    End Function

    ' Find all open clients and print the current folder/selected pages/etc to the console.
    Private Shared Function ReportClientInstances() As List(Of String)
        Dim lines As New List(Of String)()
        Using lfclient As New ClientManager()
            Dim clients As IEnumerable(Of ClientInstance) = lfclient.GetAllClientInstances()
            For Each client As ClientInstance In clients
                Dim windows As IEnumerable(Of ClientWindow) = client.GetAllClientWindows()
                For Each window As ClientWindow In windows
                    If window Is Nothing Then
                        Continue For
                    End If

                    Dim strdetails As String = ""

                    Dim conn As RepositoryConnection = window.GetCurrentRepository()
                    If conn Is Nothing Then
                        strdetails += " Not logged in"
                    Else
                        strdetails += (" (" + conn.UserName & " logged into ") + conn.RepositoryName & ")"
                    End If

                    If window.GetWindowType() = ClientWindowType.Main Then
                        Dim mainwindow As MainWindow = TryCast(window, MainWindow)
                        Dim nCurrentFolderID As Integer = mainwindow.GetCurrentFolderId()
                        If nCurrentFolderID > 0 Then
                            strdetails += " (FolderID: " & nCurrentFolderID.ToString() & ")"
                        End If

                        Dim listSelectedEntries As IList(Of Integer) = mainwindow.GetSelectedEntries()
                        If listSelectedEntries.Count > 0 Then
                            strdetails += " (Selected entries: "
                            For j As Integer = 0 To listSelectedEntries.Count - 1
                                If j > 0 Then
                                    strdetails += ","
                                End If
                                strdetails += listSelectedEntries(j).ToString()
                            Next
                            strdetails += ")"
                        End If
                    ElseIf window.GetWindowType() = ClientWindowType.DocumentViewer Then
                        Dim docwindow As DocumentViewer = TryCast(window, DocumentViewer)

                        Dim nPages As Integer = docwindow.GetPageCount()
                        strdetails += " (Page: " & docwindow.GetCurrentPageNumber().ToString() & "/" & nPages.ToString() & ")"

                        Dim listSelectedThumbnails As PageSet = docwindow.GetSelectedThumbnails()
                        If listSelectedThumbnails.RangeCount > 0 Then
                            strdetails += " (Selected pages: " & listSelectedThumbnails.ToString() & ")"
                        End If
                    End If

                    lines.Add(strdetails)
                Next
            Next
        End Using
        Return lines
    End Function

    ' Find all open clients
    Private Shared Sub FindClientWindows()
        Using lfclient As New ClientManager()
            Dim clients As IEnumerable(Of ClientInstance) = lfclient.GetAllClientInstances()
            For Each client As ClientInstance In clients
                Dim windows As IEnumerable(Of ClientWindow) = client.GetAllClientWindows()
                For Each window As ClientWindow In windows
                    If window.GetWindowType() = ClientWindowType.Main Then
                        Dim mainwindow As MainWindow = TryCast(window, MainWindow)
                        Console.WriteLine("Current folder: " & mainwindow.GetCurrentFolderId())
                    ElseIf window.GetWindowType() = ClientWindowType.DocumentViewer Then
                        Dim docwindow As DocumentViewer = TryCast(window, DocumentViewer)
                        Console.WriteLine("Current document: " & docwindow.GetDocumentId())
                    End If
                Next
            Next
        End Using
    End Sub

    ' Find all open clients
    Private Shared Function FindClientWindows2() As String
        Dim strdetails As String = ""
        Using lfclient As New ClientManager()
            Dim clients As IEnumerable(Of ClientInstance) = lfclient.GetAllClientInstances()
            For Each client As ClientInstance In clients
                Dim windows As IEnumerable(Of ClientWindow) = client.GetAllClientWindows()
                For Each window As ClientWindow In windows
                    If strdetails.Length > 0 Then
                        strdetails += vbCr & vbLf
                    End If
                    strdetails = window.GetWindowTitle()

                    Dim conn As RepositoryConnection = window.GetCurrentRepository()
                    If conn IsNot Nothing Then
                        strdetails += (" (" + conn.UserName & " logged into ") + conn.RepositoryName & ")"
                    End If

                    If window.GetWindowType() = ClientWindowType.Main Then
                        Dim mainwindow As MainWindow = TryCast(window, MainWindow)
                        Dim nCurrentFolderID As Integer = mainwindow.GetCurrentFolderId()
                        If nCurrentFolderID > 0 Then
                            strdetails += " (FolderID: " & nCurrentFolderID.ToString() & ")"
                        End If
                    ElseIf window.GetWindowType() = ClientWindowType.DocumentViewer Then
                        Dim docwindow As DocumentViewer = TryCast(window, DocumentViewer)

                        Dim nPages As Integer = docwindow.GetPageCount()
                        strdetails += " (Page: " & docwindow.GetCurrentPageNumber().ToString()
                        strdetails += "/" & nPages.ToString() & ")"
                    End If
                Next
            Next
        End Using
        Return strdetails
    End Function

    ' Do the two string lists contain the same thing?
    Private Shared Function ListsEqual(list1 As List(Of String), list2 As List(Of String)) As Boolean
        If list1.Count <> list2.Count Then
            Return False
        End If

        For i As Integer = 0 To list1.Count - 1
            If list1(i) <> list2(i) Then
                Return False
            End If
        Next

        Return True
    End Function
End Class