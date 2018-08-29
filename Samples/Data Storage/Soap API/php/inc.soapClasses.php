<?php
  // php constants: http://php.net/manual/en/soap.constants.php
  $XSD = array(
    "string" => XSD_STRING,
    "boolean" => XSD_BOOLEAN,
    //"decimal" => XSD_DECIMAL,
    //"float" => XSD_FLOAT,
    "double" => XSD_DOUBLE,
    //"duration" => XSD_DURATION,
    //"datetime" => XSD_DATETIME,
    //"time" => XSD_TIME,
    //"date" => XSD_DATE,
    //"long" => XSD_LONG,
    "integer" => XSD_INTEGER,
    //"short" => XSD_SHORT,
    //"byte" => XSD_BYTE
  );
  
  function EncodeObject($value) {
    global $XSD;
    $type = gettype($value);
    return new SoapVar($value, $XSD[$type], $type, 'http://www.w3.org/2001/XMLSchema');
  }
  
  function TimeSpan($days, $hours, $minutes, $seconds, $milliseconds) {
    $timeSpan = "P";
    $timeSpan .= $days . "DT";
    $timeSpan .= $hours . "H";
    $timeSpan .= $minutes . "M";
    $timeSpan .= ($seconds + ($milliseconds / 1000)) . "S";
    return $timeSpan;
  }
  
  class DateTimeExact
  {
    private $seconds;
    private $microseconds;
    private $decimals = 3;
    
    public function DateTimeExact() {
      $time = microtime(true);
      $this -> seconds = $time;
      $this -> microseconds = round(($time - floor($time)) * 1000000, $this -> decimals - 6);
    }
    
    public function AddDays($days) {
      $this -> seconds += $days * 24 * 60 * 60;
    }
    
    public function AddHours($hours) {
      $this -> seconds += $hours * 60 * 60;
    }
    
    public function AddMinutes($minutes) {
      $this -> seconds += $minutes * 60;
    }
    
    public function AddSeconds($seconds) {
      $this -> seconds += $seconds;
    }
    
    public function AddMilliseconds($milliseconds) {
      $this -> microseconds += $milliseconds * 1000;
    }
    
    // HELP: http://stackoverflow.com/questions/17909871/getting-date-format-m-d-y-his-u-from-milliseconds
    public function ToString() {
      $microseconds = sprintf("%06d", $this -> microseconds);
      $date = new DateTime(date('Y-m-d H:i:s.' . $microseconds, $this -> seconds));
      return $date -> format('Y-m-d\TH:i:s.uP');
    }
  }
  
  class Setting
  {
    public $name;
    public $value;
    
    public function Setting($name, $value)
    {
      $this -> name = $name;
      $this -> value = EncodeObject($value);
    }
  }
  
  class Tag
  {
    public $name;
    public $equation;
    public $timeExtension;
    
    public function Tag($name, $equation, $timeExtension)
    {
      $this -> name = $name;
      $this -> equation = $equation;
      $this -> timeExtension = $timeExtension;
    }
  }
  
  class TVQ
  {
    public $id;
    public $timestamp;
    public $value;
    public $quality;
    
    public function TVQ($id, $timestamp, $value, $quality)
    {
      $this -> id = $id;
      $this -> timestamp = $timestamp;
      $this -> value = EncodeObject($value);
      $this -> quality = $quality;
    }
  }
  
  class Property
  {
    public $id;
    public $description;
    public $timestamp;
    public $name;
    public $value;
    public $quality;
    
    public function Property($id, $description, $timestamp, $name, $value, $quality)
    {
      $this -> id = $id;
      $this -> description = $description;
      $this -> timestamp = $timestamp;
      $this -> name = $name;
      $this -> value = EncodeObject($value);
      $this -> quality = $quality;
    }
  }
  
  class Annotation
  {
    public $id;
    public $timestamp;
    public $createdAt;
    public $user;
    public $value;
    
    public function Annotation($id, $timestamp, $createdAt, $user, $value)
    {
      $this -> id = $id;
      $this -> timestamp = $timestamp;
      $this -> createdAt = $createdAt;
      $this -> user = $user;
      $this -> value = EncodeObject($value);
    }
  }
  
  class HistorianFile
  {
    public $dataSet;
    public $fileTime;
    
    public function HistorianFile($dataSet, $fileTime)
    {
      $this -> dataSet = $dataSet;
      $this -> fileTime = $fileTime;
    }
  }
?>