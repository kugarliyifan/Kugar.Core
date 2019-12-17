using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Kugar.Core.ExtMethod;

namespace Kugar.Core
{
    public static class HardwareInfo
    {
        private static int HardDriveCount = 1;

        static HardwareInfo()
        {
            HardDriveCount = GetHardDriveCount();
        }

        private static int GetHardDriveCount()
        {
            using (var searcher = new ManagementObjectSearcher("Select * FROM Win32_DiskDrive"))
            using (var moList = searcher.Get())
            {
                return moList.Count;   
            }
        }

        /// <summary>
        ///     取CPU编号
        /// </summary>
        /// <returns></returns>
        public static string[] GetCpuID()
        {
            try
            {
                var mc = new ManagementClass("Win32_Processor");
                var moc = mc.GetInstances();

                var temp = new List<string>();


                foreach (ManagementObject mo in moc)
                {

                    temp.Add(mo.Properties["ProcessorId"].Value.ToString());
                }
                return temp.ToArray();
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// 取第一块硬盘编号
        /// </summary>
        /// <returns></returns>
        public static string[] GetHardDiskID()
        {
            var temp = new List<string>();

            for (int i = 0; i < HardDriveCount; i++)
            {
                var t = AtapiDevice.GetHddInfo((byte)i);

                temp.Add(t.SerialNumber);
            }

            //using (var searcher = new ManagementObjectSearcher("Select * FROM Win32_PhysicalMedia"))
            //{
            //    var moList = searcher.Get();

            //    for (int i = 0; i < moList.Count; i++)
            //    {
            //        try
            //        {
            //            var t = AtapiDevice.GetHddInfo((byte)i);

            //            temp.Add(t.SerialNumber);
            //        }
            //        catch (Exception)
            //        {
            //            continue;
            //        }
            //    }    
            //}

            return temp.ToArray();

        }

        /// <summary>
        ///     获取第一块硬盘的序列号
        /// </summary>
        /// <returns></returns>
        public static string GetFirstHardDiskID()
        {
            try
            {
                return AtapiDevice.GetHddInfo(0).SerialNumber;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            
        }

        /// <summary>
        ///     获取本机机器码
        /// </summary>
        /// <returns></returns>
        public static string GetLocalMachineCode()
        {
            string mc ;

            mc = GetFirstHardDiskID();

            if (mc.IsNullOrEmpty())
            {
                var tempHDNet = GetHardwareNetworkCard();

                if (tempHDNet==null || tempHDNet.Length<=0)
                {
                    tempHDNet = GetWirelessNetworkCard();
                }

                if (tempHDNet!=null && tempHDNet.Length>0 )
                {
                    mc = tempHDNet[0].Id;
                }
            }

            return mc.MD5_32();
        }

        /// <summary>
        ///     读取物理网卡接口
        /// </summary>
        /// <returns></returns>
        public static NetworkInterface[] GetHardwareNetworkCard()
        {
            var fNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var macstr = new List<NetworkInterface>();

            foreach (var adapter in fNetworkInterfaces)
            {
                string fRegistryKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + adapter.Id + "\\Connection";

                RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);

                if (rk != null)
                {
                    string fPnpInstanceID = rk.GetValue("PnpInstanceID", "").ToString();

                    if (fPnpInstanceID.Length > 3 && fPnpInstanceID.Substring(0, 3) == "PCI")
                    {
                        macstr.Add(adapter);
                    }

                }

            }

            return macstr.ToArray();
        }

        /// <summary>
        ///     获取虚拟网卡接口
        /// </summary>
        /// <returns></returns>
        public static NetworkInterface[] GetVirtualNetworkCard()
        {
            var fNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var macstr = new List<NetworkInterface>();

            foreach (var adapter in fNetworkInterfaces)
            {
                string fRegistryKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + adapter.Id + "\\Connection";

                RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);

                if (rk != null)
                {
                    int fMediaSubType = Convert.ToInt32(rk.GetValue("MediaSubType", 0));

                    if (fMediaSubType == 1)
                        macstr.Add(adapter);
                }

            }

            return macstr.ToArray();
        }

        /// <summary>
        ///     获取无线网卡接口
        /// </summary>
        /// <returns></returns>
        public static NetworkInterface[] GetWirelessNetworkCard()
        {
            var fNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var macstr = new List<NetworkInterface>();

            foreach (var adapter in fNetworkInterfaces)
            {
                string fRegistryKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + adapter.Id + "\\Connection";

                RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);

                if (rk != null)
                {
                    int fMediaSubType = Convert.ToInt32(rk.GetValue("MediaSubType", 0));

                    if (fMediaSubType == 2)
                        macstr.Add(adapter);
                }

            }

            return macstr.ToArray();
        }
    }


}


	[Serializable]
	internal struct HardDiskInfo
	{
		/// <summary>
		/// 型号
		/// </summary>
		public string ModuleNumber;
		/// <summary>
		/// 固件版本
		/// </summary>
		public string Firmware;
		/// <summary>
		/// 序列号
		/// </summary>
		public string SerialNumber;
		/// <summary>
		/// 容量，以M为单位
		/// </summary>
		public uint Capacity;
	}

	#region Internal Structs

	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct GetVersionOutParams
	{
		public byte bVersion;
		public byte bRevision;
		public byte bReserved;
		public byte bIDEDeviceMap;
		public uint fCapabilities;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
		public uint[] dwReserved; // For future use.
	}

	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct IdeRegs
	{
		public byte bFeaturesReg;
		public byte bSectorCountReg;
		public byte bSectorNumberReg;
		public byte bCylLowReg;
		public byte bCylHighReg;
		public byte bDriveHeadReg;
		public byte bCommandReg;
		public byte bReserved;
	}

	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct SendCmdInParams
	{
		public uint cBufferSize;
		public IdeRegs irDriveRegs;
		public byte bDriveNumber;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public byte[] bReserved;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
		public uint[] dwReserved;
		public byte bBuffer;
	}

	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct DriverStatus
	{
		public byte bDriverError;
		public byte bIDEStatus;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
		public byte[] bReserved;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
		public uint[] dwReserved;
	}

	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct SendCmdOutParams
	{
		public uint cBufferSize;
		public DriverStatus DriverStatus;
		public IdSector bBuffer;
	}

	[StructLayout(LayoutKind.Sequential, Pack=1, Size=512)]
	internal struct IdSector
	{
		public ushort wGenConfig;
		public ushort wNumCyls;
		public ushort wReserved;
		public ushort wNumHeads;
		public ushort wBytesPerTrack;
		public ushort wBytesPerSector;
		public ushort wSectorsPerTrack;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public ushort[] wVendorUnique;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
		public byte[] sSerialNumber;
		public ushort wBufferType;
		public ushort wBufferSize;
		public ushort wECCSize;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
		public byte[] sFirmwareRev;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
		public byte[] sModelNumber;
		public ushort wMoreVendorUnique;
		public ushort wDoubleWordIO;
		public ushort wCapabilities;
		public ushort wReserved1;
		public ushort wPIOTiming;
		public ushort wDMATiming;
		public ushort wBS;
		public ushort wNumCurrentCyls;
		public ushort wNumCurrentHeads;
		public ushort wNumCurrentSectorsPerTrack;
		public uint ulCurrentSectorCapacity;
		public ushort wMultSectorStuff;
		public uint ulTotalAddressableSectors;
		public ushort wSingleWordDMA;
		public ushort wMultiWordDMA;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=128)]
		public byte[] bReserved;
	}

	#endregion

	/// <summary>
	/// ATAPI驱动器相关
	/// </summary>
	public class AtapiDevice
	{

		#region DllImport

		[DllImport("kernel32.dll", SetLastError=true)]
		static extern int CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError=true)]
		static extern IntPtr CreateFile(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			IntPtr lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport("kernel32.dll")]
		static extern int DeviceIoControl(
			IntPtr hDevice,
			uint dwIoControlCode,
			IntPtr lpInBuffer,
			uint nInBufferSize,
			ref GetVersionOutParams lpOutBuffer,
			uint nOutBufferSize,
			ref uint lpBytesReturned,
			[Out] IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		static extern int DeviceIoControl(
			IntPtr hDevice,
			uint dwIoControlCode,
			ref SendCmdInParams lpInBuffer,
			uint nInBufferSize,
			ref SendCmdOutParams lpOutBuffer,
			uint nOutBufferSize,
			ref uint lpBytesReturned,
			[Out] IntPtr lpOverlapped);

		const uint DFP_GET_VERSION = 0x00074080;
		const uint DFP_SEND_DRIVE_COMMAND = 0x0007c084;
		const uint DFP_RECEIVE_DRIVE_DATA = 0x0007c088;

		const uint GENERIC_READ = 0x80000000;
		const uint GENERIC_WRITE = 0x40000000;
		const uint FILE_SHARE_READ = 0x00000001;
		const uint FILE_SHARE_WRITE = 0x00000002;
		const uint CREATE_NEW = 1;
		const uint OPEN_EXISTING = 3;

		#endregion

		#region GetHddInfo

		/// <summary>
		/// 获得硬盘信息
		/// </summary>
		/// <param name="driveIndex">硬盘序号</param>
		/// <returns>硬盘信息</returns>
		/// <remarks>
		/// 参考lu0的文章：http://lu0s1.3322.org/App/2k1103.html
		/// by sunmast for everyone
		/// thanks lu0 for his great works
		/// 在Windows 98/ME中，S.M.A.R.T并不缺省安装，请将SMARTVSD.VXD拷贝到%SYSTEM%\IOSUBSYS目录下。
		/// 在Windows 2000/2003下，需要Administrators组的权限。
		/// </remarks>
		/// <example>
		/// AtapiDevice.GetHddInfo()
		/// </example>
		internal static HardDiskInfo GetHddInfo(byte driveIndex)
		{
			switch(Environment.OSVersion.Platform)
			{
				case PlatformID.Win32Windows:
					return GetHddInfo9x(driveIndex);
				case PlatformID.Win32NT:
					return GetHddInfoNT(driveIndex);
				case PlatformID.Win32S:
					throw new NotSupportedException("Win32s is not supported.");
				case PlatformID.WinCE:
					throw new NotSupportedException("WinCE is not supported.");
				default:
					throw new NotSupportedException("Unknown Platform.");
			}
		}

		#region GetHddInfo9x

		private static HardDiskInfo GetHddInfo9x(byte driveIndex)
		{
			GetVersionOutParams vers = new GetVersionOutParams();
			SendCmdInParams inParam = new SendCmdInParams();
			SendCmdOutParams outParam = new SendCmdOutParams();
			uint bytesReturned = 0;

			IntPtr hDevice = CreateFile(
				@"\\.\Smartvsd",
				0,
				0,
				IntPtr.Zero,
				CREATE_NEW,
				0,
				IntPtr.Zero);
			if (hDevice == IntPtr.Zero)
			{
				throw new Exception("Open smartvsd.vxd failed.");
			}
			if (0 == DeviceIoControl(
				hDevice,
				DFP_GET_VERSION,
				IntPtr.Zero,
				0,
				ref vers,
				(uint)Marshal.SizeOf(vers),
				ref bytesReturned,
				IntPtr.Zero))
			{
				CloseHandle(hDevice);
				throw new Exception("DeviceIoControl failed:DFP_GET_VERSION");
			}
			// If IDE identify command not supported, fails
			if (0 == (vers.fCapabilities & 1))
			{
				CloseHandle(hDevice);
				throw new Exception("Error: IDE identify command not supported.");
			}
			if (0 != (driveIndex & 1))
			{
				inParam.irDriveRegs.bDriveHeadReg = 0xb0;
			}
			else
			{
				inParam.irDriveRegs.bDriveHeadReg = 0xa0;
			}
			if (0 != (vers.fCapabilities & (16 >> driveIndex)))
			{
				// We don't detect a ATAPI device.
				CloseHandle(hDevice);
				throw new Exception(string.Format("Drive {0} is a ATAPI device, we don't detect it",driveIndex + 1));
			}
			else
			{
				inParam.irDriveRegs.bCommandReg = 0xec;
			}
			inParam.bDriveNumber = driveIndex;
			inParam.irDriveRegs.bSectorCountReg = 1;
			inParam.irDriveRegs.bSectorNumberReg = 1;
			inParam.cBufferSize = 512;
			if (0 == DeviceIoControl(
				hDevice,
				DFP_RECEIVE_DRIVE_DATA,
				ref inParam,
				(uint)Marshal.SizeOf(inParam),
				ref outParam,
				(uint)Marshal.SizeOf(outParam),
				ref bytesReturned,
				IntPtr.Zero))
			{
				CloseHandle(hDevice);
				throw new Exception("DeviceIoControl failed: DFP_RECEIVE_DRIVE_DATA");
			}
			CloseHandle(hDevice);

			return GetHardDiskInfo(outParam.bBuffer);
		}

		#endregion

		#region GetHddInfoNT

		private static HardDiskInfo GetHddInfoNT(byte driveIndex)
		{
			GetVersionOutParams vers = new GetVersionOutParams();
			SendCmdInParams inParam = new SendCmdInParams();
			SendCmdOutParams outParam = new SendCmdOutParams();
			uint bytesReturned = 0;

			// We start in NT/Win2000
			IntPtr hDevice = CreateFile(
				string.Format(@"\\.\PhysicalDrive{0}",driveIndex),
				GENERIC_READ | GENERIC_WRITE,
				FILE_SHARE_READ | FILE_SHARE_WRITE,
				IntPtr.Zero,
				OPEN_EXISTING,
				0,
				IntPtr.Zero);
			if (hDevice == IntPtr.Zero)
			{
				throw new Exception("CreateFile faild.");
			}
			if (0 == DeviceIoControl(
				hDevice,
				DFP_GET_VERSION,
				IntPtr.Zero,
				0,
				ref vers,
				(uint)Marshal.SizeOf(vers),
				ref bytesReturned,
				IntPtr.Zero))
			{
				CloseHandle(hDevice);
				throw new Exception(string.Format("Drive {0} may not exists.",driveIndex + 1));
			}
			// If IDE identify command not supported, fails
			if (0 == (vers.fCapabilities & 1))
			{
				CloseHandle(hDevice);
				throw new Exception("Error: IDE identify command not supported.");
			}
			// Identify the IDE drives
			if (0 != (driveIndex & 1))
			{
				inParam.irDriveRegs.bDriveHeadReg = 0xb0;
			}
			else
			{
				inParam.irDriveRegs.bDriveHeadReg=0xa0;
			}
			if (0 != (vers.fCapabilities & (16 >> driveIndex)))
			{
				// We don't detect a ATAPI device.
				CloseHandle(hDevice);
				throw new Exception(string.Format("Drive {0} is a ATAPI device, we don't detect it.",driveIndex + 1));
			}
			else
			{
				inParam.irDriveRegs.bCommandReg = 0xec;
			}
			inParam.bDriveNumber = driveIndex;
			inParam.irDriveRegs.bSectorCountReg = 1;
			inParam.irDriveRegs.bSectorNumberReg = 1;
			inParam.cBufferSize = 512;

			if (0 == DeviceIoControl(
				hDevice,
				DFP_RECEIVE_DRIVE_DATA,
				ref inParam,
				(uint)Marshal.SizeOf(inParam),
				ref outParam,
				(uint)Marshal.SizeOf(outParam),
				ref bytesReturned,
				IntPtr.Zero))
			{
				CloseHandle(hDevice);
				throw new Exception("DeviceIoControl failed: DFP_RECEIVE_DRIVE_DATA");
			}
			CloseHandle(hDevice);

			return GetHardDiskInfo(outParam.bBuffer);
		}

		#endregion

		private static HardDiskInfo GetHardDiskInfo(IdSector phdinfo)
		{
			HardDiskInfo hddInfo = new HardDiskInfo();

			ChangeByteOrder(phdinfo.sModelNumber);
			hddInfo.ModuleNumber = Encoding.ASCII.GetString(phdinfo.sModelNumber).Trim();

			ChangeByteOrder(phdinfo.sFirmwareRev);
			hddInfo.Firmware = Encoding.ASCII.GetString(phdinfo.sFirmwareRev).Trim();

			ChangeByteOrder(phdinfo.sSerialNumber);
			hddInfo.SerialNumber = Encoding.ASCII.GetString(phdinfo.sSerialNumber).Trim();

			hddInfo.Capacity = phdinfo.ulTotalAddressableSectors / 2 / 1024;

			return hddInfo;
		}

		private static void ChangeByteOrder(byte[] charArray)
		{
			byte temp;
			for(int i = 0; i < charArray.Length; i += 2)
			{
				temp = charArray[i];
				charArray[i] = charArray[i+1];
				charArray[i+1] = temp;
			}
		}

		#endregion
	}


