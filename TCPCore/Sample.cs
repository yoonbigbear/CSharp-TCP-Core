using TCPCore;

class Sample
{
	static void Main(string[] args)
	{
		ServerConfig config = new ServerConfig(
			serverId: 0,
			ipv4: "127.0.0.1",
			port: 10011);

		TCPServer server = new TCPServer();
		server.Start(config);

		Logger.DebugInfo($"Running...");

		while (true)
		{
			server.Run();
			Thread.Sleep(1);
		}
	}
}
