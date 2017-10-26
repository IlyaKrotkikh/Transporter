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
using TransporterLib.Service;
using TransporterLib;
using Microsoft.Win32;
using System.IO;
using System.Net;
using System.Globalization;

namespace Demonstration
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private TransporterLib.Transporter demoTransporter;
        public ObservableCollection<string> messageLogCollection { get; set; }
        public ObservableCollection<IPAddress> sourceIPListCollection { get; set; }
        private byte[] testData;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            demoTransporter = new TransporterLib.Transporter(true);
            messageLogCollection = new ObservableCollection<string>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            sourceIPListCollection = new ObservableCollection<IPAddress>(host.AddressList);
            InitializeComponent();

            RootGrid.DataContext = this;

            demoTransporter.onDClientCancel += Transporter_onCancel;
            demoTransporter.onSClientDataListenerCreated += Transporter_onSClientDataListenerCreated;
            demoTransporter.onSClientDataListenerClosed += Transporter_onSClientDataListenerClosed;
            demoTransporter.onSClientError += Transporter_onSClientError;
            demoTransporter.onSClientGetData += sTransporter_onGetData;
            demoTransporter.onSClientMessageListenerClosed += Transporter_onSClientMessageListenerClosed;
            demoTransporter.onSClientMessageListenerCreated += Transporter_onSClientMessageListenerCreated;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            demoTransporter.onDClientCancel -= Transporter_onCancel;
            demoTransporter.onSClientDataListenerCreated -= Transporter_onSClientDataListenerCreated;
            demoTransporter.onSClientDataListenerClosed -= Transporter_onSClientDataListenerClosed;
            demoTransporter.onSClientError -= Transporter_onSClientError;
            demoTransporter.onSClientGetData -= sTransporter_onGetData;
            demoTransporter.onSClientMessageListenerClosed -= Transporter_onSClientMessageListenerClosed;
            demoTransporter.onSClientMessageListenerCreated -= Transporter_onSClientMessageListenerCreated;
        }

        private void btnRunMessageListenerS_Click(object sender, RoutedEventArgs e)
        {
            demoTransporter.StartService();
        }

        private void btnStopMessageListenerS_Click(object sender, RoutedEventArgs e)
        {
            demoTransporter.StopService();
        }

        private void btnSendOpenDataListenerD_Click(object sender, RoutedEventArgs e)
        {
            demoTransporter.transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.OpenDataListener, metadata = new Metadata() });
        }

        private void btnSendIsFreeD_Click(object sender, RoutedEventArgs e)
        {
            demoTransporter.transporterClient.SendMessage(new Message() { messageCommands = MessageCommands.IsFree });
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
                demoTransporter.SendObject(testData);
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
            demoTransporter.SendObject(data);
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

        private void btnSetRconfig(object sender, RoutedEventArgs e)
        {
            try
            {
                RConfig newConfig;
                demoTransporter.onSClientGetData -= dTransporter_onGetData;
                demoTransporter.onSClientGetData -= sTransporter_onGetData;

                if (chkIsLocalSet.IsChecked == true)
                {
                    if (rbIsSource.IsChecked == true)
                    {
                        demoTransporter.onSClientGetData += sTransporter_onGetData;
                        messageLogCollection.Add("Client set as Source");
                        newConfig = new RConfig(true);
                    }
                    else
                    {
                        demoTransporter.onSClientGetData += dTransporter_onGetData;
                        messageLogCollection.Add("Client set as Destination");
                        newConfig = new RConfig(false);
                    }
                }
                else
                {
                    IPAddress dIpAddress = IPAddress.Parse(txtDestinationIP.Text);
                    IPAddress sIpAddress = (IPAddress)cmbSourceIP.SelectedItem;
                    if (rbIsSource.IsChecked == true)
                    {
                        demoTransporter.onSClientGetData += sTransporter_onGetData;
                        messageLogCollection.Add("Client set as Source");
                    }
                    else
                    {
                        demoTransporter.onSClientGetData += dTransporter_onGetData;
                        messageLogCollection.Add("Client set as Destination");
                    }
                    newConfig = new RConfig(sIpAddress, dIpAddress);
                }
                demoTransporter.SetConfig(newConfig);
                messageLogCollection.Add("The configuration has been updated");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong config setup! \n" + ex.Message);
            }
        }

        private void btnSetSIP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress ip = (IPAddress)cmbSourceIP.SelectedItem;
                demoTransporter.transporterConfig.dataSEndPoint.Address = ip;
                demoTransporter.transporterConfig.messageSEndPoint.Address = ip;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong ip address! \n" + ex.Message);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnNewClient_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newClient = new MainWindow();
            newClient.Show();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility result;
            if ((bool)value == true)
                result = Visibility.Hidden;
            else result = Visibility.Visible;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;
            if ((Visibility)value == Visibility.Hidden)
                result = true;
            else result = false;

            return result;
        }
    }
}
