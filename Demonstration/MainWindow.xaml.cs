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
        Transporter.Transporter mTransporter, sTransporter;
        Metadata testMetadata;
        byte[] testData;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            mTransporter = new Transporter.Transporter(true);
            sTransporter = new Transporter.Transporter(false);
        }

        private void btnRunMessageListenerM_Click(object sender, RoutedEventArgs e)
        {
            mTransporter.StartService();
        }

        private void btnRunMessageListenerS_Click(object sender, RoutedEventArgs e)
        {
            sTransporter.StartService();
        }

        private void btnRunDataListenerM_Click(object sender, RoutedEventArgs e)
        {
            mTransporter.transporterClient.StartListeningData();
        }

        private void btnRunDataListenerS_Click(object sender, RoutedEventArgs e)
        {
            sTransporter.transporterClient.StartListeningData();
        }

        private void btnSendOpenDataListenerS_Click(object sender, RoutedEventArgs e)
        {
            mTransporter.transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.OpenDataListener, metadata = new Metadata() });
        }

        private void btnSendDataListenerCreatedS_Click(object sender, RoutedEventArgs e)
        {
            mTransporter.transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.DataListenerCreated });
        }

        private void btnSendIsFreeS_Click(object sender, RoutedEventArgs e)
        {
            mTransporter.transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.IsFree });
        }

        private void btnSendTestDataS_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == true)
            {
                Stream fs = of.OpenFile();

                testData = new byte[fs.Length];
                fs.Read(testData, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                mTransporter.SendObject(testData);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
