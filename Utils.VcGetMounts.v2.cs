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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

/// <summary>
///  v2.0 added MountInfo struct
/// </summary>
namespace Utils
{
    /// <summary>
    /// info holding struct
    /// </summary>
    public class MountInfo
    {
        public char letter { get; set; }
        public string volumeName { get; set; }
        public string volumeLabel { get; set; }
        public UInt64 diskLength { get; set; }
        public bool truecryptMode { get; set; }
    }

    /// <summary>
    /// access VeraCrypt's Mounting info
    /// </summary>
    /// <see cref="http://stackoverflow.com/a/18021118/1644202"/>
    /// <remarks>
    /// based on the stackoverflow post, the missing data had to be extracted from VeraCrypts source files
    /// </remarks>
    public static class VcGetMounts
    {
        /// <summary>
        /// get mounted drives
        /// </summary>
        /// <returns></returns>
        /// <example>
        ///     var t = await Utils.VcGetMounts.getMounted();
        ///     
        ///     foreach (var i in t)
        ///     {
        ///         Debug.WriteLine("{0} -> {1}", i.Key, i.Value);
        ///     }
        /// </example>
        public static async Task<Dictionary<char, MountInfo>> getMounted()
        {
            return await Task.Run<Dictionary<char, MountInfo>>(() =>
            {
                var ret = new Dictionary<char, MountInfo>();

                uint size = (uint)Marshal.SizeOf(typeof(MOUNT_LIST_STRUCT));
                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                uint bytesReturned;
                IntPtr _hdev = CreateFile("\\\\.\\VeraCrypt", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                bool bResult = DeviceIoControl(_hdev, TC_IOCTL_GET_MOUNTED_VOLUMES, buffer, size, buffer, size, out bytesReturned, IntPtr.Zero);
                // IMPORTANT! Otherwise, the struct fills up with random bytes from memory, if no VeraCrypt is available
                if (!bResult) return ret;
                MOUNT_LIST_STRUCT mount = new MOUNT_LIST_STRUCT();
                Marshal.PtrToStructure(buffer, mount);
                Marshal.FreeHGlobal(buffer);

                for (int i = 0; i < 26; i++)
                    //Debug.WriteLine("{0}: => {1}", (char)('A' + i), mount.wszVolume[i]);
                    if (mount.wszVolume[i].ToString().Length > 0)
                    {
                        // Debug.WriteLine("{0}: => {1}", (char)('A' + i), mount.wszVolume[i]);
                        ret.Add((char)('A' + i), new MountInfo() {
                            letter = (char)('A' + i),
                            volumeName = mount.wszVolume[i].ToString(),
                            volumeLabel = mount.wszLabel[i].ToString(),
                            diskLength = mount.diskLength[i],
                            truecryptMode = mount.truecryptMode[i]
                        });
                    }

                return ret;
            });
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        class MOUNT_LIST_STRUCT
        {
            public readonly UInt32 ulMountedDrives; /* Bitfield of all mounted drive letters */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public readonly MOUNT_LIST_STRUCT_VOLUME_NAME[] wszVolume;  /* Volume names of mounted volumes */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public readonly MOUNT_LIST_STRUCT_VOLUME_LABEL[] wszLabel;  /* Volume labels of mounted volumes */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public readonly MOUNT_LIST_STRUCT_VOLUME_ID[] volumeID;  /* Volume labels of mounted volumes */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public readonly UInt64[] diskLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public readonly int[] ea;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public readonly int[] volumeType;   /* Volume type (e.g. PROP_VOL_TYPE_OUTER, PROP_VOL_TYPE_OUTER_VOL_WRITE_PREVENTED, etc.) */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public readonly bool[] truecryptMode;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct MOUNT_LIST_STRUCT_VOLUME_NAME
        {
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I2, SizeConst = 260)]
            public readonly char[] wszVolume;   /* Volume names of mounted volumes */

            public override string ToString()
            {
                return (new String(wszVolume)).TrimEnd('\0');
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct MOUNT_LIST_STRUCT_VOLUME_ID
        {
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I2, SizeConst = 32)]
            public readonly char[] volumeID;   /* Volume ids of mounted volumes */

            public override string ToString()
            {
                return (new String(volumeID)).TrimEnd('\0');
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct MOUNT_LIST_STRUCT_VOLUME_LABEL
        {
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I2, SizeConst = 33)]
            public readonly char[] wszLabel;   /* Volume labels of mounted volumes */

            public override string ToString()
            {
                return (new String(wszLabel)).TrimEnd('\0');
            }
        }

        public static int CTL_CODE(int DeviceType, int Function, int Method, int Access)
        {
            return (((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2)
              | (Method));
        }
        private static readonly uint TC_IOCTL_GET_MOUNTED_VOLUMES = (uint)CTL_CODE(0x00000022, 0x800 + (6), 0, 0);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
        IntPtr lpInBuffer, uint nInBufferSize,
        IntPtr lpOutBuffer, uint nOutBufferSize,
        out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);
    }
}
