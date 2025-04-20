using System.Net.Sockets;
using System.Text;
using ToyRedis.Server;

namespace ToyRedis.Tests.Server;

public class RedisServerTest
{
    [Fact]
    public async Task ShouldRespondsToPingWithPong()
    {
        const int port = 6381;
        using var server = new RedisServer(port);
        var serverTask = server.StartAsync();
    
        // Wait for the server to be ready
        await server.Ready.Task;

        var tcpClient = new TcpClient("localhost", port);
        
        var stream = tcpClient.GetStream();
        var buffer = "*1\r\n$4\r\nPING\r\n"u8.ToArray();
        
        await stream.WriteAsync(buffer);

        var responseBuffer = new byte[1024];
        var bytesRead = await stream.ReadAsync(responseBuffer);
        
        var response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

        Assert.Equal("+PONG\r\n", response);
        
        // propagate the exception if any
        await serverTask;
    }
}