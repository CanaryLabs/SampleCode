// Simple example to retrieve Aggregated data
// Instructions
// - Create a new application in Axiom
// - Drag and drop a listbox control onto the screen
// - Click on screen in sidebar and within screen properties, click Edit Script
// - Copy and paste the code below over the existing code
// - Script loads aggregated historian cpu data on 1 hour increments and populates a listbox with the values

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
		private string historianCPUUsageTag = "localhost.{Diagnostics}.Sys.CPU Usage Historian";
		
		protected override void OnScreenVisible()
		{
			if (!Application.Current.TagDictionary.IsTagInDictionary(historianCPUUsageTag))
				Application.Current.TagDictionary.AddTag(historianCPUUsageTag, historianCPUUsageTag, null);
			
			TagProcessedData data = Application.Current.TagDictionary.GetProcessedData(historianCPUUsageTag, DateTime.Now.AddDays(-1),  DateTime.Now, TimeSpan.FromHours(1), CanaryWebServiceHelper.HistorianWebService.HWSAggregate.TimeAverage2);
			
			ListBox1.Items.Clear();
			foreach (TVQ t in data.Tvqs)
			{
				ListBox1.Items.Add(new ListBoxItem(t.Value.ToString(), false));
			}
		}
		
		protected override void OnScreenInvisible()
		{

		}
	}
}