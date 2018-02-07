using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Data;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Cache;

namespace Qishi_Tester
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class HTTPTester : Window
    {
        public BackgroundWorker bw, bw1;
        public DispatcherTimer timer;
        DataTable dt;
        public string _type = "", _host = "", _server = "", _cookies = "", _head = "", _username = "", _password = "", ip = null;
        public int[] value1;
        public int[] value2;
        public double everVlaue;
        public int num = 10, maxValue = 0, minValue = 0;
        public int state = 0;//记录异步进程的状态

        public HTTPTester()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Close();
        }

        private void minBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (bw.CancellationPending)
                return;
            else
            {
                if (e.ProgressPercentage == 100)
                {
                    DataTable _dt = new DataTable();
                    _dt = dt.Copy();
                    dt.Clear();
                    if (!string.IsNullOrEmpty(txDomain1.Text.Trim()))
                        txHttpInfo.Text = "网址:\r    " + txDomain1.Text;
                    if (!string.IsNullOrEmpty(_server))
                        txHttpInfo.Text += "\r\r服务器地址:\r    " + _server;
                    if (!string.IsNullOrEmpty(_type))
                        txHttpInfo.Text += "\r\r请求方式:" + _type;
                    if (!string.IsNullOrEmpty(txCookies1.Text.Trim()))
                        txHttpInfo.Text += "\r\rCookies:\r    " + txCookies1.Text;
                    if (!string.IsNullOrEmpty(txUsername1.Text.Trim()))
                        txHttpInfo.Text += "\r\r用户名:" + txUsername1.Text;
                    if (!string.IsNullOrEmpty(txPwd1.Password))
                        txHttpInfo.Text += "\r\r密码:" + txPwd1.Password;
                    ListRecord.DataContext = _dt;
                    btnCheck1.Visibility = Visibility.Visible;
                    _btnCheck1.Visibility = Visibility.Hidden;
                    canvas2.Visibility = Visibility.Visible;
                    timer.Stop();
                    state = 0;
                }
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (bw.CancellationPending)
                return;
            else
            {
                if (_type.IndexOf("Head") != -1)
                {
                    dt.Rows.Add("Head请求", " 请求结果");
                    string header = "", status1 = null;
                    getHtml(_host, _cookies, _username, _password, _server, out header, out status1);
                    header = header.Replace("\r\n", "|");
                    string[] headers = header.Split('|');
                    if (status1.ToLower() == "ok")
                    {
                        dt.Rows.Add("网站状态", " " + status1);
                        for (int i = 0; i < headers.Length; i++)
                        {
                            string[] values = headers[i].Split(':');
                            if (values.Length == 2)
                                dt.Rows.Add(values[0], values[1]);
                        }
                    }
                    else
                    {
                        dt.Rows.Add("网站状态", status1);
                    }
                }
                if (_type.IndexOf("Get") != -1)
                {
                    dt.Rows.Add("Get请求", " 请求结果");
                    string header = "", status2 = null;
                    StringBuilder sb = new StringBuilder();
                    DateTime start_time = DateTime.Now;
                    sb.Append(getHtml(_host, _cookies, _username, _password, _server, out header, out status2));
                    DateTime end_time = DateTime.Now;
                    TimeSpan ts = end_time - start_time;
                    int ch = (int)ts.TotalMilliseconds;//响应时间.(毫秒)
                    string title = getTitle(sb.ToString());
                    string keywords = getKeyWords(sb.ToString());
                    string description = getDescription(sb.ToString());
                    dt.Rows.Add("网站状态", " " + status2);
                    string length = conversionByte(sb.Length);
                    dt.Rows.Add("网站大小", " " + length);
                    dt.Rows.Add("响应时间", " " + ch.ToString() + "ms");
                    if (title != "")
                        dt.Rows.Add("网站标题(Title)", " " + title);
                    if (keywords != "")
                        dt.Rows.Add("网站关键词(KeyWords)", " " + keywords);
                    if (description != "")
                        dt.Rows.Add("网站简介(Description)", " " + description);
                }
                bw.ReportProgress(100);
            }
        }

        private void bw_Cancel(object sender, EventArgs e)
        {
            bw.CancelAsync();
            bw.Dispose();
            timer.Stop();
            if (!string.IsNullOrEmpty(txDomain1.Text.Trim()))
                txHttpInfo.Text = "网址:\r    " + txDomain1.Text;
            if (!string.IsNullOrEmpty(_server))
                txHttpInfo.Text += "\r\r服务器地址:\r    " + _server;
            if (!string.IsNullOrEmpty(_type))
                txHttpInfo.Text += "\r\r请求方式:" + _type;
            if (!string.IsNullOrEmpty(txCookies1.Text.Trim()))
                txHttpInfo.Text += "\r\rCookies:\r    " + txCookies1.Text;
            if (!string.IsNullOrEmpty(txUsername1.Text.Trim()))
                txHttpInfo.Text += "\r\r用户名:" + txUsername1.Text;
            if (!string.IsNullOrEmpty(txPwd1.Password))
                txHttpInfo.Text += "\r\r密码:" + txPwd1.Password;
            DataTable _dt = new DataTable();
            _dt = dt.Clone();
            _dt.Clear();
            _dt.Rows.Add("Head请求", " 请求结果");
            _dt.Rows.Add(_host, "无法解析,通信超时。");
            ListRecord.DataContext = _dt;
            btnCheck1.Visibility = Visibility.Visible;
            _btnCheck1.Visibility = Visibility.Hidden;
            canvas2.Visibility = Visibility.Visible;
            state = 0;
        }

        private void btnCheck1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _host = txDomain1.Text;
                UrlCheck(ref _host);
                if (IsValidHttp(_host))
                {
                    string domain = _host.Replace("http://", "");
                    domain = domain.Replace("https://", "");
                    if (ValidIp(domain))
                    {
                        if (IsValidIp(domain))
                        {
                            ip = domain;
                        }
                        else
                        {
                            MessageBox.Show("IP或域名格式不正确!请重新输入。");
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("IP或域名格式不正确!请重新输入。");
                    return;
                }
                state = 1;
                btnCheck1.Visibility = Visibility.Hidden;
                _btnCheck1.Visibility = Visibility.Visible;
                btnHttpMore1.Visibility = Visibility.Visible;
                btnHttpMore2.Visibility = Visibility.Hidden;
                canvas6.Visibility = Visibility.Hidden;
                _type = "";
                dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("HttpName", typeof(string));
                dt.Columns.Add("HttpValue", typeof(string));
                bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);

                if (ckType1.IsChecked == true)
                    _type = "  Head";
                if (ckType2.IsChecked == true)
                    _type += "  Get";
                _cookies = txCookies1.Text;
                _username = txUsername1.Text;
                _password = txPwd1.Password;
                if (cbDefine.IsChecked == true)
                {
                    _server = txServer1.Text;
                }
                else
                    if (cbHTTPserver.SelectedIndex > 0)
                        _server = cbHTTPserver.Text;
                dt.Clear();
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 10);
                timer.Tick += bw_Cancel;  //你的事件
                timer.Start();
                bw.RunWorkerAsync();
            }
            catch(Exception ex)
            {
                ex.ToString();
            }
        }

        void bw_ProgressChanged2(object sender, ProgressChangedEventArgs e)
        {
            if (bw1.CancellationPending)
                return;
            else
            {
                if (e.ProgressPercentage == 100)
                {
                    timer.Stop();
                    DataTable _dt = new DataTable();
                    _dt = dt.Copy();
                    dt.Clear();
                    if (!string.IsNullOrEmpty(txDomain2.Text.Trim()))
                        txHttpInfo2.Text = "网址:\r    " + txDomain2.Text;
                    if (!string.IsNullOrEmpty(_server))
                        txHttpInfo2.Text += "\r\r服务器地址:\r    " + _server;
                    if (!string.IsNullOrEmpty(_type))
                        txHttpInfo2.Text += "\r\r请求方式:" + _type;
                    if (!string.IsNullOrEmpty(txCookies2.Text.Trim()))
                        txHttpInfo2.Text += "\r\rCookies:\r    " + txCookies2.Text;
                    if (!string.IsNullOrEmpty(txUsername2.Text.Trim()))
                        txHttpInfo2.Text += "\r\r用户名:" + txUsername2.Text;
                    if (!string.IsNullOrEmpty(txPwd2.Password))
                        txHttpInfo2.Text += "\r\r密码:" + txPwd2.Password;
                    ListRecord2.DataContext = _dt;
                    tbMark1.Text = maxValue.ToString();
                    tbMark3.Text = num.ToString();
                    Color clr1 = Color.FromRgb(228, 175, 0);
                    Color clr2 = Color.FromRgb(92, 158, 202);
                    if (value1 != null)
                        DrawingLine(Parent1.Width, Parent1.Height, value1, everVlaue, clr1);
                    if (value2 != null)
                        DrawingLine(Parent1.Width, Parent1.Height, value2, everVlaue, clr2);
                    btnCheck2.Visibility = Visibility.Visible;
                    _btnCheck2.Visibility = Visibility.Hidden;
                    canvas4.Visibility = Visibility.Visible;
                    state = 0;
                }
            }
        }

        private void bw_DoWork2(object sender, DoWorkEventArgs e)
        {
            if (bw1.CancellationPending)
                return;
            else
            {
                string header = null, status = null;
                maxValue = 0;
                minValue = 0;
                if (_type.IndexOf("Head") != -1)
                {
                    value1 = new int[num];
                    for (int i = 1; i <= num; i++)
                    {
                        int ch = 0;
                        DateTime start_time = DateTime.Now;
                        status = getHtmlStatus(_host, _cookies, _username, _password, _server);
                        if (!string.IsNullOrEmpty(status))
                        {
                            if (string.IsNullOrEmpty(ip))
                            {
                                string domain = _host.Replace("http://", "");
                                domain = domain.Replace("https://", "");
                                IPHostEntry iphost = Dns.GetHostEntry(domain);   //解析主机 
                                ip = iphost.AddressList[0].ToString();
                            }
                            DateTime end_time = DateTime.Now;
                            TimeSpan ts = end_time - start_time;
                            ch = (int)ts.TotalMilliseconds;//响应时间.(毫秒)
                            dt.Rows.Add(i, ip, status, ch.ToString() + "ms", "");
                        }
                        else
                        {
                            dt.Rows.Add(i, "无响应", status, "无响应", "");
                        }
                        if (ch < minValue)
                        {
                            minValue = ch;
                        }
                        if (ch > maxValue)
                        {
                            maxValue = ch;
                        }
                        value1[i - 1] = ch;
                    }
                    if (_type.IndexOf("Get") != -1)
                    {
                        value2 = new int[num];
                        for (int i = 1; i <= num; i++)
                        {
                            int ch = 0;
                            DateTime start_time = DateTime.Now;
                            getHtml(_host, _cookies, _username, _password, _server, out header, out status);
                            if (!string.IsNullOrEmpty(status))
                            {
                                if (string.IsNullOrEmpty(ip))
                                {
                                    string domain = _host.Replace("http://", "");
                                    domain = domain.Replace("https://", "");
                                    IPHostEntry iphost = Dns.GetHostEntry(domain);   //解析主机 
                                    ip = iphost.AddressList[0].ToString();
                                }
                                DateTime end_time = DateTime.Now;
                                TimeSpan ts = end_time - start_time;
                                ch = (int)ts.TotalMilliseconds;//响应时间.(毫秒)
                                dt.Rows[i][1] = ip;
                                dt.Rows[i][2] = status;
                                dt.Rows[i][4] = ch.ToString() + "ms";
                            }
                            else
                            {
                                dt.Rows[i][4] = "无响应";
                            }
                            if (ch < minValue)
                            {
                                minValue = ch;
                            }
                            if (ch > maxValue)
                            {
                                maxValue = ch;
                            }
                            value2[i - 1] = ch;
                        }
                    }
                }
                else
                    if (_type.IndexOf("Get") != -1)
                    {
                        value2 = new int[num];
                        for (int i = 1; i <= num; i++)
                        {
                            int ch = 0;
                            DateTime start_time = DateTime.Now;
                            getHtml(_host, _cookies, _username, _password, _server, out header, out status);
                            if (!string.IsNullOrEmpty(status))
                            {
                                if (string.IsNullOrEmpty(ip))
                                {
                                    string domain = _host.Replace("http://", "");
                                    domain = domain.Replace("https://", "");
                                    IPHostEntry iphost = Dns.GetHostEntry(domain);   //解析主机 
                                    ip = iphost.AddressList[0].ToString();
                                }
                                DateTime end_time = DateTime.Now;
                                TimeSpan ts = end_time - start_time;
                                ch = (int)ts.TotalMilliseconds;//响应时间.(毫秒)
                                dt.Rows.Add(i, ip, status, "", ch.ToString() + "ms");
                            }
                            else
                            {
                                dt.Rows.Add(i, "无响应", status, "", "无响应");
                            }
                            if (ch < minValue)
                            {
                                minValue = ch;
                            }
                            if (ch > maxValue)
                            {
                                maxValue = ch;
                            }
                            value2[i - 1] = ch;
                        }
                    }
                everVlaue = 80.0 / (maxValue - minValue);   //每一个刻度数值用everVlaue个页面px像素来表示；
                bw1.ReportProgress(100);
            }
        }

        private void bw_Cancel2(object sender, EventArgs e)
        {
            bw1.CancelAsync();
            bw1.Dispose();
            timer.Stop();
            if (!string.IsNullOrEmpty(txDomain2.Text.Trim()))
                txHttpInfo2.Text = "网址:\r    " + txDomain2.Text;
            if (!string.IsNullOrEmpty(_server))
                txHttpInfo2.Text += "\r\r服务器地址:\r    " + _server;
            if (!string.IsNullOrEmpty(_type))
                txHttpInfo2.Text += "\r\r请求方式:" + _type;
            if (!string.IsNullOrEmpty(txCookies2.Text.Trim()))
                txHttpInfo2.Text += "\r\rCookies:\r    " + txCookies2.Text;
            if (!string.IsNullOrEmpty(txUsername2.Text.Trim()))
                txHttpInfo2.Text += "\r\r用户名:" + txUsername2.Text;
            if (!string.IsNullOrEmpty(txPwd2.Password))
                txHttpInfo2.Text += "\r\r密码:" + txPwd2.Password;
            DataTable _dt = new DataTable();
            _dt = dt.Clone();
            _dt.Clear();
            _dt.Rows.Add("ID", "IP", "状态", "Head请求结果", "Get请求结果");
            _dt.Rows.Add(1, "无响应", "超时", "无响应", "无响应");
            ListRecord2.DataContext = _dt;
            btnCheck2.Visibility = Visibility.Visible;
            _btnCheck2.Visibility = Visibility.Hidden;
            canvas4.Visibility = Visibility.Visible;
            state = 0;
        }

        private void btnCheck2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Parent1.Children.Clear();
                value1 = null;
                value2 = null;
                _host = txDomain2.Text;
                UrlCheck(ref _host);
                if (IsValidHttp(_host))
                {
                    string domain = _host.Replace("http://", "");
                    domain = domain.Replace("https://", "");
                    if (ValidIp(domain))
                    {
                        if (IsValidIp(domain))
                        {
                            ip = domain;
                        }
                        else
                        {
                            MessageBox.Show("IP或域名格式不正确!请重新输入。");
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("IP或域名格式不正确!请重新输入。");
                    return;
                }
                state = 1;
                btnCheck2.Visibility = Visibility.Hidden;
                _btnCheck2.Visibility = Visibility.Visible;
                btn2HttpMore1.Visibility = Visibility.Visible;
                btn2HttpMore2.Visibility = Visibility.Hidden;
                canvas7.Visibility = Visibility.Hidden;
                _type = "";
                if (ck2Type1.IsChecked == true)
                    _type = "  Head";
                if (ck2Type2.IsChecked == true)
                    _type += "  Get";
                if (cbRequestsNum1.IsChecked == true)
                {
                    num = 10;
                }
                else
                if (cbRequestsNum2.IsChecked == true)
                {
                    num = 20;
                }
                else
                if (cbRequestsNum3.IsChecked == true)
                {
                    num = 30;
                }
                dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("id", typeof(string));
                dt.Columns.Add("ipAddress", typeof(string));
                dt.Columns.Add("status", typeof(string));
                dt.Columns.Add("headResults", typeof(string));
                dt.Columns.Add("getResults", typeof(string));
                dt.Rows.Add("ID", "IP", "状态", "Head请求结果", "Get请求结果");
                bw1 = new BackgroundWorker();
                bw1.WorkerReportsProgress = true;
                bw1.WorkerSupportsCancellation = true;
                bw1.DoWork += bw_DoWork2;
                bw1.ProgressChanged += bw_ProgressChanged2;
                //_host = txDomain2.Text;
                _cookies = txCookies2.Text;
                _username = txUsername2.Text;
                _password = txPwd2.Password;
                if (cbDefine2.IsChecked == true)
                {
                    _server = txServer2.Text;
                }
                else
                    if (cbHTTPserver2.SelectedIndex > 0)
                        _server = cbHTTPserver2.Text;
                DrawingBox(Parent1.Width, Parent1.Height, num);
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, num);
                timer.Tick += bw_Cancel2;  //你的事件
                timer.Start();
                bw1.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

        }
        
        private void txDomain1_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {  //大家在敲完后都习惯性回车确定  这里就是在Input里输入完后直接回车，相当于点击了“马上检测”这个按钮
                btnCheck1_Click(this, null);
            }
        }

        private void txDomain2_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {  //大家在敲完后都习惯性回车确定  这里就是在Input里输入完后直接回车，相当于点击了“马上检测”这个按钮
                btnCheck2_Click(this, null);
            }
        }

        public string LookUp(string domain)
        {
            string result = "";
            string[] temp = domain.Split('.');
            string suffix = temp[temp.Length - 1].ToLower();// get the last;
            //定义whois服务器库
            Dictionary<string, string> serverList = new Dictionary<string, string>();
            serverList.Add("com", "whois.crsnic.net");
            serverList.Add("cn", "whois.cnnic.net.cn");
            serverList.Add("edu", "whois.educause.net");
            serverList.Add("net", "whois.crsnic.net");
            serverList.Add("org", "whois.crsnic.net");
            serverList.Add("info", "whois.afilias.com");
            serverList.Add("de", "whois.denic.de");
            serverList.Add("nl", "whois.domain-registry.nl");
            serverList.Add("eu", "whois.eu");

            if (!serverList.Keys.Contains(suffix))
            {
                result = string.Format("不支持此域名", suffix);
                return result;
            }
            string server = serverList[suffix];
            //string server = suffix + ".whois-servers.net";
            TcpClient client = new TcpClient();
            NetworkStream ns;
            try
            {
                client.Connect(server, 43);
                ns = client.GetStream();
                byte[] buffer = Encoding.ASCII.GetBytes(domain + "\rn");
                ns.Write(buffer, 0, buffer.Length);

                buffer = new byte[8192];

                int i = ns.Read(buffer, 0, buffer.Length);
                while (i > 0)
                {
                    Encoding encoding = Encoding.UTF8;
                    result += encoding.GetString(buffer, 0, i);
                    i = ns.Read(buffer, 0, buffer.Length);
                }
            }
            catch (SocketException)
            {
                result = "链接失败";
                return result;
            }
            ns.Close();
            client.Close();

            return result;
        }

        /// <summary>
        /// 正则验证HTTP地址格式
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        bool ValidIp(string strIn)
        {
            return Regex.IsMatch(strIn, @"(\d+)\.(\d+)\.(\d+)\.(\d+)");
        }


        /// <summary>
        /// 正则验证IP地址格式
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        bool IsValidIp(string strIn)
        {
            return Regex.IsMatch(strIn, @"((25[0-5])|(2[0-4]\d)|(1\d\d)|([1-9]\d)|\d)(\.((25[0-5])|(2[0-4]\d)|(1\d\d)|([1-9]\d)|\d)){3}");
        }

        /// <summary>
        /// 正则验证HTTP地址格式
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        bool IsValidHttp(string strIn)
        {
            //return Regex.IsMatch(strIn, @"^([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$");
            return Regex.IsMatch(strIn, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
        }


        /// <summary>
        /// 正则匹配Html中的title信息
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        private string getTitle(string strIn)
        {
            string strRegex = @"<title>.*<\/title>";
            string str = Regex.Match(strIn, strRegex).Value;
            str = str.Replace("<title>", "");
            str = str.Replace("</title>", "");
            return str;
        }

        /// <summary>
        /// 正则匹配Html中的keywords信息
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        private string getKeyWords(string strIn)
        {
            string strRegex = "(?<=meta name=\"(keywords)|(Keywords)\" content=\").*?(?=\")";
            string str = Regex.Match(strIn, strRegex).Value;

            return str;
        }


        /// <summary>
        /// 正则匹配Html中的description信息
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        private string getDescription(string strIn)
        {
            string strRegex = "(?<=meta name=\"(description)|(Description)\" content=\").*?(?=\")";
            string str = Regex.Match(strIn, strRegex).Value;

            return str;
        }

        /// <summary>
        /// 正则匹配Html中的charset信息
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        private string getCharset(string strIn)
        {
            Match charSetMatch = Regex.Match(strIn, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string str = charSetMatch.Groups[2].Value;

            return str;
        }

        private string getHtml(string url, string cookie, string username, string password, string server, out string header, out string status)
        {
            if (!string.IsNullOrEmpty(url))
                UrlCheck(ref url);
            if (!string.IsNullOrEmpty(server))
                UrlCheck(ref server);
            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream = null;
            StreamReader streamReader = null;
            string getString = "";
            header = null;
            status = null;
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                httpWebRequest.CachePolicy = noCachePolicy;//从不使用和存储缓存
                httpWebRequest.Accept = "*/*";
                if (!string.IsNullOrEmpty(server))
                    httpWebRequest.Referer = server;
                if (!string.IsNullOrEmpty(cookie))
                {
                    CookieContainer co = new CookieContainer();
                    co.SetCookies(new Uri(url), cookie);
                    httpWebRequest.CookieContainer = co;
                }
                //如果服务器要验证用户名,密码 
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    NetworkCredential mycred = new NetworkCredential(username, password);
                    httpWebRequest.Credentials = mycred;
                }
                httpWebRequest.UserAgent =
                    "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
                httpWebRequest.Method = "GET";
                webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                header = webResponse.Headers.ToString();
                status = webResponse.StatusCode.ToString();
                getStream = webResponse.GetResponseStream();
                streamReader = new StreamReader(getStream, Encoding.Default);
                getString = streamReader.ReadToEnd();

                webResponse.Close();
                httpWebRequest.Abort();
                streamReader.Close();
                getStream.Close();
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }
            return getString;
        }
        
        /// <summary>
        /// 获取网站响应状态
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookie"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        private string getHtmlStatus(string url, string cookie, string username, string password, string server)
        {
            if (!string.IsNullOrEmpty(url))
                UrlCheck(ref url);
            if (!string.IsNullOrEmpty(server))
                UrlCheck(ref server);
            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            string status = "";
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                httpWebRequest.CachePolicy = noCachePolicy;//从不使用和存储缓存
                httpWebRequest.Accept = "*/*";
                if (!string.IsNullOrEmpty(server))
                    httpWebRequest.Referer = server;
                if (!string.IsNullOrEmpty(cookie))
                {
                    CookieContainer co = new CookieContainer();
                    co.SetCookies(new Uri(url), cookie);
                    httpWebRequest.CookieContainer = co;
                }
                //如果服务器要验证用户名,密码 
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    NetworkCredential mycred = new NetworkCredential(username, password);
                    httpWebRequest.Credentials = mycred;
                }
                httpWebRequest.UserAgent =
                    "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
                httpWebRequest.Method = "GET";
                webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                status = webResponse.StatusDescription;
                webResponse.Close();
                httpWebRequest.Abort();
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }
            return status;
        }

        private string getHtml(string url, string cookie, string username, string password, out string status)
        {
            if (!string.IsNullOrEmpty(url))
                UrlCheck(ref url);
            string strWebData = null;
            status = null;
            try
            {
                WebClient myWebClient = new WebClient(); //创建WebClient实例myWebClient 
                // 需要注意的： 
                //有的网页可能下不下来，有种种原因比如需要cookie,编码问题等等 
                //这是就要具体问题具体分析比如在头部加入cookie 
                if (!string.IsNullOrEmpty(cookie))
                {
                    myWebClient.Headers.Add("Cookie", cookie);
                }
                //这样可能需要一些重载方法。根据需要写就可以了 

                //获取或设置用于对向 Internet 资源的请求进行身份验证的网络凭据。 
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    //如果服务器要验证用户名,密码 
                    NetworkCredential mycred = new NetworkCredential(username, password);
                    myWebClient.Credentials = mycred;
                }
                //从资源下载数据并返回字节数组。（加@是因为网址中间有"/"符号） 
                byte[] myDataBuffer = myWebClient.DownloadData(url);
                strWebData = Encoding.Default.GetString(myDataBuffer);
                if (!string.IsNullOrEmpty(strWebData))
                    status = "OK";
                //获取网页字符编码描述信息 
                Match charSetMatch = Regex.Match(strWebData, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                string webCharSet = charSetMatch.Groups[2].Value;

                if (Encoding.GetEncoding(webCharSet) != Encoding.Default)
                    strWebData = Encoding.GetEncoding(webCharSet).GetString(myDataBuffer);
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }
            return strWebData;
        }

        /// <summary>
        /// URL拼写完整性检查
        /// </summary>
        /// <param name="url">待检查的URL</param>
        private static void UrlCheck(ref string url)
        {
            url = url.ToLower();
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;
        }

        /// <summary>
        /// 将数据字节大小转换成易读的单位字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="si"></param>
        /// <returns></returns>
        public static String conversionByte(long bytes)
        {
            int unit = 1024;
            if (bytes < unit) return bytes + " B";
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return String.Format("{0:F1} {1}B", bytes / Math.Pow(unit, exp), "KMGTPE"[exp - 1]);

        }


        private void btnHttpStatus_Click(object sender, RoutedEventArgs e)
        {
            if (state == 0)
            {
                canvas0.Visibility = Visibility.Hidden;
                canvas1.Visibility = Visibility.Visible;
                canvas2.Visibility = Visibility.Hidden;
                canvas3.Visibility = Visibility.Hidden;
                canvas4.Visibility = Visibility.Hidden;
                btn2HttpMore2.Visibility = Visibility.Hidden;
                canvas7.Visibility = Visibility.Hidden;
                txDomain2.Text = null;
                ck2Type1.IsChecked = true;
                ck2Type2.IsChecked = false;
                txUsername2.Text = null;
                txCookies2.Text = null;
                txUsername2.Text = null;
                txPwd2.Password = null;
            }
        }

        private void btnHttpFast_Click(object sender, RoutedEventArgs e)
        {
            if (state == 0)
            {
                canvas0.Visibility = Visibility.Hidden;
                canvas1.Visibility = Visibility.Hidden;
                canvas2.Visibility = Visibility.Hidden;
                canvas3.Visibility = Visibility.Visible;
                canvas4.Visibility = Visibility.Hidden;
                btnHttpMore2.Visibility = Visibility.Hidden;
                canvas6.Visibility = Visibility.Hidden;
                txDomain1.Text = null;
                ckType1.IsChecked = true;
                ckType2.IsChecked = false;
                txUsername1.Text = null;
                txCookies1.Text = null;
                txUsername1.Text = null;
                txPwd1.Password = null;
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            if (state == 0)
            {
                canvas0.Visibility = Visibility.Visible;
                canvas1.Visibility = Visibility.Hidden;
                canvas2.Visibility = Visibility.Hidden;
                canvas3.Visibility = Visibility.Hidden;
                canvas4.Visibility = Visibility.Hidden;
                btnHttpMore1.Visibility = Visibility.Visible;
                btnHttpMore2.Visibility = Visibility.Hidden;
                canvas6.Visibility = Visibility.Hidden;
                btn2HttpMore1.Visibility = Visibility.Visible;
                btn2HttpMore2.Visibility = Visibility.Hidden;
                canvas7.Visibility = Visibility.Hidden;
                txDomain1.Text = null;
                ckType1.IsChecked = true;
                ckType2.IsChecked = false;
                txUsername1.Text = null;
                txCookies1.Text = null;
                txUsername1.Text = null;
                txPwd1.Password = null;
                txDomain2.Text = null;
                ck2Type1.IsChecked = true;
                ck2Type2.IsChecked = false;
                txUsername2.Text = null;
                txCookies2.Text = null;
                txUsername2.Text = null;
                txPwd2.Password = null;
            }
        }

        private void btnHttpMore1_Click(object sender, RoutedEventArgs e)
        {
            if (state == 0)
            {
                DataTable table = new DataTable();
                table.Columns.Add("ID", typeof(int));
                table.Columns.Add("IP", typeof(string));
                table.Rows.Add(0, "请选择服务器地址");
                try
                {
                    _host = txDomain1.Text;
                    UrlCheck(ref _host);
                    if (!IsValidHttp(_host))
                    {
                        MessageBox.Show("域名格式不正确!请重新输入。");
                        return;
                    }
                    else
                    {
                        string domain = _host.Replace("http://", "");
                        domain = domain.Replace("https://", "");
                        IPHostEntry iphost = Dns.GetHostEntry(domain);   //解析主机 
                        for (int i = 1; i <= iphost.AddressList.Length; i++)
                        {

                            table.Rows.Add(i, iphost.AddressList[i - 1].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                cbHTTPserver.DataContext = table;
                cbHTTPserver.SelectedValue = 0;
                btnHttpMore1.Visibility = Visibility.Hidden;
                btnHttpMore2.Visibility = Visibility.Visible;
                canvas6.Visibility = Visibility.Visible;
                canvas2.Visibility = Visibility.Hidden;
            }
        }

        private void btnHttpMore2_Click(object sender, RoutedEventArgs e)
        {
                btnHttpMore1.Visibility = Visibility.Visible;
                btnHttpMore2.Visibility = Visibility.Hidden;
                canvas6.Visibility = Visibility.Hidden;
        }

        private void btn2HttpMore1_Click(object sender, RoutedEventArgs e)
        {
            if (state == 0)
            {
                DataTable table = new DataTable();
                table.Columns.Add("ID", typeof(int));
                table.Columns.Add("IP", typeof(string));
                table.Rows.Add(0, "请选择服务器地址");
                try
                {
                    _host = txDomain2.Text;
                    UrlCheck(ref _host);
                    if (!IsValidHttp(_host))
                    {
                        MessageBox.Show("域名格式不正确!请重新输入。");
                        return;
                    }
                    else
                    {
                        string domain = _host.Replace("http://", "");
                        domain = domain.Replace("https://", "");
                        IPHostEntry iphost = Dns.GetHostEntry(domain);   //解析主机 
                        for (int i = 1; i <= iphost.AddressList.Length; i++)
                        {
                            table.Rows.Add(i, iphost.AddressList[i - 1].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                cbHTTPserver2.DataContext = table;
                cbHTTPserver2.SelectedValue = 0;
                btn2HttpMore1.Visibility = Visibility.Hidden;
                btn2HttpMore2.Visibility = Visibility.Visible;
                canvas7.Visibility = Visibility.Visible;
                canvas4.Visibility = Visibility.Hidden;
            }
        }

        private void btn2HttpMore2_Click(object sender, RoutedEventArgs e)
        {
                btn2HttpMore1.Visibility = Visibility.Visible;
                btn2HttpMore2.Visibility = Visibility.Hidden;
                canvas7.Visibility = Visibility.Hidden;
        }

        private void cbDefine_Click(object sender, RoutedEventArgs e)
        {
            if (cbDefine.IsChecked == true)
            {
                cbHTTPserver.Visibility = Visibility.Hidden;
                txServer1.Visibility = Visibility.Visible;
            }
            else
            {
                cbHTTPserver.Visibility = Visibility.Visible;
                txServer1.Visibility = Visibility.Hidden;
            }
        }

        private void cbDefine2_Click(object sender, RoutedEventArgs e)
        {
            if (cbDefine2.IsChecked == true)
            {
                cbHTTPserver2.Visibility = Visibility.Hidden;
                txServer2.Visibility = Visibility.Visible;
            }
            else
            {
                cbHTTPserver2.Visibility = Visibility.Visible;
                txServer2.Visibility = Visibility.Hidden;
            }
        }

        protected void DrawingBox(double drawwidth, double drawheight, int datetime)
        {
            double farTop = 10;
            double farBottom = 10;
            double farLeft = 0;
            double farRight = 0;

            double useHeight = drawheight - farTop - farBottom;
            double useWidth = drawwidth - farLeft - farRight;
            double everTime = useWidth / datetime;

            LineGeometry myLineX = new LineGeometry();
            myLineX.StartPoint = new Point(farLeft, useHeight + farTop);
            myLineX.EndPoint = new Point(useWidth, useHeight + farTop);

            System.Windows.Shapes.Path myPathX = new System.Windows.Shapes.Path();
            Color clrX = Color.FromRgb(228, 175, 0);
            SolidColorBrush brushX = new SolidColorBrush(clrX);
            myPathX.Stroke = brushX;
            myPathX.StrokeThickness = 1;
            myPathX.Data = myLineX;

            Parent1.Children.Add(myPathX);

            LineGeometry myLineY = new LineGeometry();
            myLineY.StartPoint = new Point(farLeft, farTop);
            myLineY.EndPoint = new Point(farLeft, useHeight + farTop);

            System.Windows.Shapes.Path myPathY = new System.Windows.Shapes.Path();
            Color clrY = Color.FromRgb(153, 151, 151);
            SolidColorBrush brushY = new SolidColorBrush(clrY);
            myPathY.Stroke = brushY;
            myPathY.StrokeThickness = 1;
            myPathY.Data = myLineY;

            Parent1.Children.Add(myPathY);

            for (int i = 1; i <= datetime; i++)
            {
                LineGeometry _myLineY = new LineGeometry();
                _myLineY.StartPoint = new Point(farLeft + i * everTime, farTop);
                _myLineY.EndPoint = new Point(farLeft + i * everTime, useHeight + farTop);

                System.Windows.Shapes.Path _myPathY = new System.Windows.Shapes.Path();
                _myPathY.Stroke = brushY;
                _myPathY.StrokeThickness = 1;
                _myPathY.Data = _myLineY;

                Parent1.Children.Add(_myPathY);
            }

        }

        protected void DrawingLine(double drawwidth, double drawheight, int[] value, double everVlaue, Color color)
        {
            double farTop = 10;
            double farBottom = 10;
            double farLeft = 0;
            double farRight = 0;

            double useHeight = drawheight - farTop - farBottom;
            double useWidth = drawwidth - farLeft - farRight;
            double everTime = useWidth / value.Length;

            SolidColorBrush brushX = new SolidColorBrush(color);

            //double everVlaue = (useHeight - 10) / (maxValue - minValue);   //每一个刻度数值用everVlaue个页面px像素来表示；

            Point pt1 = new Point(farLeft + everTime, useHeight - value[0] * everVlaue + farTop);
            Point[] values = new Point[value.Length];
            values[0] = pt1;
            for (int i = 1; i < value.Length; i++)
            {
                Point pt = new Point(pt1.X + everTime * i, useHeight - value[i] * everVlaue + farTop);
                values[i] = pt;
            }

            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(values[0], true, false);
                EllipseGeometry ellipseOne = new EllipseGeometry();
                ellipseOne.Center = values[0];
                ellipseOne.RadiusX = 3;
                ellipseOne.RadiusY = 3;

                System.Windows.Shapes.Path myPathellipseOne = new System.Windows.Shapes.Path();
                myPathellipseOne.Stroke = brushX;
                myPathellipseOne.StrokeThickness = 1;
                myPathellipseOne.Fill = brushX;
                myPathellipseOne.Data = ellipseOne;
                myPathellipseOne.ToolTip = value[0].ToString();
                Parent1.Children.Add(myPathellipseOne);
                for (int i = 1; i < values.Length; i++)
                {
                    ctx.LineTo(values[i], true, false);
                    EllipseGeometry ellipse = new EllipseGeometry();
                    ellipse.Center = values[i];
                    ellipse.RadiusX = 3;
                    ellipse.RadiusY = 3;

                    System.Windows.Shapes.Path myPathellipse = new System.Windows.Shapes.Path();
                    myPathellipse.Stroke = brushX;
                    myPathellipse.StrokeThickness = 1;
                    myPathellipse.Fill = brushX;
                    myPathellipse.Data = ellipse;
                    myPathellipse.ToolTip = value[i].ToString();
                    Parent1.Children.Add(myPathellipse);
                }

            }
            geometry.FillRule = FillRule.Nonzero;
            geometry.Freeze();
            System.Windows.Shapes.Path myPath = new System.Windows.Shapes.Path();
            myPath.Stroke = brushX;
            myPath.StrokeThickness = 2;
            myPath.Data = geometry;
            Parent1.Children.Add(myPath);
        }

        private void cbRequestsNum1_Checked(object sender, RoutedEventArgs e)
        {
            cbRequestsNum1.IsChecked = true;
            cbRequestsNum2.IsChecked = false;
            cbRequestsNum3.IsChecked = false;

        }

        private void cbRequestsNum2_Checked(object sender, RoutedEventArgs e)
        {
            cbRequestsNum1.IsChecked = false;
            cbRequestsNum2.IsChecked = true;
            cbRequestsNum3.IsChecked = false;
        }

        private void cbRequestsNum3_Checked(object sender, RoutedEventArgs e)
        {
            cbRequestsNum1.IsChecked = false;
            cbRequestsNum2.IsChecked = false;
            cbRequestsNum3.IsChecked = true;
        }


    }
}
