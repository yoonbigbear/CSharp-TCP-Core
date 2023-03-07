using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace TCPCore
{
	static public class Logger
	{
		static object _lock = new object();

		static public bool Assert(bool condition)
		{
			Debug.Assert(condition);
			return condition;
		}
		static public bool Assert(bool condition, string msg)
		{
			Debug.Assert(condition, msg);
			return condition;
		}

		static public void DebugInfo(string str) 
		{
#if DEBUG
			lock (_lock)
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine($"[Info] {DateTimeOffset.Now.DateTime} {str}");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
#endif
		}
		static public void DebugGreen(string str)
		{
#if DEBUG
			lock (_lock)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"[Info] {DateTimeOffset.Now.DateTime} {str}");
				Console.ForegroundColor = ConsoleColor.Gray;
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
				Console.ForegroundColor = ConsoleColor.Gray;
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
				Console.ForegroundColor = ConsoleColor.Gray;
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
				Console.ForegroundColor = ConsoleColor.Gray;
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
	public class GameLog
	{
		static object _lock = new object();
		//file i/o
		static void FileIO(string str)
		{
			string path = Directory.GetCurrentDirectory() + "\\game_log";
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

		static public void Write<T>(T type, object[] param) where T : System.Enum
		{
			var json = JsonConvert.SerializeObject(param);

			StringBuilder log = new();
			log.Append("{");
			log.Append($"\"{type.ToString()}\":");
			log.Append(json);
			log.Append("}");
#if DEBUG
			Logger.DebugGreen($"{DateTimeOffset.Now.DateTime} {log}");
#else
		FileIO($"[Info] {DateTimeOffset.Now.DateTime} {log}");
#endif
		}
		static public void Write<T>(T type, object param) where T : System.Enum =>
			Write(type, new object[]{ param });
	}
}
