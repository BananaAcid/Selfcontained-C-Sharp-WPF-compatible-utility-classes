/**
 * MIT License
 * 
 * Copyright (c) 2018 "Nabil Redmann (BananaAcid) <repo@bananaacid.de>"
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Utils
{
    /// <summary>
    /// Prevent the app from running again. No Mutex required, using Task, no TCP.
    /// </summary>
    /// <example>
    ///     App.xaml.cs:
    ///     public partial class App : Application
    ///     {
    ///         public App()
    ///         {
    ///             // initiate it. Call it first.
    ///             Utils.SingleInstance.SingleInstanceWatcher();
    ///             //Utils.SingleInstance.preventSecond();
    ///
    ///             // your stuff ...
    ///         }
    ///     }
    /// </example>
    class SingleInstance
    {

        /// <summary>The event name.</summary>
        private const string UniqueEventName = "{GENERATE-YOUR-OWN-GUID}";

        /// <summary>The event wait handle.</summary>
        private static EventWaitHandle eventWaitHandle;



        /// <summary>
        /// simply prevent the second instance
        /// </summary>
        public static void preventSecond()
        {
            try
            {
                EventWaitHandle.OpenExisting(UniqueEventName); // check if it exists
                Environment.Exit(1);
            }
            catch
            {
                new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName); // register
            }
        }



        /// <summary>
        /// prevent a second instance and signal it to bring its mainwindow to foregorund
        /// </summary>
        /// <seealso cref="https://stackoverflow.com/a/23730146/1644202"/>
        public static void SingleInstanceWatcher()
        {
            // check if it is allready open.
            try
            {
                // try to open it - if another instance is running, it will exist
                eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);

                // Notify other instance so it could bring itself to foreground.
                eventWaitHandle.Set();

                // Terminate this instance.
                Environment.Exit(1);
            }
            catch
            {
                // listen to a new event
                eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            // if this instance gets the signal to show the main window
            new Task(() =>
            {
                while (eventWaitHandle.WaitOne())
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        // could be set or removed anytime
                        if (!Application.Current.MainWindow.Equals(null))
                        {
                            var mw = Application.Current.MainWindow;

                            if (mw.WindowState == WindowState.Minimized || mw.Visibility != Visibility.Visible)
                            {
                                mw.Show();
                                mw.WindowState = WindowState.Normal;
                            }

                            // According to some sources these steps gurantee that an app will be brought to foreground.
                            mw.Activate();
                            mw.Topmost = true;
                            mw.Topmost = false;
                            mw.Focus();
                        }
                    }));
                }
            })
            .Start();
        }

    }
}
