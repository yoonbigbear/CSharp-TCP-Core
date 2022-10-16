namespace TCPCore
{
	static public class Logger
	{
		static object _lock = new object();

		static public void DebugInfo(string str) 
		{
#if DEBUG
			lock (_lock)
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine($"[Info] {DateTimeOffset.Now.DateTime} {str}");
				Console.ForegroundColor = ConsoleColor.White;
			}
#endif
		}
		static public void DebugInfoHighlight(string str)
		{
#if DEBUG
			lock (_lock)
			{
				Console.BackgroundColor = ConsoleColor.White;
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"[Info] {DateTimeOffset.Now.DateTime} {str}");
				Console.ForegroundColor = ConsoleColor.White;
				Console.BackgroundColor = ConsoleColor.Black;
			}
#endif
		}
		static public void DebugWarning(string str) 
		{
#if DEBUG
			lock (_lock)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"[Warning] {DateTimeOffset.Now.DateTime} {str}");
				Console.ForegroundColor = ConsoleColor.White;
			}
#endif
		}
		static public void DebugError(string str)
		{
#if DEBUG
			lock (_lock)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"[Error] {DateTimeOffset.Now.DateTime} {str}");
				Console.ForegroundColor = ConsoleColor.White;
			}
#endif
		}
		static public void DebugSuccess(string str)
		{
#if DEBUG
			lock (_lock)
			{
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine($"[Success] {DateTimeOffset.Now.DateTime} {str}");
				Console.ForegroundColor = ConsoleColor.White;
			}
#endif
		}

		//file i/o
		static void FileIO(string str)
		{
			string path = Directory.GetCurrentDirectory() + "\\log";
			string filename = $"\\log_{DateTimeOffset.Now.Date.ToShortDateString()}.log";

			lock (_lock)
			{
				if (!Directory.Exists($"{path}"))
				{
					Directory.CreateDirectory($"{path}");
				}
				using (var sw = File.AppendText($"{path}{filename}"))
				{
					sw.WriteLine(str);
				}
			}
		}

		static public void LogInfo(string str) 
			=> FileIO($"[Info] {DateTimeOffset.Now.DateTime} {str}");
		static public void LogWarning(string str)
			=> FileIO($"[Warning] {DateTimeOffset.Now.DateTime} {str}");
		static public void LogError(string str)
			=> FileIO($"[Error] {DateTimeOffset.Now.DateTime} {str}");
	}
}
