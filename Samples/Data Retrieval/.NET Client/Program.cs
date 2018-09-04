using CanaryWebServiceHelper;
using CanaryWebServiceHelper.HistorianWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadData
{
    class Program
    {
        static void Main(string[] args)
        {
            ResizeWindow();

            string host = "localhost";
            HWS_ConnectionType connectionType = HWS_ConnectionType.NetTcp_Windows;
            using (ConnectionWrapper connection = new ConnectionWrapper(host, connectionType)
            {
                //// use credentials when connecting to username endpoint
                //Username = "name",
                //Password = "secret"
            })
            {
                try
                {
                    // handles reconnect logic
                    connection.Run((client, cci) =>
                    {
                        // test version
                        Version(client);

                        // browse nodes/tags
                        BrowseNodes(client, out string nodePath);
                        BrowseTags(client, nodePath, out List<string> tags);

                        if (tags != null && tags.Count > 0)
                        {
                            DateTime endTime = DateTime.Now;
                            DateTime startTime = endTime.Subtract(TimeSpan.FromHours(1));
                            List<string> tagSubset = tags.Take(5).ToList();

                            // read data
                            GetCurrentValue(client, cci, tagSubset);
                            GetRawData(client, cci, tagSubset, startTime, endTime);
                            GetAvailableAggregates(client);
                            GetProcessedData(client, cci, tagSubset, startTime, endTime);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static private void ResizeWindow()
        {
            try
            {
                // width in characters
                Console.WindowWidth = 150;
                Console.WindowHeight = 50;
            }
            catch
            {
                // ignore error
            }
        }

        static private void Version(HistorianWebServiceClient client)
        {
            string version = client.WebServiceVersion();
            Console.WriteLine($"Version: {version}");
        }

        static private void BrowseNodes(HistorianWebServiceClient client, out string nodePath)
        {
            Console.WriteLine();
            Console.WriteLine("Browse Nodes...");

            nodePath = "ROOT"; // base path
            while (true)
            {
                HWSBrowseNode result = client.Browse(nodePath, false);
                if (result != null)
                {
                    if (!string.IsNullOrEmpty(result.errMsg))
                        Console.WriteLine($"Browse Nodes Error: {result.errMsg}");
                    if (result.children == null || result.children.Length == 0)
                        Console.WriteLine($"\"{nodePath}\" has 0 child nodes...");
                    else
                    {
                        // browse deeper into first child node
                        HWSBrowseInfo firstNode = result.children[0];
                        Console.WriteLine($"\"{nodePath}\" path has {result.children.Length} child nodes...");
                        nodePath = firstNode.idPath;
                        continue;
                    }
                }

                break;
            }
        }

        static private void BrowseTags(HistorianWebServiceClient client, string nodePath, out List<string> tags)
        {
            Console.WriteLine();
            Console.WriteLine("Browse All Tags...");

            tags = new List<string>();
            string search = "";
            while (true)
            {
                int maxCount = 1000;
                bool includeSubNodes = true;
                bool includeProperties = false;
                HWSBrowseTags result = client.BrowseTags(nodePath, search, maxCount, includeSubNodes, includeProperties);
                if (result != null)
                {
                    if (!string.IsNullOrEmpty(result.errMsg))
                        Console.WriteLine($"Browse Tags Error: {result.errMsg}");
                    else if (result.tagNames != null)
                    {
                        Console.WriteLine($"Retrieved {result.tagNames.Length} tags from path \"{nodePath}\"...");

                        tags.AddRange(result.tagNames.Select((partialTagName) =>
                        {
                            // build full tag path
                            return $"{nodePath}.{partialTagName}";
                        }));

                        if (result.moreDataAvailable)
                        {
                            // use search context as continuation point to get more tags
                            search = result.searchContext;
                            continue;
                        }
                    }
                }

                break;
            }
        }

        static private void GetCurrentValue(HistorianWebServiceClient client, int cci, List<string> tags)
        {
            Console.WriteLine();
            Console.WriteLine("Getting Current Values...");

            foreach (string fullTag in tags)
            {
                string[] split = fullTag.Split('.');
                string historian = split[0];
                string dataset = null;
                string partialTag = string.Join(".", split.Skip(1));

                HWSTagCurrentValue result = client.GetTagCurrentValue(historian, dataset, new string[] { partialTag }, cci)[0];
                if (result != null)
                {
                    Console.WriteLine($"\"{fullTag}\" =>");
                    Console.WriteLine($"\tTime = {result.timeStamp}");
                    Console.WriteLine($"\tValue = {result.value}");
                    Console.WriteLine($"\tQuality = {client.GetReadableQuality(result.quality)}");
                }
            }
        }

        static private void GetRawData(HistorianWebServiceClient client, int cci, List<string> tags, DateTime startTime, DateTime endTime)
        {
            Console.WriteLine();
            Console.WriteLine("Getting Raw Data...");

            foreach (string fullTag in tags)
            {
                Console.WriteLine($"\"{fullTag}\" =>");

                string[] split = fullTag.Split('.');
                string historian = split[0];
                string partialTag = string.Join(".", split.Skip(1));

                byte[] continuationPoint = null;
                while (true)
                {
                    HWSTagRequest3[] request = new HWSTagRequest3[]
                    {
                        new HWSTagRequest3()
                        {
                            endTime = endTime,
                            startTime = startTime,
                            tagName = partialTag,
                            continuationPoint = continuationPoint
                        }
                    };

                    int maxCount = 1000;
                    bool returnBounds = true;
                    bool returnAnnotation = false;
                    HWSTagData3 result = client.GetRawTagData3(historian, request, maxCount, returnBounds, returnAnnotation, cci)[0];
                    if (result != null)
                    {
                        if (!string.IsNullOrEmpty(result.errMsg))
                            Console.WriteLine($"\tGet Raw Data Error: {result.errMsg}");
                        else if (result.tvqs != null)
                        {
                            Console.WriteLine($"\tRetrieved {result.tvqs.Length} raw values...");
                            if (result.continuationPoint != null && result.continuationPoint.Length > 0)
                            {
                                // get more data
                                continuationPoint = result.continuationPoint;
                                continue;
                            }
                        }
                    }

                    break;
                }
            }
        }

        static private void GetAvailableAggregates(HistorianWebServiceClient client)
        {
            Console.WriteLine();
            Console.WriteLine("Getting available aggregates for processed data...");

            HWSAggregateDefinition[] aggregates = client.GetAggregateList2();
            if (aggregates != null)
            {
                foreach (HWSAggregateDefinition aggregate in aggregates)
                    Console.WriteLine($"{aggregate.aggregateName} => {aggregate.aggregateDescription}");
            }
        }

        static private void GetProcessedData(HistorianWebServiceClient client, int cci, List<string> tags, DateTime startTime, DateTime endTime)
        {
            Console.WriteLine();
            Console.WriteLine("Getting Processed Data...");

            TimeSpan aggregateInterval = TimeSpan.FromMinutes(1);
            foreach (string fullTag in tags)
            {
                Console.WriteLine($"\"{fullTag}\" =>");

                string[] split = fullTag.Split('.');
                string historian = split[0];
                string partialTag = string.Join(".", split.Skip(1));

                HWSTagProcessedRequest2[] request = new HWSTagProcessedRequest2[]
                {
                    new HWSTagProcessedRequest2()
                    {
                        aggregateId = HWSAggregate.TimeAverage2,
                        tagName = partialTag
                    }
                };

                bool returnAnnotations = false;
                HWSTagProcessedData2 result = client.GetProcessedTagData2(historian, request, startTime, endTime, aggregateInterval, returnAnnotations, cci)[0];
                if (result != null)
                {
                    if (!string.IsNullOrEmpty(result.errMsg))
                        Console.WriteLine($"\tGet Processed Data Error: {result.errMsg}");
                    else if (result.tvqs != null)
                        Console.WriteLine($"\tRetrieved {result.tvqs.Length} processed values...");
                }
            }
        }
    }
}
