using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Parquet.Data;
using Parquet;
using CanaryWebServiceHelper;
using System.Diagnostics;
using System.Windows.Forms;
using CanaryWebServiceHelper.HistorianWebService;

namespace ParquetExport
{
    class Program
    {
        // Parquet library examples: https://github.com/aloneguid/parquet-dotnet/blob/master/doc/writing.md

        static HistorianWebServiceClient client = null;
        static int cci = 0;

        // program arguments
        static string viewsHost = "";
        static string viewsConnectionType = "";
        static string viewsUsername = "";
        static string viewsPW = "";
        static string viewsTagSource = "";
        static string viewsTagPathOrFileName = "";
        static string parquetExportDirectory = "";
        static string parquetExportFileName = "";
        static string startTime = "";
        static string endTime = "";
        static string millionsOfRecordsPerFile = "";
        static string rawDataRetrievalIntervalParm = "";
        static string parquetCompression = "";
        static string aggregateName = "";
        static string aggregateInterval = "";

        // variables
        static Dictionary<string, string> supportedAggregates = new Dictionary<string, string>()
        {
            {"TimeAverage2", "Retrieve the time weighted average data over the interval using Simple Bounding Values." },
            {"Interpolative", "At the beginning of each interval, retrieve the calculated value from the data points on either side of the requested timestamp."},
            {"Average", "Retrieve the average value of the data over the interval."},
            {"TimeAverage", "Retrieve the time weighted average data over the interval using Interpolated Bounding Values."},
            {"Total", "Retrieve the total (time integral) of the data over the interval using Interpolated Bounding Values."},
            {"Total2", "Retrieve the total (time integral in seconds) of the data over the interval using Simple Bounding Values."},
            {"TotalPerMinute", "Retrieve the total (time integral in minutes) of the data over the interval using Simple Bounding Values."},
            {"TotalPerHour", "Retrieve the total (time integral in hours) of the data over the interval using Simple Bounding Values."},
            {"TotalPer24Hours", "Retrieve the total (time integral in 24 hours) of the data over the interval using Simple Bounding Values."},
            {"Minimum", "Retrieve the minimum raw value in the interval with the timestamp of the start of the interval."},
            {"Maximum", "Retrieve the maximum raw value in the interval with the timestamp of the start of the interval."},
            {"MinimumActualTime", "Retrieve the minimum value in the interval and the timestamp of the minimum value."},
            {"MaximumActualTime", "Retrieve the maximum value in the interval and the timestamp of the maximum value."},
            {"Range", "Retrieve the difference between the minimum and maximum value over the interval."},
            {"Minimum2", "Retrieve the minimum value in the interval including the Simple Bounding Values."},
            {"Maximum2", "Retrieve the maximum value in the interval including the Simple Bounding Values."},
            {"MinimumActualTime2", "Retrieve the minimum value with the actual timestamp including the Simple Bounding Values."},
            {"MaximumActualTime2", "Retrieve the maximum value with the actual timestamp including the Simple Bounding Values."},
            {"Range2", "Retrieve the difference between the Minimum2 and Maximum2 value over the interval."},
            {"Count", "Retrieve the number of raw values over the interval."},
            {"DurationInStateZero", "Retrieve the time a Boolean or numeric was in a zero state using Simple Bounding Values."},
            {"DurationInStateNonZero", "Retrieve the time a Boolean or numeric was in a non-zero state using Simple Bounding Values."},
            {"NumberOfTransitions", "Retrieve the number of changes between zero and non-zero that a Boolean or numeric value experienced in the interval."},
            {"Start", "Retrieve the first value in the interval."},
            {"End", "Retrieve the last value in the interval."},
            {"Delta", "Retrieve the difference between the Start and End value in the interval."},
            {"StartBound", "Retrieve the value at the beginning of the interval using Simple Bounding Values."},
            {"EndBound", "Retrieve the value at the end of the interval using Simple Bounding Values."},
            {"DeltaBounds", "Retrieve the difference between the StartBound and EndBound value in the interval."},
            {"Instant", "Retrieve the value at the exact beginning of the interval."},
            {"DurationGood", "Retrieve the total duration of time in the interval during which the data is Good."},
            {"DurationBad", "Retrieve the total duration of time in the interval during which the data is Bad."},
            {"PercentGood", "Retrieve the percentage of data (0 to 100) in the interval which has Good StatusCode" },
            {"PercentBad", "Retrieve the percentage of data (0 to 100) in the interval which has Bad StatusCode."},
            {"WorstQuality", "Retrieve the worst StatusCode of data in the interval."},
            {"WorstQuality2", "Retrieve the worst StatusCode of data in the interval including the Simple Bounding Values."},
            {"StandardDeviationSample", "Retrieve the standard deviation for the interval for a sample of the population (n-1)."},
            {"VarianceSample", "Retrieve the variance for the interval as calculated by the StandardDeviationSample."},
            {"StandardDeviationPopulation", "Retrieve the standard deviation for the interval for a complete population (n) which includes Simple Bounding Values."},
            {"VariancePopulation", "Retrieve the variance for the interval as calculated by the StandardDeviationPopulation which includes Simple Bounding Values." }
        };

        static string appName;
        static string userId;
        static DateTime dataStartTime;
        static DateTime dataEndTime;
        static TimeSpan dataAggregateInterval = TimeSpan.Zero;
        static TimeSpan rawDataRetrievalInterval;
        static long numberOfRecordsPerFile;
        static List<string> tags;
        static Dictionary<string, HWSTagContext> tagContexts = new Dictionary<string, HWSTagContext>();
        static bool isInteractive;

        static void Main(string[] args)
        {
            Console.WriteLine("Canary to Parquet Export Utility");
            Console.WriteLine();
            Console.WriteLine("Supported Command Line Arguments:");
            Console.WriteLine();
            Console.WriteLine("{0}: Canary Views host");
            Console.WriteLine("     Ex: localhost or myviewsServer.domain.com or machinename for domain joined machines on an intranet");
            Console.WriteLine("{1}: Canary Views connection type");
            Console.WriteLine("     L = localhost or RU = remote username or RA = remote anonymous or RW = remote use client windows credentials");
            Console.WriteLine("{2}: Canary Views username");
            Console.WriteLine("     Only applies if connection type == RU");
            Console.WriteLine("{3}: Canary Views password");
            Console.WriteLine("     Only applies if connection type == RU");
            Console.WriteLine("{4}: Canary Views tag source type");
            Console.WriteLine("     P = views path or F = tag list file");
            Console.WriteLine("{5}: Canary Views tag path or filename");
            Console.WriteLine("     If == P, expects views path in format 'viewName.path'");
            Console.WriteLine("     If == F, expects text file of tag names. One tag name per line in format 'viewName.path'");
            Console.WriteLine("{6}: Parquet export file directory");
            Console.WriteLine("     Directory path to place 1 or more exported parquet files");
            Console.WriteLine("{7}: Parquet export file name");
            Console.WriteLine("     Start date and time will be appended to this for full file name");
            Console.WriteLine("{8}: Start time");
            Console.WriteLine("     Must be compatible string with DateTime.Parse()");
            Console.WriteLine("     For best results, use ISO 8601 datetime string");
            Console.WriteLine("{9}: End time");
            Console.WriteLine("     Must be compatible string with DateTime.Parse()");
            Console.WriteLine("     For best results, use ISO 8601 datetime string");
            Console.WriteLine("{10}: Millions of records per file");
            Console.WriteLine("     If not sure, enter 100 for max approx 1GB parquet file size");
            Console.WriteLine("{11}: Raw data retrieval interval. Amount of time of data placed in one parquet row group");
            Console.WriteLine("      Only applicable with raw data retrievel (AggregateName == null or empty)");
            Console.WriteLine("      Must be compatible with TimeSpan.Parse()");
            Console.WriteLine("      Ex: days.hours:minutes:seconds.subseconds: 1.00:01:00 = 1 day 1 minute");
            Console.WriteLine("      If not sure, enter 00:01:00 for 1 hour. For 1 second data this will place 3600 records in a row group in the parquet file");
            Console.WriteLine("{12}: Parquet file compression");
            Console.WriteLine("      Options: None, Snappy, Gzip");
            Console.WriteLine("{13}: Aggregate name (optional - if not supplied raw data will be exported");
            Console.WriteLine("      Must be one of the following: ");
            foreach (string key in supportedAggregates.Keys)
                Console.WriteLine($"      {key}");
            Console.WriteLine("{14}: Aggregate interval (optional - if not supplied raw data will be exported)");
            Console.WriteLine("      Must be compatible with TimeSpan.Parse()");
            Console.WriteLine("      Ex: days.hours:minutes:seconds.subseconds: 1.00:01:00 = 1 day 1 minute");
            Console.WriteLine();
            Console.WriteLine();

            if (args.Length > 0)
            {
                Console.WriteLine($"{args.Length} arguments detected...");

                isInteractive = false;

                if (args.Length < 13 || args.Length > 15)
                {
                    Console.WriteLine($"Insufficent arguments supplied. Expecting 13-15 arguments.");
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("Proceeding to export using supplied arguments...");

                    viewsHost = args[0].Trim();
                    viewsConnectionType = args[1].Trim();
                    viewsUsername = args[2].Trim();
                    viewsPW = args[3].Trim();
                    viewsTagSource = args[4].Trim();
                    viewsTagPathOrFileName = args[5].Trim();
                    parquetExportDirectory = args[6].Trim();
                    parquetExportFileName = args[7].Trim();
                    startTime = args[8].Trim();
                    dataStartTime = DateTime.Parse(startTime);
                    endTime = args[9].Trim();
                    dataEndTime = DateTime.Parse(endTime);
                    millionsOfRecordsPerFile = args[10].Trim();
                    numberOfRecordsPerFile = Convert.ToInt64(millionsOfRecordsPerFile) * 1000000;
                    rawDataRetrievalInterval = TimeSpan.Parse(args[11]);
                    parquetCompression = args[12].Trim();

                    if (args.Length >= 14)
                    {
                        aggregateName = args[13].Trim();
                        aggregateInterval = args[14].Trim();
                        dataAggregateInterval = TimeSpan.Parse(aggregateInterval);
                    }
                }
            }
            else
            {
                // prompt for inputs
                isInteractive = true;

                List<string> connectionTypes = new List<string>() { "L", "RU", "RA", "RW" };
                while (!connectionTypes.Contains(viewsConnectionType))
                {
                    Console.Write("Enter views service connection type (L,RU,RA,RW) or press enter for L: ");
                    viewsConnectionType = Console.ReadLine().ToUpper();
                    if (string.IsNullOrWhiteSpace(viewsConnectionType))
                    {
                        viewsConnectionType = "L";
                        viewsHost = "localhost";
                    }
                    else
                    {
                        if (!connectionTypes.Contains(viewsConnectionType))
                            Console.WriteLine($"Invalid entry. Must be one of {string.Join(",", connectionTypes)}");
                    }
                }

                if (viewsConnectionType == "RU" || viewsConnectionType == "RA" || viewsConnectionType == "RW")
                {
                    Console.Write("Enter views service host: ");
                    viewsHost = Console.ReadLine();
                }

                if (viewsConnectionType == "RU")
                {
                    Console.Write("Enter views service connection username: ");
                    viewsUsername = Console.ReadLine();

                    Console.Write("Enter views service connection password: ");
                    ConsoleKey key;
                    do
                    {
                        var keyInfo = Console.ReadKey(intercept: true);
                        key = keyInfo.Key;

                        if (key == ConsoleKey.Backspace && viewsPW.Length > 0)
                        {
                            Console.Write("\b \b");
                            viewsPW = viewsPW.Substring(viewsPW.Length - 1);
                        }
                        else if (!char.IsControl(keyInfo.KeyChar))
                        {
                            Console.Write("*");
                            viewsPW += keyInfo.KeyChar;
                        }
                    } while (key != ConsoleKey.Enter);

                    Console.WriteLine();
                }

                List<string> viewsTagPathTypes = new List<string>() { "P", "F" };
                while (!viewsTagPathTypes.Contains(viewsTagSource))
                {
                    Console.Write("Enter views tag source type (P, F) or press enter for P (views path): ");
                    viewsTagSource = Console.ReadLine().ToUpper();

                    if (string.IsNullOrWhiteSpace(viewsTagSource))
                        viewsTagSource = "P";
                    else
                    {
                        if (!viewsTagPathTypes.Contains(viewsTagSource))
                            Console.WriteLine($"Invalid entry. Must be one of {string.Join(",", viewsTagPathTypes)}");
                    }
                }

                if (viewsTagSource == "P")
                    Console.Write("Enter views tag path or press enter for {Diagnostics}: ");
                else
                    Console.Write("Enter full path to tag file: ");

                viewsTagPathOrFileName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(viewsTagPathOrFileName))
                    viewsTagPathOrFileName = $"{viewsHost}.{{Diagnostics}}";

                if (viewsTagSource == "F" && !File.Exists(viewsTagPathOrFileName))
                {
                    Console.WriteLine("Tag file does not exist! Press enter to exit");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Enter parquet file export directory or press enter for user temp directory: ");
                parquetExportDirectory = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(parquetExportDirectory))
                    parquetExportDirectory = Path.GetTempPath();

                Console.Write("Enter parquet file name or press enter for 'CanaryExport': ");
                parquetExportFileName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(parquetExportFileName))
                    parquetExportFileName = "CanaryExport";

                Console.Write("Enter start time or press enter for start of today: ");
                startTime = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(startTime))
                    startTime = DateTime.Today.ToString("O");

                dataStartTime = DateTime.Parse(startTime);

                Console.Write("Enter end time or press enter for right now: ");
                endTime = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(endTime))
                    endTime = DateTime.Now.ToString("O");

                dataEndTime = DateTime.Parse(endTime);

                Console.Write("Enter # of millions of records per file or press enter for 100M: ");
                millionsOfRecordsPerFile = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(millionsOfRecordsPerFile))
                    numberOfRecordsPerFile = 100000000; // default 100M records per parquet file = ~1GB parquet files
                else
                    numberOfRecordsPerFile = Convert.ToInt64(millionsOfRecordsPerFile) * 1000000;

                List<string> parquetCompressionOptions = new List<string>() { "None", "Snappy", "Gzip" };
                while (!parquetCompressionOptions.Contains(parquetCompression))
                {
                    Console.Write("Enter parquet compression type or press enter for Snappy: ");
                    parquetCompression = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(parquetCompression))
                        parquetCompression = CompressionMethod.Snappy.ToString();
                    else
                    {
                        if (!parquetCompressionOptions.Contains(parquetCompression))
                            Console.WriteLine($"Invalid entry. Must be one of {string.Join(",", parquetCompressionOptions)}");
                    }
                }

                Console.Write("Enter aggregate name or press enter if you want raw data: ");
                aggregateName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(aggregateName))
                {
                    while (!string.IsNullOrWhiteSpace(aggregateName) && !supportedAggregates.Keys.Contains(aggregateName))
                    {
                        Console.WriteLine($"Invalid entry. Must be one of supported aggregate names");

                        Console.Write("Enter aggregate name or press enter for raw data: ");
                        aggregateName = Console.ReadLine();
                    }
                }

                if (string.IsNullOrWhiteSpace(aggregateName))
                {
                    dataAggregateInterval = TimeSpan.FromMinutes(5);

                    Console.Write("Enter raw data retrieval interval or press enter for 1 hour: ");
                    rawDataRetrievalIntervalParm = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(rawDataRetrievalIntervalParm))
                        rawDataRetrievalInterval = TimeSpan.Parse(rawDataRetrievalIntervalParm);
                    else
                        rawDataRetrievalInterval = TimeSpan.FromHours(1);
                }
                else
                {
                    rawDataRetrievalInterval = TimeSpan.FromHours(1);

                    Console.Write("Enter aggregate interval or press enter for 5 minutes: ");
                    aggregateInterval = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(aggregateInterval))
                        dataAggregateInterval = TimeSpan.Parse(aggregateInterval);
                    else
                        dataAggregateInterval = TimeSpan.FromMinutes(5);
                }

                Console.Write("Press enter to start exporting data...");
                Console.ReadLine();
            }

            Console.WriteLine();

            // proceed to connect and export
            if (ConnectToViews())
            {
                Console.WriteLine("Press enter to retrieve tags...");
                if (isInteractive) Console.ReadLine();

                if (viewsTagSource == "P")
                {
                    tags = GetTagListFromViewsPath(viewsTagPathOrFileName);
                }
                else
                    tags = GetTagListFromFile(viewsTagPathOrFileName);

                if (tags.Count > 0)
                {
                    Console.WriteLine("Press enter to retrieve tag contexts from the views service...");
                    if (isInteractive) Console.ReadLine();

                    tagContexts = GetTagContext(tags);

                    if (tagContexts.Count > 0)
                    {
                        Console.WriteLine("Press enter to continue to export data...");
                        if (isInteractive) Console.ReadLine();

                        ExportTagDataToParquet(tagContexts);
                    }
                    else
                        if (!isInteractive) Environment.Exit(1);
                }
            }

            if (cci != 0)
            {
                try
                {
                    // release the context
                    client.ReleaseClientConnectId(appName, userId, cci);
                }
                catch { }
            }

            Console.WriteLine("Press enter to exit");
            if (isInteractive)
                Console.ReadLine();
            else
                Environment.Exit(0);
        }

        static bool ConnectToViews()
        {
            appName = Application.ExecutablePath;
            appName = appName.Substring(appName.LastIndexOf("\\") + 1);
            appName += " (PID=" + Process.GetCurrentProcess().Id.ToString() + ")";
            userId = SystemInformation.ComputerName + ":" + SystemInformation.UserName;

            Console.WriteLine("Connecting to views service...");

            try
            {
                if (viewsConnectionType == "L")
                {
                    HWS_ConnectionHelper.WebServiceConnect(HWS_ConnectionType.NetPipe_Anonymous, "localhost", appName, userId, null, null, out client, out cci);
                }
                else if (viewsConnectionType == "RA")
                {
                    HWS_ConnectionHelper.WebServiceConnect(HWS_ConnectionType.NetTcp_Anonymous, viewsHost, appName, userId, null, null, out client, out cci);
                }
                else if (viewsConnectionType == "RU")
                {
                    HWS_ConnectionHelper.WebServiceConnect(HWS_ConnectionType.NetTcp_Username, viewsHost, appName, userId, viewsUsername, viewsPW, out client, out cci);
                }
                else if (viewsConnectionType == "RW")
                {
                    HWS_ConnectionHelper.WebServiceConnect(HWS_ConnectionType.NetTcp_Windows, viewsHost, appName, userId, null, null, out client, out cci);
                }
            }
            catch
            {
                // Problems Connecting to the Endpoint ...
                client = null;
                cci = 0;

                Console.WriteLine("Failed to connect to views service");
                if (!isInteractive) Environment.Exit(1);
                return false;
            }

            Console.WriteLine("Successfully connected to views service");
            return true;
        }


        public static List<string> GetTagListFromViewsPath(string viewPath)
        {
            List<string> tagList = new List<string>();
            int startingOffset = 0;
            bool done = false;

            Console.WriteLine($"Retrieving tag list from views path '{viewPath}'");

            while (done == false)
            {
                HWSTagList info;

                try
                {
                    string viewName = viewPath.Split('.')[0];
                    string restOfThePath = string.Join(".", viewPath.Split('.').Skip(1));
                    info = client.GetTagList(viewName, restOfThePath, startingOffset, 100000, cci);
                }
                catch
                {
                    if (startingOffset == 0)
                    {
                        Console.WriteLine($"Failed to retrieve tag list from views path '{viewPath}'");
                        Environment.Exit(1);
                    }
                    else
                        Console.WriteLine($"Failed to retrieve tag list from views path '{viewPath}' and starting offset '{startingOffset}'. Returning partial tag list count of {tagList.Count}");

                    return tagList;
                }

                if (info == null)
                    done = true;

                if (info != null)
                {
                    if (info.resultFlags == -1)
                    {
                        // Some error has occurred 
                        return tagList;
                    }

                    for (int i = 0; i < info.tagNames.Length; i++)
                    {
                        if (info.tagType[i] == 0)    // Trend Tags
                            tagList.Add($"{viewPath}.{info.tagNames[i]}");
                    }

                    if (info.resultFlags == 0)
                    {
                        // Complete 
                        done = true;
                    }
                    else
                    {
                        // More data is available so set the startingOffset
                        startingOffset += info.tagNames.Length;
                    }
                }
            }

            Console.WriteLine($"Successfully retrieved '{tagList.Count}' tags from views path '{viewPath}'");
            return tagList;
        }

        public static List<string> GetTagListFromFile(string tagFile)
        {
            List<string> tags = new List<string>();

            if (File.Exists(tagFile))
            {
                using (StreamReader sr = new StreamReader(File.OpenRead(tagFile)))
                {
                    tags.Add(sr.ReadLine().Trim());
                }

                Console.WriteLine($"Successfully retrieved {tags.Count} tags from file '{tagFile}'");
            }
            else
            {
                Console.WriteLine($"Failed to retrieve tag list from file '{tagFile}'. File does not exist");
                if (!isInteractive) Environment.Exit(1);
            }

            return tags;
        }

        public static Dictionary<string, HWSTagContext> GetTagContext(List<string> tags)
        {
            Dictionary<string, HWSTagContext> contexts = new Dictionary<string, HWSTagContext>();
            List<string> tagsWithErrors = new List<string>();

            Console.WriteLine($"Retrieving tag contexts from views service for {tags.Count} tags");

            HWSTagInfo[] tagInfo = null;

            if (tags.Select(t=> t.Split('.')[0]).Distinct().Count() == 1)
            {
                try
                {
                    tagInfo = client.GetTagInfo(tags.First().Split('.')[0], "", tags.Select(t=> string.Join(".", t.Split('.').Skip(1))).ToArray(), cci);
                }
                catch
                {
                    Console.WriteLine($"Unable to retrieve tag info for all tags");
                    return contexts;
                }

                if (tagInfo != null && tagInfo.Length > 0)
                {
                    try
                    {
                        HWSTagContext[] tagContexts = client.GetTagDataContext(tags.First().Split('.')[0], "", tags.Select(t => string.Join(".", t.Split('.').Skip(1))).ToArray(), cci);

                        for (int i = 0; i< tagContexts.Length; i++)
                        {
                            contexts.Add(tags[i], tagContexts[i]);
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"Unable to retrieve tag contexts for all tags. Tags may not exist in views");
                        return contexts;
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to retrieve tag contexts for all tags. Tags may not exist in views");
                    return contexts;
                }
            }
            else
            {
                foreach (string tagName in tags)
                {
                    try
                    {
                        tagInfo = client.GetTagInfo(tagName.Split('.')[0], "", new List<string> { string.Join(".", tagName.Split('.').Skip(1)) }.ToArray(), cci);
                    }
                    catch
                    {
                        Console.WriteLine($"Unable to retrieve tag info for tag '{tagName}'. Tag may not exist in views");
                        tagsWithErrors.Add(tagName);
                    }

                    // If the tagname is bad...  GetTagInfo will return a HWSTagInfo structure
                    //   but the itemType will be -1.  

                    if (tagInfo != null && tagInfo.Length > 0 && tagInfo[0].itemType != -1)
                    {
                        try
                        {
                            HWSTagContext[] tagContexts = client.GetTagDataContext(tagName.Split('.')[0], "", new string[] { string.Join(".", tagName.Split('.').Skip(1)) }, cci);
                            contexts.Add(tagName, tagContexts[0]);
                        }
                        catch
                        {
                            Console.WriteLine($"Unable to retrieve tag context for tag '{tagName}'. Tag may not exist in views");
                            tagsWithErrors.Add(tagName);
                        }
                    }
                    else
                    {
                        tagsWithErrors.Add(tagName);
                    }
                }
            }

            Console.WriteLine($"Successful tags: {contexts.Count} - Tags with errors: {tagsWithErrors.Count}");
            return contexts;
        }

        public static void ExportTagDataToParquet(Dictionary<string, HWSTagContext> tagContexts)
        {
            long totalRecs = 0;
            int numberOfFiles = 0;
            long fileRecs;
            bool isRawDataRequest = string.IsNullOrWhiteSpace(aggregateName) || dataAggregateInterval == TimeSpan.Zero;
            DateTime batchStartTime = dataStartTime;

            while (batchStartTime < dataEndTime)
            {
                Schema schema = GetSchema();
                string dynamicExportFileName = Path.Combine(parquetExportDirectory, $"{parquetExportFileName}_{batchStartTime:yyyyMMdd_HHmmss}.parquet");
                using (FileStream fileStream = File.OpenWrite(dynamicExportFileName))
                {
                    using (ParquetWriter parquetWriter = new ParquetWriter(schema, fileStream))
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        Console.WriteLine($"Starting export of data to {dynamicExportFileName}...");

                        parquetWriter.CompressionMethod = (CompressionMethod)Enum.Parse(typeof(CompressionMethod), parquetCompression);
                        numberOfFiles++;
                        fileRecs = 0;

                        while (fileRecs < numberOfRecordsPerFile)
                        {
                            DateTime batchEndTime;

                            if (isRawDataRequest)
                            {
                                // raw data
                                batchEndTime = batchStartTime.Add(rawDataRetrievalInterval);
                            }
                            else
                            {
                                // aggregate data. max 10K values returned per aggregate data call
                                batchEndTime = batchStartTime.AddTicks(dataAggregateInterval.Ticks * 10000);
                                if (batchEndTime > dataEndTime)
                                    batchEndTime = dataEndTime;
                            }

                            foreach (string tagName in tagContexts.Keys)
                            {
                                List<TVQ> tagData;

                                if (isRawDataRequest)
                                    tagData = ReadRawDataForTag(tagName, tagContexts[tagName], batchStartTime, batchEndTime);
                                else
                                    tagData = ReadAggregateDataForTag(tagName, tagContexts[tagName], batchStartTime, batchEndTime);

                                if (tagData.Count > 0)
                                {
                                    WriteToParquet(schema, parquetWriter, tagName, tagData);
                                    fileRecs += tagData.Count;
                                    totalRecs += tagData.Count;
                                }
                            }

                            double totalProgress = ((batchEndTime.Ticks - dataStartTime.Ticks) / (double)(dataEndTime.Ticks - dataStartTime.Ticks)) * 100.0;

                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write($"Total progress: {totalProgress:N0}% complete - File records: {fileRecs:N0} - Total Files: {numberOfFiles} - Total Records: {totalRecs:N0}");

                            batchStartTime = batchEndTime;
                            if (batchStartTime >= dataEndTime)
                                break;
                        }

                        Console.WriteLine();
                        Console.WriteLine($"Finished export of {fileRecs:N0} records to {dynamicExportFileName} in {sw.Elapsed.TotalMinutes:F2} mins...");
                    }
                }
            }
        }

        public static List<TVQ> ReadRawDataForTag(string tagName, HWSTagContext context, DateTime startTime, DateTime endTime)
        {
            List<TVQ> allTVQs = new List<TVQ>();

            if (context.oldestTimeStamp < startTime || context.latestTimeStamp > endTime)
            {
                List<HWSTagRequest3> requests = new List<HWSTagRequest3>();
                HWSTagRequest3 req = new HWSTagRequest3();
                req.startTime = startTime;
                req.endTime = endTime;
                req.tagName = context.tagItemId;
                req.clientData = 0;
                requests.Add(req);
                HWSTagData3[] tagData = client.GetRawTagData3(tagName.Split('.')[0], requests.ToArray(), 100000, false, false, cci);
                bool done = false;
                while (!done)
                {
                    HWSTagData3 tdData = tagData[0];
                    if (string.IsNullOrWhiteSpace(tdData.errMsg))
                    {
                        int count = tdData.tvqs.Length;
                        for (int i = 0; i < count; i++)
                        {
                            TVQ tvq = new TVQ();
                            tvq.timeStamp = tdData.tvqs[i].timeStamp;
                            tvq.value = tdData.tvqs[i].value;
                            tvq.quality = (short)tdData.tvqs[i].quality;
                            allTVQs.Add(tvq);
                        }

                        if (tdData.resultFlags == 1)
                        {
                            // Ask for more Data...
                            requests[0].startTime = tdData.tvqs.Last().timeStamp;
                            tagData = client.GetRawTagData3("", requests.ToArray(), 100000, false, false, cci);
                        }
                        else
                            done = true;
                    }
                    else
                        done = true; // no trend vectors for tag
                }
            }

            return allTVQs;
        }

        public static List<TVQ> ReadAggregateDataForTag(string tagName, HWSTagContext context, DateTime startTime, DateTime endTime)
        {
            List<TVQ> allTVQs = new List<TVQ>();

            List<HWSTagProcessedRequest3> requests = new List<HWSTagProcessedRequest3>();
            HWSTagProcessedRequest3 req = new HWSTagProcessedRequest3();
            req.tagName = context.tagItemId;
            req.aggregateName = aggregateName;
            req.aggregateConfiguration = new AggregateConfiguration() { TreatUncertainAsBad = false, PercentDataBad = 100, PercentDataGood = 100, UseSlopedExtrapolation = false };
            req.clientData = 0;
            requests.Add(req);
            HWSTagProcessedData2[] tagData = client.GetProcessedTagData3(tagName.Split('.')[0], requests.ToArray(), startTime, endTime, dataAggregateInterval, false, cci);

            HWSTagProcessedData2 tdData = tagData[0];

            if (string.IsNullOrWhiteSpace(tdData.errMsg))
            {
                for (int i = 0; i < tdData.tvqs.Length; i++)
                {
                    TVQ tvq = new TVQ();
                    tvq.timeStamp = tdData.tvqs[i].timeStamp;
                    tvq.value = tdData.tvqs[i].value;
                    tvq.quality = (short)tdData.tvqs[i].quality;
                    allTVQs.Add(tvq);
                }
            }

            return allTVQs;
        }

        private static Schema GetSchema()
        {
            //create data columns with schema metadata and the data you need
            var idColumn = new DataColumn(
               new DataField<string>("TagName"),
               new string[0]);

            var timeSeriesColumn = new DataColumn(
               new DataField<DateTimeOffset>("TimeStamp_UTC"),
               new DateTimeOffset[0]);

            var dataTypeColumn = new DataColumn(
                new DataField<string>("ValueDataType"),
                new string[0]);

            var dataValueDoubleColumn = new DataColumn(
              new DataField<double>("Value_double"),
              new double[0]);

            var dataValueLongColumn = new DataColumn(
              new DataField<long>("Value_long"),
              new long[0]);

            var dataValueUlongColumn = new DataColumn(
              new DataField<ulong>("Value_ulong"),
              new ulong[0]);

            var dataValueStringColumn = new DataColumn(
                new DataField<string>("Value_string"),
                new string[0]);

            var dataQualityColumn = new DataColumn(
                new DataField<ushort>("Quality"),
                new ushort[0]);

            // create file schema
            return new Schema(idColumn.Field, timeSeriesColumn.Field, dataTypeColumn.Field, dataValueDoubleColumn.Field, dataValueLongColumn.Field, dataValueUlongColumn.Field, dataValueStringColumn.Field, dataQualityColumn.Field);
        }

        private static int WriteToParquet(Schema schema, ParquetWriter writer, string tagName, List<TVQ> allTVQData)
        {
            using (ParquetRowGroupWriter groupWriter = writer.CreateRowGroup())
            {
                Parquet.Data.Rows.Table table = new Parquet.Data.Rows.Table(schema);
                foreach (TVQ tvq in allTVQData)
                {
                    ushort quality = (ushort)tvq.quality;
                    string dataType = tvq.value == null ? "null" : tvq.value.GetType().Name;

                    double dv = 0.0;
                    long iv = 0L;
                    ulong uiv = 0ul;
                    string sv = "";

                    if (dataType == "Single" || dataType == "Double") dv = Convert.ToDouble(tvq.value);
                    else if (dataType == "SByte" || dataType == "Int16" || dataType == "Int32" || dataType == "Int64") iv = Convert.ToInt64(tvq.value);
                    else if (dataType == "Byte" || dataType == "UInt16" || dataType == "UInt32" || dataType == "UInt64") uiv = Convert.ToUInt64(tvq.value);
                    else if (dataType == "Boolean") uiv = Convert.ToUInt64(tvq.value);
                    else if (dataType == "String") sv = Convert.ToString(tvq.value);
                    else if (dataType != "null")
                        throw new Exception("Unhandled datatype");

                    table.Add(new Parquet.Data.Rows.Row(tagName, new DateTimeOffset(tvq.timeStamp.ToUniversalTime()), dataType, dv, iv, uiv, sv, quality));
                }
                groupWriter.Write(table);
            }

            return allTVQData.Count;
        }
    }
}
