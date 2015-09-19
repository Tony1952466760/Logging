namespace csharp Logging.Client   #  ×¢ÊÍ1

struct TLogEntity {   #  ×¢ÊÍ2 
    1: string Title 
    2: string Message 
    3: byte   Level 
    4: i64    Time 
    5: i64		IP
	6: i32    AppId
	7: string    Source
	8: i32       Thread
    9: map<string,string> Tags
  }

  struct TMetricEntity {   #  ×¢ÊÍ2 
    1: string Name 
    2: double Value 
    3: i64    Time 
    4: map<string,string> Tags
  }

struct TMessage {   #  ×¢ÊÍ2 
    1: list<TLogEntity> logEntities 
    2: list<TMetricEntity> metricEntities
  }


service LogTransferService {  #  ×¢ÊÍ3 
   
   void Log(1:TMessage msg)  
}
