using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class InputRemovedEventArgs : EventArgs
	{
		/// <summary>
		/// Name of the deleted input
		/// </summary>
		public string inputName { get; set; }
	}
}
