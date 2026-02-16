Imports System.Text.Json

Public Class MainChatForm

    Private ReadOnly _client As ChatClient
    Private ReadOnly _me As String
    Private _activeChatWith As String = Nothing

    Public Sub New(client As ChatClient, meEmail As String)
        InitializeComponent()
        _client = client
        _me = meEmail
    End Sub

    Private Sub MainChatForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler _client.MessageReceived, AddressOf OnMessageReceived
        AddHandler _client.ErrorOccurred, AddressOf OnError
        AddHandler _client.Disconnected, AddressOf OnDisconnected

        labelOnlineUsers.Text = "Online Users"
        labelChatWith.Text = "Chat with: (none)"

        firstUsers.IntegralHeight = False
        firstChat.IntegralHeight = False
    End Sub

    Private Sub firstUsers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles firstUsers.SelectedIndexChanged
        Dim selected = TryCast(firstUsers.SelectedItem, String)
        If String.IsNullOrWhiteSpace(selected) Then Return

        _activeChatWith = selected
        labelChatWith.Text = "Chat with: " & _activeChatWith
    End Sub

    Private Async Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        Dim text = txtMessage.Text.Trim()
        If String.IsNullOrWhiteSpace(text) Then Return

        If String.IsNullOrWhiteSpace(_activeChatWith) Then
            firstChat.Items.Add("[SYSTEM] Select a user on the left first.")
            Return
        End If

        Dim msg As New ChatClient.NetMessage With {.Type = "msg"}
        msg.Data("to") = JsonDocument.Parse("""" & _activeChatWith.Replace("""", "") & """").RootElement
        msg.Data("text") = JsonDocument.Parse("""" & text.Replace("""", "") & """").RootElement

        Await _client.SendAsync(msg)

        firstChat.Items.Add($"Me -> {_activeChatWith}: {text}")
        txtMessage.Clear()
        txtMessage.Focus()
    End Sub

    Private Sub OnMessageReceived(sender As Object, msg As ChatClient.NetMessage)
        If InvokeRequired Then
            Invoke(Sub() OnMessageReceived(sender, msg))
            Return
        End If

        Dim t = (If(msg.Type, "")).ToLowerInvariant()

        Select Case t
            Case "presence"
                ' data.users = ["a@x.com","b@x.com"]
                firstUsers.Items.Clear()

                If msg.Data IsNot Nothing AndAlso msg.Data.ContainsKey("users") Then
                    Dim usersEl = msg.Data("users")
                    If usersEl.ValueKind = JsonValueKind.Array Then
                        For Each u In usersEl.EnumerateArray()
                            Dim email = u.GetString()
                            If Not String.IsNullOrWhiteSpace(email) AndAlso email.ToLowerInvariant() <> _me.ToLowerInvariant() Then
                                firstUsers.Items.Add(email)
                            End If
                        Next
                    End If
                End If

            Case "deliver"
                Dim fromUser = If(msg.Data IsNot Nothing AndAlso msg.Data.ContainsKey("from"), msg.Data("from").GetString(), "(unknown)")
                Dim text = If(msg.Data IsNot Nothing AndAlso msg.Data.ContainsKey("text"), msg.Data("text").GetString(), "")
                firstChat.Items.Add($"{fromUser}: {text}")

            Case "sent"
                If Not String.IsNullOrWhiteSpace(msg.ErrorMsg) Then
                    firstChat.Items.Add("[SEND] Failed: " & msg.ErrorMsg)
                End If

            Case "error"
                firstChat.Items.Add("[ERROR] " & msg.ErrorMsg)
        End Select
    End Sub

    Private Sub OnError(sender As Object, errorMsg As String)
        If InvokeRequired Then
            Invoke(Sub() OnError(sender, errorMsg))
            Return
        End If

        firstChat.Items.Add("[ERROR] " & errorMsg)
    End Sub

    Private Sub OnDisconnected(sender As Object, e As EventArgs)
        If InvokeRequired Then
            Invoke(Sub() OnDisconnected(sender, e))
            Return
        End If

        firstChat.Items.Add("[SYSTEM] Disconnected from server.")
    End Sub

End Class
