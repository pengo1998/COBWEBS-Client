using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Structs
{
	public struct STRUCT_METER_DATA
	{
		/// <summary>
		/// first index position is L/R channel respectively
		/// second index i'm guessing is min/avg/max volumes in an unknown unit but cannot confirm
		/// </summary>
		public float[][] inputLevelsMul { get; set; }
		public string inputName { get; set; }
	}
}
