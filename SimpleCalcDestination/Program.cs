using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TransporterLib;
using TransporterLib.Service;
using Hef.Math;
using System.Threading;

namespace SimpleCalcDestination
{
    class Program
    {
        static string globalArg;
        static double answerArg;

        static void Main(string[] args)
        {
            string sIP;
            string dIP;

            Transporter demoTransporter = null;

            Console.WriteLine("Source IP:");
            sIP = Console.ReadLine();
            Console.WriteLine("Destination IP:");
            dIP = Console.ReadLine();

            try
            {
                RConfig customConfig = new RConfig(false);
                customConfig.dataDEndPoint.Address = IPAddress.Parse(dIP);
                customConfig.dataSEndPoint.Address = IPAddress.Parse(sIP);
                customConfig.messageDEndPoint.Address = IPAddress.Parse(dIP);
                customConfig.messageSEndPoint.Address = IPAddress.Parse(sIP);

                demoTransporter = new Transporter(customConfig);
                demoTransporter.StartService();

                demoTransporter.onSClientError += demoTransporter_onClientError;
                demoTransporter.onDClientCancel += demoTransporter_onDClientCancel;

                demoTransporter.onSClientGetData += (object arg) =>
                {
                    globalArg = (string)arg;
                    Interpreter destinationInterpretter = new Interpreter();
                    answerArg = destinationInterpretter.Calculate(globalArg);
                    Console.WriteLine("The argument:" + globalArg);
                    Console.WriteLine("Answer is: " + answerArg);

                    demoTransporter.SendObject(answerArg);
                };

                while (true)
                {
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (demoTransporter != null)
                    demoTransporter.StopService();
                demoTransporter.onSClientError -= demoTransporter_onClientError;
                demoTransporter.onDClientCancel -= demoTransporter_onDClientCancel;
                Console.WriteLine("\nPress ENTER to quit ...");
                Console.ReadLine();
            }
        }

        private static void demoTransporter_onClientError(object sender, Exception ex)
        {
            Console.WriteLine(sender.ToString() + ": " + ex.Message);
        }

        private static void demoTransporter_onDClientCancel(object sender, EventArgs e)
        {
            Console.WriteLine(sender.ToString() + ": Destination client send Cancel signal");
        }
    }
}
