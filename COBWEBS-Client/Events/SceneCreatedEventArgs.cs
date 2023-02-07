using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneCreatedEventArgs : EventArgs
	{
		/// <summary>
		/// Name of the new scene
		/// </summary>
		public string sceneName { get; set; }
		/// <summary>
		/// Whether the scene is a group
		/// </summary>
		public bool isGroup { get; set; }
	}
}
