NOTE:
- These functions will not be made available and were used only for testing.
- The outputed results and class structures are correct as of 10/20/2014.

<!--
Available Functions:
InterfaceVersionResponse InterfaceVersion(InterfaceVersion $parameters)
GetDataSetsResponse GetDataSets(GetDataSets $parameters)
GetSessionIdResponse GetSessionId(GetSessionId $parameters)
UpdateSettingsResponse UpdateSettings(UpdateSettings $parameters)
GetTagIdsResponse GetTagIds(GetTagIds $parameters)
StoreDataResponse StoreData(StoreData $parameters)
NoDataResponse NoData(NoData $parameters)
CreateNewFileResponse CreateNewFile(CreateNewFile $parameters)
FileRollOverResponse FileRollOver(FileRollOver $parameters)
GetErrorsResponse GetErrors(GetErrors $parameters)
KeepAliveResponse KeepAlive(KeepAlive $parameters)
ReleaseSessionResponse ReleaseSession(ReleaseSession $parameters)
ArrayOfTagResponse ArrayOfTag(ArrayOfTag $parameters)
ArrayOfTVQResponse ArrayOfTVQ(ArrayOfTVQ $parameters)
ArrayOfPropertyResponse ArrayOfProperty(ArrayOfProperty $parameters)
ArrayOfAnnotationResponse ArrayOfAnnotation(ArrayOfAnnotation $parameters)
ArrayOfSettingResponse ArrayOfSetting(ArrayOfSetting $parameters)
FileResponse File(File $parameters)
IntervalResponse Interval(Interval $parameters)

ArrayOfTag stdClass Object ( [Tag] => Array ( [0] => stdClass Object ( [equation] => Value * 10 [name] => DataSet.Tag 0001 [timeExtension] => PT30S ) [1] => stdClass Object ( [equation] => Value * 10 [name] => DataSet.Tag 0002 [timeExtension] => PT30S ) [2] => stdClass Object ( [equation] => Value * 10 [name] => DataSet.Tag 0003 [timeExtension] => PT30S ) [3] => stdClass Object ( [equation] => Value * 10 [name] => DataSet.Tag 0004 [timeExtension] => PT30S ) ) ) 

ArrayOfTVQ: stdClass Object ( [TVQ] => Array ( [0] => stdClass Object ( [id] => 0 [quality] => 192 [timestamp] => 2014-10-20T10:22:00.5445636-04:00 [value] => 100 ) [1] => stdClass Object ( [id] => 1 [quality] => 192 [timestamp] => 2014-10-20T10:22:00.5445636-04:00 [value] => 100 ) [2] => stdClass Object ( [id] => 2 [quality] => 192 [timestamp] => 2014-10-20T10:22:00.5445636-04:00 [value] => 100 ) [3] => stdClass Object ( [id] => 3 [quality] => 192 [timestamp] => 2014-10-20T10:22:00.5445636-04:00 [value] => 100 ) ) ) 

ArrayOfProperty: stdClass Object ( [Property] => Array ( [0] => stdClass Object ( [description] => [id] => 0 [name] => Name [quality] => 192 [timestamp] => 2014-10-20T10:22:00.5505642-04:00 [value] => Value ) [1] => stdClass Object ( [description] => [id] => 1 [name] => Name [quality] => 192 [timestamp] => 2014-10-20T10:22:00.5505642-04:00 [value] => Value ) [2] => stdClass Object ( [description] => [id] => 2 [name] => Name [quality] => 192 [timestamp] => 2014-10-20T10:22:00.5505642-04:00 [value] => Value ) [3] => stdClass Object ( [description] => [id] => 3 [name] => Name [quality] => 192 [timestamp] => 2014-10-20T10:22:00.5505642-04:00 [value] => Value ) ) ) 

ArrayOfAnnotation: stdClass Object ( [Annotation] => Array ( [0] => stdClass Object ( [createdAt] => 2014-10-20T10:22:00.5565648-04:00 [id] => 0 [timestamp] => 2014-10-20T10:22:00.5565648-04:00 [user] => User [value] => Value ) [1] => stdClass Object ( [createdAt] => 2014-10-20T10:22:00.5565648-04:00 [id] => 1 [timestamp] => 2014-10-20T10:22:00.5565648-04:00 [user] => User [value] => Value ) [2] => stdClass Object ( [createdAt] => 2014-10-20T10:22:00.5565648-04:00 [id] => 2 [timestamp] => 2014-10-20T10:22:00.5565648-04:00 [user] => User [value] => Value ) [3] => stdClass Object ( [createdAt] => 2014-10-20T10:22:00.5565648-04:00 [id] => 3 [timestamp] => 2014-10-20T10:22:00.5565648-04:00 [user] => User [value] => Value ) ) ) 

ArrayOfSetting: stdClass Object ( [Setting] => Array ( [0] => stdClass Object ( [name] => Boolean [value] => 1 ) [1] => stdClass Object ( [name] => Integer [value] => 100 ) [2] => stdClass Object ( [name] => Double [value] => 100.01 ) [3] => stdClass Object ( [name] => String [value] => String ) ) ) 

File: stdClass Object ( [dataSet] => Sample Data [fileTime] => 2014-10-20T10:22:00.5665658-04:00 ) 

Interval: P1DT2H3M4.005S
-->

<?php
  include "inc.soapFunctions.php";
  include "inc.soapClasses.php";
  
  // set timezone
  date_default_timezone_set("America/New_York");
  
  $soap = CreateSoapClient(null, null);
  
  // show list of available functions
  echo "<strong>Available Functions:</strong><br>";
  $functions = $soap->__getFunctions();
  foreach($functions as $function)
    echo "$function<br>";
  
  // soap tags format
  $tags = $soap -> ArrayOfTag();
  echo "<br><strong>ArrayOfTag</strong> ";
  print_r($tags -> ArrayOfTagResult);
  echo "<br>";
  
  // soap tvqs format
  $tvqs = $soap -> ArrayOfTVQ();
  echo "<br><strong>ArrayOfTVQ:</strong> ";
  print_r($tvqs -> ArrayOfTVQResult);
  echo "<br>";
  
  // soap properties format
  $property = $soap -> ArrayOfProperty();
  echo "<br><strong>ArrayOfProperty:</strong> ";
  print_r($property -> ArrayOfPropertyResult);
  echo "<br>";
  
  // soap annotations format
  $annotation = $soap -> ArrayOfAnnotation();
  echo "<br><strong>ArrayOfAnnotation:</strong> ";
  print_r($annotation -> ArrayOfAnnotationResult);
  echo "<br>";
  
  // soap settings
  $settings = $soap -> ArrayOfSetting();
  echo "<br><strong>ArrayOfSetting:</strong> ";
  print_r($settings -> ArrayOfSettingResult);
  echo "<br>";
  
  // soap file
  $file = $soap -> File();
  echo "<br><strong>File:</strong> ";
  print_r($file -> FileResult);
  echo "<br>";
  
  // soap interval
  $interval = $soap -> Interval();
  echo "<br><strong>Interval:</strong> ";
  print_r($interval -> IntervalResult);
  echo "<br>";
?>