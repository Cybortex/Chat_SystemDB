Imports System.Text.Json

Public Class LoginForm

    Private ReadOnly _client As ChatClient

    Public Sub New(client As ChatClient)
        InitializeComponent()
        _client = client
    End Sub

    Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler _client.ErrorOccurred, AddressOf OnError
        AddHandler _client.MessageReceived, AddressOf OnMessageReceived

        statusLabel.Text = "Status: Enter email and click Login or Register"
        txtPassword.UseSystemPasswordChar = True
    End Sub

    Private Async Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Await IdentifyAsync()
    End Sub

    Private Async Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        ' Dummy register = same identify for now
        Await IdentifyAsync()
    End Sub

    Private Async Function IdentifyAsync() As Task
        Dim email = txtEmail.Text.Trim().ToLowerInvariant()
        If String.IsNullOrWhiteSpace(email) Then
            statusLabel.Text = "Status: Email is required"
            Return
        End If

        statusLabel.Text = "Status: Logging in..."

        Dim msg As New ChatClient.NetMessage With {.Type = "identify"}
        msg.Data("email") = JsonDocument.Parse("""" & email.Replace("""", "") & """").RootElement

        Await _client.SendAsync(msg)
    End Function

    Private Sub OnMessageReceived(sender As Object, msg As ChatClient.NetMessage)
        If InvokeRequired Then
            Invoke(Sub() OnMessageReceived(sender, msg))
            Return
        End If

        Dim t = (If(msg.Type, "")).ToLowerInvariant()

        If t = "identified" Then
            Dim email = ""
            If msg.Data IsNot Nothing AndAlso msg.Data.ContainsKey("email") Then
                email = msg.Data("email").GetString()
            End If

            statusLabel.Text = "Status: Logged in as " & email

            ' Open MainChatForm and close this one
            Dim f As New MainChatForm(_client, email)
            f.Show()

            Me.Hide()
        ElseIf t = "error" Then
            statusLabel.Text = "Status: " & msg.ErrorMsg
        End If
    End Sub

    Private Sub OnError(sender As Object, errorMsg As String)
        If InvokeRequired Then
            Invoke(Sub() OnError(sender, errorMsg))
            Return
        End If

        statusLabel.Text = "Status: Error - " & errorMsg
    End Sub

End Class
