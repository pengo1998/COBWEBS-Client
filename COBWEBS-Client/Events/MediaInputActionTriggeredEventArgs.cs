using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class MediaInputActionTriggeredEventArgs : EventArgs
	{
		public string inputName { get; set; }
		public ObsMediaInputAction mediaAction { get; set; }
	}
}
