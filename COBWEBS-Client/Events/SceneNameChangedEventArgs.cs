using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneNameChangedEventArgs : EventArgs
	{
		public string oldSceneName { get; set; }
		public string sceneName { get; set; }
	}
}
