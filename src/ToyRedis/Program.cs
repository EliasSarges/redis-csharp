using ToyRedis.Server;

var server = new RedisServer();

await server.StartAsync();