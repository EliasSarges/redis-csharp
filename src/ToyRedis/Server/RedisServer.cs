using System.Net;
using System.Net.Sockets;

namespace ToyRedis.Server;

public class RedisServer(int port = 0)
{
    public int Port { get; private set; } = port;
    private TcpListener? _listener;

    public TaskCompletionSource<bool> Ready { get; } = new();

    public async Task StartAsync()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();

            var localEndpoint = (IPEndPoint)_listener.LocalEndpoint;
            Port = localEndpoint.Port;

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
        finally
        {
            Stop();
        }
    }

    public void Stop()
    {
        _listener?.Stop();
        _listener = null;
    }
}