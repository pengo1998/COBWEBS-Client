using COBWEBS_Client.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class InputAudioTracksChangedEventArgs : EventArgs
	{
		public string inputName { get; set; }
		public STRUCT_GET_INPUT_AUDIO_TRACKS_ARRAY inputAudioTracks { get; set; }
	}
}
