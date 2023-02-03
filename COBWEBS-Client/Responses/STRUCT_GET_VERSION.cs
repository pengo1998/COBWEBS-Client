using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_VERSION
	{
		public string obsVersion { get; set; }
		public string obsWebSocketVersion { get; set; }
		public int rpcVersion { get; set; }
		public string[] availableRequests { get; set; }
		public string[] supportedImageFormats { get; set; }
		public string platform { get; set; }
		public string platformDescription { get; set; }
	}
}
