using COBWEBS_Client.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneItemListReindexedEventArgs : EventArgs
	{
		public string sceneName { get; set; }
		public STRUCT_SCENE_ITEMS[] sceneItems { get; set; }
	}
}
