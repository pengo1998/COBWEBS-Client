using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class CurrentSceneCollectionChangingEventArgs : EventArgs
	{
		/// <summary>
		/// Name of the current scene collection
		/// </summary>
		public string sceneCollectionName { get; set; }
	}
}
