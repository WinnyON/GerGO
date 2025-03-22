using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerGO.utils
{
    class LoggerFactory
    {
        private static Logger loggerInstance = new ConsoleLogger();
        public static Logger GetLogger()
        {
            return loggerInstance;
        }
    }
}
