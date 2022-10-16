using System.Net;
using System.Net.Sockets;

public class Connector
{
	IPEndPoint? endpoint;
	public void Start()
	{
		endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
	}

	//서버에 소켓 연결 시도
	public async Task<Socket?> ConnectAsync()
	{
		try
		{
			Socket socket = new Socket(
				addressFamily: AddressFamily.InterNetwork,
				socketType: SocketType.Stream,
				protocolType: ProtocolType.Tcp);

			await socket.ConnectAsync(remoteEP: endpoint)
				.ContinueWith(continuationAction: AfterConnect);
			return socket;
		}
		catch (Exception e)
		{
			Console.WriteLine($"Connect Failed {e}");
			return null;
		}
	}

	void AfterConnect(Task task)
	{
		if (task.IsCompletedSuccessfully)
		{
			Console.WriteLine("Socket Connected");
		}
		else
		{
			Console.WriteLine("Connection Failed");
		}
	}

}

