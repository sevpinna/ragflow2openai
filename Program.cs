// Program.cs
using OpenAiProxy.Services;

var builder = WebApplication.CreateBuilder(args);
// 输出配置信息
var targetBaseUrl = Environment.GetEnvironmentVariable("TARGET_BASE_URL")
                    ?? builder.Configuration["TargetBaseUrl"];
// 添加服务
builder.Services.AddHttpClient();
builder.Services.AddSingleton<OpenAiProxyService>();
builder.Services.AddControllers();

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.MapControllers();



Console.WriteLine($"OpenAI 代理服务启动");
Console.WriteLine($"目标地址: {targetBaseUrl}");
Console.WriteLine($"代理路径: /{{dynamicId}}/v1/chat/completions");

app.Run();