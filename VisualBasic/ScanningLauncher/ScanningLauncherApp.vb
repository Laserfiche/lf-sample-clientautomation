' Copyright (c) Laserfiche.

Imports System.Windows.Forms
Imports Laserfiche.ClientAutomation

NotInheritable Class ScanningLauncherApp
    ''' <summary>
    ''' The main entry point for the application.
    ''' </summary>
    Shared Sub Main(args As String())
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
        Dim servername As String = ""
        Dim repository As String = ""
        Dim serializedconn As String = ""
        Dim username As String = ""
        Dim password As String = ""
        Dim usessl As Boolean = False
        Dim folderid As Integer = 0
        Dim documentid As Integer = 0
        Dim insertat As Integer = -1
        ' -3 = Default, -2 = Ask, -1 = End, 0 = Beginning
        Dim waitforclose As Boolean = False
        Dim closeafterstoring As Boolean = False
        Dim scanmode__1 As ScanMode = ScanMode.Standard

        Dim args As New List(Of String)()

        For i As Integer = 0 To _args.Length - 1
            args.Add(_args(i))
        Next

        For i As Integer = 0 To args.Count - 1
            Try
                If args(i) = "-server" AndAlso args.Count > i + 1 Then
                    servername = args(i + 1)
                    i += 1
                ElseIf args(i) = "-repository" AndAlso args.Count > i + 1 Then
                    repository = args(i + 1)
                    i += 1
                ElseIf args(i) = "-ssl" Then
                    usessl = True
                ElseIf args(i) = "-username" AndAlso args.Count > i + 1 Then
                    username = args(i + 1)
                    i += 1
                ElseIf args(i) = "-password" AndAlso args.Count > i + 1 Then
                    password = args(i + 1)
                    i += 1
                ElseIf args(i) = "-serializedconn" AndAlso args.Count > i + 1 Then
                    serializedconn = args(i + 1)
                    i += 1
                ElseIf args(i) = "-folder" AndAlso args.Count > i + 1 Then
                    folderid = Integer.Parse(args(i + 1))
                    i += 1
                ElseIf args(i) = "-document" AndAlso args.Count > i + 1 Then
                    documentid = Integer.Parse(args(i + 1))
                    i += 1
                ElseIf args(i) = "-insertat" AndAlso args.Count > i + 1 Then
                    insertat = Integer.Parse(args(i + 1))
                    i += 1
                ElseIf args(i) = "-waitforclose" Then
                    waitforclose = True
                ElseIf args(i) = "-closeafterstoring" Then
                    closeafterstoring = True
                ElseIf args(i) = "-mode" AndAlso args.Count > i + 1 Then
                    If args(i + 1) = "basic" Then
                        scanmode__1 = ScanMode.Basic
                    ElseIf args(i + 1) = "standard" Then
                        scanmode__1 = ScanMode.Standard
                    ElseIf args(i + 1) = "default" Then
                        scanmode__1 = ScanMode.[Default]
                    Else
                        MessageBox.Show("Invalid scan mode '" & args(i + 1) & "'")
                        Return True
                    End If

                    i += 1
                Else
                    MessageBox.Show("Invalid option '" & args(i) & "'")
                    Return True

                End If
            Catch e As Exception
                MessageBox.Show(e.Message)
            End Try
        Next

        If args.Count > 0 Then
            Using lfclient As New ClientManager()
                Dim scanoptions As New ScanOptions()
                scanoptions.ConnectionString = serializedconn
                scanoptions.RepositoryName = repository
                scanoptions.ServerName = servername
                scanoptions.UserName = username
                scanoptions.Password = password
                scanoptions.IsSecureConnection = usessl
                scanoptions.WaitForExit = waitforclose
                scanoptions.CloseAfterStoring = closeafterstoring
                scanoptions.ScanMode = scanmode__1
                If documentid > 0 Then
                    scanoptions.EntryId = documentid
                    scanoptions.IsDocument = True
                    scanoptions.InsertPagesAt = insertat
                ElseIf folderid > 0 Then
                    scanoptions.EntryId = folderid
                    scanoptions.IsDocument = False
                Else
                    MessageBox.Show("No document/folder specified")
                    Return True
                End If
                lfclient.LaunchScanning(scanoptions)
            End Using
        Else
            MessageBox.Show("No arguments specified")
            Return True
        End If
        Return True
    End Function
End Class
