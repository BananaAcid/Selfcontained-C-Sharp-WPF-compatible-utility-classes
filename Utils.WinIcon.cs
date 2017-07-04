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
using System.Windows;
using System.Windows.Media.Imaging;

namespace Utils
{
    /// <summary>
    /// get a system icon from windows resources
    /// </summary>
    /// <see cref=" http://www.pinvoke.net/default.aspx/shell32/SHGetStockIconInfo.html"/>
    public static class WinIcon
    {
        internal const int MAX_PATH = 260;
        [System.Runtime.InteropServices.DllImport("Shell32.dll", SetLastError = false)]
        internal static extern Int32 SHGetStockIconInfo(uint siid, uint uFlags, ref SHSTOCKICONINFO psii);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        internal static extern int DestroyIcon(IntPtr hIcon);

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal struct SHSTOCKICONINFO
        {
            public UInt32 cbSize;
            public IntPtr hIcon;
            public Int32 iSysIconIndex;
            public Int32 iIcon;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szPath;
        }

        internal const uint SHGSI_ICON = 0x000000100;
        internal const uint SHGSI_LARGEICON = 0x000000000; // used this and in the XAML shrunk down to the size of SMALLICON to look better
        internal const uint SHGSI_SMALLICON = 0x000000001;

        /// <summary>
        /// get the elevation-needed icon
        /// </summary>
        /// <returns></returns>
        /// <example>
        ///     this.ImageWpfControl.Source = Utils.WinIcon.getShieldIcon();
        /// </example>
        public static BitmapSource getShieldIcon()
        {
            return getIcon(SHSTOCKICONID.SIID_SHIELD);
        }

        /// <summary>
        /// get the explorer's folder icon
        /// </summary>
        /// <returns></returns>
        /// <example>
        ///     this.ImageWpfControl.Source = Utils.WinIcon.getFolderOpenIcon();
        /// </example>
        public static BitmapSource getFolderOpenIcon()
        {
            return getIcon(SHSTOCKICONID.SIID_FOLDEROPEN, SHGSI_SMALLICON);
        }


        /// <summary>
        /// generic icon extraction
        /// </summary>
        /// <param name="SIID_iconid">the specific SHSTOCKICONID icon id</param>
        /// <param name="iconSize">the size to extract, a larger one resized to smaller canvas looks better</param>
        /// <returns>the icon data as bitmap</returns>
        /// <example>
        ///     this.ImageWpfControl.Source = Utils.WinIcon.getIcon(SHSTOCKICONID.SIID_SHIELD);
        /// </example>
        public static BitmapSource getIcon(SHSTOCKICONID SIID_iconid, uint iconSize = SHGSI_LARGEICON)
        {
            BitmapSource IconSource = null;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
                sii.cbSize = (UInt32)System.Runtime.InteropServices.Marshal.SizeOf(typeof(SHSTOCKICONINFO));

                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(SHGetStockIconInfo((uint)SIID_iconid,
                    SHGSI_ICON | iconSize,
                    ref sii));

                IconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    sii.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(sii.hIcon);
            }
            else
            {
                IconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    System.Drawing.SystemIcons.Shield.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }

            return IconSource;
        }


        /// <summary>
        /// the knwon icons
        /// 
        /// check out the link <see cref="http://m.blog.csdn.net/article/details?id=18951651"/> for their looks
        /// </summary>
        public enum SHSTOCKICONID : uint
        {
            SIID_DOCNOASSOC = 0,
            SIID_DOCASSOC = 1,
            SIID_APPLICATION = 2,
            SIID_FOLDER = 3,
            SIID_FOLDEROPEN = 4,
            SIID_DRIVE525 = 5,
            SIID_DRIVE35 = 6,
            SIID_DRIVEREMOVE = 7,
            SIID_DRIVEFIXED = 8,
            SIID_DRIVENET = 9,
            SIID_DRIVENETDISABLED = 10,
            SIID_DRIVECD = 11,
            SIID_DRIVERAM = 12,
            SIID_WORLD = 13,
            SIID_SERVER = 15,
            SIID_PRINTER = 16,
            SIID_MYNETWORK = 17,
            SIID_FIND = 22,
            SIID_HELP = 23,
            SIID_SHARE = 28,
            SIID_LINK = 29,
            SIID_SLOWFILE = 30,
            SIID_RECYCLER = 31,
            SIID_RECYCLERFULL = 32,
            SIID_MEDIACDAUDIO = 40,
            SIID_LOCK = 47,
            SIID_AUTOLIST = 49,
            SIID_PRINTERNET = 50,
            SIID_SERVERSHARE = 51,
            SIID_PRINTERFAX = 52,
            SIID_PRINTERFAXNET = 53,
            SIID_PRINTERFILE = 54,
            SIID_STACK = 55,
            SIID_MEDIASVCD = 56,
            SIID_STUFFEDFOLDER = 57,
            SIID_DRIVEUNKNOWN = 58,
            SIID_DRIVEDVD = 59,
            SIID_MEDIADVD = 60,
            SIID_MEDIADVDRAM = 61,
            SIID_MEDIADVDRW = 62,
            SIID_MEDIADVDR = 63,
            SIID_MEDIADVDROM = 64,
            SIID_MEDIACDAUDIOPLUS = 65,
            SIID_MEDIACDRW = 66,
            SIID_MEDIACDR = 67,
            SIID_MEDIACDBURN = 68,
            SIID_MEDIABLANKCD = 69,
            SIID_MEDIACDROM = 70,
            SIID_AUDIOFILES = 71,
            SIID_IMAGEFILES = 72,
            SIID_VIDEOFILES = 73,
            SIID_MIXEDFILES = 74,
            SIID_FOLDERBACK = 75,
            SIID_FOLDERFRONT = 76,
            SIID_SHIELD = 77,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80,
            SIID_KEY = 81,
            SIID_SOFTWARE = 82,
            SIID_RENAME = 83,
            SIID_DELETE = 84,
            SIID_MEDIAAUDIODVD = 85,
            SIID_MEDIAMOVIEDVD = 86,
            SIID_MEDIAENHANCEDCD = 87,
            SIID_MEDIAENHANCEDDVD = 88,
            SIID_MEDIAHDDVD = 89,
            SIID_MEDIABLURAY = 90,
            SIID_MEDIAVCD = 91,
            SIID_MEDIADVDPLUSR = 92,
            SIID_MEDIADVDPLUSRW = 93,
            SIID_DESKTOPPC = 94,
            SIID_MOBILEPC = 95,
            SIID_USERS = 96,
            SIID_MEDIASMARTMEDIA = 97,
            SIID_MEDIACOMPACTFLASH = 98,
            SIID_DEVICECELLPHONE = 99,
            SIID_DEVICECAMERA = 100,
            SIID_DEVICEVIDEOCAMERA = 101,
            SIID_DEVICEAUDIOPLAYER = 102,
            SIID_NETWORKCONNECT = 103,
            SIID_INTERNET = 104,
            SIID_ZIPFILE = 105,
            SIID_SETTINGS = 106,
            SIID_DRIVEHDDVD = 132,
            SIID_DRIVEBD = 133,
            SIID_MEDIAHDDVDROM = 134,
            SIID_MEDIAHDDVDR = 135,
            SIID_MEDIAHDDVDRAM = 136,
            SIID_MEDIABDROM = 137,
            SIID_MEDIABDR = 138,
            SIID_MEDIABDRE = 139,
            SIID_CLUSTEREDDRIVE = 140,
            SIID_MAX_ICONS = 175
        }
    }
}
