using Canary.Utility.ProtobufSharedTypes;
using Canary.Views.Grpc.Api;
using Canary.Views.Grpc.Common;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace ReadData
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            ResizeWindow();

            string host = "localhost";
            string identityApiToken = ""; // Api token configured in the Identity tile of the Admin
            var client = new ViewsClientProvider(identityApiToken, host);

            try
            {
                // test version
                await GetVersionAsync(client);

                // browse nodes/tags
                string nodePath = await BrowseNodesAsync(client);
                List<string> tags = await BrowseTagsAsync(client, nodePath);

                if (tags != null && tags.Count > 0)
                {
                    DateTime endTime = DateTime.Now;
                    DateTime startTime = endTime.Subtract(TimeSpan.FromHours(1));
                    List<string> tagSubset = tags.Take(5).ToList();

                    // read data
                    await GetCurrentValueAsync(client, tagSubset);
                    await GetRawDataAsync(client, tagSubset, startTime, endTime);
                    await GetAvailableAggregatesAsync(client);
                    await GetAggregateDataAsync(client, tagSubset, startTime, endTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                await client.ReleaseCciAsync();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void ResizeWindow()
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

        private static async Task GetVersionAsync(ViewsClientProvider client)
        {
            GetWebServiceVersionResponse response = await client.MakeRequestAsync(async (client, cci) => {
                return await client.GetWebServiceVersionAsync(new Empty());
            });

            if (HasBadStatus(response.Status, nameof(GetVersionAsync), out string? errorMessage))
                Console.WriteLine($"Get Version Error: {errorMessage}");
            else
                Console.WriteLine($"Views Version: {response.Version}");
        }

        private static async Task<string> BrowseNodesAsync(ViewsClientProvider client)
        {
            Console.WriteLine();
            Console.WriteLine("Browse Nodes...");

            string nodePath = "ROOT"; // base path
            while (true)
            {
                BrowseResponse response = await client.MakeRequestAsync(async (client, cci) => {
                    return await client.BrowseAsync(new BrowseRequest()
                    {
                        NodeIdPath = nodePath,
                        ForceReload = false
                    });
                });

                if (HasBadStatus(response.Status, nameof(BrowseNodesAsync), out string? errorMessage))
                    Console.WriteLine($"Browse Nodes Error: {errorMessage}");
                if (response.Node.Children.Count == 0)
                    Console.WriteLine($"\"{nodePath}\" has 0 child nodes...");
                else
                {
                    // browse deeper into first child node
                    var firstNode = response.Node.Children[0];
                    Console.WriteLine($"\"{nodePath}\" path has {response.Node.Children.Count} child nodes...");
                    nodePath = firstNode.IdPath;
                    continue;
                }

                break;
            }

            return nodePath;
        }

        private static async Task<List<string>> BrowseTagsAsync(ViewsClientProvider client, string nodePath)
        {
            Console.WriteLine();
            Console.WriteLine("Browse All Tags...");

            List<string> tags = new List<string>();
            string search = "";
            while (true)
            {
                int maxCount = 1000;
                bool includeSubNodes = true;
                bool includeProperties = false;
                BrowseTagsResponse response = await client.MakeRequestAsync(async (client, cci) => {
                    return await client.BrowseTagsAsync(new BrowseTagsRequest()
                    {
                        NodeIdBrowse = nodePath,
                        IncludeSubNodes = includeSubNodes,
                        IncludeProperties = includeProperties,
                        MaxCount = maxCount,
                        SearchContext = search,
                    });
                });

                if (HasBadStatus(response.Status, nameof(BrowseTagsAsync), out string? errorMessage))
                    Console.WriteLine($"Browse Tags Error: {errorMessage}");
                else
                {
                    Console.WriteLine($"Retrieved {response.TagNames.Count} tags from path \"{nodePath}\"...");

                    tags.AddRange(response.TagNames.Select((partialTagName) =>
                    {
                        // build full tag path
                        return $"{nodePath}.{partialTagName}";
                    }));

                    if (response.MoreDataAvailable)
                    {
                        // use search context as continuation point to get more tags
                        search = response.SearchContext;
                        continue;
                    }
                }

                break;
            }

            return tags;
        }

        private static async Task GetCurrentValueAsync(ViewsClientProvider client, List<string> tags)
        {
            Console.WriteLine();
            Console.WriteLine("Getting Current Values...");

            foreach (string fullTag in tags)
            {
                string[] split = fullTag.Split('.');
                string view = split[0];
                string partialTag = string.Join(".", split.Skip(1));

                GetTagCurrentValueResponse response = await client.MakeRequestAsync(async (client, cci) => {
                    return await client.GetTagCurrentValueAsync(new GetTagCurrentValueRequest()
                    {
                        View = view,
                        TagNames = { partialTag },
                        UseTimeExtension = true,
                        Cci = cci
                    });
                });

                if (HasBadStatus(response.Status, nameof(GetCurrentValueAsync), out string? errorMessage))
                    Console.WriteLine($"Get Current Value Error: {errorMessage}");
                else if (response.Status.StatusType == ApiCallStatusType.CheckExtendedStatus)
                    Console.WriteLine($"Get Current Value Error. Status: {response.ExtendedStatus}, Error: {response.Status.StatusErrorMessage}");
                else
                {
                    TagCurrentValue result = response.TagValues[0];
                    Console.WriteLine($"\"{fullTag}\" =>");
                    Console.WriteLine($"\tTime = {result.Timestamp}");
                    Console.WriteLine($"\tValue = {result.Value}");
                    Console.WriteLine($"\tQuality = {result.Quality}");
                }
            }
        }

        private static async Task GetRawDataAsync(ViewsClientProvider client, List<string> tags, DateTime startTime, DateTime endTime)
        {
            Console.WriteLine();
            Console.WriteLine("Getting Raw Data...");

            foreach (string fullTag in tags)
            {
                Console.WriteLine($"\"{fullTag}\" =>");

                string[] split = fullTag.Split('.');
                string view = split[0];
                string partialTag = string.Join(".", split.Skip(1));

                byte[]? continuationPoint = null;
                while (true)
                {
                    RawTagRequest[] requests =
                    [
                        new()
                        {
                            TagName = partialTag,
                            StartTime = startTime.ToGrpcTimestamp(),
                            EndTime = endTime.ToGrpcTimestamp(),
                            ContinuationPoint = continuationPoint != null ? ByteString.CopyFrom(continuationPoint) : ByteString.Empty
                        }
                    ];

                    GetRawDataResponse response = await client.MakeRequestAsync(async (client, cci) => {
                        return await client.GetRawDataAsync(new GetRawDataRequest()
                        {
                            Requests = { requests },
                            View = view,
                            MaxCountPerTag = 1000,
                            ReturnAnnotations = false,
                            ReturnBounds = true,
                            Cci = cci
                        });
                    });

                    if (HasBadStatus(response.Status, nameof(GetRawDataAsync), out string? errorMessage))
                        Console.WriteLine($"\tGet Raw Data Error: {errorMessage}");
                    else if (response.Status.StatusType == ApiCallStatusType.CheckExtendedStatus)
                        Console.WriteLine($"\tGet Raw Data Error. Status: {response.ExtendedStatus}, Error: {response.Status.StatusErrorMessage}");
                    else
                    {
                        var result = response.RawData[0];
                        Console.WriteLine($"\tRetrieved {result.Tvqs.Count} raw values...");
                        if (result.ContinuationPoint != null && result.ContinuationPoint.Length > 0)
                        {
                            // get more data
                            continuationPoint = result.ContinuationPoint.ToByteArray();
                            continue;
                        }
                    }

                    break;
                }
            }
        }

        private static async Task GetAvailableAggregatesAsync(ViewsClientProvider client)
        {
            Console.WriteLine();
            Console.WriteLine("Getting available aggregates for processed data...");

            GetAggregateListResponse response = await client.MakeRequestAsync(async (client, cci) => {
                return await client.GetAggregateListAsync(new Empty());
            });

            if (HasBadStatus(response.Status, nameof(GetAvailableAggregatesAsync), out string? errorMessage))
                Console.WriteLine($"Get Available Aggregates Error: {errorMessage}");
            else
            {
                foreach (AggregateDefinition aggregate in response.Aggregates)
                    Console.WriteLine($"{aggregate.AggregateName} => {aggregate.AggregateDescription}");
            }
        }

        private static async Task GetAggregateDataAsync(ViewsClientProvider client, List<string> tags, DateTime startTime, DateTime endTime)
        {
            Console.WriteLine();
            Console.WriteLine("Getting Processed Data...");

            TimeSpan aggregateInterval = TimeSpan.FromMinutes(1);
            foreach (string fullTag in tags)
            {
                Console.WriteLine($"\"{fullTag}\" =>");

                string[] split = fullTag.Split('.');
                string view = split[0];
                string partialTag = string.Join(".", split.Skip(1));

                AggregateTagRequest[] requests =
                {
                    new()
                    {
                        AggregateName = "TimeAverage2",
                        TagName = partialTag
                    }
                };

                GetAggregateDataResponse response = await client.MakeRequestAsync(async (client, cci) => {
                    return await client.GetAggregateDataAsync(new GetAggregateDataRequest()
                    {
                        View = view,
                        Requests = { requests },
                        StartTime = startTime.ToGrpcTimestamp(),
                        EndTime = endTime.ToGrpcTimestamp(),
                        Interval = aggregateInterval.ToDuration(),
                        ReturnAnnotations = false,
                        Cci = cci
                    });
                });

                if (HasBadStatus(response.Status, nameof(GetAggregateDataAsync), out string? errorMessage))
                    Console.Write($"Get Aggregate Data Error: {errorMessage}");
                else if (response.Status.StatusType == ApiCallStatusType.CheckExtendedStatus)
                    Console.WriteLine($"Get Aggregate Data Error. Status {response.ExtendedStatus}, Error: {response.Status.StatusErrorMessage}");
                else
                    Console.WriteLine($"\tRetrieved {response.AggregatedData[0].Tvqs.Count} processed values...");
            }
        }

        private static bool HasBadStatus(ApiCallStatus requestStatus, string method, [NotNullWhen(true)] out string? errorMessage)
        {
            errorMessage = "";
            switch (requestStatus.StatusType)
            {
                case ApiCallStatusType.Success:
                case ApiCallStatusType.CheckExtendedStatus:
                    return false;

                case ApiCallStatusType.NoLicense:
                default:
                    errorMessage = $"Error making Views request '{method}'. Status code '{requestStatus.StatusType}'.{(!string.IsNullOrEmpty(errorMessage) ? $" Error: {errorMessage}" : string.Empty)}";
                    return true;
            };
        }
    }
}