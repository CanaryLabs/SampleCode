# Canary Labs Web API Export Data Example
# Tested with R 4.0.1.
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
# api url. must end with slash "/" (the port number is configured through the views tile in the Canary Admin. default = 55235)
api_url <- "http://localhost:55235/api/v2/"
# timezone is used to convert times to/from the clients' timezone. valid timezone strings are found via the /getTimeZones api endpoint, which returns:
# "Dateline Standard Time",
# "UTC-11",
# "Aleutian Standard Time",
# "Hawaiian Standard Time",
# "Marquesas Standard Time",
# "Alaskan Standard Time",
# "UTC-09",
# "Pacific Standard Time (Mexico)",
# "UTC-08",
# "Pacific Standard Time",
# "US Mountain Standard Time",
# "Mountain Standard Time (Mexico)",
# "Mountain Standard Time",
# "Central America Standard Time",
# "Central Standard Time",
# "Easter Island Standard Time",
# "Central Standard Time (Mexico)",
# "Canada Central Standard Time",
# "SA Pacific Standard Time",
# "Eastern Standard Time (Mexico)",
# "Eastern Standard Time",
# "Haiti Standard Time",
# "Cuba Standard Time",
# "US Eastern Standard Time",
# "Turks And Caicos Standard Time",
# "Paraguay Standard Time",
# "Atlantic Standard Time",
# "Venezuela Standard Time",
# "Central Brazilian Standard Time",
# "SA Western Standard Time",
# "Pacific SA Standard Time",
# "Newfoundland Standard Time",
# "Tocantins Standard Time",
# "E. South America Standard Time",
# "SA Eastern Standard Time",
# "Argentina Standard Time",
# "Greenland Standard Time",
# "Montevideo Standard Time",
# "Magallanes Standard Time",
# "Saint Pierre Standard Time",
# "Bahia Standard Time",
# "UTC-02",
# "Mid-Atlantic Standard Time",
# "Azores Standard Time",
# "Cape Verde Standard Time",
# "UTC",
# "GMT Standard Time",
# "Greenwich Standard Time",
# "Morocco Standard Time",
# "W. Europe Standard Time",
# "Central Europe Standard Time",
# "Romance Standard Time",
# "Sao Tome Standard Time",
# "Central European Standard Time",
# "W. Central Africa Standard Time",
# "Jordan Standard Time",
# "GTB Standard Time",
# "Middle East Standard Time",
# "Egypt Standard Time",
# "E. Europe Standard Time",
# "Syria Standard Time",
# "West Bank Standard Time",
# "South Africa Standard Time",
# "FLE Standard Time",
# "Israel Standard Time",
# "Kaliningrad Standard Time",
# "Sudan Standard Time",
# "Libya Standard Time",
# "Namibia Standard Time",
# "Arabic Standard Time",
# "Turkey Standard Time",
# "Arab Standard Time",
# "Belarus Standard Time",
# "Russian Standard Time",
# "E. Africa Standard Time",
# "Iran Standard Time",
# "Arabian Standard Time",
# "Astrakhan Standard Time",
# "Azerbaijan Standard Time",
# "Russia Time Zone 3",
# "Mauritius Standard Time",
# "Saratov Standard Time",
# "Georgian Standard Time",
# "Volgograd Standard Time",
# "Caucasus Standard Time",
# "Afghanistan Standard Time",
# "West Asia Standard Time",
# "Ekaterinburg Standard Time",
# "Pakistan Standard Time",
# "India Standard Time",
# "Sri Lanka Standard Time",
# "Nepal Standard Time",
# "Central Asia Standard Time",
# "Bangladesh Standard Time",
# "Omsk Standard Time",
# "Myanmar Standard Time",
# "SE Asia Standard Time",
# "Altai Standard Time",
# "W. Mongolia Standard Time",
# "North Asia Standard Time",
# "N. Central Asia Standard Time",
# "Tomsk Standard Time",
# "China Standard Time",
# "North Asia East Standard Time",
# "Singapore Standard Time",
# "W. Australia Standard Time",
# "Taipei Standard Time",
# "Ulaanbaatar Standard Time",
# "Aus Central W. Standard Time",
# "Transbaikal Standard Time",
# "Tokyo Standard Time",
# "North Korea Standard Time",
# "Korea Standard Time",
# "Yakutsk Standard Time",
# "Cen. Australia Standard Time",
# "AUS Central Standard Time",
# "E. Australia Standard Time",
# "AUS Eastern Standard Time",
# "West Pacific Standard Time",
# "Tasmania Standard Time",
# "Vladivostok Standard Time",
# "Lord Howe Standard Time",
# "Bougainville Standard Time",
# "Russia Time Zone 10",
# "Magadan Standard Time",
# "Norfolk Standard Time",
# "Sakhalin Standard Time",
# "Central Pacific Standard Time",
# "Russia Time Zone 11",
# "New Zealand Standard Time",
# "UTC+12",
# "Fiji Standard Time",
# "Kamchatka Standard Time",
# "Chatham Islands Standard Time",
# "UTC+13",
# "Tonga Standard Time",
# "Samoa Standard Time",
# "Line Islands Standard Time"
time_zone <- "Eastern Standard Time"
# can leave empty if the http anonymous endpoint is turned on in views service and http is used (default); required for https requests
user_name <- ""
# same rules apply as username
password <- ""
# identifies this connection in the Canary Administrator. Lets you know who's connected to it for troubleshooting
application <- "Canary R Read Demo"
# identifies the dataset and any number of paths to prefix to the tag names. If this is empty, each tag in the list needs to be fully qualified with historian and dataset path
data_set_prefix <- "localhost.{Diagnostics}"
# collection of tags that you want to read from the api. Note: only 1 tag is supported for raw data since timestamps will not be the same across tags (invalid export)
tags_to_read <- c(
    "AdminRequests/sec",
    "Reading.HistoryMax-ms",
    "Reading.HistoryRequests/sec",
    "Reading.LiveMax-ms",
    "Reading.LiveTVQs/sec",
    "Reading.NumClients",
    "Reading.TagHandles",
    "Reading.TVQs/sec",
    "Sys.CPU Usage Historian",
    "Sys.CPU Usage Total",
    "Sys.Historian Handle Count",
    "Sys.Historian Thread Count",
    "Sys.Historian Working Set (memory)",
    "Sys.Memory Page",
    "Sys.Memory Virtual",
    "Views.APICallCount/min",
    "Views.AxiomCallCount",
    "Views.AxiomLiveCallCount",
    "Views.AxiomLiveMax-ms",
    "Views.AxiomLivePixelMax-ms",
    "Views.AxiomTotalTVQS",
    "Views.CPU Usage",
    "Views.GetRawCount",
    "Views.GetRawMax-ms",
    "Views.TotalConnections/min",
    "Views.TotalTVQs/min",
    "Views.Working Set Memory",
    "Writing.NumClients",
    "Writing.Requests/sec",
    "Writing.TagHandles",
    "Writing.TVQ TimeExtensions/sec",
    "Writing.TVQs/sec"
)

# start time. can be relative or absolute time
# relative time units can be found at http://localhost:55235/help#relativeTime
# absolute time format Ex. "03/12/2019 12:00:00"
start_time <- "now-1day"
# see start time.
end_time <- "now"
# aggregate function to use for aggregated data.
aggregate_name <- "TimeAverage2"
#"TimeAverage2": "Retrieve the time weighted average data over the interval using Simple Bounding Values.",
#"Interpolative": "At the beginning of each interval, retrieve the calculated value from the data points on either side of the requested timestamp.",
#"Average": "Retrieve the average value of the data over the interval.",
#"TimeAverage": "Retrieve the time weighted average data over the interval using Interpolated Bounding Values.",
#"Total": "Retrieve the total (time integral) of the data over the interval using Interpolated Bounding Values.",
#"Total2": "Retrieve the total (time integral in seconds) of the data over the interval using Simple Bounding Values.",
#"TotalPerMinute": "Retrieve the total (time integral in minutes) of the data over the interval using Simple Bounding Values.",
#"TotalPerHour": "Retrieve the total (time integral in hours) of the data over the interval using Simple Bounding Values.",
#"TotalPer24Hours": "Retrieve the total (time integral in 24 hours) of the data over the interval using Simple Bounding Values.",
#"Minimum": "Retrieve the minimum raw value in the interval with the timestamp of the start of the interval.",
#"Maximum": "Retrieve the maximum raw value in the interval with the timestamp of the start of the interval.",
#"MinimumActualTime": "Retrieve the minimum value in the interval and the timestamp of the minimum value.",
#"MaximumActualTime": "Retrieve the maximum value in the interval and the timestamp of the maximum value.",
#"Range": "Retrieve the difference between the minimum and maximum value over the interval.",
#"Minimum2": "Retrieve the minimum value in the interval including the Simple Bounding Values.",
#"Maximum2": "Retrieve the maximum value in the interval including the Simple Bounding Values.",
#"MinimumActualTime2": "Retrieve the minimum value with the actual timestamp including the Simple Bounding Values.",
#"MaximumActualTime2": "Retrieve the maximum value with the actual timestamp including the Simple Bounding Values.",
#"Range2": "Retrieve the difference between the Minimum2 and Maximum2 value over the interval.",
#"Count": "Retrieve the number of raw values over the interval.",
#"DurationInStateZero": "Retrieve the time a Boolean or numeric was in a zero state using Simple Bounding Values.",
#"DurationInStateNonZero": "Retrieve the time a Boolean or numeric was in a non-zero state using Simple Bounding Values.",
#"NumberOfTransitions": "Retrieve the number of changes between zero and non-zero that a Boolean or numeric value experienced in the interval.",
#"Start": "Retrieve the first value in the interval.",
#"End": "Retrieve the last value in the interval.",
#"Delta": "Retrieve the difference between the Start and End value in the interval.",
#"StartBound": "Retrieve the value at the beginning of the interval using Simple Bounding Values.",
#"EndBound": "Retrieve the value at the end of the interval using Simple Bounding Values.",
#"DeltaBounds": "Retrieve the difference between the StartBound and EndBound value in the interval.",
#"Instant": "Retrieve the value at the exact beginning of the interval.",
#"DurationGood": "Retrieve the total duration of time in the interval during which the data is Good.",
#"DurationBad": "Retrieve the total duration of time in the interval during which the data is Bad.",
#"PercentGood": "Retrieve the percentage of data (0 to 100) in the interval which has Good StatusCode",
#"PercentBad": "Retrieve the percentage of data (0 to 100) in the interval which has Bad StatusCode.",
#"WorstQuality": "Retrieve the worst StatusCode of data in the interval.",
#"WorstQuality2": "Retrieve the worst StatusCode of data in the interval including the Simple Bounding Values.",
#"StandardDeviationSample": "Retrieve the standard deviation for the interval for a sample of the population (n-1).",
#"VarianceSample": "Retrieve the variance for the interval as calculated by the StandardDeviationSample.",
#"StandardDeviationPopulation": "Retrieve the standard deviation for the interval for a complete population (n) which includes Simple Bounding Values.",
#"VariancePopulation": "Retrieve the variance for the interval as calculated by the StandardDeviationPopulation which includes Simple Bounding Values."

# interval over which to aggregate. can be relative or absolute
# relative time units can be found at http://localhost:55235/help#relativeTime
# absolute interval can be expressed as a timespan "days.hours:minutes:seconds.subseconds" or "1.00:01:00" = 1 day 1 minute
aggregate_interval <- "1minute"
# if True, includes the quality of the data in the output
include_quality <- FALSE
# limits the max return amount (default is 10,000). 
max_size <- 10000
# parameter is always NA on first request (this is for paging purposes and will be returned by the first request)
continuation <- NA

# Parameter example

# 1: Processed Data
# start_time <- "now-1day"
# end_time <- "now"
# aggregate_name <- "TimeAverage2"
# aggregate_interval <- "1minute"

#############################################
# CSV Output File Settings
#############################################
# output file name
csv_file_name <- "c:\\temp\\canaryReadDemo.csv"
# if True, then deletes an existing output from the directory before writing new output (valid values are TRUE and FALSE)
purge_existing_output_file <- FALSE

#############################################
# Script Execution
#############################################

library(dplyr)
library(httr)
library(jsonlite)

# delete the output file if exists and configured to do so
if (purge_existing_output_file) {
    invisible(suppressWarnings(file.remove(csv_file_name)))
} else if (file.exists(csv_file_name)) {
    cat("Output file already exists. Delete the file or set purge_existing_output_file = TRUE to automatically delete on next run. Exiting")
    quit(save = "no")
}

# prepend the dataset and path to the tag names
fully_qualified_tags_to_read <- paste(data_set_prefix, tags_to_read, sep = ".")

request_body <- list (
    "username" = user_name,
    "password" = password,
    "timezone" = time_zone,
    "application" = application
)

# get a user token for authenticating the requests
response <- POST(paste(api_url, "getUserToken", sep = ""), body = request_body, encode = "json")
response_text <- content(response, as = "text", encoding = "UTF-8")
response_content <- fromJSON(response_text)

# check for any errors with the request
if (response_content["statusCode"] != "Good") {
    cat(paste("Error retrieving a user token from the Canary api:\n",
               paste(response_content[["errors"]], collapse = "\n"),
               sep = ""))
    quit(save = "no")
}

user_token <- response_content[["userToken"]]

# required parameters
request_body_base <- list(
    "userToken" = user_token,
    "tags" = fully_qualified_tags_to_read,
    "aggregateName" = aggregate_name,
    "aggregateInterval" = aggregate_interval
)

# optional parameters
if (exists("start_time") & length(start_time) > 0) {
    request_body_base <- c(request_body_base, "startTime" = start_time)
}
if (exists("end_time") & length(end_time) > 0) {
    request_body_base <- c(request_body_base, "endTime" = end_time)
}
if (exists("include_quality")) {
    request_body_base <- c(request_body_base, "includeQuality" = include_quality)
}
if (exists("max_size")) {
    request_body_base <- c(request_body_base, "maxSize" = max_size)
}

header_row_written <- FALSE

repeat {
    if (exists("continuation") & !is.na(continuation)[1]) {
        request_body <- c(request_body_base, "continuation" = list(continuation))
    } else {
        request_body <- request_body_base
    }

    # call the /getTagData2 endpoint to get data for the tags
    response <- POST(paste(api_url, "getTagData2", sep = ""), body = request_body, encode = "json")
    response_text <- content(response, as = "text", encoding = "UTF-8")
    response_content <- fromJSON(response_text)

    # check for any errors with the request
    if (response_content["statusCode"] != "Good") {
        cat(paste("Error retrieving tag data from the Canary api:\n",
                  paste(response_content[["errors"]], collapse = "\n"),
                  sep = ""))
        quit(save = "no")
    }

    # store the continuation point so it can be used in the next call to getTagData2
    continuation <- response_content[["continuation"]]

    tag_data <- response_content[["data"]]

    # get the timestamp column from the first tag returned.
    # since we are getting aggregate data, all of the tags 
    # will return tvqs with the same timestamps.
    common_time_stamps <- list(data.frame(Timestamp = tag_data[[1]][["t"]]))

    # add a new "Timestamp" column to the front of the tag data list
    modified_tag_data <- c(common_time_stamps, tag_data)

    # create a dataframe from the tag data list.
    result_data_frame <- data.frame(modified_tag_data, check.names = FALSE)

    # remove the timestamp column from each tag.
    result_data_frame <- result_data_frame %>%
        select(!matches(".*?\\.t$"))

    # rename the columns to be easier to read.
    colnames(result_data_frame) <- sub("(.*?)\\.v$", "\\1", colnames(result_data_frame))
    colnames(result_data_frame) <- sub("(.*?)\\.q$", "\\1 Quality", colnames(result_data_frame))

    # write the dataframe to a csv file.
    invisible(write.table(x = result_data_frame, file = csv_file_name, append = TRUE, sep = ",", row.names = FALSE, col.names = !header_row_written))
    header_row_written <- TRUE

    if (is.null(continuation) || is.na(continuation)[1]) {
        break
    }
}

# revoke the user token
request_body <- list("userToken" = user_token)
invisible(POST(paste(api_url, "revokeUserToken", sep = ""), body = request_body, encode = "json"))
