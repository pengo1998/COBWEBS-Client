using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class InputAudioSyncOffsetChangedEventArgs : EventArgs
	{
		public string inputName { get; set; }
		public long inputAudioSyncOffset { get; set; }
	}
}
