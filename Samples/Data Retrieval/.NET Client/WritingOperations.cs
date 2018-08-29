using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CanaryWebServiceHelper.HistorianWebService;

namespace HWS_API_Example
{

    // This class contains code that is related to writing data to the Historian
    // through the Historian Web Service API
    
    // Caution:  The purpose of this API is allow the loading of historical data 
    //    from user inputs (Manual Data Entry) and infrequently changing data.   
    //   This function should not be used as a constant data logging mechanism to the historian!
    //
    //   
    //   The "StoreHistoryData" method was added to the Views service before we published the
    //    API to the StoreAndForward.  Canary "best practices" strongly recommends using the 
    //    StoreAndForward API instead of the "StoreHistoryData"
    //

    class WritingOperations
    {
        Form1 parent;

        public WritingOperations(Form1 parent)
        {
            this.parent = parent;
        }


        //  Write a Block of Data to Canary Historian via the WebService
        public bool TagWriteOperations()
        {
            string dsName = "API";

            // Verify that the DataSet exists
           string[] dataSetList = parent.client.GetDataSetList(parent.myHistServer, false, parent.cci);
            if ((dataSetList == null) || (dataSetList.Contains(dsName) == false))
            {
                parent.PostMsg("Write Operation Aborted... DataSet: '" + dsName + "' does not exist on the historian: " + parent.myHistServer);
                return false;
            }

            List<HWSStoreDataRequest> requests = new List<HWSStoreDataRequest>();
            DateTime aTime = DateTime.Now;
            aTime = aTime.Truncate(TimeSpan.TicksPerMinute);    // Truncate to the minute

            // 20 Tags of Data
            for (int i = 0; i < 20; i++)
            {
                int tNum = i + 1;
                string tName = "TestTag" + String.Format("{0:d2}", tNum);
                requests.Add(GenerateDataforOneTag(dsName, tName, i, aTime));
            }
            HWSStoreDataResult[] results = parent.client.StoreHistoryData(parent.myHistServer, requests.ToArray(), parent.cci);
            if (results != null)
            {
                parent.PostMsg("StoreHistoryData: returned results for " + results.Length.ToString() + " tags.");
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].errMsg != null)
                    {
                        if (results[i].errMsg != "")
                        {
                            int index = results[i].clientData;
                            string msg = "  Tag Name: " + requests[index].tagName;
                            msg += " ClientData: " + results[i].clientData.ToString();
                            msg += "  ErrorMsg: " + results[i].errMsg;
                            parent.PostMsg(msg);
                        }
                    }
                }
            }

            //  Advance the Time --- Rollover and do the data again
            aTime = aTime.AddSeconds(130);   // 10 seconds longer than 120 updates

            //  RollOver the DataSet
            string command = "RollOver," + dsName + "," + aTime.ToString();
            string[] result = parent.client.HistorianCommand(parent.myHistServer, command, parent.cci);


            // Send 100 More TVQs Data to the Historian
            requests.Clear();
            for (int i = 0; i < 20; i++)
            {
                int tNum = i + 1;
                string tName = "TestTag" + String.Format("{0:d2}", tNum);
                requests.Add(GenerateDataforOneTag(dsName, tName, i, aTime));
            }
            results = parent.client.StoreHistoryData(parent.myHistServer, requests.ToArray(), parent.cci);
            if (results != null)
            {
                parent.PostMsg("StoreHistoryData: returned results for " + results.Length.ToString() + " tags.");
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].errMsg != null)
                    {
                        if (results[i].errMsg != "")
                        {
                            int index = results[i].clientData;
                            string msg = "  Tag Name: " + requests[index].tagName;
                            msg += " ClientData: " + results[i].clientData.ToString();
                            msg += "  ErrorMsg: " + results[i].errMsg;
                            parent.PostMsg(msg);
                        }
                    }
                }
            }
            return true;
        }



        private HWSStoreDataRequest GenerateDataforOneTag(string dataSet, string tagName, int clientValue, DateTime aTime)
        {
            HWSStoreDataRequest request = new HWSStoreDataRequest();
            request.dataSet = dataSet;
            request.tagName = tagName;
            request.clientData = clientValue;
            request.createIfTagNotFound = true;
            request.handleOutOfOrderData = true;

            // Define Tag Properties
            MyTagProperties props = new MyTagProperties();
            props.Add("Description", "Feed Rate");
            props.Add("EngUnits", "Lbs");
            props.Add("Low Scale", 0.0f);
            props.Add("High Scale", 100.0f);    // Float value
            props.Add("Low Limit", 0.0);
            props.Add("High Limit", 90.0);  // double value
            props.Add("Sample Interval", 1000);    // 1 second\
            props.ToRequest(request, aTime);  // Place properties into request

            // Generate some Simulation Test Data
            MyData data = new MyData();

            Random random = new Random(clientValue * 5);   // Seed differently to get different data!
            DateTime startTime = aTime;
            float nextValue = (float)(random.NextDouble() * 100.0);
            // Generate 120 data values ....  2 minutes of data
            for (int i = 0; i < 120; i++)
            {
                data.Add(aTime, nextValue, 192);
                float delta = (float)((random.NextDouble() * 4.0) - 2.0);   // up or down by 2
                nextValue += delta;
                aTime = aTime.AddSeconds(1.0);  // next 1 second
            }

            // Generate test data to exercise Out-Of-Order logic
            data.Add(startTime.AddSeconds(-0.5), 111, 192);     // Before the first Time
            data.Add(startTime.AddSeconds(0.5), 222, 192);      // Between the first and second values
            data.Add(startTime.AddSeconds(2.0), 333, 192);      // Replacing the third value
            data.Add(startTime.AddSeconds(120.25), 444, 192);   // After the last value ... so it will be appended not inserted

            data.ToRequest(request);    // Place data into request
            return request;
        }


        public class MyTagProperties
        {
            class MyProperty
            {
                public string propName = "";
                public object propValue = "";
            };
            List<MyProperty> properties = new List<MyProperty>();

            public bool Add(string propName, object propValue)
            {
                MyProperty prop = new MyProperty();
                prop.propName = propName;
                prop.propValue = propValue;
                properties.Add(prop);
                return true;
            }

            public bool ToRequest(HWSStoreDataRequest request, DateTime timeStamp)
            {
                request.commonPropertyTimeStamp = timeStamp;
                int cnt = properties.Count;
                request.propertyName = new string[cnt];
                request.propertyValue = new object[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    request.propertyName[i] = properties[i].propName;
                    request.propertyValue[i] = properties[i].propValue;
                }
                return true;
            }
        }

        public class MyData
        {
            struct MyTVQ
            {
                public DateTime timeStamp;
                public object value;
                public ushort quality;
            };
            List<MyTVQ> tvqs = new List<MyTVQ>();

            public bool Add(DateTime ts, object val, ushort qual)
            {
                MyTVQ tvq = new MyTVQ();
                tvq.timeStamp = ts;
                tvq.value = val;
                tvq.quality = qual;
                tvqs.Add(tvq);
                return true;
            }

            public bool ToRequest(HWSStoreDataRequest request)
            {
                int cnt = tvqs.Count;
                request.timeStamp = new DateTime[cnt];
                request.value = new object[cnt];
                request.quality = new ushort[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    request.timeStamp[i] = tvqs[i].timeStamp;
                    request.value[i] = tvqs[i].value;
                    request.quality[i] = tvqs[i].quality;
                }
                return true;
            }
        }



    }
}
