using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_INPUT_AUDIO_TRACKS_ARRAY
	{
		[JsonProperty("1")]
		public bool one { get; set; }
		[JsonProperty("2")]
		public bool two { get; set; }
		[JsonProperty("3")]
		public bool three { get; set; }
		[JsonProperty("4")]
		public bool four { get; set; }
		[JsonProperty("5")]
		public bool five { get; set; }
		[JsonProperty("6")]
		public bool six { get; set; }
	}
}
