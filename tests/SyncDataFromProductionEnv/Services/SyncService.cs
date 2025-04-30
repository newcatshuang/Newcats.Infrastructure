using System.Text;
using System.Text.Json;

namespace SyncDataFromProductionEnv.Services;

/// <summary>
/// 同步服务类，负责调用同步API进行数据同步
/// </summary>
public class SyncService
{
    private readonly HttpClient _httpClient;
    private const string SyncApiUrl = "";

    /// <summary>
    /// 构造函数，初始化HttpClient
    /// </summary>
    public SyncService()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// 同步数据
    /// </summary>
    /// <param name="keys">要同步的配置Key列表</param>
    /// <returns>同步结果，包含是否成功、消息和耗时</returns>
    public async Task<(bool Success, string Message, TimeSpan Duration)> SyncDataAsync(List<string> keys)
    {
        var startTime = DateTime.Now;
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(keys),
                Encoding.UTF8,
                "application/json");

            Console.WriteLine($"\n[API请求]");
            Console.WriteLine($"URL: {SyncApiUrl}");
            Console.WriteLine($"Method: POST");
            Console.WriteLine($"Content-Type: application/json");
            Console.WriteLine($"Request Body: {JsonSerializer.Serialize(keys, new JsonSerializerOptions { WriteIndented = true })}");

            var response = await _httpClient.PostAsync(SyncApiUrl, content);
            var duration = DateTime.Now - startTime;

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n[API响应]");
            Console.WriteLine($"Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // 尝试解析响应内容为JSON
                    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var formattedResponse = JsonSerializer.Serialize(jsonResponse, new JsonSerializerOptions { WriteIndented = true });
                    return (true, $"同步成功\n{formattedResponse}", duration);
                }
                catch
                {
                    // 如果解析失败，直接返回原始响应
                    return (true, $"同步成功\n{responseContent}", duration);
                }
            }
            else
            {
                return (false, $"同步失败: HTTP {(int)response.StatusCode} {response.StatusCode}\n{responseContent}", duration);
            }
        }
        catch (Exception ex)
        {
            var duration = DateTime.Now - startTime;
            return (false, $"同步出错: {ex.GetType().Name}\n{ex.Message}\n{ex.StackTrace}", duration);
        }
    }
}