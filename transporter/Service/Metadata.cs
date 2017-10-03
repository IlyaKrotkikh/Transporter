using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Transporter.Service
{
    [Serializable]
    public class Metadata
    {
        public uint bCount { get; set; }
        public byte bLength { get; set; }

        public Metadata()
        {
            bCount = 1;
            bLength = 64;
        }

        public Metadata(uint blockCount, byte blockLength)
        {
            bCount = blockCount;
            bLength = blockLength;
        }
    }
}