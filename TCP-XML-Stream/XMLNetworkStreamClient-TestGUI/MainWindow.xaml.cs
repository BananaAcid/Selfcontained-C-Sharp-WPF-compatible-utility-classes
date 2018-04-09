using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using System.Xml.Linq;

namespace TCP_XML_Stream
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        XMLNetworkStreamClient one;

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            if (one == null)
            {
                Tuple<XMLNetworkStreamClient, Exception> ret = await XMLNetworkStreamClient.test();

                one = ret.Item1;

                Debug.Print("test started");

                checkBox.IsChecked = true;
            }
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            if (one.isConnected)
            {
                await one.sendData("hello");
            }
        }


        private async void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            var senderCB = (CheckBox)sender;

            if (senderCB.IsChecked is true )
            {
                textBox.Text += "\n" + "CONNECTING...";

                one = new XMLNetworkStreamClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2255));

                one.OnDataReceived += DataReceived;
                one.OnStatusChange += StatusChange;

                var ex = await one.connect();

                if (ex != null)
                {
                    textBox.Text += "\n" + ex;
                    senderCB.IsChecked = false;
                }
                else if (one.isConnected)
                {
                    textBox.Text += "\n" + "CONNECTED";
                }
                else
                {
                    textBox.Text += "\n" + "Connection is not available";
                    senderCB.IsChecked = false;
                }
            }
            else
            {
                textBox.Text += "\n" + "DISCONNECTING...";

                if (one.isConnected)
                {
                    one.disconnect();
                    one = null;

                    textBox.Text += "\n" + "DISCONNECTED";
                }
                else
                    textBox.Text += "\n" + "WAS DISCONNECTED";
            }
        }

        private void DataReceived(object sender, XElement e)
        {
            Debug.Print("ANSWER:" + e);
            textBox.Text += "\n" + e;
        }


        private void StatusChange(object sender, Boolean b)
        {
            textBox.Text += "\nStatus Change: connection " + (b ? "is ALIVE" : "is not ALIVE");

            Debug.Print("ALIVE?" + (b ? "is" : "is not"));

            if (!b)
            {
                checkBox.IsChecked = false;

                // RECONNECT
                Task.Run(async () =>
                {
                    Exception ex;

                    do
                    {
                        Debug.Print("try to reconnect");

                        Thread.Sleep(1000);
                        
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            textBox.Text += "\n" + "RE..CONNECTING...";
                        }));

                        ex = await one.connect();
                        
                    } while (ex != null && one != null); // error, but one is still set

                    if (ex == null)
                    {
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            checkBox.IsChecked = true;

                            textBox.Text += "\n" + "RE..CONNECTED";
                        }));

                        Debug.Print("RE..CONNECTED");
                    }
                    else
                        Debug.Print("ended.");
                    
                });
            }
        }
    }
}
