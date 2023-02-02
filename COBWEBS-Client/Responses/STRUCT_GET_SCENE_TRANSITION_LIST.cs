using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_SCENE_TRANSITION_LIST
	{
		public string currentSceneTransitionName { get; set; }
		public string currentSceneTransitionKind { get; set; }
		public object[] transitions { get; set; }
	}
}
