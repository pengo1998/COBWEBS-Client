using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class ProfileListChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Updated list of profiles
		/// </summary>
		public string[] profiles { get; set; }
	}
}
