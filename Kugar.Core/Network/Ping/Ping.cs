using System;
using System.Net.NetworkInformation;
using System.Text;

namespace Kugar.Core.Network.Ping
{
    /// <summary>
    ///     目的地址链接测试
    /// </summary>
    public static class Ping
    {
        private static readonly byte[] testSendData;

        //const string testSendData = Encoding.ASCII.GetBytes(@"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        static Ping()
        {
            testSendData=Encoding.ASCII.GetBytes(@"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        }

        /// <summary>
        ///     ping一个指定的地址,并返回是否ping成功
        /// </summary>
        /// <param name="Address">远程地址</param>
        /// <param name="TimeOut">超时时间</param>
        /// <returns></returns>
        public static bool PingHost(string Address, int TimeOut = 1000)
        {
            using (System.Net.NetworkInformation.Ping PingSender = new System.Net.NetworkInformation.Ping())
            {
                PingOptions Options = new PingOptions();
                Options.DontFragment = true;


                PingReply Reply = PingSender.Send(Address, TimeOut, testSendData, Options);
                if (Reply.Status == IPStatus.Success)
                    return true;
                return false;
            }
        }

        public static void BeginPingHost(string hostNameOrAddress, EventHandler<PingCallBackEventArgs> pingCallBack)
        {
            BeginPingHost(hostNameOrAddress, 1000, pingCallBack,null);
        }

        public static void BeginPingHost(string hostNameOrAddress, int TimeOut, EventHandler<PingCallBackEventArgs> pingCallBack,object userState)
        {
            var PingSender = new System.Net.NetworkInformation.Ping();

            PingOptions Options = new PingOptions();

            Options.DontFragment = true;

            PingSender.PingCompleted += PingSender_PingCompleted;

            var obj = new PrivatePingUserState() { PingCallBack = pingCallBack, UserState = userState };

            PingSender.SendAsync(hostNameOrAddress, TimeOut, testSendData, obj);

        }

        static void PingSender_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (e.UserState is PrivatePingUserState)
            {
                var temp = (PrivatePingUserState) e.UserState;

                if (temp.PingCallBack!=null)
                {
                    var e1 = new PingCallBackEventArgs
                                 {
                                     IsSuccess = e.Reply.Status == IPStatus.Success,
                                     HasError = e.Error != null,
                                     Error = e.Error,
                                     UserState = temp.UserState
                                 };

                    temp.PingCallBack(sender, e1);
                }
                
            }

            if (sender is IDisposable)
            {
                ((IDisposable)sender).Dispose();
            }
        }

        private class PrivatePingUserState
        {
            public EventHandler<PingCallBackEventArgs> PingCallBack { set; get; }

            public object UserState { set; get; }
        }

        public class PingCallBackEventArgs : EventArgs
        {
            public bool IsSuccess { set; get; }

            public bool HasError { set; get; }

            public Exception Error { set; get; }

            public object UserState { set; get; }
        }

    }


}
