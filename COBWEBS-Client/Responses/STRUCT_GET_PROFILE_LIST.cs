using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Responses
{
	public struct STRUCT_GET_PROFILE_LIST
	{
		public string currentProfileName { get; set; }
		public string[] profiles { get; set; }
	}
}
