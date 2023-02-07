using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneItemRemovedEventArgs : EventArgs
	{
		public string sceneName { get; set; }
		public string sourceName { get; set; }
		public int sceneItemId { get; set; }
	}
}
