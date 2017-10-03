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
        public int messageMPort { get; set; }
        public int messageSPort { get; set; }
        public int dataMPort { get; set; }
        public int dataSPort { get; set; }
        public IPAddress MIP { get; set; }
        public IPAddress SIP { get; set; }

        public IPEndPoint messageMEndPoint { get; set; }
        public IPEndPoint messageSEndPoint { get; set; }
        public IPEndPoint dataMEndPoint { get; set; }
        public IPEndPoint dataSEndPoint { get; set; }

        public RConfig(bool isMaster)
        {
            MIP = SIP = IPAddress.Loopback;
            if (isMaster)
            {
                messageMPort = 8080;
                messageSPort = 8081;
                dataMPort = 8082;
                dataSPort = 8083;
            }
            else
            {
                messageMPort = 8081;
                messageSPort = 8080;
                dataMPort = 8083;
                dataSPort = 8082;
            }

            messageMEndPoint = new IPEndPoint(MIP, messageMPort);
            messageSEndPoint = new IPEndPoint(SIP, messageSPort);
            dataMEndPoint = new IPEndPoint(MIP, dataMPort);
            dataSEndPoint = new IPEndPoint(SIP, dataSPort);


        }

        public RConfig(bool isMaster, string slaveIP)
        {
            MIP = IPAddress.Loopback;
            SIP = IPAddress.Parse(slaveIP);

            if (isMaster)
            {
                messageMPort = 8080;
                messageSPort = 8081;
                dataMPort = 8082;
                dataSPort = 8083;
            }
            else
            {
                messageMPort = 8081;
                messageSPort = 8080;
                dataMPort = 8083;
                dataSPort = 8082;
            }

            messageMEndPoint = new IPEndPoint(MIP, messageMPort);
            messageSEndPoint = new IPEndPoint(SIP, messageSPort);
            dataMEndPoint = new IPEndPoint(MIP, dataMPort);
            dataSEndPoint = new IPEndPoint(SIP, dataSPort);
        }
    }
}