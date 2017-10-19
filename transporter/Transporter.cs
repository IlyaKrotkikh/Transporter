using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Transporter.Service;

namespace Transporter
{
    public class Transporter
    {
        private List<byte[]> listDataBlocks; // Список блоков отсылаемого объекта
        private bool transporterIsFree; // Блок на создание новой задачи
        private bool sendObjectIsFree; // Блокировка коллекции передаваемых данных
        private CancellationTokenSource ctsWaitWork; // Токен остановки задачи на прекращение ожидания ответа от клиента

        public Client transporterClient { get; private set; } // Клиент
        public RConfig transporterConfig { get; private set; } // Конфиг клиента

        // Проброс событий из клиента
        public event DataEventHandler onSClientGetData = delegate { };
        public event EventHandler onSClientDataListenerCreated = delegate { };
        public event EventHandler onSClientMessageListenerCreated = delegate { };
        public event EventHandler onSClientDataListenerClosed = delegate { };
        public event EventHandler onSClientMessageListenerClosed = delegate { };
        public event EventHandler onDClientDataListenerCreated = delegate { };
        public event EventHandler onDClientCancel = delegate { };
        public event EventHandler<Exception> onSClientError = delegate { };

        /// <summary>
        /// Конструктор для локальной работы.
        /// </summary>
        /// <param name="isSource">true - отправитель; false - получатель</param>
        public Transporter(bool isSource)
        {
            transporterConfig = new RConfig(isSource); // Инициализируем конфиг как локальный
            transporterClient = new Client(transporterConfig); // Инициализируем клиент с конфигурацией

            SetEvents();
        }

        /// <summary>
        /// Конструктор для работы в локальной сети.
        /// </summary>
        /// <param name="sourceClientIP">IP адрес отправителя в локальной сети</param>
        /// <param name="destinationClientIP">IP адрес получателя в локальной сети</param>
        public Transporter(string sourceClientIP, string destinationClientIP)
        {
            transporterConfig = new RConfig(sourceClientIP, destinationClientIP);
            transporterClient = new Client(transporterConfig);

            SetEvents();
        }

        /// <summary>
        /// Конструктор для работы в локальной сети.
        /// </summary>
        /// <param name="sourceClientIP">IP адрес отправителя в локальной сети</param>
        /// <param name="destinationClientIP">IP адрес получателя в локальной сети</param>
        public Transporter(IPAddress sourceClientIP, IPAddress destinationClientIP)
        {
            transporterConfig = new RConfig(sourceClientIP, destinationClientIP);
            transporterClient = new Client(transporterConfig);

            SetEvents();
        }

        /// <summary>
        /// Подписывается на события клиента.
        /// </summary>
        private void SetEvents()
        {
            transporterClient.onCancel += transporterClient_onCancel;
            transporterClient.onGetData += transporterClient_onGetData;
            transporterClient.onClientError += transporterClient_onClientError;

            transporterClient.onDataListenerCreated += transporterClient_onDataListenerCreated;
            transporterClient.onDataListenerClosed += transporterClient_onDataListenerClosed;
            transporterClient.onMessageListenerCreated += transporterClient_onMessageListenerCreated;
            transporterClient.onMessageListenerClosed += transporterClient_onMessageListenerClosed;
        }

        /// <summary>
        /// Горячая замена конфигурации.
        /// </summary>
        /// <param name="config">Новый конфиг</param>
        public void SetConfig(RConfig config)
        {
            this.transporterConfig = config;
            transporterClient.config = config;
        }

        /// <summary>
        /// Запускает работу сервиса.
        /// </summary>
        public void StartService()
        {
            transporterClient.StartListeningMesages();
            transporterIsFree = true;
            sendObjectIsFree = true;
        }

        /// <summary>
        /// Останавливает работу сервиса.
        /// </summary>
        public void StopService()
        {
            transporterIsFree = false;
            transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.CloseMessageListener }, transporterConfig.messageSEndPoint);
        }

        /// <summary>
        /// Отправить объект на удаленный клиент.
        /// </summary>
        /// <param name="obj"></param>
        public void SendObject(object obj)
        {
            Metadata objectMetadata = new Metadata();
            try
            {
                if (transporterIsFree != true || sendObjectIsFree != true)
                    throw new Exception("Impossible at the moment");
                sendObjectIsFree = false;

                byte[] dataMass = transporterClient.ObjectToByteArray(obj);
                listDataBlocks = DevideByteMass(ref objectMetadata, dataMass);
                transporterClient.onGetDataListenerCreated += SendObject_onDataListenerCreated;
                transporterClient.onGetDataListenerCreated += transporterClient_onGetDataListenerCreated;
                transporterClient.SendMessage(new Message { messageCommands = MessageCommands.OpenDataListener, metadata = objectMetadata });
                WaitAnswer(20);

            }
            catch (Exception ex)
            {
                transporterClient.onGetDataListenerCreated -= SendObject_onDataListenerCreated;
                transporterClient.onGetDataListenerCreated -= transporterClient_onGetDataListenerCreated;

                Console.WriteLine(ex);
                onSClientError(this, ex);
                sendObjectIsFree = true;
            }
        }

        /// <summary>
        /// Когда получает ответ о готовности принимать данные от удаленного клиента,
        /// отправляет их.
        /// </summary>
        /// <param name="sender">Ссылка на объект отправитель события</param>
        /// <param name="e">Аргументы события</param>
        private void SendObject_onDataListenerCreated(object sender, EventArgs e)
        {
            if (ctsWaitWork != null)
                ctsWaitWork.Cancel();

            transporterIsFree = false;
            Task sendTask = new Task(() =>
            {
                try
                {
                    int recivedBlocks = 0;
                    do
                    {
                        transporterClient.SendData(listDataBlocks[recivedBlocks++]);
                    } while (recivedBlocks < listDataBlocks.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    transporterClient.onGetDataListenerCreated -= SendObject_onDataListenerCreated;
                    transporterClient.onGetDataListenerCreated -= transporterClient_onGetDataListenerCreated;
                    transporterIsFree = true;
                    this.sendObjectIsFree = true;
                }
            });
            sendTask.Start();
        }

        /// <summary>
        /// ЗЗапускает задачу ожидания ответа от удаленного клиента,
        /// если ответ получен, задача прекращается, в противном случае
        /// операция по передаче отменяется.
        /// </summary>
        /// <param name="seconds"> Время ожидания в секундах</param>
        private void WaitAnswer(int seconds)
        {
            ctsWaitWork = new CancellationTokenSource();

            Task sleepTask = new Task(() => 
            {
                Thread.Sleep(seconds * 1000);

                ctsWaitWork.Token.ThrowIfCancellationRequested();

                onSClientError(this, new Exception("The answer was not received within the allotted time."));
                this.sendObjectIsFree = true;
            }, ctsWaitWork.Token);
            sleepTask.Start();
        }

        // I dont know how it make better.
        private void transporterClient_onGetDataListenerCreated(object sender, EventArgs e)
        {
            this.onDClientDataListenerCreated(sender, e);
        }

        private void transporterClient_onCancel(object sender, EventArgs e)
        {
            transporterClient.onGetDataListenerCreated -= SendObject_onDataListenerCreated;
            transporterClient.onGetDataListenerCreated -= transporterClient_onGetDataListenerCreated;
            this.onDClientCancel(sender, e);
        }

        private void transporterClient_onGetData(object data)
        {
            this.onSClientGetData(data);
        }

        private void transporterClient_onMessageListenerCreated(object sender, EventArgs e)
        {
            this.onSClientMessageListenerCreated(sender,e);
        }

        private void transporterClient_onDataListenerCreated(object sender, EventArgs e)
        {
            this.onSClientDataListenerCreated(sender, e);
        }

        private void transporterClient_onMessageListenerClosed(object sender, EventArgs e)
        {
            this.onSClientMessageListenerClosed(sender, e);
        }

        private void transporterClient_onDataListenerClosed(object sender, EventArgs e)
        {
            this.onSClientDataListenerClosed(sender, e);
        }

        private void transporterClient_onClientError(object sender, Exception e)
        {
            this.onSClientError(sender, e);
        }

        /// <summary>
        /// Делит массив байт с данными объект на блоки данных.
        /// </summary>
        /// <param name="metadata">Метадата для отправки на удаленный клиент</param>
        /// <param name="data">Данные</param>
        /// <returns></returns>
        public List<byte[]> DevideByteMass(ref Metadata metadata, byte[] data)
        {
            List<byte[]> listBlocks = new List<byte[]>();
            if (metadata == null)
                metadata = new Metadata();

            long copiedBytes = 0;
            uint blockCount = 0;
            int bytesRemain = data.Length;
            byte blockLength = metadata.bLength;

            do
            {
                if (bytesRemain < blockLength)
                    blockLength = Convert.ToByte(bytesRemain);

                byte[] cData = new byte[blockLength];
                Array.Copy(data, copiedBytes, cData, 0, blockLength);
                copiedBytes += blockLength;
                bytesRemain -= blockLength;
                blockCount++;
                listBlocks.Add(cData);
            } while (bytesRemain > 0);

            metadata.bCount = blockCount;
            return listBlocks;
        }
    }
}
