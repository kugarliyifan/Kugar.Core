using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Network.NetworkNeighborhood
{
    /// <summary>
    ///     局域网的一些操作
    /// </summary>
    public static class LocalNetwork
    {
        public static bool LoginShareFolder(string desPath, string userName = "guest", string password = "")
        {
            if (desPath.IsEmptyOrWhileSpace())
            {
                throw new ArgumentException("指定路径错误", "desPath");
            }

            NetResource   nr; 
            nr.dwDisplayType=ResourceDisplayType.RESOURCEDISPLAYTYPE_GENERIC; 
            nr.dwScope=ResourceEnumerationScope.RESOURCE_GLOBALNET; 
            nr.dwType=ResourceType.RESOURCETYPE_DISK; 
            nr.dwUsage=ResourceUsage.RESOURCEUSAGE_CONNECTABLE; 
            nr.lpComment= " "; 
            nr.lpLocalName= "z: "; 
            nr.lpProvider= " ";
            nr.lpRemoteName = desPath;

            var retCode = CopyFileByNet.WNetAddConnection2(ref nr, password, userName,
                                                           ConnectionOption.CONNECT_INTERACTIVE);

            if (retCode == WNetAddConnection2ReturnCode.NO_ERROR)
            {
                return true;
            } 
            else
            {
                throw new Exception(@"登录错误,错误代码为:" + retCode);
            } 
        }

        public static bool LogoutShareFolder(string shareFolderPath)
        {
            if (CopyFileByNet.WNetCancelConnection2(shareFolderPath, 0, true) == WNetCancelConnection2ReturnCode.NO_ERROR)
            {
                return true;
            }
            else
            {
                return false;
            } 
        }
    }


    internal class CopyFileByNet
    {
        //declare   WindowsNetwork   function 
        //cancel   net   mapped 
        [DllImport("mpr.dll ")]
        public static extern WNetCancelConnection2ReturnCode WNetCancelConnection2(
        string strResourceName,
        ConnectionType connectionType,
        bool bForce
        );
        //builde   net   mapped 
        [DllImport("mpr.dll ")]
        public static extern WNetAddConnection2ReturnCode WNetAddConnection2(
        ref   NetResource netResource,
        string strPassword,
        string strUserName,
        ConnectionOption dwFlags
        );
    }

    //ConnectionType 
    public enum ConnectionType
    {
        CONNECT_DONT_UPDATE_PROFILE = 0,
        CONNECT_UPDATE_PROFILE = 0x00000001,
        CONNECT_UPDATE_RECENT = 0x00000002,
        CONNECT_TEMPORARY = 0x00000004,
        CONNECT_INTERACTIVE = 0x00000008,
        CONNECT_PROMPT = 0x00000010,
        CONNECT_NEED_DRIVE = 0x00000020,
        CONNECT_REFCOUNT = 0x00000040,
        CONNECT_REDIRECT = 0x00000080,
        CONNECT_LOCALDRIVE = 0x00000100,
        CONNECT_CURRENT_MEDIA = 0x00000200,
        CONNECT_DEFERRED = 0x00000400,
        CONNECT_RESERVED = unchecked((int)0xFF000000)
    }

    //WNetCancelConnection2 
    public enum WNetCancelConnection2ReturnCode
    {
        NO_ERROR = 0,
        ERROR_BAD_PROFILE = 1206,
        ERROR_CANNOT_OPEN_PROFILE = 1205,
        ERROR_DEVICE_IN_USE = 2404,
        ERROR_EXTENDED_ERROR = 1208,
        ERROR_NOT_CONNECTED = 2250,
        ERROR_OPEN_FILES = 2401
    }

    //WNetAddConnection2 
    public enum WNetAddConnection2ReturnCode
    {
        NO_ERROR = 0,
        ERROR_ACCESS_DENIED = 5,
        ERROR_ALREADY_ASSIGNED = 85,
        ERROR_BAD_DEV_TYPE = 66,
        ERROR_BAD_DEVICE = 1200,
        ERROR_BAD_NET_NAME = 67,
        ERROR_BAD_PROFILE = 1206,
        ERROR_BAD_PROVIDER = 1204,
        ERROR_BUSY = 170,
        ERROR_CANCELLED = 1223,
        ERROR_CANNOT_OPEN_PROFILE = 1205,
        ERROR_DEVICE_ALREADY_REMEMBERED = 1202,
        ERROR_EXTENDED_ERROR = 1208,
        ERROR_INVALID_PASSWORD = 86,
        ERROR_NO_NET_OR_BAD_PATH = 1203,
        ERROR_NO_NETWORK = 1222
    }
    //NETRESOURCE   struc： 
    public struct NetResource
    {
        public ResourceEnumerationScope dwScope;
        public ResourceType dwType;
        public ResourceDisplayType dwDisplayType;
        public ResourceUsage dwUsage;
        public string lpLocalName;
        public string lpRemoteName;
        public string lpComment;
        public string lpProvider;
    }

    //ResourceType   enum：   
    public enum ResourceType
    {
        RESOURCETYPE_ANY = 0x00000000,
        RESOURCETYPE_DISK = 0x00000001,
        RESOURCETYPE_PRINT = 0x00000002
    }

    //ResourceDisplayType   enum：   
    public enum ResourceDisplayType
    {
        RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001, 
        RESOURCEDISPLAYTYPE_SERVER = 0x00000002, 
        RESOURCEDISPLAYTYPE_SHARE = 0x00000003, 
        RESOURCEDISPLAYTYPE_GENERIC = 0x00000000
    }

    //ResourceUsage   enum：   
    public enum ResourceUsage
    {
        RESOURCEUSAGE_CONNECTABLE = 0x00000001,
        RESOURCEUSAGE_CONTAINER = 0x00000002
    }

    //ConnectionOption   enum：   
    public enum ConnectionOption
    {
        CONNECT_INTERACTIVE = 0x00000008,
        CONNECT_PROMPT = 0x00000010,
        CONNECT_REDIRECT = 0x00000080,
        CONNECT_UPDATE_PROFILE = 0x00000001,
        CONNECT_COMMANDLINE = 0x00000800,
        CONNECT_CMD_SAVECRED = 0x00001000
    }

    public enum ResourceEnumerationScope
    {
        RESOURCE_CONNECTED = 0x00000001, /*Enumerate currently connected resources. The dwUsage member cannot be specified. */
        RESOURCE_GLOBALNET = 0x00000002, /*Enumerate all resources on the network. The dwUsage member is specified. */
        RESOURCE_REMEMBERED = 0x00000003 /*Enumerate remembered (persistent) connections. The dwUsage member cannot be specified. */
    }  
}
