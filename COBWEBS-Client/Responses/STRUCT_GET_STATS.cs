using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_STATS
	{
		public double cpuUsage { get; set; }
		public double memoryUsage { get; set; }
		public double availableDiskSpace { get; set; }
		public double activeFps { get; set; }
		public double averageFrameRenderTime { get; set; }
		public ulong renderSkippedFrames { get; set; }
		public ulong renderTotalFrames { get; set; }
		public ulong outputSkippedFrames { get; set; }
		public ulong outputTotalFrames { get; set; }
		public ulong webSocketSessionIncomingMessages { get; set; }
		public ulong webSocketSessionOutgoingMessages { get; set; }
	}
}
