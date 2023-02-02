using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_SCENE_COLLECTION_LIST
	{
		public string currentSceneCollectionName { get; set; }
		public string[] sceneCollections { get; set; }
	}
}
