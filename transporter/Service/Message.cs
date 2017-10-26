using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransporterLib.Service
{
    /// <summary>
    /// Сообщение с командой и метаданными.
    /// </summary>
    [Serializable]
    public class Message
    {
        /// <summary>
        /// Команда удаленному клиенту.
        /// </summary>
        public MessageCommands messageCommands { get; set; }
        /// <summary>
        /// Метаданные о пересылаемых данных.
        /// </summary>
        public Metadata metadata { get; set; } 
    }
}
