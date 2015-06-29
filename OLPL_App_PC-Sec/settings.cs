using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLPL_App_PC_Sec
{
    class settings
    {
        public string baseKey { get; set; }
        public string appKey { get; set; }
        public byte[] adminPass { get; set; }
        public byte[] adminName { get; set; }
        public string encPass { get; set; }
        public string logFile { get; set; }
        public string resultURL { get; set; }
        public byte[] connectUser { get; set; }
        public byte[] connectPass { get; set; }
        public byte[] adminPass1 {get;set;}
        public string timeChanged { get; set; }
        public int mode { get; set; }
    }
}
