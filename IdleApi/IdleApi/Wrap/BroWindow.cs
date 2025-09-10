using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IdleApi.Wrap
{
    /// <summary>
    /// 模拟浏览器窗口，用于管理指定用户的登录状态、Cookie 和 HTTP 请求
    /// </summary>
    public class BroWindow
    {
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;

        /// <summary>
        /// 用户名，也是 Cookie 文件名标识
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Cookie 文件存储路径（默认程序目录下：{UserName}.txt）
        /// </summary>
        public string CookiePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cookie/{UserName}.txt");

        /// <summary>
        /// 当前正在操作的地址（比如当前加载的页面 URL）
        /// </summary>
        public string CurrentAddress => _address;

        private string _address;

        /// <summary>
        /// 构造函数：初始化指定用户的 Cookie 和 HttpClient
        /// </summary>
        /// <param name="userName">用户唯一标识，用于区分 Cookie 文件</param>
        public BroWindow(string userName)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));

            // 初始化 Cookie 容器
            _cookieContainer = new CookieContainer();

            // 尝试加载已保存的 Cookie（比如上次登录保存的）
            LoadCookiesFromFile();

            // 配置 HttpClientHandler，使用我们自己的 CookieContainer
            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true,
                AllowAutoRedirect = true // 允许重定向
            };

            // 创建 HttpClient（推荐在实际项目中使用 IHttpClientFactory 管理）
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// 加载指定 URL 页面内容（模拟已登录用户访问）
        /// </summary>
        /// <param name="url">目标 URL，如：/Character/Detail?id=116</param>
        /// <returns>页面 HTML / JSON 内容</returns>
        public async Task<string> LoadUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL不能为空", nameof(url));

            _address = url;

            try
            {
                var fullUri = new Uri(new Uri("https://www.idleinfinity.cn/"), url); // 构造完整 URI
                var response = await _httpClient.GetAsync(fullUri);

                // 确保请求成功
                response.EnsureSuccessStatusCode();

                await SaveCookiesToFileAsync();
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[BroWindow] 成功加载: {fullUri}\n 页面是否在首页: {content.Contains("奶牛苦工").ToString()}");
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BroWindow] 加载 URL 失败: {url}, 错误: {ex.Message}");
                throw; // 可根据需要处理异常，比如重试、记录日志等
            }
        }

        /// <summary>
        /// 从文件加载 Cookie（通常是之前保存的登录态 Cookie）
        /// 加载时会对相同 Domain + Path + Name 的 Cookie 去重，只保留最新的（Expires 最晚的）
        /// </summary>
        public void LoadCookiesFromFile()
        {
            string path = CookiePath;

            if (!File.Exists(path))
            {
                Console.WriteLine($"[BroWindow] Cookie 文件不存在，跳过加载: {path}");
                return;
            }

            try
            {
                // 用于去重：Key = "Domain|Path|Name"，Value = Cookie
                var cookieDict = new Dictionary<string, Cookie>();

                using var reader = new StreamReader(path, Encoding.UTF8);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split('\t');
                    if (parts.Length < 5)
                    {
                        Console.WriteLine($"[BroWindow] 跳过无效 Cookie 行（字段不足，需要至少5列）: {line}");
                        continue;
                    }

                    try
                    {
                        var domain = parts[0].Trim();
                        var name = parts[1].Trim();
                        var value = parts[2].Trim();
                        var pathCookie = parts[3].Trim();
                        DateTime expires;

                        // 解析第5列为过期时间
                        if (DateTime.TryParse(parts[4].Trim(), out expires))
                        {
                            // 正常解析成功
                        }
                        else
                        {
                            // 如果解析失败，给个默认值（比如过期时间设为很早，或跳过该条）
                            Console.WriteLine($"[BroWindow] 解析过期时间失败，使用最小值: {parts[4]}, 行内容: {line}");
                            expires = DateTime.MinValue; // 表示已过期或无效，后续可过滤
                        }

                        var cookie = new Cookie(name, value, pathCookie, domain)
                        {
                            Expires = expires
                        };

                        // 构造唯一键：Domain|Path|Name
                        string key = $"{domain}|{pathCookie}|{name}";

                        if (cookieDict.TryGetValue(key, out var existingCookie))
                        {
                            // 如果已存在相同 Domain + Path + Name 的 Cookie，比较 Expires，保留较新的
                            if (cookie.Expires > existingCookie.Expires)
                            {
                                Console.WriteLine($"[BroWindow] 更新 Cookie（保留更新的）: {name}, 原过期: {existingCookie.Expires}, 新过期: {cookie.Expires}");
                                cookieDict[key] = cookie;
                            }
                            else
                            {
                                Console.WriteLine($"[BroWindow] 跳过较旧的 Cookie: {name}, 原过期: {existingCookie.Expires}, 新过期: {cookie.Expires}");
                            }
                        }
                        else
                        {
                            // 新 Cookie，直接加入字典
                            cookieDict[key] = cookie;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BroWindow] 解析 Cookie 失败: {line}, 错误: {ex.Message}");
                    }
                }

                // 第二步：将去重后的 Cookie 加入 CookieContainer
                foreach (var cookie in cookieDict.Values)
                {
                    // 可选：如果你想进一步过滤掉已经过期的 Cookie，可以加上这一句
                    if (cookie.Expires <= DateTime.Now)
                    {
                        Console.WriteLine($"[BroWindow] 过滤掉已过期的 Cookie: {cookie.Name}, 过期时间: {cookie.Expires}");
                        continue;
                    }

                    _cookieContainer.Add(cookie);
                    Console.WriteLine($"[BroWindow] 已加载 Cookie: {cookie.Name}={cookie.Value} (Domain={cookie.Domain}, Path={cookie.Path}, 过期: {cookie.Expires})");
                }

                Console.WriteLine($"[BroWindow] Cookie 加载完成，最终有效 Cookie 数量: {cookieDict.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BroWindow] 读取或加载 Cookie 文件失败: {path}, 错误: {ex.Message}");
            }
        }

        public async Task SaveCookiesToFileAsync()
        {
            try
            {
                var allCookies = _cookieContainer.GetCookies(new Uri("https://www.idleinfinity.cn/Home/Index"));

                // 用于去重：Key 是 "Domain|Path|Name"，Value 是 Cookie
                var uniqueCookies = new Dictionary<string, Cookie>();

                foreach (Cookie cookie in allCookies)
                {
                    string key = $"{cookie.Domain}|{cookie.Path}|{cookie.Name}";

                    // 如果已经存在，比较时间，保留最新的（Expires 更晚的）
                    if (uniqueCookies.TryGetValue(key, out var existingCookie))
                    {
                        if (cookie.Expires > existingCookie.Expires)
                        {
                            uniqueCookies[key] = cookie;
                        }
                        // 否则，保留已有的（已存在的 Expires 更晚）
                    }
                    else
                    {
                        uniqueCookies[key] = cookie;
                    }
                }

                // 写入文件
                await using var writer = new StreamWriter(CookiePath, append: false, Encoding.UTF8);
                foreach (var cookie in uniqueCookies.Values)
                {
                    var expiresStr = cookie.Expires == DateTime.MinValue ? "" : cookie.Expires.ToString("yyyy/MM/dd HH:mm:ss");
                    // 如果 Expires 是 MinValue（未设置过期时间），可以特殊处理，比如写空或者 01/01/0001
                    var line = $"{cookie.Domain}\t{cookie.Name}\t{cookie.Value}\t{cookie.Path}\t{expiresStr}";
                    await writer.WriteLineAsync(line);
                }

                Console.WriteLine($"[BroWindow] 去重后，共保存 {uniqueCookies.Count} 个 Cookie 到: {CookiePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BroWindow] 保存 Cookie 失败: {ex.Message}");
            }
        }
    }
}