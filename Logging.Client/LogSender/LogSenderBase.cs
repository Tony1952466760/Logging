using System.Collections.Generic;

namespace PLU.Logging.Client
{
    internal abstract class LogSenderBase
    {
        protected int SENDER_TIMEOUT = 5000;

        public abstract void Send(IList<LogEntity> logEntities);
    }
}