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
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Utils
{
    /// <summary>
    /// get installation path of an application from the registry in different ways
    /// </summary>
    /// <example>
    ///    public static string[] getPossibleEncrypters()
    ///    {
    ///        // path, exename to append
    ///        var x = new List<(string,string)>
    ///        {
    ///            (GetApplictionInstallPath("VeraCrypt"), "VeraCrypt.exe"),
    ///            (GetApplictionInstallPath("TrueCrypt"), "TrueCrypt.exe"),
    ///            (GetApplictionInstallPath("CipherShed"), "CipherShed.exe")
    ///        };
    ///        
    ///        // test if the registered file actually exists
    ///        var z = x.ConvertAll(new Converter<(string, string), string>(
    ///            (e) => e.Item1 != null && File.Exists(e.Item1 + @"\" + e.Item2) ? e.Item1 + @"\" + e.Item2 : null
    ///        )).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
    ///        
    ///        return z.ToArray();
    ///    }
    /// </example>
    public static class InstalledApplications
    {
        /// <summary>
        /// extract installation path for app from registry (exe name might NOT be contained in the value found!)
        /// </summary>
        /// <param name="nameOfAppToFind">app name (use any requried capital letters within the name)</param>
        /// <returns></returns>
        public static string GetApplictionInstallPath(string nameOfAppToFind)
        {
            foreach (var keys in new (RegistryKey, String)[] 
			{
                (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),					// search in: CurrentUser
                (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),				// search in: LocalMachine_32
                (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")		// search in: LocalMachine_64
            })
            {
                string installedPath = ExistsInSubKey(keys.Item1, keys.Item2, "DisplayName", nameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                    return installedPath;
            }

            return string.Empty;
        }

        //http://stackoverflow.com/a/26686738/1644202
        /// <summary>
        /// get a specific subkey's value
        /// </summary>
        /// <param name="root">the registry key to select</param>
        /// <param name="subKeyName">the specific subkey path you want to check</param>
        /// <param name="attributeName">the specific attribute you want the value from</param>
        /// <param name="nameOfAppToFind">must match exactly</param>
        /// <returns></returns>
        private static string ExistsInSubKey(RegistryKey root, string subKeyName, string attributeName, string nameOfAppToFind)
        {
            using (RegistryKey key = root.OpenSubKey(subKeyName))
            {
                if (key != null)
                {
                    foreach (string kn in key.GetSubKeyNames())
                    {
                        using (RegistryKey subkey = key.OpenSubKey(kn))
                        {
                            string attributeNameValue = subkey.GetValue(attributeName) as string;
                            if (nameOfAppToFind.Equals(attributeNameValue, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                string dir = subkey.GetValue("InstallLocation") as string;

                                if (dir == null)
                                {
                                    try
                                    {
                                        string szPath = new Regex("\\\"*([^\\\"]+)\\\"*").Match(subkey.GetValue("UninstallString") as string).Groups[1].Value;
                                        dir = Path.GetDirectoryName(szPath);
                                    }
                                    catch { }
                                }

                                return dir;
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
