Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Json
Imports System.Collections.Concurrent
Imports System.IO
Imports System.Threading

Public Class ChatServer

    Private ReadOnly _port As Integer
    Private _listener As TcpListener
    Private _cts As CancellationTokenSource

    Private ReadOnly _clients As New ConcurrentDictionary(Of String, StreamWriter)()

    Private ReadOnly _jsonOptions As New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }

    Public Sub New(port As Integer)
        _port = port
    End Sub

    Public Async Function StartAsync() As Task
        _cts = New CancellationTokenSource()
        _listener = New TcpListener(IPAddress.Any, _port)

        Try
            _listener.Start()
            Console.WriteLine("=== CHAT SERVER v2.0 (JSON) ===")
            Console.WriteLine($"Listening on 0.0.0.0:{_port}")
            Console.WriteLine("Press Ctrl+C to stop")
            Console.WriteLine("===============================")

            While Not _cts.Token.IsCancellationRequested
                Dim tcpClient = Await _listener.AcceptTcpClientAsync()
                Dim clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString()
                Console.WriteLine($"✓ Client connected: {clientEndPoint}")

                Task.Run(Function() HandleClient(tcpClient))
            End While

            While Not _cts.Token.IsCancellationRequested
                Dim tcpClient = Await _listener.AcceptTcpClientAsync()
                Dim clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString()
                Console.WriteLine($"✓ Client connected: {clientEndPoint}")

                Task.Run(Function() HandleClient(tcpClient))
            End While

        Catch ex As Exception When ex.HResult = -532462766
            Console.WriteLine("✓ Shutdown complete (Socket 995 expected)")
        Catch ex As ObjectDisposedException
            Console.WriteLine("Listener disposed (normal)")
        Catch ex As Exception
            Console.WriteLine($"Unexpected error: {ex.Message}")
        Finally
            _listener?.Stop()
            Console.WriteLine("Server socket closed")
        End Try
    End Function

    Private Class NetMessage
        Public Property Type As String
        Public Property Data As Dictionary(Of String, JsonElement) = New Dictionary(Of String, JsonElement)()

        Public Property ErrorMsg As String
    End Class

    Private Sub BroadcastPresence()
        Dim users = _clients.Keys.OrderBy(Function(x) x).ToArray()

        Dim json = JsonSerializer.Serialize(New With {
        .type = "presence",
        .data = New With {.users = users}
    }, _jsonOptions)

        For Each kv In _clients
            Try
                kv.Value.WriteLine(json)   ' sync write (safe for VB: not in Catch/Finally Await issues)
            Catch
            End Try
        Next
    End Sub


    Private Async Function HandleClient(client As TcpClient) As Task
        Dim endPoint = client.Client.RemoteEndPoint.ToString()
        Dim email As String = Nothing

        Try
            Using stream = client.GetStream()
                Using reader = New StreamReader(stream, Encoding.UTF8)
                    Using writer = New StreamWriter(stream, Encoding.UTF8) With {.AutoFlush = True}

                        ' Welcome
                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {
                            .type = "welcome",
                            .data = New With {.message = "Send identify: {type:'identify', data:{email:'you@x.com'}}"}
                        }, _jsonOptions))

                        While Not _cts.Token.IsCancellationRequested
                            Dim line = Await reader.ReadLineAsync()
                            If line Is Nothing Then Exit While

                            Dim msg As NetMessage = Nothing
                            Dim replyJson As String = Nothing

                            Try
                                msg = JsonSerializer.Deserialize(Of NetMessage)(line, _jsonOptions)
                            Catch
                                replyJson = JsonSerializer.Serialize(New With {
                                    .type = "error",
                                    .errorMsg = "Invalid JSON"
                                }, _jsonOptions)
                            End Try

                            If replyJson IsNot Nothing Then
                                Await writer.WriteLineAsync(replyJson)
                                Continue While
                            End If


                            If msg Is Nothing OrElse String.IsNullOrWhiteSpace(msg.Type) Then Continue While

                            Select Case msg.Type.ToLowerInvariant()

                                Case "signup"
                                    ' Create a new user: expects data.email and data.password, optional data.displayName
                                    If msg.Data Is Nothing OrElse Not msg.Data.ContainsKey("email") OrElse Not msg.Data.ContainsKey("password") Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Missing data.email or data.password"}, _jsonOptions))
                                        Continue While
                                    End If

                                    Dim signupEmail = msg.Data("email").GetString()
                                    Dim signupPassword = msg.Data("password").GetString()
                                    Dim signupDisplay As String = Nothing
                                    If msg.Data.ContainsKey("displayName") Then
                                        signupDisplay = msg.Data("displayName").GetString()
                                    End If

                                    If signupEmail Is Nothing Then signupEmail = ""
                                    signupEmail = signupEmail.Trim().ToLowerInvariant()
                                    If String.IsNullOrWhiteSpace(signupPassword) Then signupPassword = ""

                                    If signupEmail = "" OrElse signupPassword = "" Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Empty email or password"}, _jsonOptions))
                                        Continue While
                                    End If

                                    ' Check existing
                                    Dim exists = Await SupabaseRest.UserExistsAsync(signupEmail)
                                    If exists Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "User already exists"}, _jsonOptions))
                                        Continue While
                                    End If

                                    ' Create auth user and insert into users table
                                    Dim createdAuth = False
                                    Try
                                        createdAuth = Await SupabaseRest.CreateAuthUserAsync(signupEmail, signupPassword)
                                    Catch ex As Exception
                                        Console.WriteLine($"CreateAuthUserAsync failed: {ex.Message}")
                                    End Try

                                    If Not createdAuth Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Failed to create auth user"}, _jsonOptions))
                                        Continue While
                                    End If

                                    Try
                                        Await SupabaseRest.InsertUserAsync(signupEmail, If(signupDisplay, ""))
                                    Catch ex As Exception
                                        Console.WriteLine($"InsertUserAsync failed: {ex.Message}")
                                    End Try

                                    Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "signup_ok", .data = New With {.email = signupEmail}}, _jsonOptions))

                                Case "identify"
                                    ' Support identify via access_token (Supabase) or plain email for backward compatibility
                                    Dim providedToken As String = Nothing
                                    If msg.Data IsNot Nothing AndAlso msg.Data.ContainsKey("access_token") Then
                                        providedToken = msg.Data("access_token").GetString()
                                    End If

                                    If Not String.IsNullOrWhiteSpace(providedToken) Then
                                        Dim verifiedEmail = Await SupabaseAuth.GetEmailFromAccessTokenAsync(providedToken)
                                        If String.IsNullOrWhiteSpace(verifiedEmail) Then
                                            Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Invalid token"}, _jsonOptions))
                                            Continue While
                                        End If
                                        email = verifiedEmail.Trim().ToLowerInvariant()
                                    Else
                                        If msg.Data Is Nothing OrElse Not msg.Data.ContainsKey("email") Then
                                            Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {
                                                .type = "error",
                                                .errorMsg = "Missing data.email"
                                            }, _jsonOptions))
                                            Continue While
                                        End If

                                        email = msg.Data("email").GetString()
                                        If email Is Nothing Then email = ""
                                        email = email.Trim().ToLowerInvariant()
                                    End If

                                    If email = "" Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Empty email"}, _jsonOptions))
                                        Continue While
                                    End If

                                    ' Ensure the identified email exists in our users table
                                    Dim existsUser = False
                                    Try
                                        existsUser = Await SupabaseRest.UserExistsAsync(email)
                                    Catch ex As Exception
                                        Console.WriteLine($"UserExistsAsync failed: {ex.Message}")
                                    End Try

                                    If Not existsUser Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "User not registered. Please sign up first."}, _jsonOptions))
                                        Continue While
                                    End If

                                    If Not _clients.TryAdd(email, writer) Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "User already connected"}, _jsonOptions))
                                        Exit While
                                    End If

                                    Console.WriteLine($"✓ IDENTIFIED {email} @ {endPoint}")

                                    Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {
                                        .type = "identified",
                                        .data = New With {.email = email}
                                    }, _jsonOptions))

                                    BroadcastPresence()


                                Case "msg"
                                    If email Is Nothing Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Identify first"}, _jsonOptions))
                                        Continue While
                                    End If

                                    If msg.Data Is Nothing OrElse Not msg.Data.ContainsKey("to") OrElse Not msg.Data.ContainsKey("text") Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Missing data.to or data.text"}, _jsonOptions))
                                        Continue While
                                    End If

                                    Dim toEmail = msg.Data("to").GetString()
                                    If toEmail Is Nothing Then toEmail = ""
                                    toEmail = toEmail.Trim().ToLowerInvariant()

                                    Dim text = msg.Data("text").GetString()
                                    If text Is Nothing Then text = ""

                                    Dim targetWriter As StreamWriter = Nothing
                                    If _clients.TryGetValue(toEmail, targetWriter) Then
                                        Await targetWriter.WriteLineAsync(JsonSerializer.Serialize(New With {
                                            .type = "deliver",
                                            .data = New With {.from = email, .text = text}
                                        }, _jsonOptions))

                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {
                                            .type = "sent",
                                            .data = New With {.to = toEmail}
                                        }, _jsonOptions))
                                    Else
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {
                                            .type = "sent",
                                            .errorMsg = "User offline/not found"
                                        }, _jsonOptions))
                                    End If

                                    ' Persist message to Supabase (fire-and-forget)
                                    Try
                                        _ = SupabaseRest.InsertMessageAsync(email, toEmail, text)
                                    Catch ex As Exception
                                        Console.WriteLine($"Supabase insert failed: {ex.Message}")
                                    End Try

                                    ' If recipient offline, attempt push notifications
                                    If Not _clients.ContainsKey(toEmail) Then
                                        Task.Run(Async Function()
                                                   Try
                                                       Dim tokens = Await SupabaseRest.GetDeviceTokensForUserAsync(toEmail)
                                                       For Each t In tokens
                                                           _ = PushNotifications.SendFcmAsync(t, "New message", $"{email}: {text}", New With {.from = email, .text = text})
                                                       Next
                                                   Catch ex As Exception
                                                       Console.WriteLine($"Push notify failed: {ex.Message}")
                                                   End Try
                                               End Function)
                                    End If

                                Case Else
                                    Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Unknown type"}, _jsonOptions))
                            End Select
                        End While

                    End Using
                End Using
            End Using

        Catch ex As Exception
            Console.WriteLine($"[{endPoint}] ERROR: {ex.Message}")
        Finally
            If email IsNot Nothing Then
                Dim removed As StreamWriter = Nothing
                _clients.TryRemove(email, removed)
                BroadcastPresence()
            End If

            client.Close()
            Console.WriteLine($"✗ Client disconnected: {endPoint}")
        End Try
    End Function

    Public Sub StopServer()
        Console.WriteLine("Server stopping...")
        _cts?.Cancel()
        _listener?.Stop()
    End Sub

End Class
