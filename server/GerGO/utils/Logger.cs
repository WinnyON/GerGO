using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerGO.utils
{
    interface Logger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
}
