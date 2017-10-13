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
        private List<byte[]> listDataBlocks;
        private bool transporterIsFree;
        private bool sendObjectIsFree;
        private CancellationTokenSource ctsWaitWork;

        public Client transporterClient { get; private set; }
        public RConfig transporterConfig { get; private set; }

        public event DataEventHandler onSClientGetData = delegate { };
        public event EventHandler onSClientDataListenerCreated = delegate { };
        public event EventHandler onSClientMessageListenerCreated = delegate { };
        public event EventHandler onSClientDataListenerClosed = delegate { };
        public event EventHandler onSClientMessageListenerClosed = delegate { };
        public event EventHandler onDClientDataListenerCreated = delegate { };
        public event EventHandler onDClientCancel = delegate { };
        public event EventHandler<Exception> onSClientError = delegate { };


        public Transporter(bool isMaster)
        {
            transporterConfig = new RConfig(isMaster);
            transporterClient = new Client(transporterConfig);

            SetEvents();
        }

        public Transporter(bool isMaster, string sourceClientIP, string destinationClientIP)
        {
            transporterConfig = new RConfig(isMaster, sourceClientIP, destinationClientIP);
            transporterClient = new Client(transporterConfig);

            SetEvents();
        }

        public Transporter(bool isMaster, IPAddress sourceClientIP, IPAddress destinationClientIP)
        {
            transporterConfig = new RConfig(isMaster, sourceClientIP, destinationClientIP);
            transporterClient = new Client(transporterConfig);

            SetEvents();
        }

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

        public void SetConfig(RConfig config)
        {
            this.transporterConfig = config;
            transporterClient.config = config;
        }

        public void StartService()
        {
            transporterClient.StartListeningMesages();
            transporterIsFree = true;
            sendObjectIsFree = true;
        }

        public void StopService()
        {
            transporterIsFree = false;
            transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.CloseMessageListener }, transporterConfig.messageSEndPoint);
        }

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
                }
            });
            sendTask.Start();
        }

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
