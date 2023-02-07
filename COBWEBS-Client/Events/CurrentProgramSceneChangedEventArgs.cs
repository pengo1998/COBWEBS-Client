using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class CurrentProgramSceneChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Name of the scene that was switched to
		/// </summary>
		public string sceneName { get; set; }
	}
}
