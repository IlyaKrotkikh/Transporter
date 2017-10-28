using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransporterLib;
using TransporterLib.Service;

namespace SimpleCalcSource
{
    class Program
    {
        static string sIP;
        static string dIP;
        

        static string globalArgA;
        static string globalArgB;
        static double answerArgA;
        static double answerArgB;
        static string funcFab;

        static bool argAIsCalc = false;
        static bool argBIsCalc = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Source IP:");
            sIP = Console.ReadLine();
            Console.WriteLine("Destination IP:");
            dIP = Console.ReadLine();

            Transporter demoTransporter = null;
            try
            {
                RConfig customConfig = new RConfig(true);
                customConfig.dataDEndPoint.Address = IPAddress.Parse(dIP);
                customConfig.dataSEndPoint.Address = IPAddress.Parse(sIP);
                customConfig.messageDEndPoint.Address = IPAddress.Parse(dIP);
                customConfig.messageSEndPoint.Address = IPAddress.Parse(sIP);

                demoTransporter = new Transporter(customConfig);
                demoTransporter.StartService();

                RunCalc(demoTransporter);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (demoTransporter != null)
                    demoTransporter.StopService();
            }
        }

        private static void RunCalc(Transporter transporter)
        {
            try
            {
                Console.WriteLine("Enter the argument \"a = \"");
                globalArgA = Console.ReadLine();
                Console.WriteLine("Enter the argument \"b = \"");
                globalArgB = Console.ReadLine();
                Console.WriteLine("Enter the function \"F(a, b) = \"");
                funcFab = Console.ReadLine();
                funcFab = funcFab.Replace("a", "$a");
                funcFab = funcFab.Replace("b", "$b");

                SourceMath sMath = new SourceMath(transporter);

                transporter.onSClientGetData += demoTransporter_onGetData;
                transporter.onSClientError += demoTransporter_onClientError;
                transporter.onDClientCancel += demoTransporter_onDClientCancel;

                sMath.SendArg(globalArgA);
                answerArgB = sMath.CalculateArg(globalArgB);
                argBIsCalc = true;
                Console.WriteLine("B = " + answerArgB);

                bool isComplete = false;
                while (!isComplete)
                {
                    if (argAIsCalc == true && argBIsCalc == true)
                    {
                        double answer = sMath.CalcFuncFab(funcFab, answerArgA, answerArgB);
                        Console.WriteLine("Answer is: " + answer);
                        isComplete = true;
                    }
                    else
                        Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                transporter.onSClientGetData -= demoTransporter_onGetData;
                transporter.onSClientError -= demoTransporter_onClientError;
                transporter.onDClientCancel -= demoTransporter_onDClientCancel;
                Console.WriteLine("\nPress ENTER...");
                Console.ReadLine();
                RunCalc(transporter);
            }
        }

        private static void demoTransporter_onGetData(object arg)
        {
            try
            {
                answerArgA = (double)arg;
                Console.WriteLine("A = " + answerArgA);
                argAIsCalc = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
