using COBWEBS_Client.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneItemTransformChangedEventArgs : EventArgs
	{
		public string sceneName { get; set; }
		public long sceneItemId { get; set; }
		public STRUCT_GET_SCENE_ITEM_LIST_SCENE_ITEMS_SCENE_ITEM_TRANSFORM sceneItemTransform { get; set; }
	}
}
