using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_STREAM_SERVICE_SETTINGS
	{
		public string streamServiceType { get; set; }
		public STRUCT_STREAM_SERVICE_SETTINGS streamServiceSettings { get; set; }
	}

	public struct STRUCT_STREAM_SERVICE_SETTINGS
	{
		public bool bwtest { get; set; }
		public string key { get; set; }
		public string server { get; set; }
		public string service { get; set; }
	}
}
