using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLU.Logging.Server.Writer
{
    interface ILogWriter
    {
         void Write(IList<LogEntity> logs);
    }
}
