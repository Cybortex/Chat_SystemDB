Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Json
Imports System.IO

Public Class ChatClient

    Private _tcpClient As TcpClient
    Private _reader As StreamReader
    Private _writer As StreamWriter

    Private ReadOnly _jsonOptions As New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }

    Public Event MessageReceived(sender As Object, msg As NetMessage)
    Public Event Connected(sender As Object, e As EventArgs)
    Public Event Disconnected(sender As Object, e As EventArgs)
    Public Event ErrorOccurred(sender As Object, errorMsg As String)

    Public Property ServerIP As String = "192.168.13.186"
    Public Property ServerPort As Integer = 5000

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _tcpClient IsNot Nothing AndAlso _tcpClient.Connected
        End Get
    End Property

    Public Async Function ConnectAsync() As Task
        Try
            _tcpClient = New TcpClient()
            Await _tcpClient.ConnectAsync(ServerIP, ServerPort)

            Dim ns = _tcpClient.GetStream()
            _reader = New StreamReader(ns, Encoding.UTF8)
            _writer = New StreamWriter(ns, Encoding.UTF8) With {.AutoFlush = True}

            Task.Run(AddressOf ReceiveLoop)

            RaiseEvent Connected(Me, EventArgs.Empty)

        Catch ex As Exception
            RaiseEvent ErrorOccurred(Me, ex.Message)
        End Try
    End Function

    Public Async Function SendAsync(msg As NetMessage) As Task
        If _writer Is Nothing Then Return
        Dim json = JsonSerializer.Serialize(msg, _jsonOptions)
        Await _writer.WriteLineAsync(json)
    End Function

    Private Async Sub ReceiveLoop()
        Try
            While _tcpClient IsNot Nothing AndAlso _tcpClient.Connected
                Dim line = Await _reader.ReadLineAsync()
                If line Is Nothing Then Exit While

                Dim msg = JsonSerializer.Deserialize(Of NetMessage)(line, _jsonOptions)
                If msg IsNot Nothing Then
                    RaiseEvent MessageReceived(Me, msg)
                End If
            End While

        Catch ex As Exception
            RaiseEvent ErrorOccurred(Me, ex.Message)
        Finally
            RaiseEvent Disconnected(Me, EventArgs.Empty)
        End Try
    End Sub

    Public Sub Disconnect()
        Try
            _writer?.Close()
            _reader?.Close()
            _tcpClient?.Close()
        Catch
        End Try
    End Sub

    Public Class NetMessage
        Public Property Type As String
        Public Property Data As Dictionary(Of String, JsonElement) =
            New Dictionary(Of String, JsonElement)()
        Public Property ErrorMsg As String
    End Class

End Class
