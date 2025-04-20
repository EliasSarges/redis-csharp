using System.Net;
using System.Net.Sockets;

namespace ToyRedis.Server;

public class RedisServer(int port = 6380) : IDisposable
{
    private int Port { get; } = port;
    private TcpListener? _listener;

    public TaskCompletionSource<bool> Ready { get; } = new();

    public async Task StartAsync()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();

            // to indicate that the server is ready
            Ready.TrySetResult(true);

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                var stream = client.GetStream();

                await stream.WriteAsync("+PONG\r\n"u8.ToArray());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Ready.TrySetException(e);
        }
    }

    public void Dispose()
    {
        _listener?.Stop();
        _listener = null;
    }
}