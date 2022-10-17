using System.Net;
using System.Net.Sockets;

namespace TCPCore
{
	public struct ServerConfig
	{
		public ServerConfig(int serverId, string ipv4, int port)
		{
			this.ServerId = serverId;
			this.Ipv4 = ipv4;
			this.Port = port;
		}
		public readonly int ServerId;
		public readonly string Ipv4;
		public readonly int Port;
	}

	public class TCPServer
	{
		IPEndPoint endpoint;

		public virtual void Start(ServerConfig config)
		{
			String strHostName = string.Empty;
			IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress[] addr = ipEntry.AddressList;

			endpoint = new IPEndPoint(/*addr[1]*/
				address: IPAddress.Parse(config.Ipv4),
				port: config.Port);

			//Accept new session 
			_ = Task.Run(Listening);
		}
		public async void Listening()
		{
			Socket listenSocket = new Socket(
				addressFamily: endpoint.AddressFamily,
				socketType: SocketType.Stream,
				protocolType: ProtocolType.Tcp);

			listenSocket.Bind(localEP: endpoint);
			listenSocket.Listen();

			while (true)
				await listenSocket.AcceptAsync().ContinueWith(AfterAccept);

		}
		public virtual void AfterAccept(Task<Socket> socketTask)
		{
			if (socketTask.IsCompletedSuccessfully)
			{
				Logger.DebugInfo($"Task Id {socketTask.Id} Socket Accepted");
			}
			else
			{
				Logger.DebugInfo($"Accept Failed Task Id {socketTask.Id}");
			}
		}
	}
}
