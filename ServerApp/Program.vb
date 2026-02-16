Module Program
    Private server As ChatServer

    Sub Main()
        server = New ChatServer(5000)
        AddHandler Console.CancelKeyPress, AddressOf OnCtrlC
        Console.WriteLine("Starting Chat Server...")
        server.StartAsync().GetAwaiter().GetResult()
        Console.WriteLine("Server stopped.")
        Console.ReadKey()
    End Sub

    Private Sub OnCtrlC(sender As Object, e As ConsoleCancelEventArgs)
        Console.WriteLine(vbCrLf + "Ctrl+C - shutting down...")
        e.Cancel = True
        server?.StopServer()
    End Sub
End Module


