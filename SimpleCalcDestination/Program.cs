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
        /// <summary>
        /// Полученный аргумент.
        /// </summary>
        static string globalArg;
        /// <summary>
        /// Ответ, полученный при подсчете аргумента.
        /// </summary>
        static double answerArg;

        static void Main(string[] args)
        {
            // IP адреса удаленный и источник.
            string sIP;
            string dIP;

            // Объявили объект Transporter и присвоили значение null (для доступности в finally блоке).
            Transporter demoTransporter = null;

            // Получаем IP адреса.
            Console.WriteLine("Source IP:");
            sIP = Console.ReadLine();
            Console.WriteLine("Destination IP:");
            dIP = Console.ReadLine();

            try
            {
                // Инициализируем конфиг как локальный.
                RConfig customConfig = new RConfig(false);
                // Присваиваем IP адреса. В итоге имеем работу как локальную, так и по сети.
                customConfig.dataDEndPoint.Address = IPAddress.Parse(dIP);
                customConfig.dataSEndPoint.Address = IPAddress.Parse(sIP);
                customConfig.messageDEndPoint.Address = IPAddress.Parse(dIP);
                customConfig.messageSEndPoint.Address = IPAddress.Parse(sIP);

                // Инициализируем Transporter.
                demoTransporter = new Transporter(customConfig);
                demoTransporter.StartService(); // Стартуем сервис прослушки сообщений от клиента.

                // Подписываемся на события.
                demoTransporter.onSClientError += demoTransporter_onClientError;
                demoTransporter.onDClientCancel += demoTransporter_onDClientCancel;

                // Для простоты подписываемся следующим образом.
                demoTransporter.onSClientGetData += (object arg) =>
                {
                    // Присваиваем полученные данные переменной "globalArg".
                    globalArg = (string)arg;
                    // Производим вычисление.
                    Interpreter destinationInterpretter = new Interpreter();
                    // Записываем ответ в переменную.
                    answerArg = destinationInterpretter.Calculate(globalArg);

                    // Выводим результат в консоль.
                    Console.WriteLine("The argument:" + globalArg);
                    Console.WriteLine("Answer is: " + answerArg);

                    // Отправляем ответ обратно.
                    demoTransporter.SendObject(answerArg);
                };

                // Зацикливаем основной поток, чтобы программа не закрылась.
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
                    demoTransporter.StopService(); // Останавливаем сервис если инициализирован.
                // Отписываемся от событий.
                demoTransporter.onSClientError -= demoTransporter_onClientError;
                demoTransporter.onDClientCancel -= demoTransporter_onDClientCancel;
                Console.WriteLine("\nPress ENTER to quit ...");
                Console.ReadLine(); // По нажатию ENTER выходим из программы.
            }
        }

        /// <summary>
        /// Обработчик события при возникновении ошибки в клиенте.
        /// </summary>
        /// <param name="sender">Объект-генератор.</param>
        /// <param name="ex">Исключение.</param>
        private static void demoTransporter_onClientError(object sender, Exception ex)
        {
            Console.WriteLine(sender.ToString() + ": " + ex.Message);
        }

        /// <summary>
        /// Обработчик события на сообщение Cancel от удаленного клиента.
        /// </summary>
        /// <param name="sender">Объект-генератор события.</param>
        /// <param name="e">Объект EventArgs.</param>
        private static void demoTransporter_onDClientCancel(object sender, EventArgs e)
        {
            Console.WriteLine(sender.ToString() + ": Destination client send Cancel signal");
        }
    }
}
