using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_OUTPUT_STATUS
	{
		public bool outputActive { get; set; }
		public bool outputReconnecting { get; set; }
		public string outputTimecode { get; set; }
		public double outputDuration { get; set; }
		public double outputCongestion { get; set; }
		public long outputBytes { get; set; }
		public int outputSkippedFrames { get; set; }
		public int outputTotalFrames { get; set; }
	}
}
