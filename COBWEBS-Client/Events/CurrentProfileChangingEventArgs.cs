using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class CurrentProfileChangingEventArgs : EventArgs
	{
		/// <summary>
		/// Name of the current profile
		/// </summary>
		public string profileName { get; set; }
	}
}
