using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Utils
{
    public class ProxyUtil
    {
        public static void TestProxy(string ip, int port)
        {
            // 要访问的目标网页
            string page_url = "https://ip.cn/";

            // 构造请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(page_url);
            request.Method = "GET";
            request.Headers.Add("Accept-Encoding", "Gzip");  // 使用gzip压缩传输数据让访问更快

            // 代理服务器
            string proxy_ip = ip;
            int proxy_port = port;

            // 用户名密码 <私密代理/独享代理>
            string username = "d2011731996";
            string password = "v84dygqx";

            // 设置代理 <开放代理或私密/独享代理&已添加白名单>
            // request.Proxy = new WebProxy(proxy_ip, proxy_port);

            // 设置代理 <私密/独享代理&未添加白名单>
            WebProxy proxy = new WebProxy();
            proxy.Address = new Uri(String.Format("http://{0}:{1}", proxy_ip, proxy_port));
            proxy.Credentials = new NetworkCredential(username, password);
            request.Proxy = proxy;

            // 请求目标网页
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Console.WriteLine((int)response.StatusCode);  // 获取状态码
            // 解压缩读取返回内容
            using (StreamReader reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress)))
            {
                var a = reader.ReadToEnd();
                P.Log(a,emLogType.Debug);
            }
        }
    }
}
