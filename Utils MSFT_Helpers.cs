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
using System.Management;
using System.Text;

/// <summary>
/// MSFT Helpers
/// 
/// 
/// "public class Partition" contains project specific code. As well as code related to Utils.TcGetMounts.cs and Utils.VcGetMounts.cs - remove it for your own projects.
/// </summary>
/// <example>
///    public enum GetPartitionsStatus
///    {
///        ok,
///        needPermissions,
///        Error
///    }
///    
///    private static ManagementScope scopeDefault = new ManagementScope("\\\\.\\ROOT\\Microsoft\\Windows\\Storage");
///
///    public GetPartitionsStatus getPartitions(string qFilterStr = "")
///    {
///        var qStr = $"select * from MSFT_Partition {qFilterStr} ";
///        var q = new ManagementObjectSearcher(scopeDefault, new ObjectQuery(qStr));
///        
///        
///        
///        int count = 0;
///        try
///        {
///            foreach (ManagementObject partitionObj in q.Get())
///            {
///                count++;
///                
///                var p = new MSFT_Helpers.MSFT_Partition(partitionObj);
///                
///                Debug.WriteLine("AccessPaths : {0}", String.Join(" | ", (p.AccessPaths ?? new String[] { "" })));
///                
///                var uri = $@"\Device\Harddisk{p.DiskNumber}\Partition{p.PartitionNumber}";
///                
///                Debug.WriteLine($"PATH         {uri}");
///                Debug.WriteLine($"DriveLetter  {p.DriveLetter}");
///
///                Debug.WriteLine($"OperationalStatus  {p.OperationalStatus.ToString()}");
///                Debug.WriteLine($"TransitionState    {p.TransitionState}");
///
///                Debug.WriteLine($"MbrType    {p.MbrType}");
///                Debug.WriteLine($"GptType    {p.GptTypeGUID}");   // b5048902-3894-4628-a3d1-a65e9888d3c2
///                Debug.WriteLine($"GUID       {p.GUID}");
///
///                Debug.WriteLine($"UniqueId   {p.UniqueId}");
///
///                Debug.WriteLine($"isSystem   {p.IsSystem}");
///                Debug.WriteLine($"IsOffline  {p.IsOffline}");
///
///                Debug.WriteLine($"Size       {p.Size}");
///
///                var gb = MSFT_Helpers.Util.BytesToStringOS(p.Size);
///                Debug.WriteLine($"SizeOS     {gb}");
///
///                var gb2 = MSFT_Helpers.Util.BytesToStringShort(p.Size);
///                Debug.WriteLine($"Size Nice  {gb2}");
///
///
///                Debug.WriteLine(new string('-', 10));
///
///
///                var q2 = new ManagementObjectSearcher(scopeDefault, new ObjectQuery($"select * from MSFT_Disk WHERE Number = {p.DiskNumber}"));
///
///                using (var diskObj = q2.Get().Cast<ManagementBaseObject>().Single())
///                {
///                    var d = new MSFT_Helpers.MSFT_Disk(diskObj);
///                    Debug.WriteLine($"Drive: Guid           {d.GUID}");
///                    Debug.WriteLine($"Drive: Size           { MSFT_Helpers.Util.BytesToStringShort(d.Size) }");
///                    Debug.WriteLine($"Drive: UniqueId       {d.UniqueId}");
///                    Debug.WriteLine($"Drive: FriendlyName   {d.FriendlyName}");  // Manufacturer + Model
///                    Debug.WriteLine($"Drive: BusType        {d.BusType}");
///                    OnNewDrives?.Invoke(null, new OnNewDrivesArg { Partition = new MSFT_Helpers.Partition(partitionObj), Disk = d });
///                }
///
///                Debug.WriteLine(new string('-', 79));
///            }
///
///
///            if (count == 0 && specificDriveNumber != null)
///            {
///                OnNewDrives?.Invoke(null, new OnNewDrivesArg
///                {
///                    Partition = null,
///                    Disk = new MSFT_Helpers.MSFT_Disk(null) { Number = specificDriveNumber ?? 999999 }
///                });
///            }
///        }
///        catch (ManagementException ex)
///        {
///            return GetPartitionsStatus.needPermissions;
///        }
///        catch (Exception)
///        {
///            return GetPartitionsStatus.Error;
///        }
///
///        return GetPartitionsStatus.ok;
///    }
/// </example>
namespace MSFT_Helpers
{

    class Util
    {
        /// <summary>
        /// readable file size, short
        /// </summary>
        /// <param name="value">the size in bytes</param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        /// <see cref="http://stackoverflow.com/a/14488941/1644202"/>
        public static string BytesToStringShort(UInt64 value, int decimalPlaces = 1)
        {
            
            string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            //if (value < 0) { return "-" + BytesToStringShort(-value); }
            if (value == 0) { return "0.0 bytes"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern long StrFormatKBSize(ulong qdw, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)] StringBuilder pszBuf, int cchBuf);
        /// <summary>
        /// readable filesize, os style
        /// </summary>
        /// <param name="byteCount">the size in bytes</param>
        /// <returns></returns>
        public static string BytesToStringOS(UInt64 byteCount)
        {
            var sb = new StringBuilder(32);
            StrFormatKBSize(byteCount, sb, sb.Capacity);
            return sb.ToString();
        }
    }


    /// <summary>
    /// parent of the MSFT classes
    /// </summary>
    public class MSFT_StorageObject
    {
        // array accessor
        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }


        // constructor to pass content on
        public MSFT_StorageObject(ManagementBaseObject obj)
        {
            if (obj == null) return;

            foreach (var item in obj.Properties)
            {
                if (item.Value != null)
                    try
                    {
                        this[item.Name] = item.Value;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"MISSING Class Property: {item.Type} {item.Name}  ->  {item.Value.ToString()}\n - Message: {ex.Message}");
                    }
            }
        }
    }

    /// <summary>
    /// a convinience class to match the MS Documentation
    /// </summary>
    /// <example>
    /// var partitions = new ManagementObjectSearcher(..).get();
    /// var partitionInfo = new MSFT_Partition( partitions.FirstOrDefault() );
    /// </example>
    public class MSFT_Partition : MSFT_StorageObject
    {
        public MSFT_Partition(ManagementBaseObject obj) : base(obj)
        { }

        public UInt32 DiskNumber { get; set; }
        public UInt32 PartitionNumber { get; set; }

        private Char? _DriveLetter;
        public Char? DriveLetter
        {
            get { return _DriveLetter; }
            set { if (((int)value) != 0) _DriveLetter = value; }
        }
        public String[] AccessPaths { get; set; }
        public OperationalStatus OperationalStatus { get; set; }
        public TransitionState TransitionState { get; set; }
        public UInt64 Size { get; set; }
        public MbrType MbrType { get; set; } = MbrType.None;

        public Guid? GptTypeGUID { get; set; }
        public String _GptType { get; set; }
        public String GptType  // string
        {
            get { return _GptType; }
            set
            {
                _GptType = value;

                System.Guid x;
                if (System.Guid.TryParse(value.ToString(), out x))
                    GptTypeGUID = x;
            }
        }

        public Guid? GUID { get; private set; }
        private String _Guid { get; set; }
        public String Guid  // string
        {
            get { return _Guid; }
            set
            {
                _Guid = value;

                System.Guid x;
                if (System.Guid.TryParse(value.ToString(), out x))
                    GUID = x;
            }
        }

        public Boolean IsReadOnly { get; set; }
        public Boolean IsOffline { get; set; }
        public Boolean IsSystem { get; set; }
        public Boolean IsBoot { get; set; }
        public Boolean IsActive { get; set; }
        public Boolean IsHidden { get; set; }
        public Boolean IsShadowCopy { get; set; }
        public Boolean NoDefaultDriveLetter { get; set; }

        // missing in the docs
        public String ObjectId { get; set; }
        public UInt64 Offset { get; set; }
        public String UniqueId { get; set; }                    // Win 10 (NOT in Win 8)
        public String DiskId { get; set; }
        public Boolean IsDAX { get; set; }
    }

    /// <summary>
    /// convinience functions, connected to other classes outside of this one
    /// </summary>
    public class Partition : MSFT_Partition
    {
        public Partition(ManagementBaseObject obj) : base(obj)
        { }

        // THESE are GETTERs, using the new C#7 style

        /// <summary>
        /// get the partition size in a short, readable notation (E.g. 2,7 TB)
        /// </summary>
        public string SizeShort => System.Text.RegularExpressions.Regex.Replace(Util.BytesToStringShort(this.Size), @"[\,\.]0 ", " ");

        /// <summary>
        /// get the size as an OS formatted string
        /// </summary>
        public string SizeOS => Util.BytesToStringOS(this.Size);

        /// <summary>
        /// get a path to the partition to be mountable by an encrypter
        /// </summary>
        public string PathForTC => $@"\Device\Harddisk{this.DiskNumber}\Partition{this.PartitionNumber}";

        /// <summary>
        /// check if a partition known to be encrypted
        /// </summary>
        public bool isEncrypted => //(GptType?.Equals(TTCD_Client.DriveWatching.CONTAINER_GUID) ?? false) || MbrType.Equals(TTCD_Client.DriveWatching.CONTAINER_MBRTYPE);
                             requiredEncrypter != TTCD_Client.DriveWatching.Encrypters.None || this.isEncrypterMounted;

        /// <summary>
        /// check if a partition is mounted by an encrypter
        /// </summary>
        public bool isEncrypterMounted => TTCD_Client.DriveWatching.Instance.MountsList.Where(x => x.Value.path == this.PathForTC).Count() != 0 || /* workaround for others */ TTCD_Client.MainWindow.mountedDrives.Find((e) => e.Partition.UniqueId == this.UniqueId) != null;

        /// <summary>
        /// Drive letter: mounted by encrypter
        /// </summary>
        public char? EncrypterMountedDriveLetter { get {
            try
            {
                return TTCD_Client.DriveWatching.Instance.MountsList.First(x => x.Value.path == this.PathForTC).Key;
            }
            catch (InvalidOperationException) { return null; }
        } }

        /// <summary>
        /// Drive letter: any available (windows hosted, or encrypter)
        /// </summary>
        public char? MountDriveLetter => EncrypterMountedDriveLetter ?? DriveLetter;

        /// <summary>
        /// Drive letter: any available (windows hosted, or encrypter) as drv letter string -> includes colon
        /// </summary>
        public string MountDriveLetterCaption => this.MountDriveLetter.HasValue ? this.MountDriveLetter.ToString() + ":" : "";

        /// <summary>
        /// currently mounted by what encrypter
        /// </summary>
        public TTCD_Client.DriveWatching.Encrypters mountingEncrypter => TTCD_Client.DriveWatching.Instance.MountsList.FirstOrDefault(x => x.Value.path == this.PathForTC).Value.crypter;

        /// <summary>
        /// what encrypter is needed (by GPT-GUID from CONTAINERS list)
        /// </summary>
        public TTCD_Client.DriveWatching.Encrypters requiredEncrypter => (TTCD_Client.DriveWatching.Encrypters)TTCD_Client.DriveWatching.CONTAINERS.Where(item => this.GptTypeGUID?.ToString("B").Equals(item.Value.GPTGUID)??false || item.Value.MBRTYPE.Equals(this.MbrType)).FirstOrDefault().Key;

        /// <summary>
        /// check to see if it is a mounted and encrypted partition (as when it is enc with TC and gets a drv letter and windows wants to format it)
        /// </summary>
        public bool isDriveLetterButInAccessable => this.DriveLetter != null && !System.IO.Directory.Exists(this.DriveLetter.ToString() + ":");


        /// <summary>
        /// Strings for the setAttribute command's error messages
        /// </summary>
        public static Dictionary<UInt32, string> setAttribute_ErrorMessages = new Dictionary<UInt32, string>
        {
            {0, "Success"},
            {1, "Not Supported"},
            {2, "Unspecified Error"},
            {3, "Timeout"},
            {4, "Failed"},
            {5, "Invalid Parameter"},
            {6, "In Use"},
            {40001, "Access denied"},
            {40002, "There are not enough resources to complete the operation."},
            {42011, "This operation is only supported on data partitions."}
        };
    }

    /// <summary>
    /// a convinience class to match the MS Documentation
    /// 
    /// USE:
    /// var disks = new ManagementObjectSearcher(..).get();
    /// var diskInfo = new MSFT_Disk( partitions.FirstOrDefault() );
    /// </summary>
    public class MSFT_Disk : MSFT_StorageObject
    {
        public MSFT_Disk(ManagementBaseObject obj) : base(obj)
        { }

        public String Path { get; set; }
        public String Location { get; set; }
        public String FriendlyName { get; set; }
        public String UniqueId { get; set; }
        public UniqueIdFormat UniqueIdFormat { get; set; }
        public UInt32 Number { get; set; }
        public String SerialNumber { get; set; }
        public String FirmwareVersion { get; set; }
        public String Manufacturer { get; set; }
        public String Model { get; set; }
        public UInt64 Size { get; set; }
        public UInt64 AllocatedSize { get; set; }
        public UInt32 LogicalSectorSize { get; set; }
        public UInt32 PhysicalSectorSize { get; set; }
        public UInt64 LargestFreeExtent { get; set; }
        public UInt32 NumberOfPartitions { get; set; }
        public ProvisioningType ProvisioningType { get; set; }
        //public DiskOperationalStatus OperationalStatus { get; set; }       // WIN 8
        public UInt16[] OperationalStatus { get; set; }                  // WIN 10
        public HealthStatus HealthStatus { get; set; }
        public BusType BusType { get; set; }
        public PartitionStyle PartitionStyle { get; set; }
        public UInt32 Signature { get; set; }

        public Guid? GUID { get; private set; }
        private String _Guid { get; set; }
        public String Guid  // string
        {
            get { return _Guid; }
            set
            {
                _Guid = value;

                System.Guid x;
                if (System.Guid.TryParse(value.ToString(), out x))
                    GUID = x;
            }
        }

        public Boolean IsOffline { get; set; }
        public OfflineReason OfflineReason { get; set; }
        public Boolean IsReadOnly { get; set; }
        public Boolean IsSystem { get; set; }
        public Boolean IsClustered { get; set; }
        public Boolean IsBoot { get; set; }
        public Boolean BootFromDisk { get; set; }

        // missing in the docs
        public Boolean IsHighlyAvailable { get; set; }
        public Boolean IsScaleOut { get; set; }
        public String ObjectId { get; set; }
    }


    // https://www.win.tue.nl/~aeb/partitions/partition_types-1.html
    public enum MbrType
    {
        None = 0,

        // known by windows
        FAT12 = 1,
        FAT16 = 4,
        Extended = 5,
        Huge = 6,
        NTFS = 7, // Windows NT NTFS | OS/2 IFS (e.g., HPFS) | exFAT
        FAT32 = 12,

        // others
        Unused = 39,

        BananaAcid_for_TrueCrypt = 0xba,

        PC_ARMOUR_encrypted_partition = 64,
        Linux_swap = 82,
        Linux_native = 83,
        Linux_extended_partition = 85,
        Hidden_Linux = 0xc2,
        Hidden_Linux_swap = 0xc3
    }

    public enum OperationalStatus
    {
        Unknown = 0,
        Online = 1,
        No_Media = 3,
        Failed = 5,
        Offline = 4
    }

    public enum TransitionState
    {
        Reserved = 0, // This value is reserved for system use.
        Stable = 1, // The partition is stable. No configuration activity is currently in progress.
        Extended = 2, // The partition is being extended.
        Shrunk = 3, // The partition is being shrunk.
        Automagical = 4, // The partition is being automagically reconfigured.
        Restriped = 8 // The partition is being restriped.
    }


    public enum BusType
    {
        Unknown = 0,    // The bus type is unknown.
        SCSI = 1,       // SCSI
        ATAPI = 2,      // ATAPI
        ATA = 3,        // ATA
        IEEE1394 = 4,   // IEEE 1394
        SSA = 5,        // SSA
        Fibre_Channel = 6,   // Fibre Channel
        USB = 7,        // USB
        RAID = 8,       // RAID
        iSCSI = 9,      // iSCSI
        SAS = 10,       // Serial Attached SCSI (SAS)
        SATA = 11,      // Serial ATA (SATA)
        SD = 12,        // Secure Digital (SD)
        MMC = 13,       // Multimedia Card (MMC)
        Virtual = 14,       // This value is reserved for system use.
        File_Backed_Virtual = 15,   // File-Backed Virtual
        Storage_Spaces = 16,    // Storage spaces
        NVMe = 17       // NVMe
    }

    public enum UniqueIdFormat
    {
        Vendor_Specific = 0,
        Vendor_Id = 1,
        EUI64 = 2,
        FCPH_Name = 3,
        SCSI_Name_String = 8
    }

    public enum HealthStatus
    {
        Healthy = 0,    // The disk is functioning normally.
        Warning = 1,    // The disk is still functioning, but has detected errors or issues that require administrator intervention.
        Unhealthy = 2   // The volume is not functioning, due to errors or failures. The volume needs immediate attention from an administrator.
    }

    public enum OfflineReason
    {
        Policy = 1,                         //The user requested the disk to be offline.
        Redundant_Path = 2,                 //The disk is used for multi-path I/O.
        Snapshot = 3,                       //The disk is a snapshot disk.
        Collision = 4,                      //There was a signature or identifier collision with another disk.
        Resource_Exhaustion = 5,            //There were insufficient resources to bring the disk online.
        Critical_Write_Failures = 6,        //There were critical write failures on the disk.
        Data_Integrity_Scan_Required = 7    //A data integrity scan is required.
    }

    public enum DiskOperationalStatus
    {
        Unknown = 0,            // The operational status is unknown.
        Other = 1,              // A vendor-specific OperationalStatus has been specified by setting the OtherOperationalStatusDescription property.
        OK = 2,                 // The disk is responding to commands and is in a normal operating state.
        Degraded = 3,           // The disk is responding to commands, but is not running in an optimal operating state.
        Stressed = 4,           // The disk is functioning, but needs attention. For example, the disk might be overloaded or overheated.
        Predictive_Failure = 5, // The disk is functioning, but a failure is likely to occur in the near future.
        Error = 6,              // An error has occurred.
        NonRecoverable_Error = 7,   // A non-recoverable error has occurred.
        Starting = 8,           // The disk is in the process of starting.
        Stopping = 9,           // The disk is in the process of stopping.
        Stopped = 10,           // The disk was stopped or shut down in a clean and orderly fashion.
        In_Service = 11,        // The disk is being configured, maintained, cleaned, or otherwise administered.
        No_Contact = 12,        // The storage provider has knowledge of the disk, but has never been able to establish communication with it.
        Lost_Communication = 13,       // The storage provider has knowledge of the disk and has contacted it successfully in the past, but the disk is currently unreachable.
        Aborted = 14,           // Similar to Stopped, except that the disk stopped abruptly and may require configuration or maintenance.
        Dormant = 15,           // The disk is reachable, but it is inactive.
        Supporting_Entity_in_Error = 16,       // This status value does not necessarily indicate trouble with the disk, but it does indicate that another device or connection that the disk depends on may need attention.
        Completed = 17,         // The disk has completed an operation. This status value should be combined with OK, Error, or Degraded, depending on the outcome of the operation.
        Online = 0xD010,        // In Windows-based storage subsystems, this indicates that the object is online.
        Not_Ready = 0xD011,     // In Windows-based storage subsystems, this indicates that the object is not ready.
        No_Media = 0xD012,      // In Windows-based storage subsystems, this indicates that the object has no media present.
        Offline = 0xD013,       // In Windows-based storage subsystems, this indicates that the object is offline.
        Failed = 0xD014         // In Windows-based storage subsystems, this indicates that the object is in a failed state.
    }

    public enum PartitionStyle
    {
        Unknown = 0,    // The partition style is unknown.
        MBR = 1,        // Master Boot Record (MBR)
        GPT = 2         // GUID Partition Table (GPT)
    }

    public enum ProvisioningType
    {
        Unknown = 0,    // The provisioning scheme is unspecified.
        Thin = 1,       // The storage for the disk is allocated on-demand.
        Fixed = 2       // The storage is allocated when the disk is created.
    }
}