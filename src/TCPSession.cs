using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace TCPCore
{
	//https://github.com/davidfowl/TcpEcho/blob/master/src/Server/Program.cs
	public class TCPSession
	{
		public PacketQueue RecvPacketQueue { get; set; }
		public Socket socket;
		protected PipeReader reader;


		private List<ArraySegment<byte>> _sendQueue = new();
		private List<Tuple<TCPSession, Packet>> _recvQueue = new();

		public TCPSession(Socket socket) => this.socket = socket;

		object _lock = new object();
		object _leavelock = new object();

		const float _hearbeatBound = 10000;
		float _hearbeat = 0;

		public virtual void Disconnect()
		{
			lock (_leavelock)
			{
				//Logger.DebugSuccess($"session disconnect. Total sessions {Interlocked.Decrement(ref TCPServer.TotalSessions)}");
				_sendQueue.Clear();
				if (socket != null && socket.Connected)
				{
					socket.Shutdown(SocketShutdown.Both);
				}
				socket.Close();
			}
		}
		public virtual void Start()
		{
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);

			var read = new NetworkStream(socket);
			
			//change pipe buffer size
			reader = PipeReader.Create(read,
						new StreamPipeReaderOptions(bufferSize: ushort.MaxValue));

			Task.Run(() => { Receive(); });
		}
		public virtual void Update()
		{
			//send packets at one
			ReleaseSendQueue();
		}

		void Heartbeat() {
			if (_hearbeat > _hearbeatBound)
			{
				Logger.DebugError("No signal from session.. disconnect");
				Disconnect();
			}
		}

		public async void Receive()
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

						var offset = Deserialize(buffer.Slice(0), out var packet);
						SequencePosition end = buffer.GetPosition(offset);

						buffer = buffer.Slice(end);

						if (packet.id == -1) //heartbeatid
						{
							_hearbeat = 0;
							continue;
						}

						_recvQueue.Add(new Tuple<TCPSession, Packet>(this, packet));
					}

					//enqueue packets
					RecvPacketQueue.AddRange(_recvQueue);

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
		public void Send(Packet pkt)
		{
			try
			{
				socket.Send(Serialize(pkt));
			}
			catch(Exception e)
			{
				Logger.DebugError($"Socket Exception {e.Message}");
				Disconnect();
			}
		}
		public void Send(short id, short dataSize, ArraySegment<byte> bytes)
		{
			try
			{
				socket.Send(Serialize(id, dataSize, bytes));
			}
			catch (Exception e)
			{
				Logger.DebugError($"Socket Exception {e.Message}");
				Disconnect();
			}
		}
		public void Send(ArraySegment<byte> bytes)
		{
			lock (_lock)
			{
				_sendQueue.Add(bytes);
			}
		}
		public void Send(List<Packet> pkts)
		{
			List<ArraySegment<byte>> lists = new();
			foreach (var e in pkts)
			{
				lists.Add(Serialize(e));
			}
			lock (_lock)
			{
				_sendQueue.AddRange(lists);
			}
		}
		public void SendImmediate(Packet pkt)
		{
			try
			{
				socket.Send(Serialize(pkt));
			}
			catch (Exception e)
			{
				Logger.DebugError($"Socket Exception {e.Message}");
				Disconnect();
			}
		}
		public void ReleaseSendQueue()
		{
			lock (_leavelock)
			{
				if (_sendQueue.Count <= 0)
					return;
			}

			lock (_lock)
			{
				List<ArraySegment<byte>> packets = new();
				packets.AddRange(_sendQueue);
				_sendQueue.Clear();
				try
				{
					socket.Send(packets, SocketFlags.None);
				}
				catch (Exception e)
				{
					Logger.DebugError($"Socket Exception {e.Message}");
					Disconnect();
				}
			}
		}


		public ArraySegment<byte> Serialize(Packet pkt)
		{
			//header + datasize
			ArraySegment<byte> array = new ArraySegment<byte>
				(new byte[sizeof(short) + sizeof(short) + pkt.dataSize]);

			int offset = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(pkt.id),
				0, array.Array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(BitConverter.GetBytes(pkt.dataSize),
				0, array.Array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(pkt.data.ToArray(), 0, array.Array, offset, pkt.dataSize);

			return array;
		}
		public ArraySegment<byte> Serialize(short id, short dataSize, ArraySegment<byte> bytes)
		{
			//header + datasize
			ArraySegment<byte> array = new ArraySegment<byte>
				(new byte[sizeof(short) + sizeof(short) + dataSize]);

			int offset = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(id),
				0, array.Array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(BitConverter.GetBytes(dataSize),
				0, array.Array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(bytes.Array, 0, array.Array, offset, dataSize);

			return array;
		}
		protected int Deserialize(ReadOnlySequence<byte> buffer, out Packet packet)
		{
			ReadOnlySpan<byte> buf = buffer.FirstSpan;
			packet = new Packet();
			int offset = 0;

			packet.id = BitConverter.ToInt16(buf.Slice(offset, sizeof(short)));
			offset += sizeof(short);

			packet.dataSize = BitConverter.ToInt16(buf.Slice(offset, sizeof(short)));
			offset += sizeof(short);

			packet.data = buf.Slice(offset, packet.dataSize).ToArray();
			offset += packet.dataSize;

			return offset;
		}


	}
}
