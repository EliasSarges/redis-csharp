﻿using System.Net.Sockets;
using System.Text;
using ToyRedis.Server;

namespace ToyRedis.Tests.Server;

public class RedisServerTest
{
    [Fact]
    public async Task ShouldRespondsToPingWithPong()
    {
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var server = new RedisServer();
        var serverTask = server.StartAsync();

        // Wait for the server to be ready
        await server.Ready.Task;

        var client = new TcpClient("localhost", server.Port);

        var stream = client.GetStream();
        var buffer = "*1\r\n$4\r\nPING\r\n"u8.ToArray();

        await stream.WriteAsync(buffer, cancellationToken.Token);

        var responseBuffer = new byte[1024];
        var bytesRead = await stream.ReadAsync(responseBuffer, cancellationToken.Token);
        var response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

        Assert.Equal("+PONG\r\n", response);

        server.Stop();

        // propagate the exception if any
        await serverTask;
    }

    [Fact]
    public async Task ShouldPersistClientConnection()
    {
        var server = new RedisServer();
        var serverTask = server.StartAsync();

        await server.Ready.Task;

        var client = new TcpClient("localhost", server.Port);

        await using var stream = client.GetStream();
        var buffer = "*1\r\n$4\r\nPING\r\n"u8.ToArray();

        for (var i = 0; i < 2; i++)
        {
            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await stream.WriteAsync(buffer, cancellationToken.Token);

            var responseBuffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(responseBuffer, cancellationToken.Token);
            var response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            Assert.Equal("+PONG\r\n", response);
        }

        server.Stop();

        await serverTask;
    }

    [Fact]
    public async Task ShouldHandleMultipleClients()
    {
        var server = new RedisServer();
        var serverTask = server.StartAsync();

        await server.Ready.Task;

        for (var i = 0; i < 2; i++)
        {
            var client = new TcpClient("localhost", server.Port);

            await using var stream = client.GetStream();
            var buffer = "*1\r\n$4\r\nPING\r\n"u8.ToArray();

            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await stream.WriteAsync(buffer, cancellationToken.Token);

            var responseBuffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(responseBuffer, cancellationToken.Token);
            var response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            Assert.Equal("+PONG\r\n", response);
        }

        server.Stop();
        await serverTask;
    }
}