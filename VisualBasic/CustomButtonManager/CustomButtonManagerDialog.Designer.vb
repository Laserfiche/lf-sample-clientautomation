<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CustomButtonManagerDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.ButtonAddToolbar = New System.Windows.Forms.Button()
        Me.ButtonRemoveToolbar = New System.Windows.Forms.Button()
        Me.ButtonLaunchClient = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'ButtonAddToolbar
        '
        Me.ButtonAddToolbar.Location = New System.Drawing.Point(54, 24)
        Me.ButtonAddToolbar.Name = "ButtonAddToolbar"
        Me.ButtonAddToolbar.Size = New System.Drawing.Size(123, 27)
        Me.ButtonAddToolbar.TabIndex = 0
        Me.ButtonAddToolbar.Text = "Add Toolbar"
        Me.ButtonAddToolbar.UseVisualStyleBackColor = True
        '
        'ButtonRemoveToolbar
        '
        Me.ButtonRemoveToolbar.Location = New System.Drawing.Point(54, 55)
        Me.ButtonRemoveToolbar.Name = "ButtonRemoveToolbar"
        Me.ButtonRemoveToolbar.Size = New System.Drawing.Size(123, 27)
        Me.ButtonRemoveToolbar.TabIndex = 1
        Me.ButtonRemoveToolbar.Text = "Remove Toolbar"
        Me.ButtonRemoveToolbar.UseVisualStyleBackColor = True
        '
        'ButtonLaunchClient
        '
        Me.ButtonLaunchClient.Location = New System.Drawing.Point(54, 86)
        Me.ButtonLaunchClient.Name = "ButtonLaunchClient"
        Me.ButtonLaunchClient.Size = New System.Drawing.Size(123, 27)
        Me.ButtonLaunchClient.TabIndex = 2
        Me.ButtonLaunchClient.Text = "Launch Client"
        Me.ButtonLaunchClient.UseVisualStyleBackColor = True
        '
        'CustomButtonManagerDialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(230, 139)
        Me.Controls.Add(Me.ButtonLaunchClient)
        Me.Controls.Add(Me.ButtonRemoveToolbar)
        Me.Controls.Add(Me.ButtonAddToolbar)
        Me.Name = "CustomButtonManagerDialog"
        Me.Text = "CustomButtonManagerDilaog"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ButtonAddToolbar As System.Windows.Forms.Button
    Friend WithEvents ButtonRemoveToolbar As System.Windows.Forms.Button
    Friend WithEvents ButtonLaunchClient As System.Windows.Forms.Button

End Class
