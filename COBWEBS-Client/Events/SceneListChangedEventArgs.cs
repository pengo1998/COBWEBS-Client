using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SceneListChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Updated list of scenes
		/// </summary>
		public object[] scenes { get; set; }
	}
}
