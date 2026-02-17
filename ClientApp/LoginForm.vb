Imports System.Text.Json
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks

Public Class LoginForm

    Private ReadOnly _client As ChatClient
    Private _pendingSignupEmail As String
    Private _pendingSignupPassword As String

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
        Dim email = txtEmail.Text.Trim().ToLowerInvariant()
        Dim password = txtPassword.Text
        If String.IsNullOrWhiteSpace(email) OrElse String.IsNullOrWhiteSpace(password) Then
            statusLabel.Text = "Status: Email and password required"
            Return
        End If

        statusLabel.Text = "Status: Logging in..."
        Await SignInAndIdentifyAsync(email, password)
    End Sub

    Private Async Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        Dim email = txtEmail.Text.Trim().ToLowerInvariant()
        Dim password = txtPassword.Text
        Dim display = txtDisplayName.Text
        If String.IsNullOrWhiteSpace(email) OrElse String.IsNullOrWhiteSpace(password) Then
            statusLabel.Text = "Status: Email and password required"
            Return
        End If

        ' Send signup request to server (server will create auth user and add to users table)
        _pendingSignupEmail = email
        _pendingSignupPassword = password

        statusLabel.Text = "Status: Registering..."

        Dim msg As New ChatClient.NetMessage With {.Type = "signup"}
        msg.Data("email") = JsonDocument.Parse("""" & email.Replace("""", "") & """").RootElement
        msg.Data("password") = JsonDocument.Parse("""" & password.Replace("""", "") & """").RootElement
        If Not String.IsNullOrWhiteSpace(display) Then
            msg.Data("displayName") = JsonDocument.Parse("""" & display.Replace("""", "") & """").RootElement
        End If

        Await _client.SendAsync(msg)
    End Sub

    Private Async Function SignInAndIdentifyAsync(email As String, password As String) As Task
        ' Sign in to Supabase to get access_token
        Dim token = Await SupabaseSignInAsync(email, password)
        If String.IsNullOrWhiteSpace(token) Then
            statusLabel.Text = "Status: Login failed"
            Return
        End If

        ' Send identify with access_token to chat server
        Dim msg As New ChatClient.NetMessage With {.Type = "identify"}
        msg.Data("access_token") = JsonDocument.Parse("""" & token.Replace("""", "") & """").RootElement
        Await _client.SendAsync(msg)
    End Function

    Private Async Function SupabaseSignInAsync(email As String, password As String) As Task(Of String)
        Try
            Dim baseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")
            Dim anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")
            If String.IsNullOrWhiteSpace(baseUrl) Then
                statusLabel.Text = "Status: SUPABASE_URL not configured"
                Return Nothing
            End If

            Dim url = baseUrl.TrimEnd("/"c) & "/auth/v1/token?grant_type=password"
            Using http As New HttpClient()
                Dim payload = New With {Key .email = email, Key .password = password}
                Dim json = JsonSerializer.Serialize(payload)
                Using req As New HttpRequestMessage(HttpMethod.Post, url)
                    req.Content = New StringContent(json, Encoding.UTF8, "application/json")
                    If Not String.IsNullOrWhiteSpace(anonKey) Then
                        req.Headers.Add("apikey", anonKey)
                        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", anonKey)
                    End If

                    Dim resp = Await http.SendAsync(req)
                    Dim respJson = Await resp.Content.ReadAsStringAsync()
                    If Not resp.IsSuccessStatusCode Then
                        ' Try to extract error
                        Try
                            Using doc = JsonDocument.Parse(respJson)
                                If doc.RootElement.TryGetProperty("error_description", Nothing) Then
                                    statusLabel.Text = "Status: " & doc.RootElement.GetProperty("error_description").GetString()
                                ElseIf doc.RootElement.TryGetProperty("error", Nothing) Then
                                    statusLabel.Text = "Status: " & doc.RootElement.GetProperty("error").GetString()
                                End If
                            End Using
                        Catch
                        End Try
                        Return Nothing
                    End If

                    Using doc = JsonDocument.Parse(respJson)
                        If doc.RootElement.TryGetProperty("access_token", Nothing) Then
                            Return doc.RootElement.GetProperty("access_token").GetString()
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            statusLabel.Text = "Status: Sign-in error"
        End Try
        Return Nothing
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
        ElseIf t = "signup_ok" Then
            statusLabel.Text = "Status: Signup succeeded, signing in..."
            If Not String.IsNullOrWhiteSpace(_pendingSignupEmail) AndAlso Not String.IsNullOrWhiteSpace(_pendingSignupPassword) Then
                Dim e = _pendingSignupEmail
                Dim p = _pendingSignupPassword
                _pendingSignupEmail = Nothing
                _pendingSignupPassword = Nothing
                Task.Run(Async Function()
                             Await SignInAndIdentifyAsync(e, p)
                         End Function)
            End If
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
