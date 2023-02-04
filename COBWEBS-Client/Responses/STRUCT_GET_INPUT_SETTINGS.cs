using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_INPUT_SETTINGS
	{
		public JToken inputSettings { get; set; }
		public string inputKind { get; set; }
	}
}
