namespace TCPCore
{

	public struct Packet
	{
		public short id;
		public short dataSize;
		public ArraySegment<byte> data;
	}

	public class PacketQueue
	{
		public List<Tuple<TCPSession, Packet>> packets = new();

		object _lock = new object();

		public void AddRange(List<Tuple<TCPSession, Packet>> range)
		{
			lock(this._lock)
			{
				packets.AddRange(range);
			}
			range.Clear();
		}
		public void TransferTo(out List<Tuple<TCPSession, Packet>> output)
		{
			lock (this._lock)
			{
				output = packets.ToList();
				packets.Clear();
			}
		}
	}

	public class PacketHandler
	{
		private Dictionary<short, Action<TCPSession, ArraySegment<byte>>> packetHandler = new();

		public bool Bind(short protocol, Action<TCPSession, ArraySegment<byte>> callback)
		{
			return packetHandler.TryAdd(protocol, callback);
		}

		public virtual void Invoke(TCPSession session, Packet packet)
		{
			if (packetHandler.ContainsKey(packet.id))
			{
				packetHandler[packet.id].Invoke(session, packet.data);
			}
			else
			{
				Logger.DebugError($"Packet id need has no callback function. ID: {packet.id}");
			}
		}
	}
}
