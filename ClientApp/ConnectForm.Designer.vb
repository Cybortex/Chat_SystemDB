<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ConnectForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblTitle = New Label()
        lblServerIp = New Label()
        lblStatus = New Label()
        txtServerIP = New TextBox()
        txtPort = New TextBox()
        btnConnect = New Button()
        GroupBox1 = New GroupBox()
        GroupBox1.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblTitle
        ' 
        lblTitle.AutoSize = True
        lblTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblTitle.Location = New Point(6, 19)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(37, 15)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Port :"
        ' 
        ' lblServerIp
        ' 
        lblServerIp.AutoSize = True
        lblServerIp.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblServerIp.Location = New Point(6, 60)
        lblServerIp.Name = "lblServerIp"
        lblServerIp.Size = New Size(68, 15)
        lblServerIp.TabIndex = 1
        lblServerIp.Text = "Server IP : "
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Font = New Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblStatus.Location = New Point(18, 172)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(159, 20)
        lblStatus.TabIndex = 2
        lblStatus.Text = "Status: Not connected"
        ' 
        ' txtServerIP
        ' 
        txtServerIP.Font = New Font("Segoe UI Semibold", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        txtServerIP.Location = New Point(74, 52)
        txtServerIP.Name = "txtServerIP"
        txtServerIP.Size = New Size(160, 29)
        txtServerIP.TabIndex = 3
        txtServerIP.Text = "192.168.13.186"
        ' 
        ' txtPort
        ' 
        txtPort.Font = New Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        txtPort.Location = New Point(49, 12)
        txtPort.Name = "txtPort"
        txtPort.Size = New Size(53, 27)
        txtPort.TabIndex = 4
        txtPort.Text = "5000"
        ' 
        ' btnConnect
        ' 
        btnConnect.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnConnect.Location = New Point(6, 106)
        btnConnect.Name = "btnConnect"
        btnConnect.Size = New Size(68, 23)
        btnConnect.TabIndex = 5
        btnConnect.Text = "Connect"
        btnConnect.UseVisualStyleBackColor = True
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(lblTitle)
        GroupBox1.Controls.Add(btnConnect)
        GroupBox1.Controls.Add(txtPort)
        GroupBox1.Controls.Add(txtServerIP)
        GroupBox1.Controls.Add(lblServerIp)
        GroupBox1.Location = New Point(12, 12)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(260, 157)
        GroupBox1.TabIndex = 6
        GroupBox1.TabStop = False
        GroupBox1.Text = "Server"
        ' 
        ' ConnectForm
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(629, 441)
        Controls.Add(GroupBox1)
        Controls.Add(lblStatus)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False
        Name = "ConnectForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Connect"
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblTitle As Label
    Friend WithEvents lblServerIp As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents txtServerIP As TextBox
    Friend WithEvents txtPort As TextBox
    Friend WithEvents btnConnect As Button
    Friend WithEvents GroupBox1 As GroupBox

End Class
