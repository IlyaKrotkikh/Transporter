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

        private List<byte[]> listDataBlocks;


        public Transporter(bool isMaster)
        {
            transporterConfig = new RConfig(isMaster);
            transporterClient = new Client(transporterConfig);
        }

        public Transporter(bool isMaster, string clientIP)
        {
            transporterConfig = new RConfig(isMaster, clientIP);
            transporterClient = new Client(transporterConfig);
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
                transporterClient.onDataListenerCreated += SendObject_onDataListenerCreated;
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
                transporterClient.onDataListenerCreated -= SendObject_onDataListenerCreated;
            }
        }

        private void ClientRejection_onCancell(object sender, EventArgs e)
        {
            transporterClient.onDataListenerCreated -= SendObject_onDataListenerCreated;
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
