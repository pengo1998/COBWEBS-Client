using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Structs
{
	public struct STRUCT_VENDOR_RESPONSE
	{
		public string vendorName { get; set; }
		public string requestType { get; set; }
		public JToken responseData { get; set; }
	}
}
