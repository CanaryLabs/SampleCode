// Instructions
// 1. Setup Kepware with IoTGateway
//    1a. Kepware IoTGateway requires 32 bit Java JRE
// 2. Configure a rest server agent in Kepware IoTGateway configuration
// 3. Create a new application in Axiom
// 4. Add a button to the screen named btnSetTagValue
// 5. Click on the button in Axiom and click on the click event handler in the properties. This will load the scripting dialog.
// 6. Copy and paste the code below into axiom
// 7. Ensure the URL configured in Kepware IoT rest server agent matches the domain in the KepwareWriteEndpoint setting below
// 8. Configure the tagToWriteTo variable to be the tag you want to write to
// 9. Configure the value you want to write
// 10. Close the scripting dialog. 
// 11. Add a label to the screen named lblSuccess
// 12. Save the application and test

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using AxiomCore;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

namespace AxiomScripts
{
	public partial class Screen1_Script
	{
		protected override void OnScreenVisible()
		{
		
		}
		
		protected override void OnScreenInvisible()
		{
		
		}
		
		public void btnSetTagValue_Click(object sender, EventArgs e)
		{
			// insert code here
			WriteTagValue(tagToWriteTo, valueToWrite);
		}
		
		// kepware iotgateway write endpoint
		private string KepwareWriteEndpoint = "https://localhost:39320/iotgateway/write";
		// tag to write to
		string tagToWriteTo = "Data Type Examples.16 Bit Device.K Registers.Boolean1";
		// value to write to the tag
		int valueToWrite = 1;
		
		public void WriteTagValue(string tagName, int val)
        {
			// build the json to send to kewpare
			string postData = "[{\"id\": \"" + tagName + "\", \"v\":" + val + "}]";
			string response;
			
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(KepwareWriteEndpoint);
				request.Method = "POST";
				request.ServerCertificateValidationCallback += remoteCertificateValidationCallback;
				request.ContentType = "application/json";

				byte[] post = System.Text.Encoding.UTF8.GetBytes(postData);
				using (Stream rs = request.GetRequestStream())
				{
					rs.Write(post, 0, post.Length);
				}

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
				{
					response = sr.ReadToEnd();
				}
				
				lblSuccess.Text = "Success";
			}
			catch (Exception ex)
			{
				lblSuccess.Text = "Failed to write tag value: " + ex.ToString();
			}
        }
		
		private bool remoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
			      // does not perform any certificate validation to permit self signed certificates using https
            // Note: for development localhost purposes only
            return true;
        }
	}
}
