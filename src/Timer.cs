namespace TCPCore
{
	public class Timer
	{
		public static float Delta = 0.0f;
		public static float DeltaMs = 0.0f;

		public static double Rtt = 0.0f;

		//ServerTime
		public static long UnixServeTime = 0;

		//time delta
		double AccumulatedMs = 0;
		long start;
		long now;

		//time sync interval
		static long _lastRttSend;


		//desired frame rate
		float _fpsToSecond = 0;

		public void Start()
		{
			start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		public void DesiredFps(int fps)
		{
			if (fps <= 0)
				return;
			_fpsToSecond = 1000 / fps;
		}

		void UpdateDeltaTime()
		{
			now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			var interval = now - start;
			start = now;

			AccumulatedMs += (interval);
		}

		public bool UpdateDelta()
		{
			UpdateDeltaTime();
			if (AccumulatedMs > _fpsToSecond)
			{
				Delta = (float)(AccumulatedMs * 0.001f);
				DeltaMs = (float)AccumulatedMs;
				AccumulatedMs = 0;
				return true;
			}
			return false;
		}

		public static void SendRtt()
		{
			_lastRttSend = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		public static void RecvRtt()
		{
			var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			Rtt = (now - _lastRttSend) / 2;
			_lastRttSend = now;
		}
	}
}
