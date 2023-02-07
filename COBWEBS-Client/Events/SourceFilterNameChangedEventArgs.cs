﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class SourceFilterNameChangedEventArgs : EventArgs
	{
		public string sourceName { get; set; }
		public string oldFilterNam { get; set; }
		public string filterName { get; set; }
	}
}
