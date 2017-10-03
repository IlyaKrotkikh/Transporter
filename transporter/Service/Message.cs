using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transporter.Service
{
    [Serializable]
    public class Message
    {
        public MessageCommands messageCommands { get; set; }
        public Metadata metadata { get; set; } 
    }
}
