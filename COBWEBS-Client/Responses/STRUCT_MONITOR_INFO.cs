using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_MONITOR_INFO
	{
		public int monitorHeight { get; set; }
		public int monitorIndex { get; set; }
		public string monitorName { get; set; }
		public double monitorPositionX { get; set; }
		public double monitorPositionY { get; set; }
		public int monitorWidth { get; set; }
	}
}
