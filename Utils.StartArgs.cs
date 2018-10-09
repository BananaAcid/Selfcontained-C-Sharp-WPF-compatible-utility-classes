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
using System.Linq;


namespace Utils
{
    /// <summary>
    /// handle start arguments
    /// </summary>
    /// <example>
    ///     App.xaml.cs:
    ///     public partial class App : Application
    ///     {
    ///         public App()
    ///         {
    ///             // init arguments
    ///             Utils.StartArgs.get();
    ///             
    ///             // do first check for important stuff
    ///             if (Utils.StartArgs.Arguments.ContainsKey("help") || Utils.StartArgs.Arguments.ContainsKey("?"))
    ///             {
    ///                 // There is no console for GUI apps
    ///                 if (!ConsoleManager.HasConsoleAttached)
    ///                 {
    ///                     MessageBox.Show("Some help.", "Help");
    ///                 }
    ///                 else
    ///                 {
    ///                     Console.WriteLine("\nSome help.");
    ///     
    ///                     // get command prompt back
    ///                     System.Windows.Forms.SendKeys.SendWait("{ENTER}");
    ///     
    ///                     ConsoleManager.Close();
    ///                 }
    ///                 // exit after showing help. Do not start GUI (in case of a XAML/WPF app).
    ///                 Environment.Exit(0);
    ///             }
    ///             
    ///             // update app properties with commandline params -- http://stackoverflow.com/a/21021053/1644202
    ///             foreach (var arg in Utils.StartArgs.Arguments)
    ///             {
    ///                 // check if it exists and get the property type info
    ///                 var b = MyAppNamespace.Properties.Settings.Default.Properties.Cast<SettingsProperty>()
    ///                     .Where(prop => prop.Name == "option" + arg.Key).FirstOrDefault();
    /// 
    ///                 if (b != null)
    ///                     MyAppNamespace.Properties.Settings.Default["option" + arg.Key] =
    ///                      b.PropertyType.Name == "Boolean"
    ///                         ? (new string[] { "true", "on", "yes", "1" }.Contains(arg.Value.ToLower()) ? true : false)
    ///                         : (object)arg.Value;
    ///             }
    /// 
    ///         }
    ///     }
    /// </example>
    public static class StartArgs
    {
        public static Dictionary<string, string> Arguments = new Dictionary<string, string>();

        /// <summary>
        /// initializes the arguments dictionary
        /// 
        /// call in App.xaml.cs:Constructor
        /// 
        /// supports multiple arguments, and in any combination and order with optional 'val':
        /// --param val      linux long word style: --param
        /// --param=val
        /// --param:val
        /// -param val       linux short letter style:  -p
        /// -param=val
        /// -param:val
        /// /param val       windows style: /param
        /// /param=val
        /// /param:val       microsoft style: /param:value
        /// singleparam      is handled like using:  /?  .... if you want the first param as path or something, get it like in line "string[] ret = arg.Split ... "
        /// </summary>
        /// <example>
        ///   string param = Utils.StartArgs.Arguments.Where(x => x.Key == "locale").FirstOrDefault().Value; --> string | null
        ///   
        ///   string param = Utils.StartArgs.Arguments.TryGetValue("locale", out string value) ? value : null;
        ///   
        ///   bool has = Utils.StartArgs.Arguments.ContainsKey("singleparam");
        ///   
        ///   foreach (KeyValuePair<string, string>param in Utils.StartArgs.Arguments)
        ///       switch (param.Key) {
        ///           case "help": break;
        ///       }
        /// </example>
        public static void get()
        {
            string[] args = Environment.GetCommandLineArgs();
            var strLastKey = "";
            foreach (string arg in args)
            {
                if (arg.StartsWith("/") || arg.StartsWith("-"))
                {
                    string[] ret = arg.Split(new Char[] { '=', ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var key = ret[0].Trim(new Char[] { '/', '-' });
                    Arguments.Add(key, ret.Count() > 1 ? ret[1] : null);

                    // remember key
                    strLastKey = key;
                }
                else
                {
                    // missing a preceeding key
                    if (strLastKey == null)
                        Arguments.Add(arg, null);

                    // the value to a precceding key
                    else
                        Arguments[strLastKey] = arg;

                    strLastKey = null;
                }
            }
        }
    }
}
