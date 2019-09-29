using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace IPLoader
{
    public partial class Form1 : Form
    {
        DateTime time = DateTime.Now;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
            this.textBox1.Text = this.openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            time = DateTime.Now;

            QQwryHandler handler = new QQwryHandler();

            var list = handler.LoadIpRecords(this.textBox1.Text);

            Regex regx = new Regex(@"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)", RegexOptions.Compiled);

            string ipfilePathOut = textBox1.Text.Replace(".dat", ".csv");
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                var ip1 = item.StartIpNumber;
                var ip2 = item.EndIpNumber;
                string country = item.Country == null ? string.Empty : item.Country.Trim();
                string area = item.Local == null ? string.Empty : item.Local.Trim();
                string city = string.Empty;

                if (country.IndexOf("CZ88", StringComparison.CurrentCultureIgnoreCase) >= 0
                    || country.IndexOf("纯真") >= 0)
                {
                    country = string.Empty;
                    area = string.Empty;
                }
                if (area.IndexOf("CZ88", StringComparison.CurrentCultureIgnoreCase) >= 0
                    || area.IndexOf("纯真") >= 0)
                {
                    area = string.Empty;
                }
                if (string.IsNullOrEmpty(country))
                {
                    if (!string.IsNullOrEmpty(country) && country != "中国" && country == area)
                    {
                        continue;
                    }
                    if (country != area && area.Length > 4)
                    {
                        continue;
                    }
                    country = SearchIp(Utils.LongToIP(ip1)).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                }

                //if (country.Contains("大学"))
                //{
                //    area = country.TrimStart('中', '国') + area;
                //    country = "中国";
                //}
                //else
                //    country = ProcessChinaLocal(country);

                var isChina = handler.IsMainlandChina(country);
                if (isChina)
                {
                    city = country.Replace("中国", "");
                    country = "中国";
                }
                else if (country.Contains("台湾"))
                {
                    city = country;
                    country = "中国台湾";
                }
                else if (country.Contains("香港"))
                {
                    city = country;
                    country = "中国香港";
                }

                if (country.Contains("IANA"))
                {
                    country = "IANA";
                }
                string content = string.Format("{0},{1},{2},{3},{4}\r\n", ip1, ip2, country, city, area);
                sb.Append(content);
            }
            File.WriteAllText(ipfilePathOut, sb.ToString(), Encoding.UTF8);

        }

        static string[] data = new string[] { "北京", "天津", "河北", "山西", "内蒙古", "内蒙", "辽宁", "吉林", "黑龙江", "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东", "河南", "湖北", "湖南", "广东", "广西", "海南", "重庆", "四川", "贵州", "云南", "西藏", "陕西", "甘肃", "青海", "宁夏", "新疆" };

        string ProcessChinaLocal(string country)
        {
            if (string.IsNullOrEmpty(country) || country.Length < 2 || country == "中国")
            {
                return country;
            }

            var flag = 0;
            country = country.TrimStart('中', '国');
            foreach (var item in data)
            {
                flag = country.IndexOf(item);
                if (flag >= 0)
                {
                    var sta = flag + item.Length;
                    var shn = sta < country.Length ? country.Substring(sta, 1) : string.Empty;
                    string sheng = item;
                    if (shn == "省" || shn == "市")
                    {
                        sheng = country.Substring(flag, item.Length + 1);
                        flag = item.Length + 1;
                    }
                    else
                    {
                        flag = item.Length;
                    }
                    var shi = country.Substring(flag, country.Length - flag);
                    var all = "中国-" + item + "-" + shi.Replace("市", "市-").Replace("县", "县-");
                    return all.TrimEnd('-');
                }
                flag = 0;
            }
            return country;
        }



        string SearchIp(string ipString)
        {
            string url = "https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?query=" + ipString.Trim() + "&co=&resource_id=6006&t=" + time.ToFileTimeUtc() + "&ie=utf8&oe=gbk&format=json&tn=baidu";
            WebClient wc = new WebClient();
            string s = wc.DownloadString(url);
            //var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(s);
            string ids = "\"location\":\"";
            int i = s.IndexOf(ids);
            int j = s.IndexOf("\",", i + ids.Length);
            string sd = s.Substring(i + ids.Length, (j - i - ids.Length));
            //textBox2.AppendText(ipString + sd + "\r\n");
            //Application.DoEvents();
            return sd;
        }
    }
}
