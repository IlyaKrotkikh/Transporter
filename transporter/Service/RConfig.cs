using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Transporter.Service
{
    public class RConfig
    {
        public int messageSPort { get; set; }   // Source message port
        public int messageDPort { get; set; }   // Destination message port
        public int dataSPort { get; set; }      // Source data port
        public int dataDPort { get; set; }      // Destination data port
        public IPAddress SIP { get; set; }      // Source IP
        public IPAddress DIP { get; set; }      // Destination IP

        public IPEndPoint messageSEndPoint { get; set; } // Source message end point
        public IPEndPoint messageDEndPoint { get; set; } // Destination message end point
        public IPEndPoint dataSEndPoint { get; set; }    // Source data end point
        public IPEndPoint dataDEndPoint { get; set; }    // Destination data end point

        public RConfig(bool isSource)
        {
            SIP = DIP = IPAddress.Loopback;
            if (isSource)
            {
                messageSPort = 8080;
                messageDPort = 8081;
                dataSPort = 8082;
                dataDPort = 8083;
            }
            else
            {
                messageSPort = 8081;
                messageDPort = 8080;
                dataSPort = 8083;
                dataDPort = 8082;
            }

            messageSEndPoint = new IPEndPoint(SIP, messageSPort);
            messageDEndPoint = new IPEndPoint(DIP, messageDPort);
            dataSEndPoint = new IPEndPoint(SIP, dataSPort);
            dataDEndPoint = new IPEndPoint(DIP, dataDPort);


        }

        public RConfig(bool isSource, string destinationIP)
        {
            SIP = IPAddress.Loopback;
            DIP = IPAddress.Parse(destinationIP);

            if (isSource)
            {
                messageSPort = 8080;
                messageDPort = 8081;
                dataSPort = 8082;
                dataDPort = 8083;
            }
            else
            {
                messageSPort = 8081;
                messageDPort = 8080;
                dataSPort = 8083;
                dataDPort = 8082;
            }

            messageSEndPoint = new IPEndPoint(SIP, messageSPort);
            messageDEndPoint = new IPEndPoint(DIP, messageDPort);
            dataSEndPoint = new IPEndPoint(SIP, dataSPort);
            dataDEndPoint = new IPEndPoint(DIP, dataDPort);
        }
    }
}