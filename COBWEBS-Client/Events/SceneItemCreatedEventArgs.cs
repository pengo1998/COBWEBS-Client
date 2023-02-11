using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneItemCreatedEventArgs : EventArgs
	{
		public string sceneName { get; set; }
		public string sourceName { get; set; }
		public long sceneItemId { get; set; }
		public long sceneItemIndex { get; set; }
	}
}
