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
		public double renderSkippedFrames { get; set; }
		public double renderTotalFrames { get; set; }
		public double outputSkippedFrames { get; set; }
		public double outputTotalFrames { get; set; }
		public double webSocketSessionIncomingMessages { get; set; }
		public double webSocketSessionOutgoingMessages { get; set; }
	}
}
