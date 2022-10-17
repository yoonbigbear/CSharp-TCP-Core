namespace TCPCore
{
	public class Time
	{
		public static float Delta = 0.0f;

		//time delta
		DateTime start;
		DateTime now;

		public void Start()
		{
			start = DateTime.Now;
			now = DateTime.Now;
		}

		public void UpdateDeltaTime()
		{
			now = DateTime.Now;
			var interval = now - start;
			start = now;
			Delta = (float)interval.TotalMilliseconds;
		}
	}
}
