using System.Net.Sockets;

namespace SampleClient
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Connector connector = new Connector();
			connector.Start();

			Task<Socket> socketTask = connector.ConnectAsync();
			Socket socket = socketTask.Result;

			short i = 0;
			while (true)
			{
				if (i++ > 1000)
					break;

				short datasize = 10;
				byte[] data = new byte[datasize];
				Send(
					1,
					datasize,
					data,
					socket);
			}

			socket.Shutdown(SocketShutdown.Both);
			socket.Close();
		}

		static public void Send(short id, short size, byte[] data, Socket socket)
		{
			//header + datasize
			byte[] array = new byte[sizeof(short) + sizeof(short) + size];

			int offset = 0;
			Buffer.BlockCopy(BitConverter.GetBytes(id),
				0, array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(BitConverter.GetBytes(size),
				0, array, offset, sizeof(short));
			offset += sizeof(short);
			Buffer.BlockCopy(data, 0, array, offset, size);

			socket.Send(array);
		}
	}
}