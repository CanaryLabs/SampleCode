using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CanaryWebServiceHelper.HistorianWebService;
using System.Diagnostics;
using CanaryWebServiceHelper;

namespace HWS_API_Example
{
    class ReadingPerformance
    {
       Form1 parent;

       string myHistServer;
       string myDataSet;
       List<string> myTagNameList = new List<string>();
       DateTime oldestTS;
       DateTime latestTS;


        public ReadingPerformance(Form1 parent)
        {
            this.parent = parent;

            myHistServer = parent.myHistServer;
            myDataSet = parent.myDataSet;
        }


        // Initialize the parameters needed to run the performance test
        private bool Initialization()
        {
            // Make this a Method in the parent....
            string[] dataSetList = parent.GetDataSetList(myHistServer);
            if (dataSetList.Length == 0)
                return false;
            // Try to find the "Sample Data" or "BenchmarkRead" in the dataSet List
            //  If not found select the first dataset available..
            myDataSet = dataSetList[0];
            for (int i = 0; i < dataSetList.Length; i++)
            {
                if (dataSetList[i] == "Sample Data")
                    myDataSet = dataSetList[i];
                if (dataSetList[i] == "BenchmarkRead")
                {
                    myDataSet = dataSetList[i];
                    break;
                }
            }
            parent.PostMsg("Selected DataSet: " + myDataSet);
            myTagNameList = parent.ReadTagList(myHistServer, myDataSet);

            HWSTagContext[] tagContext = parent.client.GetTagDataContext(myHistServer, myDataSet, myTagNameList.ToArray(), parent.cci);
            if (tagContext.Length == 0)
                return false;

            // Extract the range information from the first tag
            HWSTagContext context = tagContext[0];
            oldestTS = context.oldestTimeStamp;
            latestTS = context.latestTimeStamp;
            return true;
        }




        // This method shows how to use the GetRawData2 method
        //  To get data for multiple tags in a single call..
        //  and to get lots of data values... (Reading all available data...)
        public bool  PerformanceTest1(bool useCustomMashaling)
        {
            Initialization();
            Stopwatch overall = new Stopwatch();

            parent.PostMsg("Performance Test #1 -  Using GetRawTagData2  -- Reading all the data for 5 tags");

            double speed = HWS_ConnectionHelper.CommunicationSpeedCheck(parent.client);
            parent.PostMsg("Communication Speed to HWS: " + speed.ToString("###,###") +" bps"); // 

            if (myTagNameList.Count == 0)
            {
                parent.PostMsg("No Tags in the List");
                return false;
            }

            HWS_FasterRawData frd = null;
            if (useCustomMashaling)
            {
                frd = new HWS_FasterRawData(parent.client);
                frd.EnableZip = parent.checkBox1.Checked;
            }
            List<HWSTagData2Consolidated> consolidatedResults = null;

            int tagCount = 5;
            if (myTagNameList.Count < 5)
                tagCount = myTagNameList.Count;
            List<HWSTagRequest> requests = new List<HWSTagRequest>();

            
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
                overall.Start();

                HWSTagData2[] results;
                if (useCustomMashaling)
                {
                    // Maximum TVQs returned per call is limited to 100,000
                    results = frd.GetRawTagData2(myHistServer, myDataSet, requests.ToArray(), 100000, parent.cci);
                }
                else
                {
                    // Maximum TVQs returned per call is limited to 10,000
                    results = parent.client.GetRawTagData2(myHistServer, myDataSet, requests.ToArray(), 10000, parent.cci);

                }
                overall.Stop();

                // Process the results...
                List<HWSTagRequest> nextRequests = new List<HWSTagRequest>();

                if (consolidatedResults == null)
                {   // FirstTime Initialization
                    consolidatedResults = new List<HWSTagData2Consolidated>();
                    foreach (HWSTagData2 result in results)
                         consolidatedResults.Add(new HWSTagData2Consolidated(result));
                }

                for (int i = 0; i < results.Length; i++)
                {
                    // We use the client data value - as a index
                    //   to associate the return data back to the correct tag..
                    int index = results[i].clientData;
                    if (results[i].value != null)
                        consolidatedResults[index].Consolidate(results[i]);

                    if (results[i].resultFlags == 1)
                    {
                        // More Data is available...
                        HWSTagRequest aRequest = requests[i];
                        // new request startTime is where the last one left off...
                        aRequest.startTime = results[i].upToTime;

                        nextRequests.Add(aRequest);
                    }
                }

                // Provide user feedback every 5 seconds...
                if (reportAt < DateTime.Now)
                {
                    parent.PostMsg("GetRawTagData2 for " + requests.Count.ToString() + " tags   # Calls: " + callCount.ToString());
                    reportAt = DateTime.Now.AddSeconds(5);
                }

                // replace the old 'requests' with the 'nextRequests'
                requests = nextRequests;
            }

            int totalTVQs = 0;
            string msg = "All Available Data was read: ";
            msg += "   Total number of WebService calls: " + callCount.ToString();
            for (int i = 0; i < tagCount; i++)
            {
                int tagTvqCnt = consolidatedResults[i].value.Count;
                totalTVQs += tagTvqCnt;
                msg += "\n\t\t " + myTagNameList[i] + "   TVQ Cnt: " + tagTvqCnt.ToString("#,###,###");
            }
            msg += "\n\tTotal number of TVQs: " + totalTVQs.ToString("#,###,###");
            double totalSeconds  = overall.ElapsedMilliseconds/1000.0;
            double tvqsPerSecond = totalTVQs / totalSeconds;
                
            msg += "\n\tTotal WebService Time: " + totalSeconds.ToString("##.### seconds");
            msg += "   TVQs/sec: " + tvqsPerSecond.ToString("#,###");
            parent.PostMsg(msg);
            return true;
        }




        //  Using GetRawTagData instead of GetRawTagData2
        public bool PerformanceTest2()
        {
            Initialization();
            Stopwatch overall = new Stopwatch();


            parent.PostMsg("Performance Test #2 -  Using GetRawTagData  -- Reading all the data for 5 tags");

            if (myTagNameList.Count == 0)
            {
                parent.PostMsg("No Tags in the List");
                return false;
            }

            int tagCount = 5;
            if (myTagNameList.Count < 5)
                tagCount = myTagNameList.Count;
            List<HWSTagRequest> requests = new List<HWSTagRequest>();

            // An Array to count the number of TVQs we have read for each tag
            int[] tvqCounter = new int[5];
            int callCount = 0;
            DateTime reportAt = DateTime.Now;

            for (int i = 0; i < tagCount; i++)
            {
                DateTime startTime = oldestTS;
                DateTime endTime = latestTS;

                bool done = false;
                while (!done)
                {
                    callCount++;
                    overall.Start();
                    HWSTagData data = parent.client.GetRawTagData(myHistServer, myDataSet, myTagNameList[i], startTime, endTime, 10000, parent.cci);
                    overall.Stop();

                    tvqCounter[i] += data.value.Length;
                    if (data.resultFlags == 0)
                        done = true;
                    else
                        startTime = data.timeStamp.Last();

                    // Provide user feedback every 5 seconds...
                    if (reportAt < DateTime.Now)
                    {
                        parent.PostMsg("GetRawTagData  # Calls: " + callCount.ToString());
                        reportAt = DateTime.Now.AddSeconds(5);
                    }
                }
            }
          
            int totalTVQs = 0;
            string msg = "All Available Data was read: ";
            msg += "   Total number of WebService calls: " + callCount.ToString();
            for (int i = 0; i < tagCount; i++)
            {
                totalTVQs += tvqCounter[i];
                msg += "\n\t\t " + myTagNameList[i] + "   TVQ Cnt: " + tvqCounter[i].ToString("#,###,###");
            }
            msg += "\n\tTotal number of TVQs: " + totalTVQs.ToString("#,###,###");
            double totalSeconds = overall.ElapsedMilliseconds / 1000.0;
            double tvqsPerSecond = totalTVQs / totalSeconds;

            msg += "\n\tTotal WebService Time: " + totalSeconds.ToString("##.### seconds");
            msg += "   TVQs/sec: " + tvqsPerSecond.ToString("#,###");
            parent.PostMsg(msg);
            return true;
        }


        // Expecting Data in "BenchmarkRead" dataset
        public bool PerformanceTest3()
        {
            Stopwatch overall = new Stopwatch();

            parent.PostMsg("");
            parent.PostMsg("Performance Test #3 -  Using GetRawTagData  -- Reading all the data for 5 tags");

            if (myTagNameList.Count == 0)
            {
                parent.PostMsg("No Tags in the List");
                return false;
            }

            // Just Do 5 Tags
            if (myTagNameList.Count > 5)
                myTagNameList.RemoveRange(5, myTagNameList.Count - 5);

            List<HWSTagRequest> requests = new List<HWSTagRequest>();
            // An Array to count the number of TVQs we have read for each tag
            int[] tvqCounter = new int[5];
            DateTime reportAt = DateTime.Now;
            int totalTVQs = 0;

            foreach (string tagName in myTagNameList)
            {
                DateTime startTime = oldestTS;
                DateTime endTime = latestTS;
                overall.Start();
                HWSTagProcessedData data = parent.client.GetProcessedTagData(myHistServer, myDataSet, tagName, startTime, endTime, 10000, TimeSpan.FromMinutes(1), "TimeAverage2", parent.cci);
                overall.Stop();

                totalTVQs += data.value.Length;

                // Provide user feedback every 5 seconds...
                if (reportAt < DateTime.Now)
                {
                    parent.PostMsg("GetProcessedTagData: " + totalTVQs.ToString());
                    reportAt = DateTime.Now.AddSeconds(5);
                }
            }

            string msg = "All Available Data was read: ";
            msg += "\n\tTotal number of TVQs: " + totalTVQs.ToString("#,###,###");
            double totalSeconds = overall.ElapsedMilliseconds / 1000.0;
            double tvqsPerSecond = totalTVQs / totalSeconds;

            msg += "\n\tTotal WebService Time: " + totalSeconds.ToString("##.### seconds");
            msg += "   TVQs/sec: " + tvqsPerSecond.ToString("#,###");
            parent.PostMsg(msg);
            return true;
        }





    }
}
