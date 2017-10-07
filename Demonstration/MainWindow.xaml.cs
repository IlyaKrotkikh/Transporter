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
        private Transporter.Transporter sTransporter, dTransporter;
        public ObservableCollection<string> messageLogCollection { get; set; }
        private byte[] testData;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            sTransporter = new Transporter.Transporter(true);
            dTransporter = new Transporter.Transporter(false);
            messageLogCollection = new ObservableCollection<string>();

            InitializeComponent();

            RootGrid.DataContext = this;

            dTransporter.onDClientCancel += Transporter_onCancel;
            dTransporter.onSClientDataListenerCreated += Transporter_onSClientDataListenerCreated;
            dTransporter.onSClientDataListenerClosed += Transporter_onSClientDataListenerClosed;
            dTransporter.onSClientError += Transporter_onSClientError;
            dTransporter.onSClientGetData += dTransporter_onGetData;
            dTransporter.onSClientMessageListenerClosed += Transporter_onSClientMessageListenerClosed;
            dTransporter.onSClientMessageListenerCreated += Transporter_onSClientMessageListenerCreated;
            sTransporter.onDClientCancel += Transporter_onCancel;
            sTransporter.onSClientDataListenerCreated += Transporter_onSClientDataListenerCreated;
            sTransporter.onSClientDataListenerClosed += Transporter_onSClientDataListenerClosed;
            sTransporter.onSClientError += Transporter_onSClientError;
            sTransporter.onSClientGetData += sTransporter_onGetData;
            sTransporter.onSClientMessageListenerClosed += Transporter_onSClientMessageListenerClosed;
            sTransporter.onSClientMessageListenerCreated += Transporter_onSClientMessageListenerCreated;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            dTransporter.onDClientCancel -= Transporter_onCancel;
            dTransporter.onSClientDataListenerCreated -= Transporter_onSClientDataListenerCreated;
            dTransporter.onSClientDataListenerClosed -= Transporter_onSClientDataListenerClosed;
            dTransporter.onSClientError -= Transporter_onSClientError;
            dTransporter.onSClientGetData -= dTransporter_onGetData;
            dTransporter.onSClientMessageListenerClosed -= Transporter_onSClientMessageListenerClosed;
            dTransporter.onSClientMessageListenerCreated -= Transporter_onSClientMessageListenerCreated;
            sTransporter.onDClientCancel -= Transporter_onCancel;
            sTransporter.onSClientDataListenerCreated -= Transporter_onSClientDataListenerCreated;
            sTransporter.onSClientDataListenerClosed -= Transporter_onSClientDataListenerClosed;
            sTransporter.onSClientError -= Transporter_onSClientError;
            sTransporter.onSClientGetData -= sTransporter_onGetData;
            sTransporter.onSClientMessageListenerClosed -= Transporter_onSClientMessageListenerClosed;
            sTransporter.onSClientMessageListenerCreated -= Transporter_onSClientMessageListenerCreated;
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
            Dispatcher.Invoke(new Action(() =>
            {
                messageLogCollection.Add("Source Client: Get data" + data.ToString());
                byte[] bmassFile = data as byte[];
                if (bmassFile != null)
                {
                    SaveFileDialog sf = new SaveFileDialog();
                    if (sf.ShowDialog() == true)
                    {
                        Stream fs = sf.OpenFile();
                        fs.Write(bmassFile, 0, bmassFile.Length);
                        fs.Close();
                    }
                }
            }));
        }

        private void dTransporter_onGetData(object data)
        {
            Dispatcher.Invoke(new Action(() => 
            {
                messageLogCollection.Add("Destination Client: Get data" + data.ToString());
            }));
            dTransporter.SendObject(data);
        }

        private void Transporter_onCancel(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                messageLogCollection.Add(sender.ToString() + ": Client send cancel message");
            }));
        }

        private void Transporter_onSClientDataListenerCreated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                messageLogCollection.Add(sender.ToString() + ": Data listener was created");
            }));
        }

        private void Transporter_onSClientMessageListenerCreated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                messageLogCollection.Add(sender.ToString() + ": Message listener was created");
            }));
        }

        private void Transporter_onSClientDataListenerClosed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                messageLogCollection.Add(sender.ToString() + ": Data listener was closed");
            }));
        }

        private void Transporter_onSClientMessageListenerClosed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                messageLogCollection.Add(sender.ToString() + ": Message listener was closed");
            }));
        }

        private void Transporter_onSClientError(object sender, Exception e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                messageLogCollection.Add(sender.ToString() + e.Message);
            }));
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
