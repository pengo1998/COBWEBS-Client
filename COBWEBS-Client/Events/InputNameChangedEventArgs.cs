using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class InputNameChangedEventArgs : EventArgs
	{
		public string oldInputName { get; set; }
		public string inputName { get; set; }
	}
}
