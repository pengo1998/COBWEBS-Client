﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COBWEBS_Client.Events
{
	public class CurrentEvenTransitionDurationChangedEventArgs : EventArgs
	{
		public long transitionDuration { get; set; }
	}
}
