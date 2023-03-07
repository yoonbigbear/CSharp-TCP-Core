using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace TCPCore
{
	//https://github.com/davidfowl/TcpEcho/blob/master/src/Server/Program.cs
	public class TCPSession
	{
		public PacketQueue _recvPacketQueue { get; set; }
		public Socket socket { get; set; }
		protected PipeReader reader { get; set; }
		public long SocketHandle { get; set; }


		private List<ArraySegment<byte>> _sendQueue { get; set; } = new();
		private Dictionary<short, Packet> _recvPair { get; set; } = new();

		public TCPSession(Socket socket) => this.socket = socket;

		object _lock = new object();
		object _leavelock = new object();

		const float _heartbeatTimeOut = 10000;
		float _hearbeat = 0;
		int _heartbeatId = -1;

		public Action OnDisconnect;

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
			OnDisconnect?.Invoke();
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
			//send all packets at one time
			ReleaseSendQueue();
		}

		protected void HeartBeat() {
			if (_hearbeat > _heartbeatTimeOut)
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
					var buffer = result.Buffer;

					//no more
					if (result.IsCompleted)
						break;

					var remainData = buffer.Length;
					//client disconnected
					if (remainData == 0)
						return;

					//read data
					while (true)
					{
						if (buffer.Start.Equals(buffer.End))
							break;

						var offset = Deserialize(buffer.Slice(0), out var packet);
						SequencePosition end = buffer.GetPosition(offset);
						buffer = buffer.Slice(end);

						if (packet.id == _heartbeatId) //heartbeatid
						{
							_hearbeat = 0;
							continue;
						}

						_recvPair[packet.id] = packet;
					}

					//enqueue packets
					_recvPacketQueue.AddRange(_recvPair.Values.ToList());
					_recvPair.Clear();
					//move cursor
					reader.AdvanceTo(buffer.Start, buffer.End);
				}
			}
			catch (Exception e)
			{
				Logger.DebugError($"Receive Error {e.Message}");
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
		public void Send(short id, int dataSize, ArraySegment<byte> bytes)
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
					socket.Send(packets);
				}
				catch (Exception e)
				{
					Logger.DebugError($"Socket Exception {e.Message}");
					Disconnect();
				}
			}
		}


		static public ArraySegment<byte> Serialize(Packet pkt)
		{
			//header + datasize
			ArraySegment<byte> array = new ArraySegment<byte>
				(new byte[sizeof(short) + sizeof(int) + pkt.dataSize]);

			int offset = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(pkt.id),
				0, array.Array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(BitConverter.GetBytes(pkt.dataSize),
				0, array.Array, offset, sizeof(int));
			offset += sizeof(int);
			Buffer.BlockCopy(pkt.data.ToArray(), 0, array.Array, offset, pkt.dataSize);

			return array;
		}
		static public ArraySegment<byte> Serialize(short id, int dataSize, ArraySegment<byte> bytes)
		{
			//header + datasize
			ArraySegment<byte> array = new ArraySegment<byte>
				(new byte[sizeof(short) + sizeof(int) + dataSize]);

			int offset = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(id),
				0, array.Array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(BitConverter.GetBytes(dataSize),
				0, array.Array, offset, sizeof(int));
			offset += sizeof(int);
			Buffer.BlockCopy(bytes.Array, 0, array.Array, offset, dataSize);

			return array;
		}
		protected int Deserialize(ReadOnlySequence<byte> buffer, out Packet packet)
		{
			ReadOnlySpan<byte> buf = buffer.FirstSpan;
			packet = new Packet();

			//return transferred size
			int offset = 0;

			packet.id = BitConverter.ToInt16(buf.Slice(offset, sizeof(short)));
			offset += sizeof(short);

			packet.dataSize = BitConverter.ToInt32(buf.Slice(offset, sizeof(int)));
			offset += sizeof(int);

			packet.data = buf.Slice(offset, packet.dataSize).ToArray();
			offset += packet.dataSize;

			return offset;
		}


	}
}
