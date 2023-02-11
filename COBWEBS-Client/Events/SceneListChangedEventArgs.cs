using COBWEBS_Client.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using COBWEBS_Client.Structs;

namespace COBWEBS_Client.Events
{
	public class SceneListChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Updated list of scenes
		/// </summary>
		public STRUCT_SCENES[] scenes { get; set; }
	}
}
