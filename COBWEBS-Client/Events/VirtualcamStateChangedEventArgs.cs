using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class VirtualcamStateChangedEventArgs : EventArgs
	{
		public bool outputActive { get; set; }
		public string outputState { get; set; }
	}
}
