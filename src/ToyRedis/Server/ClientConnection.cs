using System.Net.Sockets;

namespace ToyRedis.Server;

public class ClientConnection(TcpClient client)
{
    public async Task HandleAsync()
    {
        var stream = client.GetStream();

        while (true)
        {
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer);

            // client disconnected
            if (bytesRead == 0) break;

            await stream.WriteAsync("+PONG\r\n"u8.ToArray());
        }
    }
}