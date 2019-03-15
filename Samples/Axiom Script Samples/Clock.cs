// Simple example to print the current time in a label on the screen
// Instructions
// - Create a new application in Axiom
// - Drag and drop a label control onto the screen
// - Click on screen in sidebar and within screen properties, click Edit Script
// - Copy and paste the code below over the existing code

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using AxiomCore;

namespace AxiomScripts
{
	public partial class Screen1_Script
	{
		private System.Timers.Timer refreshTimer;
		
		protected override void OnScreenVisible()
		{
			if (refreshTimer == null)
			{
				refreshTimer = new System.Timers.Timer();
				refreshTimer.Interval = 1000;
				refreshTimer.Elapsed += TimerFired;
			}
			
			refreshTimer.Enabled = true;
		}
		
		protected override void OnScreenInvisible()
		{
			refreshTimer.Enabled = false;
		}
		
		// refreshes the screen (timer execution)
		private void TimerFired(Object source, System.Timers.ElapsedEventArgs e)
		{
			Label1.Text = DateTime.Now.ToString("F");
		}
	}
}