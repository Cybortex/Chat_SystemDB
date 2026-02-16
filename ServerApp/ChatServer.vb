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

                                Case "identify"
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

                                    If email = "" Then
                                        Await writer.WriteLineAsync(JsonSerializer.Serialize(New With {.type = "error", .errorMsg = "Empty email"}, _jsonOptions))
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
