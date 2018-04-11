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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class ManagementConsoleWatchers
    {

        public static void genericDriveWatcher()
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();

            // Win32_DiskDrive Win32_DiskPartition  Win32_DiskDriveToDiskPartition
            watcher.Query = new WqlEventQuery(@"SELECT * FROM __InstanceOperationEvent WITHIN 3 WHERE TargetInstance ISA 'Win32_DiskDrive'");

            watcher.EventArrived += (s, ev) =>
            {
                Debug.WriteLine("NEW EVENT");

                try
                {
                    foreach (var p in ev.NewEvent.Properties)
                        try
                        {
                            Debug.WriteLine($"NEW EVENT: {p.Name} -> {p.Value}");
                        }
                        catch { }

                }
                catch { }


                try
                {
                    var x = ev.NewEvent.Properties["TargetInstance"].Value as ManagementBaseObject;

                    foreach (var p in x.Properties)
                        try
                        {
                            Debug.WriteLine($"OBJ PROPS: {p.Name} -> {p.Value}");
                        }
                        catch { }

                }
                catch { }

            };

            watcher.Start();
        }



        public static void genericVolumeWatcher()
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();

            // Win32_DiskDrive Win32_DiskPartition  Win32_DiskDriveToDiskPartition
            watcher.Query = new WqlEventQuery(@"SELECT * FROM __InstanceOperationEvent WITHIN 3 WHERE TargetInstance ISA 'win32_volume'");

            watcher.EventArrived += (s, ev) =>
            {
                Debug.WriteLine("NEW EVENT");

                try
                {
                    foreach (var p in ev.NewEvent.Properties)
                        try
                        {
                            Debug.WriteLine($"NEW EVENT: {p.Name} -> {p.Value}");
                        }
                        catch { }

                }
                catch { }


                try
                {
                    var x = ev.NewEvent.Properties["TargetInstance"].Value as ManagementBaseObject;

                    foreach (var p in x.Properties)
                        try
                        {
                            Debug.WriteLine($"OBJ PROPS: {p.Name} -> {p.Value}");
                        }
                        catch { }


                }
                catch { }


            };

            watcher.Start();
        }




        private static ManagementScope scopeDefault = new ManagementScope("\\\\.\\ROOT\\Microsoft\\Windows\\Storage");

        public static async Task GetVolume(/*string DeviceId*/)
        {
            Debug.WriteLine(":: CHECKING Volumes");

            await Task.Run(() =>
            {


                var q2 = new ManagementObjectSearcher(scopeDefault, new ObjectQuery($"select * from MSFT_Volume" /* WHERE Number = {p.DiskNumber}"*/)); // MSFT_Volume MSFT_PartitionToVolume

                using (var diskObj = q2.Get())
                {
                    foreach (var item in diskObj)
                    {
                        foreach (var prop in item.Properties)
                        {
                            try
                            {
                                Debug.WriteLine($"PROP: {prop.Name} -> {prop.Value}");
                            }
                            catch
                            {
                                Debug.WriteLine($"PROP: {prop.Name} -> ----ERROR----");
                            }
                        }


                        Debug.WriteLine(new string('-', 79));
                    }
                }

            });


        }
    }
}
