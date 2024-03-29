﻿namespace TCPCore
{

	public struct Packet
	{
		public short id { get; set; }
		public int dataSize { get; set; }
		public ArraySegment<byte> data { get; set; }
	}

	public class PacketQueue
	{
		public List<Packet> packets = new();

		object _lock = new object();

		public void AddRange(List<Packet> range)
		{
			lock(this._lock)
			{
				packets.AddRange(range);
			}
			range.Clear();
		}
		public void TransferTo(out List<Packet> output)
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
		private Dictionary<short, Action<ArraySegment<byte>>> packetHandler = new();

		public bool Bind(short protocol, Action<ArraySegment<byte>> callback)
		{
			return packetHandler.TryAdd(protocol, callback);
		}

		public virtual void Invoke(Packet packet)
		{
			if (packetHandler.ContainsKey(packet.id))
			{
				packetHandler[packet.id].Invoke(packet.data);
			}
			else
			{
				Logger.DebugError($"Packet id need has no callback function. ID: {packet.id}");
			}
		}
	}
}
