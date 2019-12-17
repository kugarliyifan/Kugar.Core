using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Configuration.Install;
using System.ServiceProcess;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.SystemManager
{
    public static class SystemManager
    {
        static SystemManager()
        {
            _osVersion = getSystemVersion();
        }

        /// <summary>
        ///    注册指定的程序到自动启动
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="progranPath">用于指定要自动启动的程序路径,如果该路径为空,则表示删除keyName指定的键值</param>
        public static void RegisterToAutoStartup(string keyName,string progranPath)
        {
            var regKey=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

            if (regKey!=null)
            {
                if (progranPath.IsNullOrEmpty())
                {
                    regKey.DeleteValue(keyName, false);
                }
                else
                {
                    regKey.SetValue(keyName,progranPath,RegistryValueKind.String);
                }
            }

            //const string startupPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";

            //RegistryManager.SetKeyValue(startupPath,keyName,progranPath,RegistryValueKind.String)
        }

        #region "关机所需的api函数"

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int DoFlag, int rea);

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;

        #endregion

        private static void DoExitWin(int DoFlag)
        {
            UpgradeProcessPrivilege();

            bool ok;
            //TokPriv1Luid tp;
            //IntPtr hproc = GetCurrentProcess();
            //IntPtr htok = IntPtr.Zero;
            //ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            //tp.Count = 1;
            //tp.Luid = 0;
            //tp.Attr = SE_PRIVILEGE_ENABLED;
            //ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            //ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = ExitWindowsEx(DoFlag, 0);


        }

        /// <summary>
        ///     提升当前进程的权限
        /// </summary>
        public static void UpgradeProcessPrivilege()
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        ///     重启计算机
        /// </summary>
        public static void Reboot()
        {
            DoExitWin(EWX_FORCE | EWX_REBOOT);
        }

        /// <summary>
        ///     关机
        /// </summary>
        public static void PowerOff()
        {
            DoExitWin(EWX_FORCE | EWX_POWEROFF);
        }

        /// <summary>
        ///     注销
        /// </summary>
        public static void LogOff()
        {
            DoExitWin(EWX_FORCE | EWX_LOGOFF);
        }



        /// <summary>
        ///     清空IE 的Cookie缓存
        /// </summary>
        public static void ClearCookie()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);

            var cookieFiles = Directory.GetFiles(path);

            foreach (var cookieFile in cookieFiles)
            {
                try
                {
                    File.Delete(cookieFile);
                }
                catch (Exception)
                {

                }

            }
        }

        public static void InstallService(string serviceName,string serviceInstallPath)
        {
            //var strServiceName = "KugarDataSyncClient";
            //var strServiceInstallPath = System.Environment.CommandLine;

            IDictionary mySavedState = new Hashtable();  
  
            try  
            {  
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);  
  
                //服务已经存在则卸载  
                if (ServiceIsExisted(serviceName))  
                {  
                    //StopService(strServiceName);  
                    UnInstallService(serviceName, serviceInstallPath);  
                }  
                service.Refresh();  
                //注册服务  
                AssemblyInstaller myAssemblyInstaller = new AssemblyInstaller();  
  
                mySavedState.Clear();  
                myAssemblyInstaller.Path = serviceInstallPath;  
                myAssemblyInstaller.UseNewContext = true;  
                myAssemblyInstaller.Install(mySavedState);  
                myAssemblyInstaller.Commit(mySavedState);  
                myAssemblyInstaller.Dispose();  
  
                service.Start();  
            }  
            catch (Exception ex)  
            {  
                throw new Exception("注册服务时出错：" + ex.Message);  
            }  
        }

        /// <summary>  
        /// 卸载服务  
        /// </summary>  
        /// <param name="strServiceName">服务名称</param>  
        /// <param name="strServiceInstallPath">服务安装程序完整路径（.exe程序完整路径）</param>  
        public static void UnInstallService(string strServiceName, string strServiceInstallPath)  
        {  
            try  
            {  
                if (ServiceIsExisted(strServiceName))  
                {  
                    //UnInstall Service  
                    AssemblyInstaller myAssemblyInstaller = new AssemblyInstaller();  
                    myAssemblyInstaller.UseNewContext = true;  
                    myAssemblyInstaller.Path = strServiceInstallPath;  
                    myAssemblyInstaller.Uninstall(null);  
                    myAssemblyInstaller.Dispose();  
                }  
            }  
            catch (Exception ex)  
            {  
                throw new Exception("卸载服务时出错：" + ex.Message);  
            }  
        }  
  
        /// <summary>  
        /// 判断服务是否存在  
        /// </summary>  
        /// <param name="serviceName">服务名</param>  
        /// <returns></returns>  
        public static bool ServiceIsExisted(string serviceName)
        {  
            ServiceController[] services = ServiceController.GetServices();  
            foreach (ServiceController s in services)  
            {  
                if (s.ServiceName == serviceName)  
                {  
                    return true;  
                }  
            }  
            return false;  
        }  

        /// <summary>
        /// 启动一个指定名称的服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        public static void StartService(string serviceName)
        {

            if (ServiceIsExisted(serviceName))

            {

                ServiceController service = new ServiceController(serviceName);

                if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending)

                {

                    service.Start();

                    for (int i = 0; i < 60; i++)

                    {

                        service.Refresh();

                        Thread.Sleep(1000);

                        if (service.Status == ServiceControllerStatus.Running)

                        {

                            break;

                        }

                        if (i == 59)

                        {

                            throw new Exception("服务启动失败");

                        }

                    }

                }

            }

        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviseName"></param>
        /// <returns></returns>
        public static bool ServiceStop(string serviseName)
        {
            try
            {
                ServiceController service = new ServiceController(serviseName);
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    return true;
                }
                else
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(1000 * 10);
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
            }
            catch
            {

                return false;
            }
            return true;
        }

        /// <summary>
        /// 修改服务的启动项 2为自动,3为手动
        /// </summary>
        /// <param name="startType"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool ChangeServiceStartType(int startType, string serviceName)
        {
            try
            {
                RegistryKey regist = Registry.LocalMachine;
                RegistryKey sysReg = regist.OpenSubKey("SYSTEM");
                RegistryKey currentControlSet = sysReg.OpenSubKey("CurrentControlSet");
                RegistryKey services = currentControlSet.OpenSubKey("Services");
                RegistryKey servicesName = services.OpenSubKey(serviceName, true);
                servicesName.SetValue("Start", startType);
            }
            catch (Exception ex)
            {

                return false;
            }
            return true;


        }

        /// <summary>
        /// 获取服务启动类型 2为自动 3为手动 4 为禁用
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static string GetServiceStartType(string serviceName)
        {

            try
            {
                RegistryKey regist = Registry.LocalMachine;
                RegistryKey sysReg = regist.OpenSubKey("SYSTEM");
                RegistryKey currentControlSet = sysReg.OpenSubKey("CurrentControlSet");
                RegistryKey services = currentControlSet.OpenSubKey("Services");
                RegistryKey servicesName = services.OpenSubKey(serviceName, true);
                return servicesName.GetValue("Start").ToString();
            }
            catch (Exception ex)
            {

                return string.Empty;
            }

        }

        /// <summary>
        /// 验证服务是否启动
        /// </summary>
        /// <returns></returns>
        public static bool ServiceIsRunning(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region "注册系统热键"

        private static int id = 0;
        /// <summary>
        ///     注册系统热键
        /// </summary>
        /// <param name="keyModifiers">将要注册的功能键</param>
        /// <param name="key">将要注册的字母键</param>
        /// <returns>返回是否注册成功</returns>
        public static bool RegisterSystemHotKey(KeyModifiers keyModifiers,Keys key)
        {
            var oldKey = new HotKeyEventArgs(keyModifiers, key);

            if (hotKeyHandle.ContainsValue(oldKey))
            {
                return true;
            }

            var newID = Interlocked.Increment(ref id);

            try
            {
                return Api.RegisterHotKey(frmForSystemHotKey.Value.Handle, newID, keyModifiers, key);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     注册系统热键
        /// </summary>
        /// <param name="keyModifiers">将要注销的功能键</param>
        /// <param name="key">将要注销的字母键</param>
        /// <returns>返回是否注销成功</returns>
        public static bool UnRegisterSystemHotKey(KeyModifiers keyModifiers, Keys key)
        {
            var t = new HotKeyEventArgs(keyModifiers, key);

            if(!hotKeyHandle.ContainsValue(t))
            {
                return false;
            }

            try
            {
                var oldID=hotKeyHandle.First((s) => s.Value == t).Key;

                Api.UnregisterHotKey(frmForSystemHotKey.Value.Handle, oldID);

                hotKeyHandle.Remove(oldID);

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private static Dictionary<int, HotKeyEventArgs> hotKeyHandle = new Dictionary<int, HotKeyEventArgs>();

        private static Lazy<FrmForHotKey> frmForSystemHotKey = new Lazy<FrmForHotKey>(() => new FrmForHotKey());

        public static event EventHandler<HotKeyEventArgs> HotKeyPress;

        public class HotKeyEventArgs:EventArgs, IEquatable<HotKeyEventArgs>
        {
            public HotKeyEventArgs(KeyModifiers modifyKey, Keys key)
            {
                ModifyKey = modifyKey;
                Key = key;
            }

            public KeyModifiers ModifyKey { get; private set; }
            public Keys Key { get; private set; }

            public bool Equals(HotKeyEventArgs other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.ModifyKey, ModifyKey) && Equals(other.Key, Key);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (HotKeyEventArgs)) return false;
                return Equals((HotKeyEventArgs) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (ModifyKey.GetHashCode()*397) ^ Key.GetHashCode();
                }
            }

            public static bool operator ==(HotKeyEventArgs left, HotKeyEventArgs right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(HotKeyEventArgs left, HotKeyEventArgs right)
            {
                return !Equals(left, right);
            }
        }

        private class FrmForHotKey:Form
        {
             protected override void WndProc(ref Message m)
            {
                const int WM_HOTKEY = 0x0312;
                 var hotKeyID = m.WParam.ToInt32();

                //按快捷键 
                switch (m.Msg)
                {
                    case WM_HOTKEY:
                        var handle = hotKeyHandle.TryGetValue(hotKeyID);
                        Core.Events.EventHelper.Raise(HotKeyPress,this, handle);
                    break;
                }
                base.WndProc(ref m);
            }
        }

        #endregion

        private static WinVer _osVersion;

        public static WinVer OSVersion
        {
            get { return _osVersion; }
        }

        private static WinVer getSystemVersion()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            // Determine the platform.
            switch (osInfo.Platform)
            {
                    // Platform is Windows 95, Windows 98, 
                    // Windows 98 Second Edition, or Windows Me.
                case PlatformID.Win32Windows:
                    switch (osInfo.Version.Minor)
                    {
                        case 10:
                            //if (osInfo.Version.Revision.ToString() == "2222A")
                            //    Console.WriteLine("Windows 98 Second Edition");
                            //else
                            //    Console.WriteLine("Windows 98");
                            return WinVer.Windows98;
                        case 90:
                            return WinVer.WindowsMe;
                    }
                    break;
                    // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                    // or Windows XP.
                case PlatformID.Win32NT:
                    switch (osInfo.Version.Major)
                    {
                        case 3:
                            return WinVer.WindowsNT351;
                        case 4:
                            return WinVer.WindowsNT40;
                        case 5:
                            switch (osInfo.Version.Minor)
                            {
                                case 0:
                                    return WinVer.Windows2000;
                                case 1:
                                    return WinVer.WindowsXP;
                                case 2:
                                    return WinVer.WindowsServer2003;
                            }
                            break;
                        case 6:
                            switch (osInfo.Version.Minor)
                            {
                                case 0:
                                    return WinVer.WindowsVista;
                                case 1:
                                    return WinVer.Windows7;
                                case 2:
                                    return WinVer.Windows2008;
                            }
                            break;
                    }
                    break;
            }

            return WinVer.None;

        }


    }

    public enum WinVer
    {
        Windows98=0,
        
        WindowsMe=1,

        /// <summary>
        ///     Windows NT 3.51
        /// </summary>
        WindowsNT351,

        /// <summary>
        ///     Windows NT 4.0
        /// </summary>
        WindowsNT40,


        Windows2000=0x0500,

        WindowsXP = 0x0501,
        WindowsServer2003 = 0x0502,
        WindowsVista=0x0600 ,
        Windows7=0x0601,
        Windows2008=0x602,
        None=0
    }

    



}
