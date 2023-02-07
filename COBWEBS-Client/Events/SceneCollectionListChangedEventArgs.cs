using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneCollectionListChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Updated list of scene collections
		/// </summary>
		public string[] sceneCollections { get; set; }
	}
}
