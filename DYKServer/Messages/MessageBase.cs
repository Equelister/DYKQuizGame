using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Messages
{
    [Serializable]
    public class MessageBase
    {
        public string message { get; set; }
    }
}
