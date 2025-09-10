using System.Text;
using System.Text.Json;

namespace IdleApi.Service
{
    public class HttpClientSerivce
    {
        private readonly HttpClient _httpClient;
        // 通过构造函数注入 HttpClient
        public HttpClientSerivce(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // 可设置默认请求头，比如 BaseAddress 或 User-Agent
            _httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ApiHttpClientService/1.0");
        }

        // POST JSON 并反序列化返回对象
        public async Task<T?> PostJsonAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseBody);
        }
    }
}
