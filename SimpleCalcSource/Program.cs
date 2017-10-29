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
            // Получаем адреса машин.
            Console.WriteLine("Source IP:");
            sIP = Console.ReadLine();
            Console.WriteLine("Destination IP:");
            dIP = Console.ReadLine();

            // Объявили экземпляр класса Transporter и присвоили значение null.
            // Необходимо для того, чтобы объект был доступен в блоке finally.
            Transporter demoTransporter = null;
            try
            {
                // Объявляем и инициализируем конфигурацию.
                RConfig customConfig = new RConfig(true);
                // Такая конфигурация позволит работать и в локальном режиме, и по сети.
                customConfig.dataDEndPoint.Address = IPAddress.Parse(dIP);
                customConfig.dataSEndPoint.Address = IPAddress.Parse(sIP);
                customConfig.messageDEndPoint.Address = IPAddress.Parse(dIP);
                customConfig.messageSEndPoint.Address = IPAddress.Parse(sIP);

                // Инициализировали Transporter.
                demoTransporter = new Transporter(customConfig);
                demoTransporter.StartService(); //Запустили прослушку сообщений.

                // Запускаем калькулятор.
                RunCalc(demoTransporter);
            }
            catch(Exception ex)
            {
                // В случае возникновения ошибки, выводим в консоль сообщение.
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (demoTransporter != null)
                    demoTransporter.StopService(); // Выключаем прослушку сообщений ели Transporter инициализирован.
            }
        }

        /// <summary>
        /// Стартует логику калькулятора.
        /// </summary>
        /// <param name="transporter"> Инициализированный объект Transporter.</param>
        private static void RunCalc(Transporter transporter)
        {
            try
            {
                if (transporter == null)
                    throw new NullReferenceException("Transporter not initialised!");

                // Получаем аргументы функции и саму функцию.
                Console.WriteLine("Enter the argument \"a = \"");
                globalArgA = Console.ReadLine();
                Console.WriteLine("Enter the argument \"b = \"");
                globalArgB = Console.ReadLine();
                Console.WriteLine("Enter the function \"F(a, b) = \"");
                funcFab = Console.ReadLine();
                funcFab = funcFab.Replace("a", "$a");
                funcFab = funcFab.Replace("b", "$b");

                //Объявляем и инициализируем объект sMath.
                SourceMath sMath = new SourceMath(transporter);

                // Подписываемся на события Transporter-а.
                transporter.onSClientGetData += demoTransporter_onGetData;
                transporter.onSClientError += demoTransporter_onClientError;
                transporter.onDClientCancel += demoTransporter_onDClientCancel;

                // Отправляем для подсчета аргумент функции "A".
                sMath.SendArg(globalArgA);
                // Считаем аргумент функции "B"
                answerArgB = sMath.CalculateArg(globalArgB);
                // Статус подсчета аргумента "B" - true.
                argBIsCalc = true;
                // Выводим результат подсчета аргумента "B".
                Console.WriteLine("B = " + answerArgB);

                // Ожидаем подсчета всех аргументов.
                bool isComplete = false;
                while (!isComplete)
                {
                    if (argAIsCalc == true && argBIsCalc == true)
                    {
                        // Считаем функцию.
                        double answer = sMath.CalcFuncFab(funcFab, answerArgA, answerArgB);
                        // Выводим результат аргумента.
                        Console.WriteLine("Answer is: " + answer);
                        // Назначаем false для выхода из цикла
                        isComplete = true;
                    }
                    else
                        Thread.Sleep(50); // Если не готовы аргументы, спим 50 мс.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Выводим сообщение об ошибке.
            }

            finally
            {
                // Отписываемся от событий.
                transporter.onSClientGetData -= demoTransporter_onGetData;
                transporter.onSClientError -= demoTransporter_onClientError;
                transporter.onDClientCancel -= demoTransporter_onDClientCancel;
                Console.WriteLine("\nPress ENTER...");
                Console.ReadLine();
                // Запускаем заново калькулятор.
                RunCalc(transporter);
            }
        }
        /// <summary>
        /// Обработчик события onGetData (на получение данных от удаленного клиента).
        /// </summary>
        /// <param name="arg"> Полученный аргумент.</param>
        private static void demoTransporter_onGetData(object arg)
        {
            try
            {
                // Присваиваем полученный ответ, отведенной для этого переменной.
                answerArgA = (double)arg;
                // Выводим ответ в консоль.
                Console.WriteLine("A = " + answerArgA);
                // Статус подсчета аргумента "A" - true.
                argAIsCalc = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Выводим Сообщение об ошибке.
            }
        }

        /// <summary>
        /// Обработчик события на возникновение ошибки в клиенте.
        /// </summary>
        /// <param name="sender">Объект-генератор  события.</param>
        /// <param name="ex">Ошибка.</param>
        private static void demoTransporter_onClientError(object sender, Exception ex)
        {
            Console.WriteLine(sender.ToString() + ": " + ex.Message);
        }

        /// <summary>
        /// Обработчик события на сообщение Cancel от удаленного клиента.
        /// </summary>
        /// <param name="sender">Объект-генератор события.</param>
        /// <param name="e">объект EventArgs</param>
        private static void demoTransporter_onDClientCancel(object sender, EventArgs e)
        {
            Console.WriteLine(sender.ToString() + ": Destination client send Cancel signal");
        }
    }
}
