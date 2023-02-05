using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_MEDIA_INPUT_STATUS
	{
		public MediaState mediaState { get; set; }
		public ulong? mediaDuration { get; set; }
		public ulong? mediaCursor { get; set; }
	}
}
