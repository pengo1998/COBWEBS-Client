using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneItemEnableStateChangedEventArgs : EventArgs
	{
		public string sceneName { get; set; }
		public int sceneItemId { get; set; }
		public bool sceneItemEnabled { get; set; }
	}
}
