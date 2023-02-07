using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneRemovedEventArgs : EventArgs
	{
		/// <summary>
		/// Name of the deleted scene
		/// </summary>
		public string sceneName { get; set; }
		/// <summary>
		/// Whether the scene was a group
		/// </summary>
		public bool isGroup { get; set; }
	}
}
