using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Transporter.Service;
using Transporter;
using Microsoft.Win32;
using System.IO;

namespace Demonstration
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Transporter.Transporter sTransporter, dTransporter;
        byte[] testData;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            sTransporter = new Transporter.Transporter(true);
            dTransporter = new Transporter.Transporter(false);

            sTransporter.onSClientGetData += sTransporter_onGetData;
            dTransporter.onSClientGetData += dTransporter_onGetData;
            sTransporter.onDClientCancell += Transporter_onCancell;
            dTransporter.onDClientCancell += Transporter_onCancell;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            sTransporter.onSClientGetData -= sTransporter_onGetData;
            dTransporter.onSClientGetData -= sTransporter_onGetData;
            sTransporter.onDClientCancell -= Transporter_onCancell;
            dTransporter.onDClientCancell -= Transporter_onCancell;
        }

        private void btnRunMessageListenerS_Click(object sender, RoutedEventArgs e)
        {
            sTransporter.StartService();
        }

        private void btnRunMessageListenerD_Click(object sender, RoutedEventArgs e)
        {
            dTransporter.StartService();
        }

        private void btnRunDataListenerS_Click(object sender, RoutedEventArgs e)
        {
            sTransporter.transporterClient.StartListeningData();
        }

        private void btnRunDataListenerD_Click(object sender, RoutedEventArgs e)
        {
            dTransporter.transporterClient.StartListeningData();
        }

        private void btnSendOpenDataListenerD_Click(object sender, RoutedEventArgs e)
        {
            sTransporter.transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.OpenDataListener, metadata = new Metadata() });
        }

        private void btnSendDataListenerCreatedD_Click(object sender, RoutedEventArgs e)
        {
            sTransporter.transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.DataListenerCreated });
        }

        private void btnSendIsFreeD_Click(object sender, RoutedEventArgs e)
        {
            sTransporter.transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.IsFree });
        }

        private void btnSendTestDataD_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == true)
            {
                Stream fs = of.OpenFile();

                testData = new byte[fs.Length];
                fs.Read(testData, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                sTransporter.SendObject(testData);
            }
        }

        private void sTransporter_onGetData(object data)
        {
            MessageBox.Show("Get data" + data.ToString(), "Source Client");
        }

        private void dTransporter_onGetData(object data)
        {
            MessageBox.Show("Get data" + data.ToString(), "Destination Client");
        }

        private void Transporter_onCancell(object sender, EventArgs e)
        {
            MessageBox.Show("Client send cancell message\n" + sender.ToString());
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
