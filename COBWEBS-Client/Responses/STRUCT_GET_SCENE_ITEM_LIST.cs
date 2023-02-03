using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_SCENE_ITEM_LIST
	{
		public STRUCT_GET_SCENE_ITEM_LIST_SCENE_ITEMS[] sceneItems { get; set; }
	}

	public struct STRUCT_GET_SCENE_ITEM_LIST_SCENE_ITEMS
	{
		public string inputKind { get; set; }
		public bool isGroup { get; set; }
		public BlendMode sceneItemBlendMode { get; set; }
		public bool sceneItemEnabled { get; set; }
		public int sceneItemId { get; set; }
		public int sceneItemIndex { get; set; }
		public bool sceneItemLocked { get; set; }
		public STRUCT_GET_SCENE_ITEM_LIST_SCENE_ITEMS_SCENE_ITEM_TRANSFORM sceneItemTransform { get; set; }
		public string sourceName { get; set; }
		public string sourceType { get; set; }
	}

	public struct STRUCT_GET_SCENE_ITEM_LIST_SCENE_ITEMS_SCENE_ITEM_TRANSFORM
	{
		public int alignment { get; set; }
		public int boundsAlignment { get; set; }
		public double boundsHeight { get; set; }
		public string boundsType { get; set; }
		public double boundsWidth { get; set; }
		public int cropBottom { get; set; }
		public int cropLeft { get; set; }
		public int cropRight { get; set; }
		public int cropTop { get; set; }
		public double height { get; set; }
		public double positionX { get; set; }
		public double positionY { get; set; }
		public double rotation { get; set; }
		public double scaleX { get; set; }
		public double scaleY { get; set; }
		public double sourceHeight { get; set; }
		public double sourceWidth { get; set; }
		public double width { get; set; }
	}
}
