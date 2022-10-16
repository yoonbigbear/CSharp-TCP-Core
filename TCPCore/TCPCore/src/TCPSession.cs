using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;

namespace TCPCore
{
	//https://github.com/davidfowl/TcpEcho/blob/master/src/Server/Program.cs
	public class TCPSession
	{
		Socket socket;
		PipeReader reader;

		List<Packet> packets = new List<Packet>();

		public TCPServer Server { get; set; }
		public int SessionId { get; set; }

		public TCPSession(Socket socket)
		{
			this.socket = socket;
		}

		public virtual void Init()
		{
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);

			var read = new NetworkStream(socket);

			reader = PipeReader.Create(read,
						new StreamPipeReaderOptions(bufferSize: ushort.MaxValue));

			Task.Run(() => { Receive(); });
		}

		async void Receive()
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

						var offset = ReadBuffer(buffer.Slice(0));
						SequencePosition end = buffer.GetPosition(offset);

						buffer = buffer.Slice(end);
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

		public void Send(byte[] data)
		{
			Task.Run(() => socket.Send(data));
		}

		int ReadBuffer(ReadOnlySequence<byte> buffer)
		{
			ReadOnlySpan<byte> buf = buffer.FirstSpan;

			var packet = new Packet();
			int offset = 0;

			packet.id = BitConverter.ToInt16(buf.Slice(offset, sizeof(short)));
			offset += sizeof(short);

			packet.dataSize = BitConverter.ToInt16(buf.Slice(offset, sizeof(short)));
			offset += sizeof(short);

			packet.data = buf.Slice(offset, packet.dataSize).ToArray();
			offset += packet.dataSize;

			packets.Add(packet);
			return offset;
		}

		public virtual void Disconnect()
		{
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();
			Logger.LogInfo("session disconnect");
		}

	}
}
