namespace TCPCore
{
	public struct Packet
	{
		public short id;
		public short dataSize;
		public Memory<byte> data;
	}

	public class PacketQueue
	{
		public List<Packet> packets = new List<Packet>();

		object _lock = new object();

		public void AddRange(List<Packet> range)
		{
			lock(this._lock)
			{
				packets.AddRange(range);
			}
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
}
