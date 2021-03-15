using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace Gleee.Net
{
    public static class NetExtension
    {
        public static string GetInfoString(this PingReply reply)
        {
            string re = $"";
            re += $"状态：{reply.Status}";
            if (reply.Status == IPStatus.Success)
                re += $"\t延迟：{reply.RoundtripTime}ms";
            return re;
        }
        public static string GetInfoString_Detailed(this PingReply reply)
        {
            string re = $"";
            re += $"状态：{reply.GetStatusErrorString()}";
            if (reply.Status == IPStatus.Success)
                re += $"\t延迟：{reply.RoundtripTime}ms";
            return re;
        }
        public static string GetStatusErrorString(this PingReply reply)
        {
            string status;
            if (reply.Status == IPStatus.BadDestination) status = "不正确的目标地址";
            else if (reply.Status == IPStatus.BadHeader) status = "不正确的报头";
            else if (reply.Status == IPStatus.BadOption) status = "不正确的配置";
            else if (reply.Status == IPStatus.BadRoute) status = "源主机和目标主机之间没有有效的路径";
            else if (reply.Status == IPStatus.DestinationHostUnreachable) status = "无法到达目标主机";
            else if (reply.Status == IPStatus.DestinationNetworkUnreachable) status = "无法到达目标主机所在的目标网络";
            else if (reply.Status == IPStatus.DestinationPortUnreachable) status = "无法到达目标端口";
            else if (reply.Status == IPStatus.DestinationProhibited) status = "与目标主机之间的连接被禁止";
            else if (reply.Status == IPStatus.DestinationProtocolUnreachable) status = "由于协议不支持，无法到达目标主机";
            else if (reply.Status == IPStatus.DestinationScopeMismatch) status = "源主机地址和目标主机地址处于不用的作用域中";
            else if (reply.Status == IPStatus.DestinationUnreachable) status = "无法到达目标主机，而且原因未知";
            else if (reply.Status == IPStatus.HardwareError) status = "硬件错误";
            else if (reply.Status == IPStatus.IcmpError) status = "ICMP协议错误";
            else if (reply.Status == IPStatus.NoResources) status = "无效的网络资源";
            else if (reply.Status == IPStatus.PacketTooBig) status = "数据包过大";
            else if (reply.Status == IPStatus.ParameterProblem) status = "数据包参数错误";
            else if (reply.Status == IPStatus.SourceQuench) status = "数据包被取消";
            else if (reply.Status == IPStatus.Success) status = "成功";
            else if (reply.Status == IPStatus.TimedOut) status = "超时";
            else if (reply.Status == IPStatus.TimeExceeded) status = "TTL超时";
            else if (reply.Status == IPStatus.TtlExpired) status = "TTL超时";
            else if (reply.Status == IPStatus.TtlReassemblyTimeExceeded) status = "TTL重组超时";
            else if (reply.Status == IPStatus.Unknown) status = "由于未知原因失败";
            else if (reply.Status == IPStatus.UnrecognizedNextHeader) status = "'NextHeader'项的值未知";
            else status = "由于未知原因失败";
            return status;
        }
        public static string GetInfoString(this IPInterfaceProperties adapterProperties)
        {
            string re = "";
            IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
            if (dnsServers != null)
            {
                re += "DNS服务器：";
                foreach (IPAddress dns in dnsServers)
                {
                    re += $"{dns}\n";
                }
                re += "\n";
            }
            IPAddressInformationCollection anyCast = adapterProperties.AnycastAddresses;
            if (anyCast != null)
            {
                re += "任播地址：";
                foreach (IPAddressInformation any in anyCast)
                {
                    re += $"{any.Address}\n";
                }
                re += "\n";
            }
            MulticastIPAddressInformationCollection multiCast = adapterProperties.MulticastAddresses;
            if (multiCast != null)
            {
                re += "组播地址：";
                foreach (IPAddressInformation multi in multiCast)
                {
                    re += $"{multi.Address}\n";
                }
                re += "\n";
            }
            UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
            if (uniCast != null)
            {
                string lifeTimeFormat = "dddd, MMMM dd, yyyy  hh:mm:ss tt";
                foreach (UnicastIPAddressInformation uni in uniCast)
                {
                    DateTime time;
                    re += "单播地址：";
                    re += $"{uni.Address}\n";
                    re += $"\t前缀来源：\t{uni.PrefixOrigin}\n";
                    re += $"\t后缀来源：\t{uni.SuffixOrigin}\n";
                    re += $"\t重复地址检测：\t{uni.DuplicateAddressDetectionState}\n";
                    time = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressValidLifetime);
                    time = time.ToLocalTime();
                    re += $"\t\t有效期至：\t{time.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture)}\n";
                    time = DateTime.UtcNow + TimeSpan.FromSeconds(uni.DhcpLeaseLifetime);
                    time = time.ToLocalTime();
                    re += $"\t\tDHCP释放时间：\t{time.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture)}\n";
                }
            }
            return re;
        }
    }

    public class Heartbeat
    {
        private Thread loop;

        public HeartbeatType HeartbeatType { get; private set; }
        public string IP { get; private set; } = "127.0.0.1";
        public int TimeOut { get; set; } = 2000;
        public int SleepTime { get; set; } = 1;
        public bool HasStarted { get; private set; } = false;
        public bool IsAlive { get => loop.IsAlive; }
        public long TotalSentCount { get; private set; } = 0;
        public long TotalLossCount { get; private set; } = 0;
        public double Loss { get => TotalLossCount * 100.0 / TotalSentCount; }

        public Heartbeat() 
        {
            InitializeLoop();
        }
        public Heartbeat(string ip)
        {
            IP = ip;
            InitializeLoop();
        }
        public Heartbeat(string ip, int sleep_time)
        {
            IP = ip;
            SleepTime = sleep_time;
            InitializeLoop();
        }
        public Heartbeat(string ip, int sleep_time, int timeout)
        {
            IP = ip;
            SleepTime = sleep_time;
            TimeOut = timeout;
            InitializeLoop();
        }


        private void InitializeLoop()
        {
            if (HeartbeatType == HeartbeatType.Ping)
            {
                loop = new Thread(() =>
                {
                    using (Ping ping = new Ping())
                    {
                        PingOptions opt = new PingOptions();
                        opt.DontFragment = true;
                        byte[] buffer = new byte[] { 0xde, 0xad, 0xbe, 0xef };
                        while (true)
                        {
                            if (ping.Send(IP, TimeOut, buffer, opt).Status != IPStatus.Success) TotalLossCount++;
                            TotalSentCount++;
                            Thread.Sleep(SleepTime);
                        }
                    }
                });
            }
            else throw new NotImplementedException();
        }
        public void Start()
        {
            HasStarted = true;
            loop.Start();
        }
        public void Stop()
        {
            if (IsAlive) loop.Abort();
        }
    }
    public enum HeartbeatType
    {
        Ping
    }

}
