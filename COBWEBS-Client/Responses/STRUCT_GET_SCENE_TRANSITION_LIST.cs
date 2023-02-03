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
		public STRUCT_GET_SCENE_TRANSITION_LIST_TRANSITIONS[] transitions { get; set; }
	}

	public struct STRUCT_GET_SCENE_TRANSITION_LIST_TRANSITIONS
	{
		public bool transitionConfigurable { get; set; }
		public bool transitionFixed { get; set; }
		public string transitionKind { get; set; }
		public string transitionName { get; set; }
	}
}
