using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class InputVolumeChangedEventArgs : EventArgs
	{
		public string inputName { get; set; }
		public double inputVolumeMul { get; set; }
		public double inputVolumeDb { get; set; }
	}
}
