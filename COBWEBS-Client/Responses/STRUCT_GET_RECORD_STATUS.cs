using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_RECORD_STATUS
	{
		public bool outputActive { get; set; }
		public bool outputPaused { get; set; }
		public string outputTimecode { get; set; }
		public ulong outputDuration { get; set; }
		public ulong outputBytes { get; set; }
	}
}
