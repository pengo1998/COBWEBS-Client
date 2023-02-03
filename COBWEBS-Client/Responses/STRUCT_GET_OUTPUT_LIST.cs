using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_OUTPUT_LIST
	{
		public STRUCT_GET_OUTPUT_LIST_OUTPUTS[] outputs { get; set; }
	}

	public struct STRUCT_GET_OUTPUT_LIST_OUTPUTS
	{
		public bool outputActive { get; set; }
		public STRUCT_GET_OUTPUT_LIST_OUTPUT_OUTPUT_FLAGS outputFlags { get; set; }
		public int outputHeight { get; set; }
		public string outputKind { get; set; }
		public string outputName { get; set; }
		public int outputWidth { get; set; }
	}

	public struct STRUCT_GET_OUTPUT_LIST_OUTPUT_OUTPUT_FLAGS
	{
		public bool OBS_OUTPUT_AUDIO { get; set; }
		public bool OBS_OUTPUT_ENCODED { get; set; }
		public bool OBS_OUTPUT_MULTI_TRACK { get; set; }
		public bool OBS_OUTPUT_SERVICE { get; set; }
		public bool OBS_OUTPUT_VIDEO { get; set; }
	}
}
