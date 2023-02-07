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
		public int sceneItemId { get; set; }
		public object sceneItemTransform { get; set; }
	}
}
