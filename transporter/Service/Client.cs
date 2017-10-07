using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Transporter.Service
{
    public delegate void DataEventHandler(object data);

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

        private RConfig config { get; set; }

        private Task MessageListener { get; set; }
        private Task DataListener { get; set; }

        private Metadata tempMetadata { get; set; }
        private bool isFree;

        public Client(RConfig config)
        {
            this.config = config;
            MessageListener = new Task(ListenMesage);
            isFree = true;
        }

        private bool MessageHandler(Message message)
        {
            bool status = false;
            switch (message.messageCommands)
            {
                case MessageCommands.OpenDataListener:
                    PrepareDataListener(message.metadata);
                    status = true;
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

        private void PrepareDataListener(Metadata metadata)
        {
            if (isFree)
            {
                isFree = false;
                tempMetadata = metadata;
                StartListeningData();
            }
            else
            {
                SendMessage(new Message() { messageCommands = MessageCommands.Cancel });
                return;
            }
        }

        public void SendMessage(Message message)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.SendTo(ObjectToByteArray(message), config.messageDEndPoint);
            }
            catch (Exception ex)
            {
                //onClientError(this, ex);
                Console.WriteLine(ex);
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

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
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

        }

        public void StartListeningData()
        {
            DataListener = new Task(ListenData);
            DataListener.Start();
        }

        public void StartListeningMesages()
        {
            try
            {
                MessageListener.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ListenMesage()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(config.messageSEndPoint);
                onMessageListenerCreated(this, null);

                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
                while (true)
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
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                onMessageListenerClosed(this, null);
            }
        }

        private void ListenData()
        {
            // Для передачи данных не объязательно использовать один и тот же сокет, что принимает данные.
            // Так как метод выполняется в другом потоке, в теории, может возникнуть ситтуация, 
            // когда указатель на сокет будет указывать на сокет из последнего зарегистрированного таска.
            // Так же стоит блокировать новые задачи, до тех пор, пока не отработает текущий сокет, 
            // иначе ноый сокет выдаст исключении о занятом порте.
            // Поэтому целесообразнее инициализировать и закрывать сокеты не выхходя за пределы данного метода,
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

                    //if (socket.Available > 0)
                    //{
                        bytesCount = socket.ReceiveFrom(buffer, ref remoteIp);
                        memoryStream.Write(buffer,0, bytesCount);
                    //}
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
                isFree = true; // снять блокировку! isFree = true;
                onDataListenerClosed(this, null);
            }
        }

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