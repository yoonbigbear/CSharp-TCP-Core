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
		protected Socket socket;
		protected PipeReader reader;

		public TCPSession(Socket socket) => this.socket = socket;

		public virtual void Disconnect()
		{
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();
			Logger.LogInfo("session disconnect");
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
					}

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


		public void Send(byte[] bytes) => socket.Send(bytes);
		public void Send(Packet pkt) => socket.Send(Serealize(pkt));
		public void Send(List<Packet> pkts)
		{
			List<ArraySegment<byte>> array = new List<ArraySegment<byte>>();

			foreach (var e in pkts)
				array.Add(Serealize(e));

			socket.Send(array);
		}



		protected byte[] Serealize(Packet pkt)
		{
			//header + datasize
			byte[] array = new byte[sizeof(short) + sizeof(short) + pkt.dataSize];

			int offset = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(pkt.id),
				0, array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(BitConverter.GetBytes(pkt.dataSize),
				0, array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(pkt.data.ToArray(), 0, array, offset, pkt.dataSize);

			return array;
		}
		protected int Deserealize(ReadOnlySequence<byte> buffer, out Packet packet)
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
