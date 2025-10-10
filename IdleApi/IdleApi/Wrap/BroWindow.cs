using HtmlAgilityPack;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
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

        public UserModel User { get; set; }
        /// <summary>
        /// 用户名，也是 Cookie 文件名标识
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Cookie 文件存储路径（默认程序目录下：{UserName}.txt）
        /// </summary>
        public string CookiePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cookie/{UserName}.txt");

        public string Token { get; set; }

        /// <summary>
        /// 当前正在操作的地址（比如当前加载的页面 URL）
        /// </summary>
        public string CurrentAddress => _address;

        private string _address;

        /// <summary>
        /// 当前页面内容
        /// </summary>
        public string CurrentContent { get; set; }

        /// <summary>
        /// 构造函数：初始化指定用户的 Cookie 和 HttpClient
        /// </summary>
        /// <param name="userName">用户唯一标识，用于区分 Cookie 文件</param>
        public BroWindow(string userName)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));

            User = AccountCfg.Instance.GetUserModel(userName);
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
            _httpClient.BaseAddress = new Uri("https://www.idleinfinity.cn");

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
                // 动态构造 URI：支持相对路径和完整 URL
                Uri requestUri;
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    // 如果传入的是完整 URL（如 https://...），直接使用
                    requestUri = new Uri(url);
                }
                else
                {
                    // 如果传入的是相对路径（如 /Home/Index），拼接 BaseAddress
                    if (_httpClient.BaseAddress == null)
                        throw new InvalidOperationException("未设置 BaseAddress，无法处理相对路径");

                    requestUri = new Uri(_httpClient.BaseAddress, url);
                }

                var response = await _httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                if (!content.Contains("删除角色") && url.Contains("Index"))
                {
                    throw new Exception("登录失效");
                }
                else
                {
                    await SaveCookiesToFileAsync();
                }
                CurrentContent = content;
                await Task.Delay(2000);

                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BroWindow] 加载 URL 失败: {url}, 错误: {ex.Message}");
                throw ex;
            }
        }



        /// <summary>
        /// 提交表单并返回重定向的页面
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> SubmitFormAsync(string url, Dictionary<string, string> dic = null)
        {
            await Task.Delay(2000);
            var formData = ParseForm(CurrentContent);
            if (dic != null)
            {
                foreach(var item in dic)
                {
                    formData[item.Key] = item.Value;
                }
            }
            // 2. 设置请求头（根据抓包结果调整）
            // 3. 设置请求头（可选，但某些服务器会检查）
            if (!_httpClient.DefaultRequestHeaders.Contains("Referer"))
            {
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                _httpClient.DefaultRequestHeaders.Add("Origin", "https://www.idleinfinity.cn");
                _httpClient.DefaultRequestHeaders.Add("Referer", CurrentAddress);
            }

         

            
            // 动态构造 URI：支持相对路径和完整 URL
            Uri requestUri;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                // 如果传入的是完整 URL（如 https://...），直接使用
                requestUri = new Uri(url);
            }
            else
            {
                // 如果传入的是相对路径（如 /Home/Index），拼接 BaseAddress
                if (_httpClient.BaseAddress == null)
                    throw new InvalidOperationException("未设置 BaseAddress，无法处理相对路径");

                requestUri = new Uri(_httpClient.BaseAddress, url);
            }
            // 4. 发送请求
            var response = await _httpClient.PostAsync(requestUri, new FormUrlEncodedContent(formData));

            // 5. 处理重定向
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                var redirectUrl = response.Headers.Location;
                Console.WriteLine($"重定向到: {redirectUrl}");

                // 发送重定向请求（GET）
                response = await _httpClient.GetAsync(redirectUrl);
            }

            // 6. 检查结果
            Console.WriteLine($"最终状态码: {response.StatusCode}");
            Console.WriteLine($"账号: {User.AccountName}|执行地址:{url}");
            var content = await response.Content.ReadAsStringAsync();
            CurrentContent = content;
            response.EnsureSuccessStatusCode();
            return content;
        }

        public static Dictionary<string, string> ParseForm(string htmlContent)
        {
            var formData = new Dictionary<string, string>();

            // 加载HTML内容
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // 查找id为"form"的表单
            var formNode = htmlDoc.DocumentNode.Descendants("form")
                                     .FirstOrDefault(f => f.Attributes.Contains("id") && f.Attributes["id"].Value == "form");

            if (formNode == null)
            {
                Console.WriteLine("未找到id为'form'的表单。");
                return formData;
            }

            // 遍历表单中的所有输入元素
            var inputNodes = formNode.Descendants("input");

            foreach (var input in inputNodes)
            {
                var name = input.Attributes["name"]?.Value;
                var value = input.Attributes["value"]?.Value;

                if (!string.IsNullOrEmpty(name))
                {
                    formData[name] = value ?? string.Empty;
                }
            }

            return formData;
        }

        /// <summary>
        /// 从文件加载 Cookie（通常是之前保存的登录态 Cookie）
        /// 加载时会对相同 Domain + Path + Name 的 Cookie 去重，只保留最新的（Expires 最晚的）
        /// </summary>
        private void LoadCookiesFromFile()
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
                if (File.Exists(CookiePath))
                {
                    var lastWriteTime = File.GetLastWriteTime(CookiePath);
                    var span = DateTime.Now - lastWriteTime;
                    if (span.TotalMinutes < 10)
                    {
                        return;
                    }
                }
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