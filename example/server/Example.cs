using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using TCPCore;

class Example
{
	static void Main(string[] args)
	{
		ServerConfig config = new ServerConfig(
			serverId: 0,
			ipv4: "127.0.0.1",
			port: 10011);

		Server server = new Server();
		server.Start(config);

		Logger.DebugInfo($"Running...");

		while (true)
		{
			server.Run();
			Thread.Sleep(1);
		}
	}
}

// inherited TCPServer
public class Server : TCPServer
{

	//packet queue
	public PacketQueue packetQueue = new PacketQueue();

	//server invoke packet functions
	PacketHandler _packetHandler = new PacketHandler();

	UserSession _user;

	Time _time = new Time();

	const float _frameRate = 0.1f;

	public override void Start(ServerConfig config)
	{
		//time value initialize
		_time.Start();

		base.Start(config);

	}
	public override void AfterAccept(Task<Socket> task)
	{
		if (task.IsCompletedSuccessfully)
		{
			Logger.DebugInfo($"Task Id {task.Id} Socket Accepted");

			//Single user Session
			_user = new UserSession(task.Result);
			_user.SessionId = task.Result.Handle.ToInt32();
			_user.Server = this;
			_user.Init();

		}
		else
		{
			Logger.DebugInfo($"Accept Failed Task Id {task.Id}");
		}
	}


	//Running logic
	public void Run()
	{
		_time.UpdateDeltaTime();

		if (Time.Delta > _frameRate)
		{
			packetQueue.TransferTo(out var packets);

			//need packet handler
			int i = 0;
			int count = packets.Count;
			for (i = 0; i < count; ++i)
			{
				_packetHandler.Invoke(packets[i].Item1, packets[i].Item2);
			}

			Time.Delta = 0;
		}
	}

	//bind packet callback functions
	void PacketBinding() => _packetHandler.Bind(1, TempCallback);

	//dummy packet callback
	void TempCallback(TCPSession ss, ArraySegment<byte> datas) => Logger.DebugInfo($"Invoke temp");
}

// inherited TCPSession
public class UserSession : TCPSession
{
	List<Tuple<TCPSession, Packet>> packets = new();


	public UserSession(Socket socket) : base(socket) { }
	public Server Server { get; set; }
	public int SessionId { get; set; }

	public virtual async void Receive()
	{
		try
		{
			while (true)
			{
				//recv data must be less than max buffer size
				ReadResult result = await reader.ReadAsync();
				ReadOnlySequence<byte> buffer = result.Buffer;

				//no more
				if (result.IsCompleted)
					break;

				//client disconnected
				if (buffer.Length == 0)
					return;

				//read data
				while (true)
				{
					if (buffer.Start.Equals(buffer.End))
						break;

					var offset = Deserealize(buffer.Slice(0), out var packet);
					SequencePosition end = buffer.GetPosition(offset);

					buffer = buffer.Slice(end);
					packets.Add(new Tuple<TCPSession, Packet>(this, packet));
				}

				//enqueue packets
				Server.packetQueue.AddRange(packets);
				packets.Clear();

				//move cursor
				reader.AdvanceTo(buffer.Start, buffer.End);
			}
		}
		catch (Exception e)
		{
			Logger.DebugError($"Receive Error {e}");
		}
		finally
		{
			reader.Complete();
			Disconnect();
		}
	}
}
