using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SourceFilterCreatedEventArgs : EventArgs
	{
		public string sourceName { get; set; }
		public string filterName { get; set; }
		public string filterKind { get; set; }
		public long filterIndex { get; set; }
		public object filterSettings { get; set; }
		public object defaultFilterSettings { get; set; }
	}
}
