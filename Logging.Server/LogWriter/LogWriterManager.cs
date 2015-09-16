using System;
using System.Configuration;

namespace PLU.Logging.Server.Writer
{
    internal sealed class LogWriterManager
    {
        private LogWriterManager()
        { }

        public static ILogWriter GetLogWriter()
        {
            string LoggingStorage = ConfigurationManager.AppSettings["LoggingStorage"];

            if (LoggingStorage.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
            {
                return new MongoDbWriter();
            }
            else if (LoggingStorage.Equals("hbase", StringComparison.OrdinalIgnoreCase))
            {
                return new HBaseWriter();
            }
            else
            {
                return new MongoDbWriter();
            }
        }
    }
}