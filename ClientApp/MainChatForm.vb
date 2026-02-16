Imports System.Text.Json

Public Class MainChatForm

    Private ReadOnly _client As ChatClient
    Private ReadOnly _me As String

    Private _activeChatWith As String = Nothing

    ' Per-user chat history (prevents mixing A/B/C messages in one panel)
    Private ReadOnly _chatHistory As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

    ' Per-user unread count (for left list badge: "email (3)")
    Private ReadOnly _unreadCounts As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

    ' Online users list as raw emails (we render badges from this)
    Private ReadOnly _onlineUsers As New List(Of String)()

    ' *** FIX: prevents infinite loop when rendering users ***
    Private _isRenderingUsers As Boolean = False

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

    Private Sub RenderUsers()
        _isRenderingUsers = True
        Try
            ' Keep selection by raw email if possible
            Dim selectedRaw As String = _activeChatWith

            firstUsers.Items.Clear()

            For Each email In _onlineUsers
                Dim unread As Integer = 0
                If _unreadCounts.ContainsKey(email) Then unread = _unreadCounts(email)

                Dim display = If(unread > 0, $"{email} ({unread})", email)
                firstUsers.Items.Add(display)
            Next

            ' Restore selection (match by prefix "email" because display may include "(n)")
            If Not String.IsNullOrWhiteSpace(selectedRaw) Then
                For i As Integer = 0 To firstUsers.Items.Count - 1
                    Dim itemText = firstUsers.Items(i).ToString()
                    If itemText.StartsWith(selectedRaw, StringComparison.OrdinalIgnoreCase) Then
                        firstUsers.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If
        Finally
            _isRenderingUsers = False
        End Try
    End Sub

    Private Function GetRawEmailFromUserItem(item As Object) As String
        Dim s = TryCast(item, String)
        If String.IsNullOrWhiteSpace(s) Then Return Nothing

        ' Strip " (n)" if present
        Dim idx = s.IndexOf(" (", StringComparison.Ordinal)
        If idx > 0 Then s = s.Substring(0, idx)

        Return s.Trim()
    End Function

    Private Sub LoadActiveChat()
        firstChat.Items.Clear()

        If String.IsNullOrWhiteSpace(_activeChatWith) Then
            labelChatWith.Text = "Chat with: (none)"
            Return
        End If

        labelChatWith.Text = "Chat with: " & _activeChatWith

        If _chatHistory.ContainsKey(_activeChatWith) Then
            For Each line In _chatHistory(_activeChatWith)
                firstChat.Items.Add(line)
            Next
        End If
    End Sub

    Private Sub AddToHistory(otherUser As String, line As String)
        If String.IsNullOrWhiteSpace(otherUser) Then Return

        If Not _chatHistory.ContainsKey(otherUser) Then
            _chatHistory(otherUser) = New List(Of String)()
        End If

        _chatHistory(otherUser).Add(line)

        ' Only show immediately if this conversation is currently open
        If Not String.IsNullOrWhiteSpace(_activeChatWith) AndAlso
           otherUser.Equals(_activeChatWith, StringComparison.OrdinalIgnoreCase) Then
            firstChat.Items.Add(line)
        End If
    End Sub

    Private Sub firstUsers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles firstUsers.SelectedIndexChanged
        ' *** FIX: prevent re-entrancy from RenderUsers ***
        If _isRenderingUsers Then Return

        Dim selectedRaw = GetRawEmailFromUserItem(firstUsers.SelectedItem)
        If String.IsNullOrWhiteSpace(selectedRaw) Then Return

        _activeChatWith = selectedRaw

        ' Clear unread on click
        If _unreadCounts.ContainsKey(_activeChatWith) Then
            _unreadCounts(_activeChatWith) = 0
        End If

        RenderUsers()
        LoadActiveChat()
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

        AddToHistory(_activeChatWith, $"Me -> {_activeChatWith}: {text}")

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
                _onlineUsers.Clear()

                If msg.Data IsNot Nothing AndAlso msg.Data.ContainsKey("users") Then
                    Dim usersEl = msg.Data("users")
                    If usersEl.ValueKind = JsonValueKind.Array Then
                        For Each u In usersEl.EnumerateArray()
                            Dim email = u.GetString()
                            If Not String.IsNullOrWhiteSpace(email) AndAlso
                               email.ToLowerInvariant() <> _me.ToLowerInvariant() Then
                                _onlineUsers.Add(email)
                            End If
                        Next
                    End If
                End If

                RenderUsers()

            Case "deliver"
                Dim fromUser = If(msg.Data IsNot Nothing AndAlso msg.Data.ContainsKey("from"),
                                  msg.Data("from").GetString(),
                                  "(unknown)")

                Dim text = If(msg.Data IsNot Nothing AndAlso msg.Data.ContainsKey("text"),
                              msg.Data("text").GetString(),
                              "")

                AddToHistory(fromUser, $"{fromUser}: {text}")

                ' If message is not for the currently open chat, increment unread badge
                If String.IsNullOrWhiteSpace(_activeChatWith) OrElse
                   Not fromUser.Equals(_activeChatWith, StringComparison.OrdinalIgnoreCase) Then

                    If Not _unreadCounts.ContainsKey(fromUser) Then _unreadCounts(fromUser) = 0
                    _unreadCounts(fromUser) += 1

                    RenderUsers()
                End If

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
