using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_SOURCE_FILTER_LIST_FILTERS
	{
		public bool filterEnabled { get; set; }
		public int filterIndex { get; set; }
		public string filterKind { get; set; }
		public string filterName { get; set; }
		public JToken filterSettings { get; set; }
	}
}
