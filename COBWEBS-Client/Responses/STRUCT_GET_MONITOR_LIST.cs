using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_MONITOR_LIST
	{
		public string videoMixType { get; set; }
		public int? monitorIndex { get; set; }
		public string? projectorGeometry { get; set; }
	}
}
