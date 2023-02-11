using COBWEBS_Client.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class InputVolumeMetersEventArgs : EventArgs
	{
		public STRUCT_METER_DATA[] inputs { get; set; }
	}
}
