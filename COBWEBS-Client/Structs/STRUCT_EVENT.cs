using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Structs
{
	public struct STRUCT_EVENT
	{
		public JToken eventData { get; set; }
		public int eventIntent { get; set; }
		public EventType eventType { get; set; }
	}
}
