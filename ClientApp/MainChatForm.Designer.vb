<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainChatForm
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
        SplitContainer1 = New SplitContainer()
        firstUsers = New ListBox()
        labelOnlineUsers = New Label()
        firstChat = New ListBox()
        labelChatWith = New Label()
        pnlSend = New Panel()
        btnSend = New Button()
        txtMessage = New TextBox()
        CType(SplitContainer1, ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        pnlSend.SuspendLayout()
        SuspendLayout()
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.Location = New Point(0, 0)
        SplitContainer1.Margin = New Padding(4)
        SplitContainer1.Name = "SplitContainer1"
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(firstUsers)
        SplitContainer1.Panel1.Controls.Add(labelOnlineUsers)
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(firstChat)
        SplitContainer1.Panel2.Controls.Add(labelChatWith)
        SplitContainer1.Panel2.Controls.Add(pnlSend)
        SplitContainer1.Size = New Size(1029, 630)
        SplitContainer1.SplitterDistance = 342
        SplitContainer1.SplitterWidth = 5
        SplitContainer1.TabIndex = 0
        ' 
        ' firstUsers
        ' 
        firstUsers.Dock = DockStyle.Fill
        firstUsers.FormattingEnabled = True
        firstUsers.ItemHeight = 21
        firstUsers.Location = New Point(0, 21)
        firstUsers.Name = "firstUsers"
        firstUsers.Size = New Size(342, 609)
        firstUsers.TabIndex = 1
        ' 
        ' labelOnlineUsers
        ' 
        labelOnlineUsers.AutoSize = True
        labelOnlineUsers.Dock = DockStyle.Top
        labelOnlineUsers.Location = New Point(0, 0)
        labelOnlineUsers.Name = "labelOnlineUsers"
        labelOnlineUsers.Size = New Size(101, 21)
        labelOnlineUsers.TabIndex = 0
        labelOnlineUsers.Text = "Online Users"
        ' 
        ' firstChat
        ' 
        firstChat.Dock = DockStyle.Fill
        firstChat.FormattingEnabled = True
        firstChat.ItemHeight = 21
        firstChat.Location = New Point(0, 21)
        firstChat.Name = "firstChat"
        firstChat.Size = New Size(682, 570)
        firstChat.TabIndex = 3
        ' 
        ' labelChatWith
        ' 
        labelChatWith.AutoSize = True
        labelChatWith.BackColor = SystemColors.Control
        labelChatWith.Dock = DockStyle.Top
        labelChatWith.Location = New Point(0, 0)
        labelChatWith.Name = "labelChatWith"
        labelChatWith.Size = New Size(90, 21)
        labelChatWith.TabIndex = 2
        labelChatWith.Text = "Chat with : "
        ' 
        ' pnlSend
        ' 
        pnlSend.Controls.Add(btnSend)
        pnlSend.Controls.Add(txtMessage)
        pnlSend.Dock = DockStyle.Bottom
        pnlSend.Location = New Point(0, 591)
        pnlSend.Margin = New Padding(4)
        pnlSend.Name = "pnlSend"
        pnlSend.Size = New Size(682, 39)
        pnlSend.TabIndex = 1
        ' 
        ' btnSend
        ' 
        btnSend.Dock = DockStyle.Right
        btnSend.Location = New Point(564, 0)
        btnSend.Margin = New Padding(4)
        btnSend.Name = "btnSend"
        btnSend.Size = New Size(118, 39)
        btnSend.TabIndex = 1
        btnSend.Text = "Send"
        btnSend.UseVisualStyleBackColor = True
        ' 
        ' txtMessage
        ' 
        txtMessage.BackColor = SystemColors.Info
        txtMessage.Dock = DockStyle.Fill
        txtMessage.Location = New Point(0, 0)
        txtMessage.Margin = New Padding(4)
        txtMessage.Multiline = True
        txtMessage.Name = "txtMessage"
        txtMessage.Size = New Size(682, 39)
        txtMessage.TabIndex = 0
        ' 
        ' MainChatForm
        ' 
        AutoScaleDimensions = New SizeF(9F, 21F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1029, 630)
        Controls.Add(SplitContainer1)
        Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Margin = New Padding(4)
        Name = "MainChatForm"
        Text = "MainChatForm"
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel1.PerformLayout()
        SplitContainer1.Panel2.ResumeLayout(False)
        SplitContainer1.Panel2.PerformLayout()
        CType(SplitContainer1, ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        pnlSend.ResumeLayout(False)
        pnlSend.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents pnlSend As Panel
    Friend WithEvents btnSend As Button
    Friend WithEvents txtMessage As TextBox
    Friend WithEvents labelChatWith As Label
    Friend WithEvents firstUsers As ListBox
    Friend WithEvents labelOnlineUsers As Label
    Friend WithEvents firstChat As ListBox
End Class
