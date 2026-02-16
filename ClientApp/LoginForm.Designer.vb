<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class LoginForm
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
        GroupBox1 = New GroupBox()
        statusLabel = New Label()
        btnRegister = New Button()
        btnLogin = New Button()
        passwordLabel = New Label()
        emailLabel = New Label()
        txtPassword = New TextBox()
        txtEmail = New TextBox()
        GroupBox1.SuspendLayout()
        SuspendLayout()
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(statusLabel)
        GroupBox1.Controls.Add(btnRegister)
        GroupBox1.Controls.Add(btnLogin)
        GroupBox1.Controls.Add(passwordLabel)
        GroupBox1.Controls.Add(emailLabel)
        GroupBox1.Controls.Add(txtPassword)
        GroupBox1.Controls.Add(txtEmail)
        GroupBox1.Location = New Point(185, 64)
        GroupBox1.Margin = New Padding(4, 4, 4, 4)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Padding = New Padding(4, 4, 4, 4)
        GroupBox1.Size = New Size(474, 451)
        GroupBox1.TabIndex = 0
        GroupBox1.TabStop = False
        GroupBox1.Text = "Account"
        ' 
        ' statusLabel
        ' 
        statusLabel.AutoSize = True
        statusLabel.Location = New Point(33, 413)
        statusLabel.Margin = New Padding(4, 0, 4, 0)
        statusLabel.Name = "statusLabel"
        statusLabel.Size = New Size(67, 21)
        statusLabel.TabIndex = 1
        statusLabel.Text = "Status : "
        ' 
        ' btnRegister
        ' 
        btnRegister.Location = New Point(279, 333)
        btnRegister.Margin = New Padding(4, 4, 4, 4)
        btnRegister.Name = "btnRegister"
        btnRegister.Size = New Size(96, 32)
        btnRegister.TabIndex = 5
        btnRegister.Text = "Register"
        btnRegister.UseVisualStyleBackColor = True
        ' 
        ' btnLogin
        ' 
        btnLogin.Location = New Point(123, 333)
        btnLogin.Margin = New Padding(4, 4, 4, 4)
        btnLogin.Name = "btnLogin"
        btnLogin.Size = New Size(96, 32)
        btnLogin.TabIndex = 4
        btnLogin.Text = "Login"
        btnLogin.UseVisualStyleBackColor = True
        ' 
        ' passwordLabel
        ' 
        passwordLabel.AutoSize = True
        passwordLabel.Location = New Point(33, 203)
        passwordLabel.Margin = New Padding(4, 0, 4, 0)
        passwordLabel.Name = "passwordLabel"
        passwordLabel.Size = New Size(79, 21)
        passwordLabel.TabIndex = 3
        passwordLabel.Text = "Password"
        ' 
        ' emailLabel
        ' 
        emailLabel.AutoSize = True
        emailLabel.Location = New Point(33, 115)
        emailLabel.Margin = New Padding(4, 0, 4, 0)
        emailLabel.Name = "emailLabel"
        emailLabel.Size = New Size(48, 21)
        emailLabel.TabIndex = 2
        emailLabel.Text = "Email"
        ' 
        ' txtPassword
        ' 
        txtPassword.Location = New Point(123, 199)
        txtPassword.Margin = New Padding(4, 4, 4, 4)
        txtPassword.Name = "txtPassword"
        txtPassword.PasswordChar = "*"c
        txtPassword.Size = New Size(251, 29)
        txtPassword.TabIndex = 1
        txtPassword.UseSystemPasswordChar = True
        ' 
        ' txtEmail
        ' 
        txtEmail.Location = New Point(123, 111)
        txtEmail.Margin = New Padding(4, 4, 4, 4)
        txtEmail.Name = "txtEmail"
        txtEmail.Size = New Size(251, 29)
        txtEmail.TabIndex = 0
        ' 
        ' LoginForm
        ' 
        AutoScaleDimensions = New SizeF(9F, 21F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(809, 617)
        Controls.Add(GroupBox1)
        Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Margin = New Padding(4, 4, 4, 4)
        Name = "LoginForm"
        Text = "LoginForm"
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents emailLabel As Label
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents txtEmail As TextBox
    Friend WithEvents btnRegister As Button
    Friend WithEvents btnLogin As Button
    Friend WithEvents passwordLabel As Label
    Friend WithEvents statusLabel As Label
End Class
