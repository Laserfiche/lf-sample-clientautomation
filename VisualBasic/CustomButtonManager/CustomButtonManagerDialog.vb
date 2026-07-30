' Copyright (c) Laserfiche.

Imports System.Windows.Forms

Public Class CustomButtonManagerDialog

    Private Sub ButtonAddToolbar_Click(sender As System.Object, e As System.EventArgs) Handles ButtonAddToolbar.Click
        DisableButtons()

        Try
            CustomButtonManagerApp.SetupToolbar(False)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        EnableButtons()
    End Sub

    Private Sub ButtonRemoveToolbar_Click(sender As System.Object, e As System.EventArgs) Handles ButtonRemoveToolbar.Click
        DisableButtons()

        Try
            CustomButtonManagerApp.RemoveToolbar(False)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        EnableButtons()
    End Sub

    Private Sub ButtonLaunchClient_Click(sender As System.Object, e As System.EventArgs) Handles ButtonLaunchClient.Click
        DisableButtons()

        Try
            CustomButtonManagerApp.LaunchClient()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        EnableButtons()
    End Sub

    Private Sub DisableButtons()
        ButtonAddToolbar.Enabled = False
        ButtonRemoveToolbar.Enabled = False
        ButtonLaunchClient.Enabled = False
        UseWaitCursor = True
    End Sub

    Private Sub EnableButtons()
        ButtonAddToolbar.Enabled = True
        ButtonRemoveToolbar.Enabled = True
        ButtonLaunchClient.Enabled = True
        UseWaitCursor = False
    End Sub
End Class