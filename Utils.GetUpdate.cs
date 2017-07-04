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
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Utils
{
    /// <summary>
    /// handle updates
    /// </summary>
    /// <example>
    /// do not forget to use
    ///   Application.Current.Dispatcher.Invoke(() =>
    ///   { ... uiElement ... }
    ///   
    /// usage:
    ///    string VERSIONAPP = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    ///    string VERSIONAPI = 1
    ///    Uri REPO = new Uri($"http://domain.tld/update?v={VERSIONAPP}&api={VERSIONAPI}");
    /// 
    ///    var y = await Utils.GetUpdates.Instance.Check(REPO);
    ///    var x = await Utils.GetUpdates.Instance.Download(y.Updates[(int)y.Preferred];,
    ///    handlerProgress: (r, ev) =>
    ///    {
    ///        Application.Current.Dispatcher.Invoke(() =>
    ///        {
    ///            isUpdateProgress.Value2 = ev.ProgressPercentage;
    ///            //prgrUpdate.Value = ev.ProgressPercentage;
    ///            Debug.WriteLine(ev.ProgressPercentage);
    ///        });
    ///    },
    ///    handlerCompleted: (a, ev) => {
    ///        if (ev.Error != null)
    ///            MessageBox.Show(ev.Error.Message, "szUpdateMsgTitle", MessageBoxButton.OK, MessageBoxImage.Error);
    ///        else
    ///        {
    ///            if (MessageBox.Show("szUpdateMsgRestarText", "szUpdateMsgRestartTitle", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
    ///            {
    ///                Utils.GetUpdates.Instance.ExecuteExecLastDl();
    ///            }
    ///        }
    ///        
    ///        isUpdateProgress.Value = Visibility.Hidden;
    ///        (sender as Button).IsEnabled = true;
    ///    });
    /// </example>
    public class GetUpdates
    {
        // Singleton
        private static readonly Lazy<GetUpdates> lazy = new Lazy<GetUpdates>(() => new GetUpdates());
        public static GetUpdates Instance { get { return lazy.Value; } }
        private GetUpdates() { }


        private Response data;
        private string lastTarget;


        // https://msdn.microsoft.com/en-us/library/hh674188.aspx

        /// <summary>
        /// get download info from a remote JSON generating file by checking
        /// </summary>
        /// <param name="requestUrl">example http://domain.tld/update?v={VERSION}&api=1 </param>
        /// <returns>the data structure, based on the Response Contract</returns>
        public async Task<Response> Check(Uri requestUrl)
        {
            return data = await Task.Run(() =>
            {
                try
                {
                    HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                            throw new Exception(String.Format(
                                "Server error (HTTP {0}: {1}).",
                                response.StatusCode,
                                response.StatusDescription
                            ));

                        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                        object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());

                        Response jsonResponse = objResponse as Response;
                        return jsonResponse;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            });
        }

        /// <summary>
        /// downloads an update
        /// </summary>
        /// <param name="data">only the update-json-block with the targeted update (from Check()) is required</param>
        /// <param name="handlerProgress">async executed code with event progress info</param>
        /// <param name="handlerCompleted">async executed code with event Error info</param>
        /// <returns>an awaitable</returns>
        /// <example>
        /// do not forget to use
        ///   Application.Current.Dispatcher.Invoke(() =>
        ///   { ... uiElement ... }
        /// </example>
        public async Task<bool?> Download(Response.ResponseUpdates data, DownloadProgressChangedEventHandler handlerProgress = null, System.ComponentModel.AsyncCompletedEventHandler handlerCompleted = null)
        {
            return await Task.Run<bool?>(() =>
            {

                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    var target = Path.GetTempPath() + Path.GetFileName(data.Location.LocalPath);

                    lastTarget = target; //Uri.TryCreate(target, UriKind.Absolute, out Uri result) ? result : null;

                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        if (handlerProgress != null)
                            client.DownloadProgressChanged += handlerProgress;

                        if (handlerCompleted != null)
                            client.DownloadFileCompleted += handlerCompleted;

                        Debug.WriteLine("GET: " + data.Location);
                        client.DownloadFileAsync(data.Location, target);
                    }

                    return true;
                }
                else
                    return null;
            });

        }

        /// <summary>
        /// executed the last downloaded file
        /// </summary>
        /// <returns></returns>
        public bool ExecuteExecLastDl()
        {
            if (lastTarget == null)
                return false;

            try
            {
                Process.Start(lastTarget);


                System.Threading.Thread.Sleep(200);
                Environment.Exit(0);
                return true;
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return false;
            }

        }



        /*
        Expected remote JSON Data:

        {
            "api": 1,
            "status": {"success": true, "text": null},

            "preferred": null,         <- the updates[] index of the file the requester should get, based on the supplied version. 
            "latest": 0,               <- is null, if updates[] is empty, since this is a 0 based index
            "updates": [
                {
                    "version": "1.0.1.0",
                    "release": "2017-05-18",   <- must be parseable by DateTime

                    "changenote": "some description about what's new",

                    "location": "http://bananaacid.de/EDM/EBM_SETUP_1.0.1.0"
                }
            ]
        }
        */
        [DataContract]
        public class Response
        {
            [DataMember(Name = "api")]
            public string Api { get; set; }

            [DataMember(Name = "status")]
            public ResponseStatus Status { get; set; }

            /// <summary>
            /// the updates[] index of the file the requester should get, based on the supplied version. 
            /// </summary>
            [DataMember(Name = "preferred")]
            public int? Preferred { get; set; }

            /// <summary>
            /// is null, if updates[] is empty, since this is a 0 based index
            /// </summary>
            [DataMember(Name = "latest")]
            public int? Latest { get; set; }

            [DataMember(Name = "updates")]
            public ResponseUpdates[] Updates { get; set; }
        

            [DataContract]
            public class ResponseStatus
            {
                [DataMember(Name = "success")]
                public bool Success { get; set; }
                [DataMember(Name = "text")]
                public string Text { get; set; }
            }

            [DataContract]
            public class ResponseUpdates
            {
                [DataMember(Name = "version")]
                public string Version { get; set; }

                [DataMember(Name = "release")]
                private string _release { get; set; }
                public DateTime? Release => DateTime.TryParse(_release, out DateTime result) ? result : (DateTime?)null;

                [DataMember(Name = "changenote")]
                public string ChangeNote { get; set; }

                [DataMember(Name = "location")]
                private string _location { get; set; }
                public Uri Location => Uri.TryCreate(_location, UriKind.Absolute, out Uri result) ? result : null;
            }
        }
    }
}
