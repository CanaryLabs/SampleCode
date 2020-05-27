# Canary Labs Web API Export Data Example
# Tested with Python 3.8.
# See http://localhost:55235/help for documentation regarding Canary api requests/response
# By default, this script reads aggregated diagnostic data (1 minute aggregate from the last day) from a local historian and outputs the results to c:\temp\canaryReadDemo.csv

# default Canary views web api ports are:
#   55235: http
#   55236: https
# ex: http://localhost:55235/api/v2/
# ex: https://localhost:55236/api/v2/

#############################################
# Canary Views Web API Settings
#############################################
# api url. must end with slash '/' (the port number is configured through the views tile in the Canary Admin. default = 55235)
apiURL = 'http://localhost:55235/api/v2/'
# timezone is used to convert times to/from the clients' timezone. valid timezone strings are found via the /getTimeZones api endpoint, which returns:
# 'Dateline Standard Time',
# 'UTC-11',
# 'Aleutian Standard Time',
# 'Hawaiian Standard Time',
# 'Marquesas Standard Time',
# 'Alaskan Standard Time',
# 'UTC-09',
# 'Pacific Standard Time (Mexico)',
# 'UTC-08',
# 'Pacific Standard Time',
# 'US Mountain Standard Time',
# 'Mountain Standard Time (Mexico)',
# 'Mountain Standard Time',
# 'Central America Standard Time',
# 'Central Standard Time',
# 'Easter Island Standard Time',
# 'Central Standard Time (Mexico)',
# 'Canada Central Standard Time',
# 'SA Pacific Standard Time',
# 'Eastern Standard Time (Mexico)',
# 'Eastern Standard Time',
# 'Haiti Standard Time',
# 'Cuba Standard Time',
# 'US Eastern Standard Time',
# 'Turks And Caicos Standard Time',
# 'Paraguay Standard Time',
# 'Atlantic Standard Time',
# 'Venezuela Standard Time',
# 'Central Brazilian Standard Time',
# 'SA Western Standard Time',
# 'Pacific SA Standard Time',
# 'Newfoundland Standard Time',
# 'Tocantins Standard Time',
# 'E. South America Standard Time',
# 'SA Eastern Standard Time',
# 'Argentina Standard Time',
# 'Greenland Standard Time',
# 'Montevideo Standard Time',
# 'Magallanes Standard Time',
# 'Saint Pierre Standard Time',
# 'Bahia Standard Time',
# 'UTC-02',
# 'Mid-Atlantic Standard Time',
# 'Azores Standard Time',
# 'Cape Verde Standard Time',
# 'UTC',
# 'GMT Standard Time',
# 'Greenwich Standard Time',
# 'Morocco Standard Time',
# 'W. Europe Standard Time',
# 'Central Europe Standard Time',
# 'Romance Standard Time',
# 'Sao Tome Standard Time',
# 'Central European Standard Time',
# 'W. Central Africa Standard Time',
# 'Jordan Standard Time',
# 'GTB Standard Time',
# 'Middle East Standard Time',
# 'Egypt Standard Time',
# 'E. Europe Standard Time',
# 'Syria Standard Time',
# 'West Bank Standard Time',
# 'South Africa Standard Time',
# 'FLE Standard Time',
# 'Israel Standard Time',
# 'Kaliningrad Standard Time',
# 'Sudan Standard Time',
# 'Libya Standard Time',
# 'Namibia Standard Time',
# 'Arabic Standard Time',
# 'Turkey Standard Time',
# 'Arab Standard Time',
# 'Belarus Standard Time',
# 'Russian Standard Time',
# 'E. Africa Standard Time',
# 'Iran Standard Time',
# 'Arabian Standard Time',
# 'Astrakhan Standard Time',
# 'Azerbaijan Standard Time',
# 'Russia Time Zone 3',
# 'Mauritius Standard Time',
# 'Saratov Standard Time',
# 'Georgian Standard Time',
# 'Volgograd Standard Time',
# 'Caucasus Standard Time',
# 'Afghanistan Standard Time',
# 'West Asia Standard Time',
# 'Ekaterinburg Standard Time',
# 'Pakistan Standard Time',
# 'India Standard Time',
# 'Sri Lanka Standard Time',
# 'Nepal Standard Time',
# 'Central Asia Standard Time',
# 'Bangladesh Standard Time',
# 'Omsk Standard Time',
# 'Myanmar Standard Time',
# 'SE Asia Standard Time',
# 'Altai Standard Time',
# 'W. Mongolia Standard Time',
# 'North Asia Standard Time',
# 'N. Central Asia Standard Time',
# 'Tomsk Standard Time',
# 'China Standard Time',
# 'North Asia East Standard Time',
# 'Singapore Standard Time',
# 'W. Australia Standard Time',
# 'Taipei Standard Time',
# 'Ulaanbaatar Standard Time',
# 'Aus Central W. Standard Time',
# 'Transbaikal Standard Time',
# 'Tokyo Standard Time',
# 'North Korea Standard Time',
# 'Korea Standard Time',
# 'Yakutsk Standard Time',
# 'Cen. Australia Standard Time',
# 'AUS Central Standard Time',
# 'E. Australia Standard Time',
# 'AUS Eastern Standard Time',
# 'West Pacific Standard Time',
# 'Tasmania Standard Time',
# 'Vladivostok Standard Time',
# 'Lord Howe Standard Time',
# 'Bougainville Standard Time',
# 'Russia Time Zone 10',
# 'Magadan Standard Time',
# 'Norfolk Standard Time',
# 'Sakhalin Standard Time',
# 'Central Pacific Standard Time',
# 'Russia Time Zone 11',
# 'New Zealand Standard Time',
# 'UTC+12',
# 'Fiji Standard Time',
# 'Kamchatka Standard Time',
# 'Chatham Islands Standard Time',
# 'UTC+13',
# 'Tonga Standard Time',
# 'Samoa Standard Time',
# 'Line Islands Standard Time'
timeZone = 'Eastern Standard Time'
# can leave empty if the http anonymous endpoint is turned on in views service and http is used (default); required for https requests
userName = ''
# same rules apply as username
password = ''
# identifies this connection in the Canary Administrator. Lets you know who's connected to it for troubleshooting
application = 'Canary Python Read Demo'
# identifies the dataset and any number of paths to prefix to the tag names. If this is empty, each tag in the list needs to be fully qualified with historian and dataset path
dataSetPrefix = 'localhost.{Diagnostics}'
# collection of tags that you want to read from the api. Note: only 1 tag is supported for raw data since timestamps will not be the same across tags (invalid export)
tagsToRead = [
    'AdminRequests/sec',
    'Reading.HistoryMax-ms',
    'Reading.HistoryRequests/sec',
    'Reading.LiveMax-ms',
    'Reading.LiveTVQs/sec',
    'Reading.NumClients',
    'Reading.TagHandles',
    'Reading.TVQs/sec',
    'Sys.CPU Usage Historian',
    'Sys.CPU Usage Total',
    'Sys.Historian Handle Count',
    'Sys.Historian Thread Count',
    'Sys.Historian Working Set (memory)',
    'Sys.Memory Page',
    'Sys.Memory Virtual',
    'Views.APICallCount/min',
    'Views.AxiomCallCount',
    'Views.AxiomLiveCallCount',
    'Views.AxiomLiveMax-ms',
    'Views.AxiomLivePixelMax-ms',
    'Views.AxiomTotalTVQS',
    'Views.CPU Usage',
    'Views.GetRawCount',
    'Views.GetRawMax-ms',
    'Views.TotalConnections/min',
    'Views.TotalTVQs/min',
    'Views.Working Set Memory',
    'Writing.NumClients',
    'Writing.Requests/sec',
    'Writing.TagHandles',
    'Writing.TVQ TimeExtensions/sec',
    'Writing.TVQs/sec'
]

# start time. can be relative or absolute time
# relative time units can be found at http://localhost:55235/help#relativeTime
# absolute time format Ex. '03/12/2019 12:00:00'
startTime = 'now-1day'
# see start time.
endTime = 'now'
# aggregate function to use for aggregated data.
aggregateName = 'TimeAverage2'
#'TimeAverage2': 'Retrieve the time weighted average data over the interval using Simple Bounding Values.',
#'Interpolative': 'At the beginning of each interval, retrieve the calculated value from the data points on either side of the requested timestamp.',
#'Average': 'Retrieve the average value of the data over the interval.',
#'TimeAverage': 'Retrieve the time weighted average data over the interval using Interpolated Bounding Values.',
#'Total': 'Retrieve the total (time integral) of the data over the interval using Interpolated Bounding Values.',
#'Total2': 'Retrieve the total (time integral in seconds) of the data over the interval using Simple Bounding Values.',
#'TotalPerMinute': 'Retrieve the total (time integral in minutes) of the data over the interval using Simple Bounding Values.',
#'TotalPerHour': 'Retrieve the total (time integral in hours) of the data over the interval using Simple Bounding Values.',
#'TotalPer24Hours': 'Retrieve the total (time integral in 24 hours) of the data over the interval using Simple Bounding Values.',
#'Minimum': 'Retrieve the minimum raw value in the interval with the timestamp of the start of the interval.',
#'Maximum': 'Retrieve the maximum raw value in the interval with the timestamp of the start of the interval.',
#'MinimumActualTime': 'Retrieve the minimum value in the interval and the timestamp of the minimum value.',
#'MaximumActualTime': 'Retrieve the maximum value in the interval and the timestamp of the maximum value.',
#'Range': 'Retrieve the difference between the minimum and maximum value over the interval.',
#'Minimum2': 'Retrieve the minimum value in the interval including the Simple Bounding Values.',
#'Maximum2': 'Retrieve the maximum value in the interval including the Simple Bounding Values.',
#'MinimumActualTime2': 'Retrieve the minimum value with the actual timestamp including the Simple Bounding Values.',
#'MaximumActualTime2': 'Retrieve the maximum value with the actual timestamp including the Simple Bounding Values.',
#'Range2': 'Retrieve the difference between the Minimum2 and Maximum2 value over the interval.',
#'Count': 'Retrieve the number of raw values over the interval.',
#'DurationInStateZero': 'Retrieve the time a Boolean or numeric was in a zero state using Simple Bounding Values.',
#'DurationInStateNonZero': 'Retrieve the time a Boolean or numeric was in a non-zero state using Simple Bounding Values.',
#'NumberOfTransitions': 'Retrieve the number of changes between zero and non-zero that a Boolean or numeric value experienced in the interval.',
#'Start': 'Retrieve the first value in the interval.',
#'End': 'Retrieve the last value in the interval.',
#'Delta': 'Retrieve the difference between the Start and End value in the interval.',
#'StartBound': 'Retrieve the value at the beginning of the interval using Simple Bounding Values.',
#'EndBound': 'Retrieve the value at the end of the interval using Simple Bounding Values.',
#'DeltaBounds': 'Retrieve the difference between the StartBound and EndBound value in the interval.',
#'Instant': 'Retrieve the value at the exact beginning of the interval.',
#'DurationGood': 'Retrieve the total duration of time in the interval during which the data is Good.',
#'DurationBad': 'Retrieve the total duration of time in the interval during which the data is Bad.',
#'PercentGood': 'Retrieve the percentage of data (0 to 100) in the interval which has Good StatusCode',
#'PercentBad': 'Retrieve the percentage of data (0 to 100) in the interval which has Bad StatusCode.',
#'WorstQuality': 'Retrieve the worst StatusCode of data in the interval.',
#'WorstQuality2': 'Retrieve the worst StatusCode of data in the interval including the Simple Bounding Values.',
#'StandardDeviationSample': 'Retrieve the standard deviation for the interval for a sample of the population (n-1).',
#'VarianceSample': 'Retrieve the variance for the interval as calculated by the StandardDeviationSample.',
#'StandardDeviationPopulation': 'Retrieve the standard deviation for the interval for a complete population (n) which includes Simple Bounding Values.',
#'VariancePopulation': 'Retrieve the variance for the interval as calculated by the StandardDeviationPopulation which includes Simple Bounding Values.'

# interval over which to aggregate. can be relative or absolute
# relative time units can be found at http://localhost:55235/help#relativeTime
# absolute interval can be expressed as a timespan 'days.hours:minutes:seconds.subseconds' or '1.00:01:00' = 1 day 1 minute
aggregateInterval = '1minute'
# if True, includes the quality of the data in the output
includeQuality = False
# limits the max return amount (default is 10,000). 
maxSize = 10000
# parameter is always None on first request (this is for paging purposes and will be returned by the first request)
continuation = None

# Parameter examples 
# 1: Last Value. Returns the last (most recent) value for the tags
# startTime = None
# endTime = None

# 2: Raw Data. Returns raw data for the tags
# startTime = 'now-1day'
# endTime = 'now'
# aggregateName = None
# aggregateInterval = None

# 3: Processed Data
# startTime = 'now-1day'
# endTime = 'now'
# aggregateName = 'TimeAverage2'
# aggregateInterval = '1minute'

#############################################
# CSV Output File Settings
#############################################
# output file name
csvFileName = 'c:\\temp\\canaryReadDemo.csv'
# if True, then deletes an existing output from the directory before writing new output (valid values are True and False)
purgeExistingOutputFile = False

#############################################
# Script Execution
#############################################

import os, sys, requests, json, csv

# delete the output file if exists and configured to do so
if os.path.isfile(csvFileName):
    if purgeExistingOutputFile:
        try:
            os.remove(csvFileName)
        except OSError:
            pass
    else:
        sys.exit('Output file already exists. Delete the file or set purgeExistingOutputFile = True to automatically delete on next run. Exiting')

# prepend the dataset and path to the tag names
fullyQualifiedTagsToRead = [
    (dataSetPrefix + '.' if dataSetPrefix else '') + i for i in tagsToRead
]

session = requests.Session()

reqBody = {
    'username':userName,
    'password':password,
    'timezone':timeZone,
    'application':application
}

# get a user token for authenticating the requests
response = session.post(apiURL + 'getUserToken', data=json.dumps(reqBody))
responseJson = response.json()

if responseJson['statusCode'] != 'Good':
    sys.exit('Error retrieving a user token from the Canary api\n' +
             'Response' + response.text)

userToken = responseJson['userToken']

headerRowWritten = False
while True:
    # required parameters
    reqBody = {
        'userToken':userToken,
        'tags':fullyQualifiedTagsToRead
    }

    # optional parameters
    if startTime: reqBody['startTime'] = startTime
    if endTime: reqBody['endTime'] = endTime
    if aggregateName: reqBody['aggregateName'] = aggregateName
    if aggregateInterval: reqBody['aggregateInterval'] = aggregateInterval
    if includeQuality: reqBody['includeQuality'] = includeQuality
    if maxSize: reqBody['maxSize'] = maxSize
    if continuation: reqBody['continuation'] = continuation

    # call the /getTagData endpoint to get data for the tags
    response = session.post(apiURL + 'getTagData', data=json.dumps(reqBody))
    tagData = response.json()

    # check for errors
    if tagData['statusCode'] != 'Good':
        session.post(apiURL + 'revokeUserToken', data=json.dumps({'userToken':userToken}))
        sys.exit('Error retrieving tag data from the Canary api. Exiting\n' +
                'Response' + response.text)

    # iterate over the response and export to CSV
    continuation = tagData['continuation']
    done = False
    recordIndex = 0
    
    with open(csvFileName, 'a', newline='') as csvFile:
        writer = csv.DictWriter(csvFile, fieldnames=[])

        while not done:
            # every iteration of this loop is a csv record with columns being timestamp, tag1, tag2, tag3, etc...
            csvObj = {}
            writeRecord = False
            
            # using aggregate data, all tags will have the same number of records
            for tagKey, tagTvqs in tagData['data'].items():
                # each tag with its list of tvqs
                # tagKey is the tag name
                if recordIndex < len(tagTvqs):
                    record = tagTvqs[recordIndex]
                    # each tvq record for the tag
                    # record is a dict with the keys 't', 'v', and 'q' that corrospond to timestamp, value, and quality
                    csvObj['Timestamp'] = record['t']
                    csvObj[tagKey] = record['v']
                    if includeQuality:
                        csvObj[tagKey + ' Quality'] = record['q']

                    done = recordIndex + 1 > len(tagTvqs)
                    writeRecord = True

            recordIndex += 1
            if writeRecord:
                writer.fieldnames = csvObj.keys()

                if not headerRowWritten:
                    writer.writeheader()
                    headerRowWritten = True
                
                writer.writerow(csvObj)
            else:
                break

    # Exit the loop once there is no continuation point.
    if not continuation:
        break

# Revoke user token
session.post(apiURL + 'revokeUserToken', data=json.dumps({'userToken':userToken}))