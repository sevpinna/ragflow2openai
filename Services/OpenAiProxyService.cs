// Services/OpenAiProxyService.cs
using System.Net.Http.Headers;

namespace OpenAiProxy.Services;

public class OpenAiProxyService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenAiProxyService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // 设置较长的超时时间以支持流式传输
        _httpClient.Timeout = TimeSpan.FromMinutes(30);
    }

    public async Task ProxyOpenAiRequestAsync(HttpContext context, string dynamicId)
    {
        var targetBaseUrl = Environment.GetEnvironmentVariable("TARGET_BASE_URL") 
            ?? _configuration["TargetBaseUrl"] 
            ?? "http://www.nisedt.cn:33322";
        
        var targetUrl = $"{targetBaseUrl}/api/v1/chats_openai/{dynamicId}/chat/completions";
        
        try
        {
            using var request = CreateProxyRequest(context, targetUrl);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
            
            await CopyProxyResponse(context, response);
        }
        catch (TaskCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // 客户端取消请求，正常处理
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Proxy error: {ex.Message}");
        }
    }

    private HttpRequestMessage CreateProxyRequest(HttpContext context, string targetUrl)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(targetUrl),
            Method = new HttpMethod(context.Request.Method)
        };

        // 复制请求体（仅 POST/PUT 等方法）
        if (context.Request.ContentLength > 0 && 
            (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH"))
        {
            request.Content = new StreamContent(context.Request.Body);
            
            // 设置 Content-Type
            if (!string.IsNullOrEmpty(context.Request.ContentType))
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType);
            }
        }

        // 复制重要的请求头
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
        }

        return request;
    }

    private async Task CopyProxyResponse(HttpContext context, HttpResponseMessage response)
    {
        context.Response.StatusCode = (int)response.StatusCode;

        // 复制响应头
        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in response.Content.Headers)
        {
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
        }

        // 移除可能影响流式传输的头
        context.Response.Headers.Remove("Transfer-Encoding");

        // 流式传输响应
        using var responseStream = await response.Content.ReadAsStreamAsync();
        await responseStream.CopyToAsync(context.Response.Body, context.RequestAborted);
    }
}
