using System.Net;
using System.Net.Sockets;

namespace TCPCore
{
	
	public class TCPServer
	{
		public static int TotalSessions = 0;

		short _port { get; set; }
		// _ip

		public virtual void Start(short port)
		{
			String strHostName = string.Empty;
			IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress[] addr = ipEntry.AddressList;

			this._port = port;

			_ = Task.Run(Listening);
		}
		public async void Listening()
		{
			//Recv both ipv6 & ipv4
			Socket listenSocket = new Socket(
				addressFamily: AddressFamily.InterNetworkV6,
				socketType: SocketType.Stream,
				protocolType: ProtocolType.Tcp);

			listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			listenSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

			listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, _port));
			listenSocket.Listen(100);

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
