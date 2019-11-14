using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IPLoader
{
    /// <summary>
    /// QQwry数据库处理器
    /// </summary>
    public class QQwryHandler
    {
        public static string[] mainlandChinaFilter = new string[] { "中国", "北京", "天津", "河北", "山西", "内蒙", "辽宁", "吉林", "黑龙江", "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东", "河南", "湖北", "湖南", "广东", "广西", "海南", "重庆", "四川", "贵州", "云南", "西藏", "陕西", "甘肃", "青海", "宁夏", "新疆", "大学" };

        #region 私有成员
        private string country;
        private string local;
        private FileStream objfs = null;
        private long startIp = 0;
        private long endIp = 0;
        private int countryFlag = 0;
        private long endIpOff = 0;

        #endregion

        #region 构造函数
        public QQwryHandler()
        {

        }
        #endregion


        /// <summary>
        /// 从QQwry加载数据
        /// </summary>
        /// <param name="filePath">QQwry数据路径</param>
        /// <returns></returns>
        public List<IpRecord> LoadIpRecords(string filePath)
        {
            try
            {
                List<IpRecord> ipRecords = new List<IpRecord>();
                objfs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                objfs.Position = 0;
                byte[] buffer1 = new Byte[8];
                objfs.Read(buffer1, 0, 8);

                int firstStartIp = buffer1[0] + buffer1[1] * 256 + buffer1[2] * 256 * 256 + buffer1[3] * 256 * 256 * 256;
                int lastStartIp = buffer1[4] * 1 + buffer1[5] * 256 + buffer1[6] * 256 * 256 + buffer1[7] * 256 * 256 * 256;
                long recordCount = Convert.ToInt64((lastStartIp - firstStartIp) / 7.0);
                if (recordCount <= 1)
                {
                    country = "文件有误！";
                    objfs.Close();
                }
                long rangE = recordCount;

                for (int i = 0; i <= recordCount; i++)
                {
                    long offSet = firstStartIp + i * 7;
                    objfs.Position = offSet;

                    byte[] buffer2 = new Byte[7];
                    objfs.Read(buffer2, 0, 7);

                    endIpOff = Convert.ToInt64(buffer2[4].ToString()) + Convert.ToInt64(buffer2[5].ToString()) * 256 + Convert.ToInt64(buffer2[6].ToString()) * 256 * 256;
                    startIp = Convert.ToInt64(buffer2[0].ToString()) + Convert.ToInt64(buffer2[1].ToString()) * 256 + Convert.ToInt64(buffer2[2].ToString()) * 256 * 256 + Convert.ToInt64(buffer2[3].ToString()) * 256 * 256 * 256;

                    objfs.Position = endIpOff;
                    byte[] buffer3 = new Byte[5];
                    objfs.Read(buffer3, 0, 5);
                    this.endIp = Convert.ToInt64(buffer3[0].ToString()) + Convert.ToInt64(buffer3[1].ToString()) * 256 + Convert.ToInt64(buffer3[2].ToString()) * 256 * 256 + Convert.ToInt64(buffer3[3].ToString()) * 256 * 256 * 256;
                    this.countryFlag = buffer3[4];

                    this.GetCountry();

                    IpRecord ipRecord = new IpRecord();

                    ipRecord.StartIpNumber = startIp;

                    ipRecord.EndIpNumber = endIp;

                    if (this.country.IndexOf("CZ88", StringComparison.CurrentCultureIgnoreCase) >= 0
                   || this.country.IndexOf("纯真") >= 0)
                    {
                        this.country = string.Empty;
                        this.local = string.Empty;
                    }
                    else if (this.country.Contains("IANA"))
                    {
                        this.country = "IANA";
                    }

                    if (IsMainlandChina(this.country) && !string.IsNullOrEmpty(this.country) && !this.country.StartsWith("中国"))
                    {
                        ipRecord.Country = string.Concat("中国", this.country);
                    }
                    else
                    {
                        ipRecord.Country = this.country;
                    }

                    if (this.local.IndexOf("CZ88", StringComparison.CurrentCultureIgnoreCase) >= 0
                        || this.local.IndexOf("纯真") >= 0)
                    {
                        this.local = string.Empty;
                    }

                    ipRecord.Local = this.local.Trim('/');
                    ipRecords.Add(ipRecord);

                }
                ipRecords = ipRecords.OrderBy(m => m.StartIpNumber).ToList();

                //记录合并操作
                int idx = ipRecords.Count - 1;
                int end = 1;
                int itr = 0;

            lbb:
                itr++;
                idx = ipRecords.Count - 1;
                do
                {
                    idx--;
                    var ips = ipRecords[idx];
                    var pre = ipRecords[idx + 1];
                    var next = ipRecords[idx - 1];
                    if (ips.Country == pre.Country && ips.Local == pre.Local)
                    {
                        ips.EndIpNumber = pre.EndIpNumber;
                        ipRecords.RemoveAt(idx + 1);
                        continue;
                    }
                    if (ips.StartIpNumber == ips.EndIpNumber)
                    {
                        if (pre.Country == next.Country)
                        {
                            next.EndIpNumber = pre.EndIpNumber;
                            ipRecords.RemoveAt(idx + 1);
                            ipRecords.RemoveAt(idx);
                        }
                    }

                } while (idx > end);
                if (itr <= 1)
                {
                    goto lbb;
                }

                return ipRecords;
            }
            catch (Exception ex)
            {
                // ServiceHub.AddLog(RuntimeLogType.Exception, this.GetType(), (string.Format("加载IP数据时出现异常：{0},{1}", ex.Message, ex.StackTrace)));
                throw;
            }
            finally
            {
                try
                {
                    if (objfs != null)
                    {
                        objfs.Close();
                    }
                }
                catch { }
            }
            return null;
        }


        /// <summary>
        /// 获取国家/区域偏移量
        /// </summary>
        /// <returns></returns>
        private string GetCountry()
        {
            switch (this.countryFlag)
            {
                case 1:
                case 2:
                    this.country = GetInfo(this.endIpOff + 4);
                    this.local = (1 == this.countryFlag) ? " " : this.GetInfo(this.endIpOff + 8);
                    break;
                default:
                    this.country = this.GetInfo(this.endIpOff + 4);
                    this.local = this.GetInfo(objfs.Position);
                    break;
            }
            return " ";
        }


        /// <summary>
        /// 获取国家/区域字符串
        /// </summary>
        /// <param name="offSet"></param>
        /// <returns></returns>
        private string GetInfo(long offSet)
        {
            int flag = 0;
            byte[] buffer = new Byte[3];
            while (1 == 1)
            {
                objfs.Position = offSet;
                flag = objfs.ReadByte();
                if (flag == 1 || flag == 2)
                {
                    objfs.Read(buffer, 0, 3);
                    if (flag == 2)
                    {
                        this.countryFlag = 2;
                        this.endIpOff = offSet - 4;
                    }
                    offSet = Convert.ToInt64(buffer[0].ToString()) + Convert.ToInt64(buffer[1].ToString()) * 256 + Convert.ToInt64(buffer[2].ToString()) * 256 * 256;
                }
                else
                {
                    break;
                }
            }
            if (offSet < 12)
                return " ";
            objfs.Position = offSet;
            return GetInfo();
        }
        private string GetInfo()
        {
            byte lowByte = 0;
            byte upByte = 0;
            StringBuilder sb = new StringBuilder();
            byte[] buffer = new byte[2];
            while (1 == 1)
            {
                lowByte = (byte)objfs.ReadByte();
                if (lowByte == 0)
                    break;
                if (lowByte > 127)
                {
                    upByte = (byte)objfs.ReadByte();
                    buffer[0] = lowByte;
                    buffer[1] = upByte;
                    sb.Append(Encoding.GetEncoding("GB2312").GetString(buffer));
                }
                else
                {
                    sb.Append((char)lowByte);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// 是否为中国大陆地区
        /// </summary>
        public bool IsMainlandChina(string country)
        {
            string[] data = mainlandChinaFilter;
            if (string.IsNullOrEmpty(country))
                return false;
            bool flag = false;
            foreach (var item in data)
            {
                flag = country.Contains(item);
                if (flag == true)
                    return flag;
            }
            return flag;
        }

    }
}
