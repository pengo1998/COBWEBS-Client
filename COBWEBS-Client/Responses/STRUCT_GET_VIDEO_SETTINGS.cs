using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_VIDEO_SETTINGS
	{
		public int fpsNumerator { get; set; }
		public int fpsDenominator { get; set; }
		public int baseWidth { get; set; }
		public int baseHeight { get; set; }
		public int outputWidth { get; set; }
		public int outputHeight { get; set; }
	}
}
