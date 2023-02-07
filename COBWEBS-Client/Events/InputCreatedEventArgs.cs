using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class InputCreatedEventArgs : EventArgs
	{
		public string inputName { get; set; }
		public string inputKind { get; set; }
		public string unversionedInputKind { get; set; }
		public JToken inputSettings { get; set; }
		public JToken defaultInputsettings { get; set; }
	}
}
