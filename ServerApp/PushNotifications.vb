Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks

Public Module PushNotifications
    Private ReadOnly http As New HttpClient()

    Public Async Function SendFcmAsync(deviceToken As String, title As String, body As String, data As Object) As Task(Of Boolean)
        Dim serverKey = Environment.GetEnvironmentVariable("FCM_SERVER_KEY")
        If String.IsNullOrWhiteSpace(serverKey) Then
            Return False
        End If

        Dim url = "https://fcm.googleapis.com/fcm/send"
        Dim payload = New With {
            Key .to = deviceToken,
            Key .notification = New With {.title = title, .body = body},
            Key .data = data
        }

        Dim json = JsonSerializer.Serialize(payload)
        Dim req = New HttpRequestMessage(HttpMethod.Post, url) With {
            .Content = New StringContent(json, Encoding.UTF8, "application/json")
        }
        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("key", serverKey)

        Dim resp = Await http.SendAsync(req)
        Return resp.IsSuccessStatusCode
    End Function
End Module
