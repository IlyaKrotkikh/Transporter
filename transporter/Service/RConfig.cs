using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Transporter.Service
{
    /// <summary>
    /// Конфигурация клиента.
    /// </summary>
    public class RConfig
    {
        public IPEndPoint messageSEndPoint { get; set; } // Source message end point
        public IPEndPoint messageDEndPoint { get; set; } // Destination message end point
        public IPEndPoint dataSEndPoint { get; set; }    // Source data end point
        public IPEndPoint dataDEndPoint { get; set; }    // Destination data end point

        /// <summary>
        /// Конструктор на локальную работу.
        /// </summary>
        /// <param name="isSource">true - отправитель; false - получатель</param>
        public RConfig(bool isSource)
        {
            // Для локальной работы двух клиентов 
            // требуется резервация дополнительных портов. 

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

        /// <summary>
        /// Конфигурация для работы по локальной сети
        /// </summary>
        /// <param name="sourceIP">IP адрес отправителя</param>
        /// <param name="destinationIP">IP адрес получателя</param>
        public RConfig(string sourceIP , string destinationIP)
        {
            int messagePort = 8080;
            int dataPort = 8082;
            IPAddress SIP = IPAddress.Parse(sourceIP);
            IPAddress DIP = IPAddress.Parse(destinationIP);

            messageSEndPoint = new IPEndPoint(SIP, messagePort);
            messageDEndPoint = new IPEndPoint(DIP, messagePort);
            dataSEndPoint = new IPEndPoint(SIP, dataPort);
            dataDEndPoint = new IPEndPoint(DIP, dataPort);
        }

        public RConfig(IPAddress sourceIP, IPAddress destinationIP)
        {
            int messagePort = 8080;
            int dataPort = 8082;
            IPAddress SIP = sourceIP;
            IPAddress DIP = destinationIP;

            messageSEndPoint = new IPEndPoint(SIP, messagePort);
            messageDEndPoint = new IPEndPoint(DIP, messagePort);
            dataSEndPoint = new IPEndPoint(SIP, dataPort);
            dataDEndPoint = new IPEndPoint(DIP, dataPort);
        }
    }
}