using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transporter.Service;

namespace Transporter
{
    public class Transporter
    {
        public Client transporterClient;
        public RConfig transporterConfig;

        public event DataEventHandler onSClientGetData = delegate { };
        public event EventHandler onSClientDataListenerCreated = delegate { };
        public event EventHandler onSClientMessageListenerCreated = delegate { };
        public event EventHandler onSClientDataListenerClosed = delegate { };
        public event EventHandler onSClientMessageListenerClosed = delegate { };
        public event EventHandler onDClientDataListenerCreated = delegate { };
        public event EventHandler onDClientCancel = delegate { };
        public event EventHandler<Exception> onSClientError = delegate { };

        private List<byte[]> listDataBlocks;

        public Transporter(bool isMaster)
        {
            transporterConfig = new RConfig(isMaster);
            transporterClient = new Client(transporterConfig);

            SetEvents();
        }

        public Transporter(bool isMaster, string destinationClientIP)
        {
            transporterConfig = new RConfig(isMaster, destinationClientIP);
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

        public void StartService()
        {
            transporterClient.StartListeningMesages();
        }

        public void SendObject(object obj)
        {
            Metadata objectMetadata = new Metadata();
            try
            {
                byte[] dataMass = transporterClient.ObjectToByteArray(obj);
                listDataBlocks = DevideByteMass(ref objectMetadata, dataMass);
                transporterClient.onGetDataListenerCreated += SendObject_onDataListenerCreated;
                transporterClient.onGetDataListenerCreated += transporterClient_onGetDataListenerCreated;
                transporterClient.SendMessage(new Message { messageCommands = MessageCommands.OpenDataListener, metadata = objectMetadata });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SendObject_onDataListenerCreated(object sender, EventArgs e)
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
            }
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
