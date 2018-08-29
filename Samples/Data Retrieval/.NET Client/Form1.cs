using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using CanaryWebServiceHelper.HistorianWebService;
using CanaryWebServiceHelper;

namespace HWS_API_Example
{
    public partial class Form1 : Form
    {
        public HistorianWebServiceClient client = null;
        public int cci = 0;

        public string myHistServer;
        public string myDataSet;
        List<string> myTagNameList = new List<string>();
        DateTime oldestTS;
        DateTime latestTS;
        HWSTagData asyncRawData = null;

        CanaryWebServiceHelper.HWS_LiveData liveData = null;
        List<string> liveTags;
        bool resizeColumns = true;
        bool currentDataExtension;

        CanaryWebServiceHelper.HWS_Discovery discover = new CanaryWebServiceHelper.HWS_Discovery();

        public Form1()
        {
            InitializeComponent();
        }

        private void startToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            APIExample();
        }


        public void PostMsg(string msg)
        {
            string msg1 = DateTime.Now.ToString("HH:mm:ss.fff") + "    " + msg + "\n";
            richTextBox1.AppendText(msg1);

            richTextBox1.Focus();
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void APIExample()
        {
            richTextBox1.Clear();
            PostMsg("Reading Data Example Started");
            if (ConnectToWebService("localHost") == false)
                return;

            if (ServerAndDataSetOperations() == true)
            {
                TagOperations();
                //AdvancedReading();
                AsyncReading();
            }
            
            //DisconnectFromWebService();  // Handled during Form Closing
            PostMsg("Reading Data Example Finished");
        }


        private bool ConnectToWebService(string wsServerName)
        {
            if (dataExtensionCheckBox.Checked != currentDataExtension)
                DisconnectFromWebService();

            if (cci != 0)
                return true;     // We already have a connection and license...

            string appName = Application.ExecutablePath;
            appName = appName.Substring(appName.LastIndexOf("\\") + 1);
            appName += "(PID=" + Process.GetCurrentProcess().Id.ToString() + ")";
            string userId = SystemInformation.ComputerName + ":" + SystemInformation.UserName;

            // If you are connecting programmatically net.tcp is faster than http
            try
            {
                HWS_ConnectionHelper.WebServiceConnect(HWS_ConnectionType.NetTcp_Anonymous, wsServerName, appName, userId, null, null, out client, out cci);
                //HWS_ConnectionHelper.WebServiceConnect(HWS_ConnectionType.Http_Anonymous, wsServerName, appName, userId, null, null, out client, out cci);
                //HWS_ConnectionHelper.WebServiceConnect(HWS_ConnectionType.NetPipe_Anonymous, wsServerName, appName, userId, null, null, out client, out cci);
            }
            catch (Exception ex)
            {
                // Problems Connecting to the Endpoint ...
                string errMsg = ex.Message;
                if (ex.InnerException != null)
                    errMsg += "\n\n" + ex.InnerException.Message;
                PostMsg(errMsg);
                return false;
            }
            // Everything is Good!

            string wsVersion = client.WebServiceVersion();
            string interfaceVersion = client.WebServiceInterfaceVersion();

            PostMsg("Connected to Web Service on " + wsServerName);
            PostMsg("Web Service Version " + wsVersion);
            PostMsg("API Version " + interfaceVersion);

            if (cci == 0)
            {
                PostMsg("License for " + appName + " is not available at this time from the Canary Historian WebService");
                return false;
            }

            currentDataExtension = false;  
            if (dataExtensionCheckBox.Checked)
            {
                client.SetVirtualTimeExtensionFlag(true, cci);
                currentDataExtension = true;
            }
            return true;
        }


        private void DisconnectFromWebService()
        {
            if (client == null)
                return;

            if (client.State == System.ServiceModel.CommunicationState.Opened)
            {
                if (cci != 0)
                {
                    try
                    {
                        client.ReleaseClientConnectId(null, null, cci);
                    }
                    catch (Exception ex)
                    {
                        string errMsg = ex.Message;
                        if (ex.InnerException != null)
                            errMsg += "\n\n" + ex.InnerException.Message;
                        PostMsg(errMsg);
                    }
                    cci = 0;
                }
                client.Close();
                client = null;
                cci = 0;
            }
        }
        

        public string[] GetDataSetList(string serverName)
        {
            // Get a List of DataSet on the specified historian
            string[] dataSetList = client.GetDataSetList(myHistServer, false, cci);
            string msg = "GetDataSetList from " + myHistServer + ":  ";
            msg += dataSetList.Length.ToString() + " datasets returned";
            PostMsg(msg);
            return dataSetList;
        }



        private bool ServerAndDataSetOperations()
        {
            //  Get a List of Historian accessible from the Web Service
            string[] histServerList = client.GetServerList(cci);
            string msg = "GetServerList: ";
            for (int i = 0; i < histServerList.Length; i++)
                msg += "\n\t\t" + histServerList[i];
            PostMsg(msg);
            if (histServerList.Length == 0)
                return false;

            string[] status = client.CommandStatus("LocalHistorianName");
            if (status.Length > 0)
                myHistServer = status[0];

            // Get Properties for a specific Historian
            HWSPropertyInfo properties = client.GetServerInfo(myHistServer, cci);
            msg = "GetServerInfo from " + myHistServer + ":  ";
            msg += properties.propName.Length.ToString() + " properties returned";
            for (int i = 0; i < properties.propName.Length; i++)
                msg += "\n\t\t" + properties.propName[i] + ":  \t" + properties.propValue[i];
            PostMsg(msg);


            // Get a List of DataSet on the specified historian
            string[] dataSetList = GetDataSetList(myHistServer);
            if (dataSetList.Length == 0)
                return false;

            // Try to find the "Sample Data" in the dataSet List
            //  If not found select the first dataset available..
            myDataSet = dataSetList[0];
            for (int i = 0; i < dataSetList.Length; i++)
            {
                if (dataSetList[i] == "Sample Data")
                    myDataSet = dataSetList[i];
            }
          
            PostMsg("Selected DataSet: " + myDataSet);

            // Get DataSet information for a specified DataSet...
            properties = client.GetDataSetInfo(myHistServer, myDataSet, cci);
            msg = "GetDataSetInfo from " + myDataSet + ":  ";
            msg += properties.propName.Length.ToString() + " properties returned";
            for (int i = 0; i < properties.propName.Length; i++)
                msg += "\n\t\t" + properties.propName[i] + ":  \t" + properties.propValue[i];
            PostMsg(msg);


            myTagNameList = ReadTagList(myHistServer, myDataSet);
            if (myTagNameList.Count == 0)
                return false;

            return true;
        }

      
 
        // Read all the Tags in  DataSet
        // If there are a lot of tags in the DataSet, they need to be read in blocks
        public List<string> ReadTagList(string serverName, string dataSetName)
        {
            List<string> tagList = new List<string>();
            int blockSize = 100000; 
            int startingOffset = 0;
            bool done = false;
            Stopwatch overall = Stopwatch.StartNew();
            while (done == false)
            {
                Stopwatch sw = Stopwatch.StartNew();
                HWSTagList info = null;
                try
                {
                    info = client.GetTagList(serverName, dataSetName, startingOffset, blockSize, cci);
                }
                catch (Exception ex)
                {
                    PostMsg(ex.Message);
                    return tagList;
                }
                sw.Stop();
                if (info == null)
                    done = true;

                if (info != null)
                {
                    if (info.resultFlags == -1)
                    {   // Some error has occurred 
                        PostMsg(info.errMsg);
                        return tagList;
                    }

                    string msg = "GetTagList: " + info.tagNames.Length.ToString() + " tags returned in ";
                    msg += sw.ElapsedMilliseconds.ToString("#,##0 ms");
                    PostMsg(msg);
                    for (int i = 0; i < info.tagNames.Length; i++)
                    {
                        if (info.tagType[i] == 0)    // Trend Tags
                            tagList.Add(info.tagNames[i]);
                    }

                    if (info.resultFlags == 0)
                    {
                        // Complete 
                        done = true; 
                    }
                    else
                    {   // More data is available so set the startingOffset
                        startingOffset += info.tagNames.Length;
                    }
                }
            }
            string msg1 = "Total number of Tags in Dataset:: " + tagList.Count.ToString();
            msg1 += " in " + overall.ElapsedMilliseconds.ToString("#,##0 ms");
            PostMsg(msg1);
            return tagList;
        }



        //  Get Individual Tag Information and Data
        private bool TagOperations()
        {
            // GetTagInformation for all Tags
            Stopwatch sw = Stopwatch.StartNew();
            HWSTagInfo[] tagInfo = null;
            try
            {
                tagInfo = client.GetTagInfo(myHistServer, myDataSet, myTagNameList.ToArray(), cci);
            }
            catch (Exception ex)
            {
                PostMsg(ex.Message);
                return false;
            }
            sw.Stop();
            string msg = "GetTagInfo: Returned information for " + tagInfo.Length.ToString() + " tags.";
            msg += " in " + sw.ElapsedMilliseconds.ToString("#,##0 ms");
            PostMsg(msg);

            // If the tagname is bad...  GetTagInfo will return a HWSTagInfo structure
            //   but the itemType will be -1.  

            if (tagInfo.Length == 0)
                return false;
            HWSTagInfo info = tagInfo[0];
            msg = "First Tag: " + info.tagItemId + "  Returned: " + info.tagProperties.Length.ToString() + " properties.";
            for (int i = 0; i < info.tagProperties.Length; i++)
                msg += "\n\t\t" + info.tagProperties[i].propName + ":  \t" + info.tagProperties[i].propValue;
            PostMsg(msg);


            //  Get TagContext for all Tags
            sw = Stopwatch.StartNew();
            HWSTagContext[] tagContext = client.GetTagDataContext(myHistServer, myDataSet, myTagNameList.ToArray(), cci);
            sw.Stop();
            msg = "GetTagDataContext: Returned context data for " + tagInfo.Length.ToString() + " tags.";
            msg += " in " + sw.ElapsedMilliseconds.ToString("#,##0 ms"); 
            PostMsg(msg);

            if (tagContext.Length == 0)
                return false;

            // Extract the range information from the first tag
            HWSTagContext context = tagContext[0];
            msg = "First Tag: ";
            msg += "  Oldest Timestamp: " + context.oldestTimeStamp.ToString();
            msg += "  Newest Data: " + context.latestTimeStamp.ToString() + "    value: " + context.latestValue.ToString();
            PostMsg(msg);
            oldestTS = context.oldestTimeStamp;
            latestTS = context.latestTimeStamp;


            // Get the Current Value for all tags
            int msgCnt = 0;
            sw = Stopwatch.StartNew();
            HWSTagCurrentValue[] currentValues = client.GetTagCurrentValue(myHistServer, myDataSet, myTagNameList.ToArray(), cci);
            sw.Stop();
            if (currentValues == null)
                PostMsg("GetTagCurrentValue: Returned null");
            else
            {
                msg = "GetTagCurrentValue: Returned current value data for " + currentValues.Length.ToString() + " tags.";
                msg += " in " + sw.ElapsedMilliseconds.ToString("#,##0 ms");
                PostMsg(msg);
                foreach (HWSTagCurrentValue current in currentValues)
                {
                    string cvMsg = "\t" + current.tagItemId + "  ";
                    cvMsg += ReadableTime(current.timeStamp) + "  ";
                    cvMsg += current.value.ToString();
                    cvMsg += current.quality.ToString();
                    cvMsg += "   (" + ReadableQuality((ushort)current.quality) + ")";
                    PostMsg(cvMsg);
                    msgCnt++;
                    if (msgCnt > 10)
                        break;
                }

            }



            
            // Get 100 values of Raw data
            HWSTagData rawData = client.GetRawTagData(myHistServer, myDataSet, myTagNameList[0], context.oldestTimeStamp, context.latestTimeStamp, 100, cci);
            msg = "GetRawTagData for tag: " + myTagNameList[0];
            msg += "   Returned " + rawData.value.Length.ToString() + " data values";
            msg += "   ResultFlag: " + rawData.resultFlags.ToString();
            PostMsg(msg);

            MyTVQ myTVQ = new MyTVQ(rawData, 0);
            PostMsg("First TVQ: " + ReadableTVQ(myTVQ));

            int lastOne = rawData.value.Length - 1;
            myTVQ = new MyTVQ(rawData, lastOne);
            PostMsg("Last TVQ: " + ReadableTVQ(myTVQ));

            // Get a List of Aggregates that the WebService can process
            string [] aggList = client.GetAggregateList();
            msg = "GetAggregateList: Returned " + aggList.Length.ToString() + " aggregates.";
            for (int i = 0; i < aggList.Length; i++)
                msg += "\n\t\t" + aggList[i];
            PostMsg(msg);


            // AggregateList is formatted as:   aggregateName, aggregateDescription
            string aggName = aggList[0].Substring(0, aggList[0].IndexOf(","));
            // The first one returned should be "TimeAverage2"
            

            // Get Processed Data (in 5 Minute intervals) for first tag in the dataSet
            TimeSpan interval = TimeSpan.FromMinutes(5);
            HWSTagProcessedData processedData = client.GetProcessedTagData(myHistServer, myDataSet, myTagNameList[0],
                        context.oldestTimeStamp, context.latestTimeStamp, 100, interval, aggName, cci);

            msg = "GetProcessedTagData for tag: " + myTagNameList[0];
            msg += "   Interval: " + interval.ToString();
            msg += "   Aggregate: " + aggName;
            msg += "   Returned " + processedData.value.Length.ToString() + " data values";
            msg += "   ResultFlag: " + rawData.resultFlags.ToString();
            PostMsg(msg);

            myTVQ = new MyTVQ(processedData, 0);
            PostMsg("First TVQ: " + ReadableTVQ(myTVQ));
            
            lastOne = processedData.value.Length-1;
            myTVQ = new MyTVQ(processedData, lastOne);
            PostMsg("Last TVQ: " + ReadableTVQ(myTVQ));

            // Build an List of TVQs returned from GetProcessedTagData
            List<MyTVQ> allTVQs = new List<MyTVQ>();
            for (int i = 0; i < processedData.value.Length; i++)
                allTVQs.Add(new MyTVQ(processedData, i));

            return true;
        }


        // This method shows how to use the GetRawData2 method
        //  To get data for multiple tags in a single call..
        //  and to get lots of data values... (Reading all available data...)
        public bool AdvancedReading()
        {
            PostMsg("Advanced - Using GetRawTagData2");

            int tagCount = 5;
            if (myTagNameList.Count < 5)
                tagCount = myTagNameList.Count;
            List<HWSTagRequest> requests = new List<HWSTagRequest>();

            // An Array to count the number of TVQs we have read for each tag
            int[] tvqCounter = new int[5];  

            for (int i = 0; i < tagCount; i++)
            {
                HWSTagRequest aRequest = new HWSTagRequest();
                aRequest.tagName = myTagNameList[i];
                aRequest.clientData = i;
                aRequest.startTime = oldestTS;
                aRequest.endTime = latestTS;
                requests.Add(aRequest);
            }

            DateTime reportAt = DateTime.Now;
            int callCount = 0;
            while (requests.Count > 0)
            {
                callCount++;
                HWSTagData2[] results = client.GetRawTagData2(myHistServer, myDataSet, requests.ToArray(), 10000, cci);

                // Process the results...
                List<HWSTagRequest> nextRequests = new List<HWSTagRequest>();
                for (int i = 0; i < results.Length; i++)
                {
                    // We use the client data value - as a index
                    //   to associate the return data back to the correct tag..
                    int index = results[i].clientData;
                    tvqCounter[index] += results[i].value.Length;

                    // Here is where you want to process the returned historical data...

                    if (results[i].resultFlags == 1)
                    {
                        // More Data is available...
                        HWSTagRequest aRequest = requests[i];
                        // new request startTime is where the last one left off...
                        aRequest.startTime = results[i].timeStamp.Last();

                        nextRequests.Add(aRequest);
                    }
                }

                // Provide user feedback every 5 seconds...
                if (reportAt < DateTime.Now)
                {
                    PostMsg("GetRawTagData2 for " + requests.Count.ToString() + " tags   # Calls: " + callCount.ToString());
                    reportAt = DateTime.Now.AddSeconds(5);
                }

                // replace the old 'requests' with the 'nextRequests'
                requests = nextRequests;
            }

            int totalTVQs = 0;
            string msg = "All Available Data was read: ";
            msg += "   Total number of webService calls: " + callCount.ToString();
            for (int i = 0; i < tagCount; i++)
            {
                totalTVQs += tvqCounter[i];
                msg += "\n\t\t " + myTagNameList[i] + "   TVQ Cnt: " + tvqCounter[i].ToString("#,###,###");
            }
            msg += "\n\tTotal number of TVQs: " + totalTVQs.ToString("#,###,###");
            PostMsg(msg);
            return true;
        }








        //  This demonstrates the use of an Async call with the WebService
        public bool AsyncReading()
        {
            PostMsg("Async Reading of RawTagData");
            // Make the Call to Get raw data... and identify the method for the callback
            asyncRawData = null;
            client.BeginGetRawTagData(myHistServer, myDataSet, myTagNameList[0], oldestTS, latestTS, 10000, cci, new AsyncCallback(GetRawTagDataCallback), client);


            // For this example we are going to wait around for the Async to return data..
            DateTime till = DateTime.Now.AddSeconds(5);
            while (asyncRawData == null)
            {
                if (DateTime.Now > till)
                {
                    PostMsg("Async request to get raw data took longer than 5 seconds");
                    return false;
                }
            }

            string msg = "Async Callback of GetRawTagData for tag: " + myTagNameList[0];
            msg += "   Returned " + asyncRawData.value.Length.ToString() + " data values";
            msg += "   ResultFlag: " + asyncRawData.resultFlags.ToString();
            PostMsg(msg);

            MyTVQ myTVQ = new MyTVQ(asyncRawData, 0);
            PostMsg("First TVQ: " + ReadableTVQ(myTVQ));

            int lastOne = asyncRawData.value.Length - 1;
            myTVQ = new MyTVQ(asyncRawData, lastOne);
            PostMsg("Last TVQ: " + ReadableTVQ(myTVQ));

            return true;
        }

        void GetRawTagDataCallback(IAsyncResult ar)
        {
            HistorianWebServiceClient client = ar.AsyncState as HistorianWebServiceClient;
            asyncRawData = client.EndGetRawTagData(ar);

            // This is a different thread ... 
            // so making a PostMsg call to write data to the display will not work
            // from within this thread.
        }







        

        // General Purpose Utility Helper Functions
        Dictionary<ushort, string> qualityMapping = new Dictionary<ushort, string>();
        public string ReadableQuality(ushort quality)
        {
            // Rely on the server to translate the bits into a readable quality string
            // However these qualities will not change so put in in a local dictionary
            // so that the client doesn't need to go back to the server to translate every quality
            string readableQual = "";
            if (qualityMapping.TryGetValue(quality, out readableQual))
            {
                return readableQual;
            }
            else
            {
                // Ask the Service to translate the quality into a readable String
                try
                {
                    if (client != null)
                    {
                        string result = client.GetReadableQuality(quality);
                        qualityMapping.Add(quality, result);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                    if (ex.InnerException != null)
                        errMsg += "\n\n" + ex.InnerException.Message;
                    client = null;
                    MessageBox.Show(errMsg, "Error");
                    return "";
                }
                return "";
            }
        }

        public string ReadableTime(DateTime tStamp)
        {
            // if there is millisecond info show it
            if (tStamp.Millisecond == 0)
                return tStamp.ToString("MM/dd/yyyy HH:mm:ss");
            return tStamp.ToString("MM/dd/yyyy HH:mm:ss.fff");
        }

        public string ReadableTVQ(MyTVQ tvq)
        {
            string str = "Timestamp:" + ReadableTime(tvq.timestamp);
            if (tvq.value != null)
                str += "  Value:" + tvq.value.ToString();
            str += "  Quality:" + tvq.quality.ToString();
            str += "   (" + ReadableQuality(tvq.quality) + ")";
            return str;
        }


        private void writingDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            PostMsg("Writing Data  Example Started");

            if (ConnectToWebService("localhost") == false)
                return;

            // Get the First Historian Name in List
            string[] histServerList = client.GetServerList(cci);
            if (histServerList.Length == 0)
                return;
            myHistServer = histServerList[0];

            myHistServer = "LocalHost"; // Force it to be local

            WritingOperations wo = new WritingOperations(this);
            wo.TagWriteOperations();

            //DisconnectFromWebService();   // Handled during Form Closing
            PostMsg("Writing Data Example Finished");
        }

        private void liveDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();

            bool liveStream = false;
            ToolStripItem mi = (ToolStripItem)sender;
            if (mi.Text.Contains("Stream"))
            {
                liveStream = true;
                PostMsg("Live Stream - Example Started");
            }
            else
            {
                liveStream = false;
                PostMsg("Live Data - Example Started");
            }

            if (ConnectToWebService("localhost") == false)
                return;

            // Get the First Historian Name in List
            string[] histServerList = client.GetServerList(cci);
            if (histServerList.Length == 0)
                return;
            myHistServer = histServerList[0];
            foreach (string server in histServerList)
            {
                if (server.ToLower() == Environment.MachineName.ToLower())
                {
                    myHistServer = server;
                    break;
                }
            }
            if (histServerList.Contains("localHost"))
                myHistServer = "localHost";


            // Get a List of DataSet on the specified historian
            string[] dataSetList = client.GetDataSetList(myHistServer, false, cci);
            string msg = "GetDataSetList from " + myHistServer + ":  ";
            if (dataSetList == null)
            {
                PostMsg("Unable to connect to the historian '" + myHistServer + "' through the web Service");
                return;
            }
            msg += dataSetList.Length.ToString() + " datasets returned";
            PostMsg(msg);
            if (dataSetList.Length == 0)
                return;

            // Try to find the "Sample Data" in the dataSet List
            //  If not found select the first dataset available..
            myDataSet = dataSetList[0];
            for (int i = 0; i < dataSetList.Length; i++)
            {
                if (dataSetList[i] == "{Diagnostics}")
                    myDataSet = dataSetList[i];
            }
            //myDataSet = "DemoServer$Wastewater";   //  Special Debug Case....

            PostMsg("Selected DataSet: " + myDataSet);

            List<string> dsTagList = ReadTagList(myHistServer, myDataSet);
            myTagNameList = new List<string>();
            foreach(string tagName in dsTagList)
            {
                //if (tagName.Contains("Bsn1"))
                myTagNameList.Add(tagName);
                if (myTagNameList.Count == 20)  // Select first 10 tags
                    break;
            }

            liveTags = new List<string>();
            for (int i = 0; i < myTagNameList.Count; i++)
            {
                if (i < 5)   // On First 5 Tags - GetVirtualTimeExtensions
                {
                    // Decorate the TagName
                    liveTags.Add("..." + myDataSet + "." + myTagNameList[i]);
                }
                else
                    liveTags.Add(myDataSet + "." + myTagNameList[i]);

            }

            // Add in a Bad TagName for testing purposes
            //liveTags.Add(myDataSet + "." + "BADTAGNAME");
            //liveTags.Add(myDataSet + "." + "MORETHANONEBADTAG");

            // Initialize the LiveData 
            liveData = new HWS_LiveData(client, myHistServer, cci);
            liveData.StoreStream = liveStream;


            // Load the tags that we need live data from into the class
            liveData.AddTagRange(liveTags.ToArray());

            // This is how fast the LiveData class will retrieve data 
            // through the WebService to the Historian. By default
            // this is set to (1000) 1 second.
            liveData.UpdateFrequency = 500;     // 500 ms update rate
            liveData.StartLiveMode();
            button1.Visible = true;

            resizeColumns = true;

            if (liveStream)
            {
                this.liveModeTimer.Interval = 3000; // Get results every 3 seconds
                PostMsg("LiveData Update Rate: 3 seconds");
            }
            else
            {
                this.liveModeTimer.Interval = 250;  // Get results every 250 milliseconds
                PostMsg("LiveData Update Rate: 250 milliseconds");
            }
            liveModeTimer.Enabled = true;  
        }


        // Update the LiveMode data on the screen.
        private void liveModeTimer_Tick(object sender, EventArgs e)
        {
            if (liveData == null)
                return;

            label2.Text = "Live Data updated at " + DateTime.Now.ToString("HH:mm:ss.fff");
            if (liveData.StoreStream == false)
            {
                TVQ[] tvqs = liveData.GetLiveData(liveTags.ToArray());
                if (liveData.LastErrorMessage != "")
                {
                    PostMsg("GetLiveData Error:" + liveData.LastErrorMessage);
                    listView1.Items.Clear();
                    liveData.LastErrorMessage = ""; // Reset Error
                }

                // The TVQs returned parallel the tags that were requested.
                listView1.Items.Clear();
                int index = 0;
                foreach (var tvq in tvqs)
                {
                    // Load the value into the ListView
                    string itemId = liveTags[index++];
                    if (itemId.StartsWith("..."))
                        itemId = itemId.Remove(0, 3);    // Remove tag decoration...
                    ListViewItem lvi = listView1.Items.Add(itemId);
                    string value = "null";
                    if (tvq.value != null)
                        value = tvq.value.ToString();
                    lvi.SubItems.Add(value);
                    lvi.SubItems.Add(tvq.timeStamp.ToString("HH:mm:ss.fff"));
                    lvi.SubItems.Add("0x" + tvq.quality.ToString("X"));
                }
            }
            else
            {
                // LiveData Stream data
                List<TagTVQs> tvqtvqs = liveData.GetLiveStream(liveTags.ToArray());
                if (liveData.LastErrorMessage != "")
                {
                    PostMsg("GetLiveStream Error:" + liveData.LastErrorMessage);
                    listView1.Items.Clear();
                    liveData.LastErrorMessage = "";  // Reset Error
                }

                listView1.Items.Clear();
                foreach (TagTVQs tagtvq in tvqtvqs)
                {
                    // Load the values into the ListView
                    if (tagtvq.tvqs != null)
                    {
                        foreach (TVQ tvq in tagtvq.tvqs)
                        {
                            string itemId = tagtvq.tagName;
                            if (itemId.StartsWith("..."))
                                itemId = itemId.Remove(0, 3);    // Remove tag decoration...
                            ListViewItem lvi = listView1.Items.Add(itemId);
                            lvi.SubItems.Add(tvq.value.ToString());
                            lvi.SubItems.Add(tvq.timeStamp.ToString("HH:mm:ss.fff"));
                            lvi.SubItems.Add("0x" + tvq.quality.ToString("X"));
                        }
                    }
                }
            }
            
            // Only do the resize of the columns the first time
            if (resizeColumns)
            {
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                resizeColumns = false;
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            liveModeTimer.Enabled = false;
            if (liveData != null)
            {
                liveData.StopLiveMode();
                liveData.Dispose();
            }
            liveData = null;

            // Make sure we have released the license...
            DisconnectFromWebService();
        }

        private void discoverWebServicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            PostMsg("Discovered EndPoints");
            foreach (var ep in discover.DiscoveredEndPoints)
            {
                PostMsg(ep);
            }

            PostMsg("\n\n Parsed EndPoints");
            foreach (var ep in discover.DiscoveredEndPointsParsed)
            {
                string msg = ep.ConnectionType.ToString();
                msg += "  " + ep.Host;
                msg += ":" + ep.Port.ToString();
                PostMsg(msg);
            }

            PostMsg("\n\n Discovered Hosts");
            foreach (var ep in discover.DiscoveredHosts)
            {
                PostMsg(ep);
            }
        }

        private void readingPerformanceTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            PostMsg("Reading Performance Tests");
            
            string wsComputer = "localHost";    // WebServiceComputer
            if (ConnectToWebService(wsComputer) == false)
            {
                PostMsg("Can't connect to WebService on " + wsComputer);
                return;
            }

            // Get the First Historian Name in List
            string[] histServerList = client.GetServerList(cci);
            if (histServerList.Length == 0)
            {
                PostMsg(String.Format("No Historians on {0} WebService", wsComputer));
                return;
            }
            foreach (string histComputer in histServerList)
            {
                if (histComputer.ToLower() == wsComputer)
                    myHistServer = histComputer;
                if (histComputer.ToLower() == Environment.MachineName.ToLower())
                    myHistServer = histComputer;
            }
            if (myHistServer == "")
            {
                PostMsg(String.Format("No Historian"));
                return;
            }

            ReadingPerformance rp = new ReadingPerformance(this);
            rp.PerformanceTest1(true);
            //rp.PerformanceTest2();
            rp.PerformanceTest3();

            //DisconnectFromWebService();   // Handled during Form Closing
            PostMsg("Finished");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Stop the LiveData
            liveModeTimer.Enabled = false; 
            liveData.StopLiveMode();
            PostMsg("Live Mode Stopped");
        }

        private void dataExtensionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
           if (liveModeTimer.Enabled)
           {
               liveModeTimer.Enabled = false;  

               liveData.StopLiveMode();
               bool state = dataExtensionCheckBox.Checked;
               client.SetVirtualTimeExtensionFlag(state, cci);
               liveData.StartLiveMode();

               liveModeTimer.Enabled = true;  
           }
        }
    }


    public static class DateTimeUtils
    {
        public static DateTime Truncate(this DateTime date, long resolution)
        {
            return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
        }

        public static DateTime SafeFromFileTime(long ts)
        {
            try
            {
                //if (ts > DateTime.MaxValue.Ticks)
                //    return DateTime.MaxValue;   // Try not to invoke a Exception
                return DateTime.FromFileTime(ts);
            }
            catch
            {   // ts probably exceeds 12-31-9999
                return DateTime.MaxValue;
            }
        }
    }

    //
    //  A small class to help with the TVQ management
    //
    public class MyTVQ
    {
        public DateTime timestamp;
        public DateTime validUpTo;
        public object value;
        public ushort quality;

        public MyTVQ(HWSTagData rawData, int index)
        {
            if (index >= rawData.value.Length)
                return; // index out of bounds

            timestamp = rawData.timeStamp[index];
            validUpTo = rawData.timeStamp[index + 1];
            // Convert a string and dataType back to the original object format
            if (rawData.value[index] != "Empty")
                value = (object)Convert.ChangeType(rawData.value[index], Type.GetType("System." + rawData.dataType[index]));
            quality = rawData.quality[index];
        }


        public MyTVQ(HWSTagProcessedData processedData, int index)
        {
            if (index >= processedData.quality.Length)
                return; // index out of bounds

            timestamp = processedData.timeStamp[index];
            validUpTo = processedData.timeStamp[index + 1];
            if (processedData.value != null)
                value = processedData.value[index];   // double
            else
                value = processedData.valueS[index]; // string
            quality = processedData.quality[index];
        }
    }


}
