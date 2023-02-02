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
		public object streamServiceSettings { get; set; }
	}
}
