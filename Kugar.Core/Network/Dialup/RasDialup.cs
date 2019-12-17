using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Kugar.Core.SystemManager;

namespace Kugar.Core.Network.Dialup
{
    public static class RasDialup
    {
        #region "结构体声明"

        public struct GUID
        {
            public uint Data1;
            public ushort Data2;
            public ushort Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct LUID
        {
            int LowPart;
            int HighPart;
        }

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        //public struct RASCONN
        //{
        //    public int dwSize;
        //    public IntPtr hrasconn;

        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        //    public byte[] szEntryNameBytes;

        //    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst =RAS_MaxDeviceType + 1)]
        //    //public string szDeviceType;

        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = RAS_MaxDeviceType + 1)]
        //    public byte[] szDeviceType;

        //    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst =RAS_MaxDeviceName + 1)]
        //    //public string szDeviceName;

        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = RAS_MaxDeviceName + 1)]
        //    public byte[] szDeviceName;

        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]//MAX_PAPTH=260
        //    public string szPhonebook;
        //    public int dwSubEntry;
        //    public GUID guidEntry;
        //#if (WINVER501)
        //   int     dwFlags;
        //   public LUID      luid;
        //#endif
        //        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct RASCONN_XP
        {
            public int dwSize;
            public IntPtr hrasconn;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = RAS_MaxEntryName + 1)]
            public byte[] szEntryNameBytes;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = RAS_MaxDeviceType + 1)]
            public byte[] szDeviceType;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = RAS_MaxDeviceName + 1)]
            public byte[] szDeviceName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PATH)]//MAX_PAPTH=260
            public byte[] szPhonebook;

            public int subEntryId;
            public Guid entryId;

            public RASCF connectionOptions;
            public LUID sessionId;
           
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct RASCONN_Win7
        {
            public int dwSize;
            public IntPtr hrasconn;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = RAS_MaxEntryName + 1)]
            public byte[] szEntryNameBytes;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = RAS_MaxDeviceType + 1)]
            public byte[] szDeviceType;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = RAS_MaxDeviceName + 1)]
            public byte[] szDeviceName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PATH)]//MAX_PAPTH=260
            public byte[] szPhonebook;

            public int subEntryId;
            public Guid entryId;

            public RASCF connectionOptions;
            public LUID sessionId;
            public Guid correlationId;
        }

        [Flags]
        public enum RASCF
        {
            /// <summary>
            /// No connection options specified.
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Specifies the connection is available to all users.
            /// </summary>
            AllUsers = 0x1,

            /// <summary>
            /// Specifies the credentials used for the connection are the default credentials.
            /// </summary>
            GlobalCredentials = 0x2,

            /// <summary>
            /// Specifies the owner of the connection is known.
            /// </summary>
            OwnerKnown = 0x4,

            /// <summary>
            /// Specifies the owner of the connection matches the current user.
            /// </summary>
            OwnerMatch = 0x8
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct RasStats
        {
            public int dwSize;
            public int dwBytesXmited;
            public int dwBytesRcved;
            public int dwFramesXmited;
            public int dwFramesRcved;
            public int dwCrcErr;
            public int dwTimeoutErr;
            public int dwAlignmentErr;
            public int dwHardwareOverrunErr;
            public int dwFramingErr;
            public int dwBufferOverrunErr;
            public int dwCompressionRatioIn;
            public int dwCompressionRatioOut;
            public int dwBps;
            public int dwConnectionDuration;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct RasEntryName
        {
            public int dwSize;
            //[MarshalAs(UnmanagedType.ByValTStr,SizeConst=(int)RasFieldSizeConstants.RAS_MaxEntryName + 1)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxEntryName + 1)]
            public string szEntryName;
            //#if WINVER5
            //  public int dwFlags;
            //  [MarshalAs(UnmanagedType.ByValTStr,SizeConst=260+1)]
            //  public string szPhonebookPath;
            //#endif
        }

        public enum RASCONNSUBSTATE
        {
            RASCSS_None,
            RASCSS_Dormant,
            RASCSS_Reconnecting,
            RASCSS_Reconnected  
        }

        #endregion

        #region "API函数声明"

        //[DllImport("Rasapi32.dll", EntryPoint = "RasEnumConnectionsA",//EntryPoint = "RasEnumConnectionsA",
        // SetLastError = true)]
        //private static extern int RasEnumConnectionsSingle
        //    (
        //    [In, Out] ref RASCONN lprasconn, // buffer to receive connections data
        //    ref int lpcb, // size in bytes of buffer
        //    ref int lpcConnections // number of connections written to buffer
        //    );

        //[DllImport("rasapi32.dll", EntryPoint = "RasEnumConnectionsA" ,SetLastError = true, CharSet = CharSet.Auto)]
        //public static extern int RasEnumConnectionsSingle(
        //    [In, Out] RASCONN[] rasconn,
        //    [In, Out] ref int cb,
        //    [Out] out int connections);

        //[DllImport("Rasapi32.dll", EntryPoint = "RasEnumConnectionsA",SetLastError = true)]
        //public static extern int RasEnumConnectionsSingle(ref RASCONN lprasconn, ref int lpcb,  ref int lpcConnections );
           
        //[DllImport("Rasapi32.dll", EntryPoint = "RasEnumConnectionsA",SetLastError = true)]
        //private static extern int RasEnumConnections([In, Out] RASCONN[] lprasconn,ref int lpcb,ref int lpcConnections);


        [DllImport("Rasapi32.dll", EntryPoint = "RasEnumConnectionsA", SetLastError = true)]
        private static extern int RasEnumConnections_XP([In, Out] RASCONN_XP[] lprasconn, ref int lpcb, ref int lpcConnections);

        //[DllImport("Rasapi32.dll", EntryPoint = "RasEnumConnectionsA", SetLastError = true)]
        //private static extern int RasEnumConnections_2000([In, Out] RASCONN_2000[] lprasconn, ref int lpcb, ref int lpcConnections);

        [DllImport("Rasapi32.dll", EntryPoint = "RasEnumConnectionsA", SetLastError = true)]
        private static extern int RasEnumConnections_Win7([In, Out] RASCONN_Win7[] lprasconn, ref int lpcb, ref int lpcConnections);


        [DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
        private static extern uint RasGetConnectionStatistics(
            IntPtr hRasConn,       // handle to the connection
            [In, Out]ref RasStats lpStatistics  // buffer to receive statistics
            );
        [DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
        private extern static uint RasHangUp(
            IntPtr hrasconn  // handle to the RAS connection to hang up
            );

        [DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
        private extern static uint RasEnumEntries(
            string reserved,              // reserved, must be NULL
            string lpszPhonebook,         // pointer to full path and
            //  file name of phone-book file
            [In, Out]RasEntryName[] lprasentryname, // buffer to receive
            //  phone-book entries
            ref int lpcb,                  // size in bytes of buffer
            out int lpcEntries             // number of entries written
            //  to buffer
            );


        [DllImport("wininet.dll", CharSet = CharSet.Auto)]
        private extern static int InternetDial(
            IntPtr hwnd,
            [In]string lpszConnectoid,
            uint dwFlags,
            ref int lpdwConnection,
            uint dwReserved
            );

        [DllImport("WININET", CharSet = CharSet.Auto)]
        private static extern bool InternetGetConnectedState(
            ref int lpdwFlags,
            int dwReserved);

        #endregion

        #region "常数"
        //const int RAS_MaxEntryName = 256;
        //const int RAS_MaxDeviceType = 16;
        //const int RAS_MaxDeviceName = 128;

        const int RAS_MaxEntryName = 256; //20;
        const int RAS_MaxDeviceType = 16;
        const int RAS_MaxDeviceName = 128;

        const int RAS_MaxEntryName_XP = 20;

        const int RAS_MaxPhoneNumber = 128;
        const int RAS_MaxCallbackNumber = RAS_MaxPhoneNumber;

        const int MAX_PATH = 260;
        const int ERROR_BUFFER_TOO_SMALL = 603;
        const int ERROR_SUCCESS = 0;


        const int RAS_Connected = 0x2000;

        const int INTERNET_RAS_INSTALLED = 0x10;
        #endregion

        public class DialupConnectInfo
        {
            private string _name;
            private int _status;
            private string _deviceName;
            private TimeSpan _dialupSpan;
            private IntPtr _handle;

            public DialupConnectInfo(string name, int status, string deviceName, TimeSpan dialupSpan, IntPtr handle)
            {
                _name = name;
                _status = status;
                _deviceName = deviceName;
                _dialupSpan = dialupSpan;
                _handle = handle;
            }

            public string Name { get { return _name; } }
            public int Status { get { return _status; } }
            public string DeviceName { get { return _deviceName; } }
            public TimeSpan DialupSpan { get { return _dialupSpan; } }
            internal IntPtr Handle { get { return _handle; } }
        }

        private static ConcurrentDictionary<Guid, IAsyncResult> _cacheAsyncResult = new ConcurrentDictionary<Guid, IAsyncResult>();

        /// <summary>
        ///     获取所有拨号连接名称
        /// </summary>
        /// <returns></returns>
        public static string[] GetDialupName()
        {
            int lpNames = 1;
            int lpSize = 0;
            RasEntryName[] names = null;

            int entryNameSize = Marshal.SizeOf(typeof(RasEntryName));

            lpSize = lpNames * entryNameSize;

            names = new RasEntryName[lpNames];

            names[0].dwSize = entryNameSize;

            uint retval = RasEnumEntries(null, null, names, ref lpSize, out lpNames);

            if (retval != 0 && lpNames <=0)
            {
                return null;
            }

            if (lpNames > 1)    
            {
                names = new RasEntryName[lpNames];
                for (int i = 0; i < names.Length; i++)
                {
                    names[i].dwSize = entryNameSize;
                }
                retval = RasEnumEntries(null, null, names, ref lpSize, out lpNames);

                if (retval!=0)
                {
                    return null ;
                }
            }

            if (lpNames > 0)
            {
                var m_ConnectionNames = new string[names.Length];

                for (int i = 0; i < names.Length; i++)
                {
                    m_ConnectionNames[i] = names[i].szEntryName;
                }

                return m_ConnectionNames;
            }

            return null;
        }

        /// <summary>
        ///     获取所有已连接的拨号连接信息
        /// </summary>
        /// <returns></returns>
        public static DialupConnectInfo[] GetConnectedDialupInfo()
        {
            if (SystemManager.SystemManager.OSVersion>=WinVer.Windows7)
            {
                return getConnectedDialupInfoWin7();
            }
            else
            {
                return getConnectedDialupInfoWinXP();
            }
            

            //RASCONN lprasConn = new RASCONN();
            //lprasConn.dwSize = 0x19c;// Marshal.SizeOf(typeof(RASCONN));
            //lprasConn.hrasconn = IntPtr.Zero;

            //int lpcb = 0;
            //int lpcConnections = 0;
            //int nRet = 0;
            //lpcb = 0x19c;// Marshal.SizeOf(typeof(RASCONN));
            //nRet = RasEnumConnectionsSingle(ref lprasConn, ref lpcb, ref  lpcConnections);

            //if (nRet != 0 && lpcConnections <= 0)
            //{
            //    throw new Exception(nRet.ToString());
            //    return null;
            //}

            //RASCONN[] arr = new RASCONN[lpcConnections];

            //for (int i = 0; i < lpcConnections; i++)
            //{
            //    arr[i].dwSize = lpcb;
            //}

            //nRet = RasEnumConnections(arr, ref lpcb, ref lpcConnections);

            //if (nRet!=0 && lpcConnections<=0)
            //{
            //    throw new Exception(nRet.ToString());
            //    return null;
            //}

            //var infoList = new DialupConnectInfo[lpcConnections];

            //for (int i = 0; i < lpcConnections; i++)
            //{
            //    var status = new RasStats();

            //    status.dwSize = Marshal.SizeOf(typeof (RasStats));

            //    var newRet = RasGetConnectionStatistics(arr[i].hrasconn,ref status);

            //    if (newRet != 0)
            //    {
            //        throw new Exception(newRet.ToString());
            //        return null;
            //    }

            //    int hours = 0;
            //    int minutes = 0;
            //    int seconds = 0;

            //    hours = ((status.dwConnectionDuration / 1000) / 3600);
            //    minutes = ((status.dwConnectionDuration / 1000) / 60) - (hours * 60);
            //    seconds = ((status.dwConnectionDuration / 1000)) - (minutes * 60) - (hours * 3600);

            //    var name = Encoding.Default.GetString(lprasConn.szEntryNameBytes).TrimEnd('\0');
            //    var deviceName = Encoding.Default.GetString(lprasConn.szDeviceName).TrimEnd('\0');
            //    infoList[i] = new DialupConnectInfo(name, 0, deviceName, new TimeSpan(hours, minutes, seconds), arr[i].hrasconn); ;
            //}

            //return infoList;

        }

        public static int Dialup(string connectName)
        {
            int temp = 0;

            uint INTERNET_AUTO_DIAL_UNATTENDED = 2;

            int retVal = InternetDial(IntPtr.Zero, connectName, 0X8000, ref temp, 0);

            return retVal;
        }

        public static IAsyncResult BeginDialup(string connectName, AsyncCallback callback, object state)
        {
            var id = Guid.NewGuid();

            var temp = new DialupAsyncResult(id, callback, state);

            if(_cacheAsyncResult.TryAdd(id, temp))
            {
                ThreadPool.QueueUserWorkItem((s) =>
                                             {
                                                 Dialup(connectName);

                                                 temp.IsCompleted = true;

                                                 temp.Callback(temp);

                                             }, null);
            }

            return temp;
        }

        public static void EndDialup(IAsyncResult iar)
        {
            var t = (DialupAsyncResult) iar;

#if NET2
            _cacheAsyncResult.Remove(t.ID);
#else

            IAsyncResult iar1;

            _cacheAsyncResult.TryRemove(t.ID, out iar1);
#endif

        }

        public static uint DiconnectConnection(string connectName)
        {
            int flags = 0;
            InternetGetConnectedState(ref flags, 0);
            if (!((flags & INTERNET_RAS_INSTALLED) == INTERNET_RAS_INSTALLED))
                throw new NotSupportedException();

            var conns = GetConnectedDialupInfo();

            if (conns == null || conns.Length <= 0)
            {
                return 0;
                //throw new Exception("没有可用的活动连接");
            }

            foreach (var connectInfo in conns)
            {
                if (connectInfo.Name == connectName && connectInfo.Handle != IntPtr.Zero)
                {
                    var t=RasHangUp(connectInfo.Handle);

                    if (t!=0)
                    {
                        return t;
                    }
                }
            }

            return 0;
        }

        public static void DiconnectConnection(DialupConnectInfo dialupConnectInfo)
        {
            if (dialupConnectInfo != null)
            {
                throw new ArgumentNullException("dialupConnectInfo");
            }

            int flags = 0;
            InternetGetConnectedState(ref flags, 0);
            if (!((flags & INTERNET_RAS_INSTALLED) == INTERNET_RAS_INSTALLED))
                throw new NotSupportedException();


            if (dialupConnectInfo.Handle != IntPtr.Zero)
            {
                RasHangUp(dialupConnectInfo.Handle);
            }

        }

        public static void DisconnectAllConnection()
        {
            int flags = 0;
            InternetGetConnectedState(ref flags, 0);
            if (!((flags & INTERNET_RAS_INSTALLED) == INTERNET_RAS_INSTALLED))
                throw new NotSupportedException();

            var conns = GetConnectedDialupInfo();

            if (conns == null || conns.Length <= 0)
            {
                return;
            }

            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i].Handle == IntPtr.Zero) continue;
                RasHangUp(conns[i].Handle);

            }
        }

        public static IAsyncResult BeginDisconnectAllConnection(AsyncCallback callback,object state)
        {
            var id = Guid.NewGuid();

            var temp = new DialupAsyncResult(id, callback, state);

            if (_cacheAsyncResult.TryAdd(id, temp))
            {
                ThreadPool.QueueUserWorkItem((s) =>
                                             {
                                                 DisconnectAllConnection();

                                                 temp.IsCompleted = true;

                                                 temp.Callback(temp);

                                             }, null);
            }

            return temp;
        }

        public static void EndDisconnectAllConnection(IAsyncResult iar)
        {
            var t = (DialupAsyncResult)iar;

            //_cacheAsyncResult.Remove(t.ID);

#if NET2
            _cacheAsyncResult.Remove(t.ID);
#else

            IAsyncResult iar1;

            _cacheAsyncResult.TryRemove(t.ID, out iar1);
#endif

            //IAsyncResult iar1;

            //_cacheAsyncResult.TryRemove(t.ID, out iar1);
        }

        private struct DialupAsyncResult : IAsyncResult
        {
            private Lazy<AutoResetEvent> _lazyValue;
            private bool _isCompleted;
            private AsyncCallback _callback;
            private object _state;
            private Guid _id;
            private Exception _error;
            private string _connectName;

            public DialupAsyncResult(Guid id,Exception error, AsyncCallback callback, object state)
                : this()
            {
                _lazyValue = new Lazy<AutoResetEvent>(BuildWaitHandle);
                _callback = callback;
                _state = state;
                _isCompleted = false;
                _id = id;
                _error = error;
            }

            public DialupAsyncResult(Guid id, AsyncCallback callback, object state):this(id,null,callback,state){}

            public AsyncCallback Callback { get { return _callback; } }

            public Guid ID { get { return _id; } }

            public Exception Error{get { return _error; }}

            public bool HasError
            {
                get { return _error != null; }
            }

            #region Implementation of IAsyncResult

            public bool IsCompleted
            {
                get { return _isCompleted; }
                set
                {
                    _isCompleted = value;

                    if (value)
                    {
                        if (_lazyValue.IsValueCreated)
                        {
                            _lazyValue.Value.Set();
                        }
                    }
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return _lazyValue.Value; }
            }

            public object AsyncState
            {
                get { return _state; }
            }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            #endregion

            private AutoResetEvent BuildWaitHandle()
            {
                return new AutoResetEvent(false);
            }
        }

        private static DialupConnectInfo[] getConnectedDialupInfoWin7()
        {
            RASCONN_Win7[] lprasConn = new[] { new RASCONN_Win7() };
            lprasConn[0].dwSize =  Marshal.SizeOf(typeof(RASCONN_Win7));
            lprasConn[0].hrasconn = IntPtr.Zero;

            int lpcb = 0;
            int lpcConnections = 0;
            int nRet = 0;
            lpcb = Marshal.SizeOf(typeof(RASCONN_Win7));
            nRet = RasEnumConnections_Win7(lprasConn, ref lpcb, ref  lpcConnections);

            if (nRet != 0 && lpcConnections <= 0)
            {
                //throw new Exception(nRet.ToString());
                return null;
            }

            if (lpcConnections<=0)
            {
                return null;
            }

            lprasConn = new RASCONN_Win7[lpcConnections];

            for (int i = 0; i < lpcConnections; i++)
            {
                lprasConn[i].dwSize = lpcb;
            }

            nRet = RasEnumConnections_Win7(lprasConn, ref lpcb, ref lpcConnections);

            if (nRet != 0 && lpcConnections <= 0)
            {
                throw new Exception(nRet.ToString());
                return null;
            }

            var infoList = new DialupConnectInfo[lpcConnections];

            for (int i = 0; i < lpcConnections; i++)
            {
                var status = new RasStats();

                status.dwSize = Marshal.SizeOf(typeof(RasStats));

                var newRet = RasGetConnectionStatistics(lprasConn[i].hrasconn, ref status);

                if (newRet != 0)
                {
                    throw new Exception(newRet.ToString());
                    return null;
                }

                int hours = 0;
                int minutes = 0;
                int seconds = 0;

                hours = ((status.dwConnectionDuration / 1000) / 3600);
                minutes = ((status.dwConnectionDuration / 1000) / 60) - (hours * 60);
                seconds = ((status.dwConnectionDuration / 1000)) - (minutes * 60) - (hours * 3600);

                var name = Encoding.Default.GetString(lprasConn[i].szEntryNameBytes).TrimEnd('\0');
                var deviceName = Encoding.Default.GetString(lprasConn[i].szDeviceName).TrimEnd('\0');
                infoList[i] = new DialupConnectInfo(name, 0, deviceName, new TimeSpan(hours, minutes, seconds), lprasConn[i].hrasconn); ;
            }

            return infoList;
        }

        private static DialupConnectInfo[] getConnectedDialupInfoWinXP()
        {
            RASCONN_XP[] lprasConn = new[] { new RASCONN_XP() };
            lprasConn[0].dwSize = Marshal.SizeOf(typeof(RASCONN_XP));
            lprasConn[0].hrasconn = IntPtr.Zero;

            int lpcb = 0;
            int lpcConnections = 0;
            int nRet = 0;
            lpcb = Marshal.SizeOf(typeof(RASCONN_XP));
            nRet = RasEnumConnections_XP(lprasConn, ref lpcb, ref  lpcConnections);

            if (nRet != 0 && lpcConnections <= 0)
            {
                //throw new Exception(nRet.ToString());
                return null;
            }

            if (lpcConnections <= 0)
            {
                return null;
            }

            lprasConn = new RASCONN_XP[lpcConnections];

            for (int i = 0; i < lpcConnections; i++)
            {
                lprasConn[i].dwSize = lpcb;
            }

            nRet = RasEnumConnections_XP(lprasConn, ref lpcb, ref lpcConnections);

            if (nRet != 0 && lpcConnections <= 0)
            {
                //throw new Exception(nRet.ToString());
                return null;
            }

            var infoList = new DialupConnectInfo[lpcConnections];

            for (int i = 0; i < lpcConnections; i++)
            {
                var status = new RasStats();

                status.dwSize = Marshal.SizeOf(typeof(RasStats));

                var newRet = RasGetConnectionStatistics(lprasConn[i].hrasconn, ref status);

                if (newRet != 0)
                {
                    //throw new Exception(newRet.ToString());
                    return null;
                }

                int hours = 0;
                int minutes = 0;
                int seconds = 0;

                hours = ((status.dwConnectionDuration / 1000) / 3600);
                minutes = ((status.dwConnectionDuration / 1000) / 60) - (hours * 60);
                seconds = ((status.dwConnectionDuration / 1000)) - (minutes * 60) - (hours * 3600);

                var name = Encoding.Default.GetString(lprasConn[i].szEntryNameBytes).TrimEnd('\0');
                var deviceName = Encoding.Default.GetString(lprasConn[i].szDeviceName).TrimEnd('\0');
                infoList[i] = new DialupConnectInfo(name, 0, deviceName, new TimeSpan(hours, minutes, seconds), lprasConn[i].hrasconn); ;
            }

            return infoList;
        }
    }


}
