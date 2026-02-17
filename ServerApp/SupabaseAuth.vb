Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading.Tasks

Public Module SupabaseAuth
    Private ReadOnly http As New HttpClient()

    Public Async Function GetEmailFromAccessTokenAsync(accessToken As String) As Task(Of String)
        If String.IsNullOrWhiteSpace(accessToken) Then
            Return Nothing
        End If

        Dim baseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")
        If String.IsNullOrWhiteSpace(baseUrl) Then
            Throw New InvalidOperationException("SUPABASE_URL environment variable is not set.")
        End If

        Dim url = baseUrl.TrimEnd("/"c) & "/auth/v1/user"
        Dim req = New HttpRequestMessage(HttpMethod.Get, url)
        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken)

        Dim resp = Await http.SendAsync(req)
        If Not resp.IsSuccessStatusCode Then
            Return Nothing
        End If

        Dim json = Await resp.Content.ReadAsStringAsync()
        Try
            Using doc = JsonDocument.Parse(json)
                If doc.RootElement.TryGetProperty("email", Nothing) Then
                    Return doc.RootElement.GetProperty("email").GetString()
                End If

                If doc.RootElement.TryGetProperty("user", Nothing) Then
                    Dim user = doc.RootElement.GetProperty("user")
                    If user.TryGetProperty("email", Nothing) Then
                        Return user.GetProperty("email").GetString()
                    End If
                End If
            End Using
        Catch
        End Try

        Return Nothing
    End Function
End Module
