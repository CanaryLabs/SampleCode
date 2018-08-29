<!--
Version: 10.1.0.14282
DataSets: Array ( [0] => Data Generation [1] => DG1 [2] => DG2 [3] => DG3 [4] => DG4 [5] => DG5 [6] => PHP_SOAP [7] => SAF Performance [8] => Sample Data [9] => Testing [10] => TH Audit [11] => TH Performance [12] => Transform ) 
SessionId: Localhost/PHP_SOAP_CLIENT
UpdateSettings: Successful
TagIds: Array ( [0] => 1 [1] => 2 [2] => 3 [3] => 4 ) 
StoreData: Successful
NoData: Successful
GetErrors: Successful
Errors: stdClass Object ( [errors] => stdClass Object ( [KeyValuePairOfintstring] => Array ( [0] => stdClass Object ( [key] => 1 [value] => SessionId: ISAAC(Localhost/PHP_SOAP_CLIENT) Method: HistorianWrite.WriteTVQs Description: Error 0x80040413 - The timestamp of the data moved in a backward direction. The data has been ignored. Detail: Timestamp failed at 10/20/2014 10:46:35.1000 ) [1] => stdClass Object ( [key] => 2 [value] => SessionId: ISAAC(Localhost/PHP_SOAP_CLIENT) Method: HistorianWrite.WriteTVQs Description: Error 0x80040413 - The timestamp of the data moved in a backward direction. The data has been ignored. Detail: Timestamp failed at 10/20/2014 10:46:35.1000 ) [2] => stdClass Object ( [key] => 3 [value] => SessionId: ISAAC(Localhost/PHP_SOAP_CLIENT) Method: HistorianWrite.WriteTVQs Description: Error 0x80040413 - The timestamp of the data moved in a backward direction. The data has been ignored. Detail: Timestamp failed at 10/20/2014 10:46:35.1000 ) [3] => stdClass Object ( [key] => 4 [value] => SessionId: ISAAC(Localhost/PHP_SOAP_CLIENT) Method: HistorianWrite.WriteTVQs Description: Error 0x80040413 - The timestamp of the data moved in a backward direction. The data has been ignored. Detail: Timestamp failed at 10/20/2014 10:46:35.1000 ) ) ) [forwardDataCalls] => 1 [storeDataCalls] => 1 ) 
Release: Successful
-->

<?php
  // variables
  $config = new stdClass();
  $config -> historian = 'localhost';
  $config -> dataset = 'PHP_SOAP';
  $config -> url = 'http://localhost:55251/saf/sender/anonymous';
  $config -> wsdl = 'http://localhost:55251/saf/sender/anonymous?wsdl';
  //$config -> url = 'https://localhost:55252/saf/sender/username';
  //$config -> wsdl = 'https://localhost:55252/saf/sender/username?wsdl';
  $config -> user = null;
  $config -> pswd = null;
  
  include "inc.soapFunctions.php";
  include "inc.soapClasses.php";
  
  $soap = CreateSoapClient($config -> url, $config -> wsdl, $config -> user, $config -> pswd);
  
  // show interface version
  $version = Version($soap);
  echo "Version: $version<br>";
  
  // get data sets
  $dataSets = GetDataSets($soap, $config -> historian, $success);
  echo "DataSets: ";
  print_r($dataSets);
  echo "<br>";
  
  // build array of settings
  $settings = new stdClass();
  $settings -> Setting = array();
  $settings -> Setting[] = new Setting("AutoCreateDataSets", true);
  $settings -> Setting[] = new Setting("AutoWriteNoData", false);
  $settings -> Setting[] = new Setting("TrackErrors", true);
  
  // get session id
  // NOTE: client id should be unique if trying to use more than one session to write data
  $sessionId = GetSessionId($soap, $config -> historian, "PHP_SOAP_CLIENT", null, $success);
  echo "SessionId: $sessionId<br>";
  
  // update settings
  $updateSettings = UpdateSettings($soap, $sessionId, $settings, $success);
  if($success)
    echo "UpdateSettings: Successful<br>";
  else {
    echo "UpdateSettings: ";
    print_r($updateSettings);
    echo "<br>";
  }
  
  // get tag ids
  $tagNames = array(
    $config -> dataset . ".Tag 0001",
    $config -> dataset . ".Tag 0002",
    $config -> dataset . ".Tag 0003",
    $config -> dataset . ".Tag 0004"
  );
  $equation = "Value * 10";
  $timeExtension = true;
  $tags = new stdClass();
  $tags -> Tag = array();
  for($i = 0; $i < count($tagNames); $i++)
    $tags -> Tag[] = new Tag($tagNames[$i], $equation, $timeExtension);
  $tagIds = GetTagIds($soap, $sessionId, $tags, $success);
  echo "TagIds: ";
  print_r($tagIds);
  echo "<br>";
  
  // current datetime value
  date_default_timezone_set("America/New_York");
  $dateTime1 = new DateTimeExact();
  $now = $dateTime1 -> ToString();
  $next = new DateTimeExact();
  
  // build array of tvqs
  $tvqs = new stdClass();
  $tvqs -> TVQ = array();
  for($i = 0; $i < 500; $i++) {
    foreach($tagIds as $tagId) {
      $tvqs -> TVQ[] = new TVQ($tagId, $next -> ToString(), $i, 192);
    }
    $next -> AddMilliseconds(1);
  }
  
  // build array of properties
  $properties = new stdClass();
  $properties -> Property = array();
  foreach($tagIds as $tagId) {
    $properties -> Property[] = new Property($tagId, "Description", $now, "Name", "Value", 192);
  }
  
  // build array of annotations
  $annotations = new stdClass();
  $annotations -> Annotation = array();
  foreach($tagIds as $tagId) {
    $annotations -> Annotation[] = new Annotation($tagId, $now, $now, "User", "Annotation");
  }

  // write data to historian
  $storeData = StoreData($soap, $sessionId, $tvqs, $properties, $annotations, $success);
  if($success)
    echo "StoreData: Successful<br>";
  else {
    echo "StoreData: ";
    print_r($storeData);
    echo "<br>";
  }

  // write no data qualities at the end of data
  $noData = NoData($soap, $sessionId, $tagIds, $success);
  if($success)
    echo "NoData: Successful<br>";
  else {
    echo "NoData: ";
    print_r($noData);
    echo "<br>";
  }

  /*
  // create new file
  $historianFile = new HistorianFile($config -> dataset, $now);
  $createNewFile = CreateNewFile($soap, $sessionId, $historianFile, $success);
  if($success)
    echo "CreateNewFile: Successful<br>";
  else {
    echo "CreateNewFile: ";
    print_r($createNewFile);
    echo "<br>";
  }

  // rollup dataset file
  $historianFile = new HistorianFile($config -> dataset, $now);
  $fileRollOver = FileRollOver($soap, $sessionId, $historianFile, $success);
  if($success)
    echo "FileRollOver: Successful<br>";
  else {
    echo "FileRollOver: ";
    print_r($fileRollOver);
    echo "<br>";
  }
  */

  sleep(5);

  // get errors that occurred while sending/writing data
  // NOTE: 'TrackErrors' setting must be set to true
  $getErrors = GetErrors($soap, $sessionId, $errors, $success);
  if($success)
    echo "GetErrors: Successful<br>";
  else {
    echo "GetErrors: ";
    print_r($getErrors);
    echo "<br>";
  }
  echo "Errors: ";
  print_r($errors);
  echo "<br>";

  // release session
  $release = ReleaseSession($soap, $sessionId, $success);
  if($success)
    echo "Release: Successful<br>";
  else {
    echo "Release: ";
    print_r($release);
    echo "<br>";
  }

  /*
  // ------------------------ //
  // WITHOUT INCLUDED CLASSES //
  // ------------------------ //

  // build array of settings
  $setting = new stdClass();
  $setting -> name = "AutoCreateDataSets";
  $setting -> value = EncodeObject(true);
  $settings = new stdClass();
  $settings -> Setting = array();
  $settings -> Setting[] = $setting;

  // build array of tvqs
  $tvqs = new stdClass();
  $tvqs -> TVQ = array();
  foreach($tagIds as $tagId) {
    $tvq = new stdClass();
    $tvq -> id = $tagId;
    $tvq -> timestamp = $now;
    $tvq -> value = EncodeObject(rand(0, 100));
    $tvq -> quality = 192;
    $tvqs -> TVQ[] = $tvq;
  }

  // build array of properties
  $properties = new stdClass();
  $properties -> Property = array();
  foreach($tagIds as $tagId) {
    $property = new stdClass();
    $property -> id = $tagId;
    $property -> description = "Description";
    $property -> timestamp = $now;
    $property -> name = "Name";
    $property -> value = EncodeObject("Value");
    $property -> quality = 192;
    $properties -> Property[] = $property;
  }

  // build array of annotations
  $annotations = new stdClass();
  $annotations -> Annotation = array();
  foreach($tagIds as $tagId) {
    $annotation = new stdClass();
    $annotation -> id = $tagId;
    $annotation -> timestamp = $now;
    $annotation -> createdAt = $now;
    $annotation -> user = "User";
    $annotation -> value = EncodeObject("Annotation");
    $annotations -> Annotation[] = $annotation;
  }

  // historian file
  $historianFile = new stdClass();
  $historianFile -> dataSet = $config -> dataset;
  $historianFile -> fileTime = $now;
  */
?>