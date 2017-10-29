using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TransporterLib.Service
{
    public delegate void DataEventHandler(object data); // Делегат для события на получение данных.

    public class Client
    {
        public event DataEventHandler onGetData = delegate { };
        public event EventHandler<Exception> onClientError = delegate { };
        public event EventHandler onGetDataListenerCreated = delegate { };
        public event EventHandler onMessageListenerCreated = delegate { };
        public event EventHandler onDataListenerCreated = delegate { };
        public event EventHandler onMessageListenerClosed = delegate { };
        public event EventHandler onDataListenerClosed = delegate { };
        public event EventHandler onCancel = delegate { };

        public RConfig config { get; set; } // Текущий конфиг.

        private Task MessageListener { get; set; } // Задача просушки сообщений от удаленного клиента.
        private Task DataListener { get; set; } // Задача на получение данных от удаленного клиента.

        private Metadata tempMetadata { get; set; } // Временные метаданные о принимаемых данных. 
        private bool isDataListenerFree; // Блокировка операций с слушателем данных когда он работает.
        private bool isMessageListenerFree; // Блокировка операций с слушателем сообщений когда он работает.
        private bool messageListenerRunStatus; // Требуется для остановки слушателя сообщений.

        /// <summary>
        /// Конструктор клиента.
        /// </summary>
        /// <param name="config">Конфигурация клиента</param>
        public Client(RConfig config)
        {
            this.config = config;
            isDataListenerFree = true;
            isMessageListenerFree = true;
        }

        /// <summary>
        /// Управление реакцией на сообщение.
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        private bool MessageHandler(Message message)
        {
            bool status = false;
            switch (message.messageCommands)
            {
                case MessageCommands.OpenDataListener:
                    PrepareDataListener(message.metadata);
                    status = true;
                    break;
                case MessageCommands.CloseMessageListener:
                    StopListeningMesages();
                    break;
                case MessageCommands.DataListenerCreated:
                    //Порт прослушивается, уведомляем подписчиков, что можно отправлять данные.
                    onGetDataListenerCreated(this, null);
                    status = true;
                    break;
                case MessageCommands.IsFree:
                    //Добавить логику
                    status = false;
                    break;
                case MessageCommands.OK:
                    status = true;
                    break;
                case MessageCommands.Cancel:
                    onCancel(this, null);
                    status = false;
                    //Some code there
                    break;
                default:
                    status = false;
                    SendMessage(new Message() { messageCommands = MessageCommands.Cancel });
                    break;
            }
            return status;
        }

        /// <summary>
        /// Подготовка слушателя данных к работе.
        /// </summary>
        /// <param name="metadata">Метаданные о получаемых данных</param>
        private void PrepareDataListener(Metadata metadata)
        {
            if (isDataListenerFree)
            {
                isDataListenerFree = false;
                tempMetadata = metadata;
                StartListeningData();
            }
            else
            {
                SendMessage(new Message() { messageCommands = MessageCommands.Cancel });
                return;
            }
        }

        /// <summary>
        /// Отправляет сообщение на указанный адрес.
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        /// <param name="ipEndPoint">Удаленный IP адрес</param>
        public void SendMessage(Message message, IPEndPoint ipEndPoint)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.SendTo(ObjectToByteArray(message), ipEndPoint);
            }
            catch (Exception ex)
            {
                onClientError(this, ex);
                Console.WriteLine(ex);
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        /// <summary>
        /// Отправляет сообщение на удаленный адрес из конфига.
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        public void SendMessage(Message message)
        {
            this.SendMessage(message, config.messageDEndPoint);
        }

        /// <summary>
        /// Отправляет массив байт на удаленный клиент.
        /// </summary>
        /// <param name="data">Массив байт данных</param>
        public void SendData(byte[] data)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.SendTo(data, config.dataDEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                onClientError(this, ex);
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        /// <summary>
        /// Стартует задачу слушателя данных.
        /// </summary>
        public void StartListeningData()
        {
            DataListener = new Task(ListenData);
            DataListener.Start();
        }

        /// <summary>
        /// Стартует задачу слушателя сообщений.
        /// </summary>
        public void StartListeningMesages()
        {
            try
            {
                if (isMessageListenerFree)
                {
                    messageListenerRunStatus = true;
                    isMessageListenerFree = false;
                    MessageListener = new Task(ListenMesage);
                    MessageListener.Start();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                onClientError(this, ex);
            }
        }

        /// <summary>
        /// Переводит статус слушателя сообщений в выключение.
        /// </summary>
        private void StopListeningMesages()
        {
            messageListenerRunStatus = false;
        }

        /// <summary>
        /// Логика прослушки сообщений.
        /// </summary>
        private void ListenMesage()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(config.messageSEndPoint);
                onMessageListenerCreated(this, null);

                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
                while (messageListenerRunStatus)
                {
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[508]; // буфер для получаемых данных = мин. длинна ipv4 пакета
                    MemoryStream memoryStream = new MemoryStream();
                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    do
                    {
                        bytes = socket.ReceiveFrom(data, ref remoteIp);
                        memoryStream.Write(data, 0, bytes);
                    }
                    while (socket.Available > 0);

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    Message recivedMessage = binaryFormatter.Deserialize(memoryStream) as Message;
                    MessageHandler(recivedMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                onClientError(this, ex);
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                isMessageListenerFree = true;
                onMessageListenerClosed(this, null);
            }
        }

        /// <summary>
        /// Логика прослушки и получения данных.
        /// </summary>
        private void ListenData()
        {
            // Для передачи данных не обязательно использовать один и тот же сокет, что принимает данные.
            // Так как метод выполняется в другом потоке, в теории, может возникнуть ситуация, 
            // когда указатель на сокет будет указывать на сокет из последнего зарегистрированного задания.
            // Так же стоит блокировать новые задачи, до тех пор, пока не отработает текущий сокет, 
            // иначе новый сокет выдаст исключении о занятом порте.
            // Поэтому целесообразнее инициализировать и закрывать сокеты не выходя за пределы данного метода,
            // а так же, снимать блок, когда сокет будет закрыт. 

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
            uint blockCount = tempMetadata.bCount;

            try
            {
                socket.Bind(config.dataSEndPoint);
                SendMessage(new Message() { messageCommands = MessageCommands.DataListenerCreated });
                onDataListenerCreated(this,null);

                socket.ReceiveTimeout = 60000;// TODO вынести в конфиг

                while (blockCount-- > 0)
                {
                    int bytesCount = 0; // количество полученных байтов
                    byte[] buffer = new byte[tempMetadata.bLength]; // буфер для получаемых данных

                    bytesCount = socket.ReceiveFrom(buffer, ref remoteIp);
                    memoryStream.Write(buffer, 0, bytesCount);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                if (memoryStream.Length > 0)
                {
                    onGetData(binaryFormatter.Deserialize(memoryStream));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                onClientError(this, ex);
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both); // разорвать соединение!
                socket.Close();
                isDataListenerFree = true; // снять блокировку! isFree = true;
                onDataListenerClosed(this, null);
            }
        }

        /// <summary>
        /// Сериализует объект в массив байт.
        /// </summary>
        /// <param name="obj"> объект для сериализации</param>
        /// <returns></returns>
        public byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }
    }
}