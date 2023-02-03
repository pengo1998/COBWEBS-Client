using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_STREAM_STATUS
	{
		public bool outputActive { get; set; }
		public bool outputReconnecting { get; set; }
		public string outputTimecode { get; set; }
		public ulong outputDuration { get; set; }
		public ulong outputCongestion { get; set; }
		public ulong outputBytes { get; set; }
		public ulong outputSkippedFrames { get; set; }
		public ulong outputTotalFrames { get; set; }
	}
}
