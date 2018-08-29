<?php
  function CreateSoapClient($url, $wsdl, $user, $pswd) {
    // credentials
    if(isset($user) && isset($pwsd)) {
      return $soap = @new SoapClient($wsdl, array(
        'login' => $user,
        'password' => $pswd,
        'location' => $url, // location would be a different url from anonymous
        'exceptions' => true,
        'connection_timeout' => 60)
      );
    }
    // anonymous
    else {
      return $soap = @new SoapClient($wsdl, array(
        'trace' => 1,
        'location' => $url,
        'exceptions' => true,
        'connection_timeout' => 60)
      );
    }
  }
  
  function Version($soap) {
    $version = $soap -> Version();
    return $version -> VersionResult;
  }
  
  function GetDataSets($soap, $historian, &$success) {
    $parameters = new stdClass();
    $parameters -> historian = $historian;
    
    $getDataSets = $soap -> GetDataSets($parameters);
    $result = $getDataSets -> GetDataSetsResult;
    $success = !($getDataSets -> failed);
    return $result -> string;
  }
  
  function GetSessionId($soap, $historian, $clientId, $settings, &$success) {
    $parameters = new stdClass();
    $parameters -> historian = $historian;
    $parameters -> clientId = $clientId;
    $parameters -> settings = $settings;

    $getSessionId = $soap -> GetSessionId($parameters);
    $success = !($getSessionId -> failed);
    return $getSessionId -> GetSessionIdResult;
  }
  
  function UpdateSettings($soap, $sessionId, $settings, &$success) {
    $parameters = new stdClass();
    $parameters -> sessionId = $sessionId;
    $parameters -> settings = $settings;
    
    $updateSettings = $soap -> UpdateSettings($parameters);
    $success = !($updateSettings -> failed);
    return $updateSettings -> UpdateSettingsResult;
  }
  
  function GetTagIds($soap, $sessionId, $tags, &$success) {
    $parameters = new stdClass();
    $parameters -> sessionId = $sessionId;
    $parameters -> tags = $tags;
    
    $getTagIds = $soap -> GetTagIds($parameters);
    $result = $getTagIds -> GetTagIdsResult;
    $success = !($getTagIds -> failed);
    return $result -> anyType;
  }
  
  function StoreData($soap, $sessionId, $tvqs, $properties, $annotations, &$success) {
    $parameters = new stdClass();
    $parameters -> sessionId = $sessionId;
    $parameters -> tvqs = $tvqs;
    $parameters -> properties = $properties;
    $parameters -> annotations = $annotations;
    
    $storeData = $soap -> StoreData($parameters);
    $success = !($storeData -> failed);
    return $storeData -> StoreDataResult;
  }
  
  function NoData($soap, $sessionId, $tagIds, &$success) {
    $parameters = new stdClass();
    $parameters -> sessionId = $sessionId;
    $parameters -> tagIds = $tagIds;
    
    $noData = $soap -> NoData($parameters);
    $success = !($noData -> failed);
    return $noData -> NoDataResult;
  }
  
  function CreateNewFile($soap, $sessionId, $historianFile, &$success) {
    $parameters = new stdClass();
    $parameters -> sessionId = $sessionId;
    $parameters -> newFile = $historianFile;
    
    $createNewFile = $soap -> CreateNewFile($parameters);
    $success = !($createNewFile -> failed);
    return $createNewFile -> CreateNewFileResult;
  }
  
  function FileRollOver($soap, $sessionId, $historianFile, &$success) {
    $parameters = new stdClass();
    $parameters -> sessionId = $sessionId;
    $parameters -> rollOverFile = $historianFile;
    
    $fileRollOver = $soap -> FileRollOver($parameters);
    $success = !($fileRollOver -> failed);
    return $fileRollOver -> FileRollOverResult;
  }
  
  function GetErrors($soap, $sessionId, &$errors, &$success) {
    $parameters = new stdClass();
    $parameters -> sessionId = $sessionId;
    
    $getErrors = $soap -> GetErrors($parameters);
    $errors = $getErrors -> errors;
    $success = !($getErrors -> failed);
    return $getErrors -> GetErrorsResult;
  }
  
  function ReleaseSession($soap, $sessionId, &$success) {
    $parameters = new stdClass();
    $parameters -> sessionId = $sessionId;
    
    $releaseSession = $soap -> ReleaseSession($parameters);
    $success = !($releaseSession -> failed);
    return $releaseSession -> ReleaseSessionResult;
  }
?>