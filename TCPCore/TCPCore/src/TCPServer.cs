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
		Time time = new Time();

		//packet queue
		public PacketQueue packetQueue = new PacketQueue();

		const float frameRate = 0.1f;

		TCPSession user = null;

		public virtual void Start(ServerConfig config)
		{
			String strHostName = string.Empty;
			IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress[] addr = ipEntry.AddressList;

			endpoint = new IPEndPoint(/*addr[1]*/
				address: IPAddress.Parse(config.Ipv4),
				port: config.Port);

			//time value initialize
			time.Start();

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

				//Single user Session
				user = new TCPSession(socketTask.Result);
				user.SessionId = socketTask.Result.Handle.ToInt32();
				user.Server = this;
				user.Init();

			}
			else
			{
				Logger.DebugInfo($"Accept Failed Task Id {socketTask.Id}");
			}
		}

		//Single server logic
		public void Run()
		{
			time.UpdateDeltaTime();

			if (Time.Delta > frameRate)
			{
				packetQueue.TransferTo(out var packets);

				//need packet handler
				int i = 0;
				int count = packets.Count;
				for (i = 0; i < count; ++i)
				{
					var packet = packets[i];
					Logger.DebugInfo($"Id: {packet.id}, size: {packet.dataSize}");
				}

				Time.Delta = 0;
			}
		}
	}
}
