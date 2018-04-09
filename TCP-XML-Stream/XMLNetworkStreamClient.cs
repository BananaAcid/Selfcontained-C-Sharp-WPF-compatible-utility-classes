/**
 * MIT License
 * 
 * Copyright (c) 2017 "Nabil Redmann (BananaAcid) <repo@bananaacid.de>"
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace TCP_XML_Stream // .NET CORE compatible
{
    public class XMLNetworkStreamClient
    {
        public TcpClient _tcpClient;

        // Custom Param in EventHandler http://stackoverflow.com/a/19951714/1644202
        public EventHandler<XElement> OnDataReceived;
        public EventHandler<Boolean> OnStatusChange; // bool = isAlive

        private Task _reader;
        private IPEndPoint _remoteEndPoint;
        private CancellationTokenSource _taskCancellationToken;
        private CancellationTokenSource _taskCancellationTokenForWatcher = new CancellationTokenSource();

        public bool isConnected { get { return _tcpClient?.Connected ?? false; } } //  if (_tcpClient != null && _tcpClient.Connected) return true; else return false; 
        private bool _isAlive = false;
        public bool isAlive { get { return _isAlive; } }



        // Tuple the old way: http://stackoverflow.com/a/10278769/1644202
        public static async Task<Tuple<XMLNetworkStreamClient, Exception>> test()
        {
            // NODE-RED TEST SERVER: [{"id":"149cb080.14cbb","type":"tcp in","z":"2899ebae.74aa74","name":"Server holding connections at :2255","server":"server","host":"","port":"2255","datamode":"stream","datatype":"buffer","newline":"\\n","topic":"text","base64":false,"x":172,"y":208,"wires":[["89fad7bc.2d3d58","d502c12b.ba725"]]},{"id":"30d26cc3.d85624","type":"tcp out","z":"2899ebae.74aa74","host":"","port":"","beserver":"reply","base64":false,"end":false,"name":"reply: all open connections will be messaged","x":1182,"y":208,"wires":[]},{"id":"6b43d0a4.e23d9","type":"debug","z":"2899ebae.74aa74","name":"show incomming text at the debug tab","active":true,"console":"false","complete":"payload","x":482,"y":308,"wires":[]},{"id":"51790032.c9ee6","type":"inject","z":"2899ebae.74aa74","name":"button: create a msg","topic":"","payload":"","payloadType":"str","repeat":"","crontab":"","once":false,"x":852,"y":128,"wires":[["92be8cb4.45da8"]]},{"id":"89fad7bc.2d3d58","type":"template","z":"2899ebae.74aa74","name":"\"SERVER GOT:\" +","field":"payload","fieldType":"msg","format":"handlebars","syntax":"mustache","template":"SERVER GOT:\n{{payload}}","x":422,"y":268,"wires":[["6b43d0a4.e23d9"]]},{"id":"d502c12b.ba725","type":"template","z":"2899ebae.74aa74","name":"modify received msg to be send back","field":"payload","fieldType":"msg","format":"handlebars","syntax":"mustache","template":"<answer><note>You were sending</note><returning>{{payload}}</returning></answer>\n","x":682,"y":208,"wires":[["30d26cc3.d85624"]]},{"id":"92be8cb4.45da8","type":"template","z":"2899ebae.74aa74","name":"+ newline","field":"payload","fieldType":"msg","format":"handlebars","syntax":"mustache","template":"<root>\n    <a>a msg</a>\n</root>","x":1012,"y":128,"wires":[["30d26cc3.d85624"]]}]

            //IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            //IPAddress ipAddress = ipHostInfo.AddressList[0].MapToIPv4();

            // OR;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, 2255);



            var del = new EventHandler<XElement>((sender, e) =>
            {
                Debug.Print("GOT:" + e);
            });

            var delAlive = new EventHandler<Boolean>((sender, e) =>
            {
                Debug.Print("ALIVE?" + (e ? "is" : "is not"));
            });

            var con = new XMLNetworkStreamClient(remoteEndPoint) { /*OnDataReceivedAC = delAC*/ };
            con.OnDataReceived += del;
            con.OnStatusChange += delAlive;

            var error = await con.connect();


            if (error == null)
            {
                con.sendData("TEST init"); // do not await
            }
            

            return Tuple.Create(con, error);
        }

        public XMLNetworkStreamClient(IPEndPoint remoteEndPoint)
        {
            _remoteEndPoint = remoteEndPoint;

            // start connection watcher
            new Task(isAliveTest, _taskCancellationTokenForWatcher.Token).Start();
        }

        ~XMLNetworkStreamClient()
        {
            // let watchers close
            _taskCancellationTokenForWatcher.Cancel();
        }

        public async Task<Exception> connect()
        {
            try
            {
                _tcpClient = new TcpClient();

                await _tcpClient.ConnectAsync(_remoteEndPoint.Address, _remoteEndPoint.Port);
            }
            catch (Exception ex)
            {
                _tcpClient = null;
                Debug.Print("Connection Error: " + ex.ToString());

                return ex;
            }

            
            if (_tcpClient != null)
            {
                _taskCancellationToken = new CancellationTokenSource();
                
                _reader = new Task(getData, _taskCancellationToken.Token);
                _reader.Start();
            }

            _isAlive = true;
            // we do not want to await.
#pragma warning disable CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                OnStatusChange?.Invoke(this, true);
            }));
#pragma warning restore CS4014

            return null;
        }

       
        public void disconnect()
        {
            if (_tcpClient == null)
                return;

            // mark the token, so the XmlReader within the task knows, when kill it, it is intentionally
            _taskCancellationToken.Cancel();

            //_reader.Wait(); - hangs the app.

            // by closing the stream, the XmlReader within the task will throw an error - that is, how we close the task
            _tcpClient.GetStream().Close();

            //_reader.Dispose();  - we killed the task, no dispose needed.

            
            _tcpClient.Close();
            _tcpClient = null;

            _isAlive = false;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                OnStatusChange?.Invoke(this, false);
            }));
        }

        private void getData()
        {
            var cStream = _tcpClient.GetStream();
            XmlReader reader;

            try
            {
                reader = XmlReader.Create(cStream, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment });


                while (!_taskCancellationToken.IsCancellationRequested && reader.Read())
                {
                    // there might be crap in the pipeline (like newlines and more) , we just catch and ignore it
                    try
                    {
                        using (XmlReader subTreeReader = reader.ReadSubtree())
                        {
                            XElement xmlData = XElement.Load(subTreeReader);

                            // execute on main UI thread - http://stackoverflow.com/a/25966447/1644202
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                OnDataReceived?.Invoke(this, xmlData);
                            }));
                        }

                    }
                    catch (InvalidOperationException)
                    {
                        // expected error, there was crap in the pipeline
                    }
                    catch (Exception ex)
                    {
                        //unexpected error

                        Debug.Print(ex.ToString());
                        throw;
                    }

                }
            }
            catch (System.IO.IOException)
            {
                if (_taskCancellationToken.IsCancellationRequested) // killed
                    return;
                else
                    return; // server closed connection
            }
            
        }


        public async Task sendData(XElement data)
        {
            var cStream = _tcpClient.GetStream();

            var client = XmlWriter.Create(cStream);  // not using, will break the connection

            data.WriteTo(client);
            await client.FlushAsync();
        }

        public async Task sendData(string data)
        {
            var cStream = _tcpClient.GetStream();

            var client = new StreamWriter(cStream);  // not using, will break the connection
            
            await client.WriteAsync(data);
            await client.FlushAsync();
        }

        
        public async void isAliveTest()
        {
            int interval = 150;

            Boolean isAlive = false;

            while (!_taskCancellationTokenForWatcher.IsCancellationRequested)
            {
                isAlive = await Task.Run<Boolean>(() =>
                {
                    // http://stackoverflow.com/a/6993334/1644202
                    try
                    {
                        if (_tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.Connected)
                        {
                            /* pear to the documentation on Poll:
                             * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                             * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                             * -or- true if data is available for reading; 
                             * -or- true if the connection has been closed, reset, or terminated; 
                             * otherwise, returns false
                             */

                            // Detect if client disconnected
                            if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                            {
                                byte[] buff = new byte[1];
                                if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                                {
                                    // Client disconnected
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                });


                if (_isAlive != isAlive)
                {
                    _isAlive = isAlive;

#pragma warning disable CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
                    if (Application.Current != null) // app exit will throw here otherwise
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            OnStatusChange?.Invoke(this, isAlive);
                        }));
                    }
#pragma warning restore CS4014
                }

                Thread.Sleep(interval);
            }
        }
    }
    
}
