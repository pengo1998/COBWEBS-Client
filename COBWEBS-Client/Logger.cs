using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client
{
	internal static class Logger
	{
		private static int _logLevel;
		public static void Init(COBWEBSConfiguration config)
		{
			_logLevel = config.LogLevel switch
			{
				LogLevel.Debug => 0,
				LogLevel.Information => 1,
				LogLevel.Warning => 2,
				LogLevel.Error => 3,
				LogLevel.None => 4,
				_ => throw new Exception("Invalid LogLevel in configuration.")
			};
		}

		public static void LogDebug(string msg)
		{
			if (_logLevel > 0) return;
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write("[DEBUG] ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(msg);
		}

		public static void LogInfo(string msg)
		{
			if (_logLevel > 1) return;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("[INFO] ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(msg);
		}

		public static void LogWarning(string msg)
		{
			if (_logLevel > 2) return;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("[WARN] ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(msg);
		}

		public static void LogError(string msg)
		{
			if (_logLevel > 3) return;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("[ERROR] ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(msg);
		}
	}
}
