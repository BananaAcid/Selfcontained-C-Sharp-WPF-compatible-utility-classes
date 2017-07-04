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
using Microsoft.Win32;


namespace Utils
{
    /// <summary>
    /// add this app to registry
    /// </summary>
    public static class StartWithWindows
    {
        internal const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// check if it was added
        /// </summary>
        /// <returns></returns>
        public static bool IsSetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(subKey, true);

            var AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            return rk.GetValue(AppName) != null;
        }

        /// <summary>
        /// set it up to started with windows
        /// </summary>
        /// <param name="activate"></param>
        public static void SetStartup(bool activate)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(subKey, true);

            var AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            if (activate)
                rk.SetValue(AppName, AppPath);
            else
                rk.DeleteValue(AppName, false);
        }
    }
}
