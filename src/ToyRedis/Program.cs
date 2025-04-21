using ToyRedis.Server;

var server = new RedisServer(6380);

await server.StartAsync();