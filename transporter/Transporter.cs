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
        public event EventHandler onDClientDataListenerCreated = delegate { };
        public event EventHandler onDClientCancell = delegate { };

        private List<byte[]> listDataBlocks;

        public Transporter(bool isMaster)
        {
            transporterConfig = new RConfig(isMaster);
            transporterClient = new Client(transporterConfig);

            transporterClient.onCancell += transporterClient_onCancell;
            transporterClient.onGetDataListenerCreated += transporterClient_onDataListenerCreated;
            transporterClient.onGetData += transporterClient_onGetData;
        }

        public Transporter(bool isMaster, string clientIP)
        {
            transporterConfig = new RConfig(isMaster, clientIP);
            transporterClient = new Client(transporterConfig);

            transporterClient.onCancell += transporterClient_onCancell;
            transporterClient.onGetDataListenerCreated += transporterClient_onDataListenerCreated;
            transporterClient.onGetData += transporterClient_onGetData;
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
                transporterClient.onGetDataListenerCreated += transporterClient_onDataListenerCreated;
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
                transporterClient.onGetDataListenerCreated -= transporterClient_onDataListenerCreated;
            }
        }

        // I dont know how it make better.
        private void transporterClient_onDataListenerCreated(object sender, EventArgs e)
        {
            this.onDClientDataListenerCreated(sender, e);
        }

        private void transporterClient_onCancell(object sender, EventArgs e)
        {
            transporterClient.onGetDataListenerCreated -= SendObject_onDataListenerCreated;
            transporterClient.onGetDataListenerCreated -= transporterClient_onDataListenerCreated;
            this.onDClientCancell(sender, e);
        }

        private void transporterClient_onGetData(object data)
        {
            this.onSClientGetData(data);
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
