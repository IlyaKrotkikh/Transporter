using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TransporterLib.Service
{
    /// <summary>
    /// Метаданные о передаваемом объекте.
    /// </summary>
    [Serializable]
    public class Metadata
    {
        public uint bCount { get; set; } // Количество блоков.
        public byte bLength { get; set; } // Длинна блока.

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Metadata()
        {
            bCount = 1;
            bLength = 64;
        }

        /// <summary>
        /// Конструктор с указанием количества блоков и их длинны.
        /// </summary>
        /// <param name="blockCount">Количество блоков</param>
        /// <param name="blockLength">Длинна блока</param>
        public Metadata(uint blockCount, byte blockLength)
        {
            bCount = blockCount;
            bLength = blockLength;
        }
    }
}