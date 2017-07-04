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
using System.Reflection;
using System.Security.Principal;

namespace Utils
{
    /// <summary>
    /// handle admin related stuff
    /// </summary>
    public static class Admin
    {
        public enum restartAdminReturn
        {
            noNeed = 0,
            restarting = 1,

            error = -1
        }

        /// <summary>
        /// restarts app in elevated mode, if required
        /// you need to exit the app manually or add some exiting code to the preExecuteAction
        /// </summary>
        /// <param name="args">startup arguments</param>
        /// <param name="preExecuteAction">an anonymous function to call beforehand, if a restart is needed</param>
        /// <returns>what it actually did</returns>
        /// <example>
        /// Action x = () => ReleaseMutex();
        /// 
        /// // check if it has to be restarted as admin
        /// if (Utils.Admin.restartAsAdmin(preExecuteAction: x) == Utils.Admin.restartAdminReturn.restarting)
        /// {
        ///     Application.Current.Shutdown();
        ///     return;
        /// }
        /// </example>
        public static restartAdminReturn restartAsAdmin(string[] args = null, Action preExecuteAction = null)
        {
            if (!IsRunAsAdmin())
            {
                var proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Assembly.GetEntryAssembly().CodeBase;

                if (args != null)
                    foreach (string arg in args)
                    {
                        proc.Arguments += String.Format("\"{0}\" ", arg);
                    }

                proc.Verb = "runas";

                try
                {
                    preExecuteAction?.Invoke();

                    Process.Start(proc);

                    return restartAdminReturn.restarting;
                }
                catch
                {
                    Debug.WriteLine("This application requires elevated credentials, for the system features!");
                    return restartAdminReturn.error;
                }
            }
            else
            {
                return restartAdminReturn.noNeed;
            }
        }

        /// <summary>
        /// Check if it was run with elevated rights
        /// </summary>
        /// <returns>if it was run with elevated rights</returns>
        public static bool IsRunAsAdmin()
        {
            var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

    }
}
