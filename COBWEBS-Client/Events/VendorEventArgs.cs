using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class VendorEventArgs : EventArgs
	{
		public string vendorName { get; set; }
		public string eventType { get; set; }
		public object eventData { get; set; }
	}
}
