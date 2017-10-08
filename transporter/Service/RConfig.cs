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
        public IPEndPoint messageSEndPoint { get; set; } // Source message end point
        public IPEndPoint messageDEndPoint { get; set; } // Destination message end point
        public IPEndPoint dataSEndPoint { get; set; }    // Source data end point
        public IPEndPoint dataDEndPoint { get; set; }    // Destination data end point

        public RConfig(bool isSource)
        {
            int messageSPort;   // Source message port
            int messageDPort;   // Destination message port
            int dataSPort;      // Source data port
            int dataDPort;      // Destination data port
            IPAddress SIP;      // Source IP
            IPAddress DIP;      // Destination IP

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

        public RConfig(bool isSource,string sourceIP , string destinationIP)
        {
            int messageSPort;   // Source message port
            int messageDPort;   // Destination message port
            int dataSPort;      // Source data port
            int dataDPort;      // Destination data port
            IPAddress SIP;      // Source IP
            IPAddress DIP;      // Destination IP

            SIP = IPAddress.Parse(sourceIP);
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