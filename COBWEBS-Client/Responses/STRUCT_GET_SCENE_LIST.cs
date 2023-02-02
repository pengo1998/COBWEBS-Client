using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_SCENE_LIST
	{
		public string currentProgramSceneName { get; set; }
		public string currentPreviewSceneName { get; set; }
		public object[] scenes { get; set; }
	}
}
