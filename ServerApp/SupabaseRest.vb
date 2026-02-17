Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks

Public Module SupabaseRest
    Private ReadOnly http As New HttpClient()

    Private Function GetServiceKey() As String
        Return Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
    End Function

    Private Function GetBaseUrl() As String
        Dim baseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")
        If String.IsNullOrWhiteSpace(baseUrl) Then
            Throw New InvalidOperationException("SUPABASE_URL is not set.")
        End If
        Return baseUrl.TrimEnd("/"c) & "/rest/v1"
    End Function

    Public Async Function InsertMessageAsync(fromEmail As String, toEmail As String, text As String) As Task(Of Boolean)
        Dim url = GetBaseUrl() & "/messages"
        Dim payload = New With {
            Key .from_email = fromEmail,
            Key .to_email = toEmail,
            Key .text = text
        }
        Dim json = JsonSerializer.Serialize(payload)
        Dim req = New HttpRequestMessage(HttpMethod.Post, url) With {
            .Content = New StringContent(json, Encoding.UTF8, "application/json")
        }
        Dim key = GetServiceKey()
        If String.IsNullOrWhiteSpace(key) Then
            Throw New InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY is not set.")
        End If
        req.Headers.Add("apikey", key)
        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key)
        req.Headers.Add("Prefer", "return=representation")

        Dim resp = Await http.SendAsync(req)
        Return resp.IsSuccessStatusCode
    End Function

    Public Async Function InsertDeviceAsync(userEmail As String, deviceToken As String, platform As String) As Task(Of Boolean)
        Dim url = GetBaseUrl() & "/devices"
        Dim payload = New With {
            Key .user_email = userEmail,
            Key .device_token = deviceToken,
            Key .platform = platform
        }
        Dim json = JsonSerializer.Serialize(payload)
        Dim req = New HttpRequestMessage(HttpMethod.Post, url) With {
            .Content = New StringContent(json, Encoding.UTF8, "application/json")
        }
        Dim key = GetServiceKey()
        If String.IsNullOrWhiteSpace(key) Then
            Throw New InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY is not set.")
        End If
        req.Headers.Add("apikey", key)
        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key)
        req.Headers.Add("Prefer", "return=representation")

        Dim resp = Await http.SendAsync(req)
        Return resp.IsSuccessStatusCode
    End Function

    Public Async Function GetDeviceTokensForUserAsync(userEmail As String) As Task(Of List(Of String))
        Dim url = GetBaseUrl() & $"/devices?user_email=eq.{Uri.EscapeDataString(userEmail)}"
        Dim req = New HttpRequestMessage(HttpMethod.Get, url)
        Dim key = GetServiceKey()
        If String.IsNullOrWhiteSpace(key) Then
            Throw New InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY is not set.")
        End If
        req.Headers.Add("apikey", key)
        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key)

        Dim resp = Await http.SendAsync(req)
        If Not resp.IsSuccessStatusCode Then
            Return New List(Of String)()
        End If

        Dim json = Await resp.Content.ReadAsStringAsync()
        Dim tokens As New List(Of String)()
        Try
            Using doc = JsonDocument.Parse(json)
                If doc.RootElement.ValueKind = JsonValueKind.Array Then
                    For Each el In doc.RootElement.EnumerateArray()
                        If el.TryGetProperty("device_token", Nothing) Then
                            tokens.Add(el.GetProperty("device_token").GetString())
                        End If
                    Next
                End If
            End Using
        Catch
        End Try
        Return tokens
    End Function

    ' Check if a user exists in the "users" table
    Public Async Function UserExistsAsync(userEmail As String) As Task(Of Boolean)
        Dim url = GetBaseUrl() & $"/users?email=eq.{Uri.EscapeDataString(userEmail)}&select=email"
        Dim req = New HttpRequestMessage(HttpMethod.Get, url)
        Dim key = GetServiceKey()
        If String.IsNullOrWhiteSpace(key) Then
            Throw New InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY is not set.")
        End If
        req.Headers.Add("apikey", key)
        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key)

        Dim resp = Await http.SendAsync(req)
        If Not resp.IsSuccessStatusCode Then
            Return False
        End If

        Dim json = Await resp.Content.ReadAsStringAsync()
        Try
            Using doc = JsonDocument.Parse(json)
                If doc.RootElement.ValueKind = JsonValueKind.Array Then
                    Return doc.RootElement.GetArrayLength() > 0
                End If
            End Using
        Catch
        End Try
        Return False
    End Function

    ' Insert a new row into the "users" table
    Public Async Function InsertUserAsync(userEmail As String, displayName As String) As Task(Of Boolean)
        Dim url = GetBaseUrl() & "/users"
        Dim payload = New With {
            Key .email = userEmail,
            Key .display_name = displayName
        }
        Dim json = JsonSerializer.Serialize(payload)
        Dim req = New HttpRequestMessage(HttpMethod.Post, url) With {
            .Content = New StringContent(json, Encoding.UTF8, "application/json")
        }
        Dim key = GetServiceKey()
        If String.IsNullOrWhiteSpace(key) Then
            Throw New InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY is not set.")
        End If
        req.Headers.Add("apikey", key)
        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key)
        req.Headers.Add("Prefer", "return=representation")

        Dim resp = Await http.SendAsync(req)
        Return resp.IsSuccessStatusCode
    End Function

    ' Create an auth user via Supabase Admin API (service_role key required)
    Public Async Function CreateAuthUserAsync(userEmail As String, password As String) As Task(Of Boolean)
        Dim baseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")
        If String.IsNullOrWhiteSpace(baseUrl) Then
            Throw New InvalidOperationException("SUPABASE_URL is not set.")
        End If

        Dim url = baseUrl.TrimEnd("/"c) & "/auth/v1/admin/users"
        Dim payload = New With {
            Key .email = userEmail,
            Key .password = password,
            Key .email_confirm = True
        }
        Dim json = JsonSerializer.Serialize(payload)
        Dim req = New HttpRequestMessage(HttpMethod.Post, url) With {
            .Content = New StringContent(json, Encoding.UTF8, "application/json")
        }
        Dim key = GetServiceKey()
        If String.IsNullOrWhiteSpace(key) Then
            Throw New InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY is not set.")
        End If
        req.Headers.Add("apikey", key)
        req.Headers.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key)

        Dim resp = Await http.SendAsync(req)
        Return resp.IsSuccessStatusCode
    End Function

End Module
