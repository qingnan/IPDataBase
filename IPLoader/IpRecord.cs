using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IPLoader
{
    /// <summary>
    /// IP信息记录
    /// </summary>
    [Serializable]
    public class IpRecord
    {
        public IpRecord()
        {

        }

        /// <summary>
        /// 起始IP
        /// </summary>
        public string GetStartIp()
        {
            return Utils.LongToIP(this.StartIpNumber);
        }

        /// <summary>
        /// 结束IP
        /// </summary>
        public string GetEndIp()
        {
            return Utils.LongToIP(this.EndIpNumber);
        }

        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string Local { get; set; }

        /// <summary>
        /// 起始IP转换为long型后的值
        /// </summary>
        public long StartIpNumber { get; set; }

        /// <summary>
        /// 结束IP转换为long型后的值
        /// </summary>
        public long EndIpNumber { get; set; }

    }

    
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 将IP转为long类型
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <returns></returns>
        public static long IpToLong(string ip)
        {
            try
            {
                char[] dot = new char[] { '.' };
                string[] ipArr = ip.Split(dot);
                if (ipArr.Length == 3)
                    ip = ip + ".0";

                ipArr = ip.Split(dot);
                if (ipArr.Length != 4)
                    return -1;

                long longIP = 0;
                long p1 = long.Parse(ipArr[0]) * 256 * 256 * 256;
                long p2 = long.Parse(ipArr[1]) * 256 * 256;
                long p3 = long.Parse(ipArr[2]) * 256;
                long p4 = long.Parse(ipArr[3]);
                longIP = p1 + p2 + p3 + p4;
                return longIP;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 将long类型转为ip
        /// </summary>
        /// <param name="longIp"></param>
        /// <returns></returns>
        public static string LongToIP(long longIp)
        {
            long seg1 = (longIp & 0xff000000) >> 24;
            if (seg1 < 0)
                seg1 += 0x100;
            long seg2 = (longIp & 0x00ff0000) >> 16;
            if (seg2 < 0)
                seg2 += 0x100;
            long seg3 = (longIp & 0x0000ff00) >> 8;
            if (seg3 < 0)
                seg3 += 0x100;
            long seg4 = (longIp & 0x000000ff);
            if (seg4 < 0)
                seg4 += 0x100;
            string ip = string.Concat(seg1.ToString(), ".", seg2.ToString(), ".", seg3.ToString(), ".", seg4.ToString());
            return ip;
        }

        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIpV4(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
    }
}
