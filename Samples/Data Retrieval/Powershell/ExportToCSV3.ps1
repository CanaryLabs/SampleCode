# Canary Labs Web API Export Data Example
# Tested with Powershell 5.1 and Powershell Core v6.
# See http://localhost:55235/help for documentation regarding Canary api requests/response
# Accepts a path as an argument instead of a tag file
# Dynamically loads all tags from the path and exports the data for those tags

# args
# 0: url ex. http://localhost:55235
# 1: path to read ex. <machineName>.{Diagnostics}
# 2: output file ex. c:\temp\canaryReadDemo.csv
# 3: start time ex. now-1day
# 4: end time ex. now
# 5: aggregate name ex. TimeAverage2
# 6: aggregate interval ex. 5minute
# 7: timezone ex. Eastern Standard Time
# 8: username 
# 9: password
# 10: application name ex. Canary Read Demo
# 11: include quality ex. 0 or 1
# 12: numeric precision digits
# 13: search text ex. 'cpu' (all tags that match 'cpu' in their name. multiple strings can be added separated by a space like 'flow oil')

$apiURL = $args[0]+"/api/v2/"
$pathToRead = $args[1]
$csvFileName = $args[2]
$startTime = $args[3]
$endTime = $args[4]
$aggregateName = $args[5]
$aggregateInterval = $args[6]
$timezone = $args[7]
$userName = $args[8]
$password = $args[9]
$application = $args[10]
$includeQuality = $args[11] -eq 1
$numericPrecision = $args[12]
$searchText = $args[13]

# parameter is always null on first request (this is for paging purposes and will be returned by the first request)
$continuation = $null

#############################################
# Script Execution
#############################################

# get a user token for authenticating the requests
$reqBody = new-object psobject
$reqBody | Add-Member -membertype NoteProperty -name "username" -value $userName
$reqBody | Add-Member -membertype NoteProperty -name "password" -value $password
$reqBody | Add-Member -membertype NoteProperty -name "timezone" -value $timezone
$reqBody | Add-Member -membertype NoteProperty -name "application" -value $application
$authReqBodyJson = ConvertTo-Json $reqBody -Depth 100
$userTokenResponseRaw = Invoke-WebRequest -Uri ($apiURL+"getUserToken") -Method POST -Body $authReqBodyJson -ContentType "application/json"
$userTokenResponse = $userTokenResponseRaw.Content | ConvertFrom-Json
if ($userTokenResponse.StatusCode -ne "Good")
{
    Write-Host "Error retrieving a user token from the Canary api"
    Write-Host "Response"+ $userTokenResponseRaw
    exit 1
}

$userToken = $userTokenResponse.userToken
$tagsToRead = [System.Collections.ArrayList]@()

# get the tags to read from the browse path command (does a deep browse to retrieve all tags under this path)
do
{
    $reqBody = new-object psobject
    $reqBody | Add-Member -membertype NoteProperty -Name 'userToken' -Value $userToken
    $reqBody | Add-Member -MemberType NoteProperty -Name 'path' -Value $pathToRead
    $reqBody | Add-Member -MemberType NoteProperty -Name 'deep' -Value $true
    if ($null -ne $searchText) {$reqBody | Add-Member -MemberType NoteProperty -Name 'search' -Value $searchText}
    if ($null -ne $continuation) {$reqBody | Add-Member -MemberType NoteProperty -Name 'continuation' -Value $continuation}

    # convert to JSON for sending
    $reqBodyJson = ConvertTo-Json $reqBody -Depth 100

    # call the /browseTags endpoint to get the tags for this particular node
    $browseDataRaw = (Invoke-WebRequest -Uri ($apiURL+"browseTags") -Method POST -Body $reqBodyJson -ContentType "application/json").Content
    $browseData = $browseDataRaw | ConvertFrom-Json
    if ($browseData.statusCode -ne "Good")
    {
        Write-Host "Error retrieving browse data from the Canary api. Exiting"
        Write-Host "Response: $browseDataRaw"
        exit 1
    }

    if ($browseData.tags.Length -eq 0)
    {
        Write-Host "The tag browse returned 0 tags. Please check your path and search settings. Exiting"
        Write-Host "Response: $browseDataRaw"
        exit 1
    }

    # add the tags to the list of tags to get data for
    foreach ($tag in $browseData.tags)
    {
        $tagsToRead.add($tag) > $null
    }

    $continuation = $browseData.continuation

}while($null -ne $continuation)

# proceed to read the tag data for all of the tags in the path
$continuation = $null
do
{
    # build the request body
    $reqBody = new-object psobject

    # required parameters
    $reqBody | Add-Member -MemberType NoteProperty -Name 'userToken' -Value $userToken
    $reqBody | Add-Member -MemberType NoteProperty -Name 'tags' -Value $tagsToRead

    # optional parameters
    if ($null -ne $startTime) {$reqBody | Add-Member -MemberType NoteProperty -Name 'startTime' -Value $startTime}
    if ($null -ne $endTime) {$reqBody | Add-Member -MemberType NoteProperty -Name 'endTime' -Value $endTime}
    if ($null -ne $aggregateName) {$reqBody | Add-Member -MemberType NoteProperty -Name 'aggregateName' -Value $aggregateName}
    if ($null -ne $aggregateInterval) {$reqBody | Add-Member -MemberType NoteProperty -Name 'aggregateInterval' -Value $aggregateInterval}
    if ($null -ne $includeQuality) {$reqBody | Add-Member -MemberType NoteProperty -Name 'includeQuality' -Value $includeQuality}
    if ($null -ne $continuation) {$reqBody | Add-Member -MemberType NoteProperty -Name 'continuation' -Value $continuation}

    # convert to JSON for sending
    $reqBodyJson = ConvertTo-Json $reqBody -Depth 100

    # call the /getTagData2 endpoint to get data for the tags
    $tagDataRaw = (Invoke-WebRequest -Uri ($apiURL+"getTagData2") -Method POST -Body $reqBodyJson -ContentType "application/json").Content
    $tagData = $tagDataRaw | ConvertFrom-Json
    if ($tagData.statusCode -ne "Good")
    {
        Write-Host "Error retrieving tag data from the Canary api. Exiting"
        Write-Host "Response"+ $tagDataRaw
        exit 1
    }

    # iterate over the response and export to CSV
    $continuation = $tagData.continuation
    $done = $true
    $recIndex = 0
    do {
        # every iteration of this loop is a csv record with columns being timestamp, tag1, tag2, tag3, etc...
        $csvObj = new-object psobject
        $csvObj | add-member -membertype NoteProperty -name "Timestamp" -value "timestamp"
        $writeRec = $false

        # using aggregate data, all tags will have the same number of records
        foreach($tag in $tagData.data.PsObject.Properties)
        {
            # each tag with its list of tvqs
            # $tagKey is the tag name
            if ($recIndex -lt $tag.value.Count)
            {
                $rec = $tag.value[$recIndex]
                # each tvq record for the tag
                # $rec is an array of values in this order: timestamp, value, quality
                $csvObj.Timestamp = $rec[0]

                $csvObj | add-member -membertype NoteProperty -name $tag.Name -value ([math]::Round($rec[1], $numericPrecision))
                if ($includeQuality)
                {
                    # include the quality as an additional column if specified in the request
                    $csvObj | add-member -membertype NoteProperty -name ($tag.Name + " Quality") -value $rec[2]
                }
                
                $done = $recIndex+1 -ge $tag.value.Count
                $writeRec = $true
            }

            $tagIndex++
        }

        $recIndex++
        if($writeRec)
        {
            Export-Csv $csvFileName -InputObject $csvObj -Append -Encoding UTF8 -NoTypeInformation
        }
    } while (!$done)
}while ($null -ne $continuation)

# revoke the user token so that the session is released to the pool for other connections to use
$revokeBody = "{ 'userToken':'$userToken' }"
Invoke-WebRequest -Uri ($apiURL+"revokeUserToken") -Method POST -Body $revokeBody -ContentType "application/json" > $null