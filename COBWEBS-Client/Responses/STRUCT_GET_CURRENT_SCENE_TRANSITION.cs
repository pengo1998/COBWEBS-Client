using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_CURRENT_SCENE_TRANSITION
	{
		public string transitionName { get; set; }
		public string transitionKind { get; set; }
		public bool transitionFixed { get; set; }
		public int transitionDuration { get; set; }
		public bool transitionConfigurable { get; set; }
		public object transitionSettings { get; set; }
	}
}
