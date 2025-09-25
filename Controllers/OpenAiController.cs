// Controllers/OpenAiController.cs
using Microsoft.AspNetCore.Mvc;
using OpenAiProxy.Services;

namespace OpenAiProxy.Controllers;

[ApiController]
[Route("{dynamicId}/v1/chat/completions")]
public class OpenAiController : ControllerBase
{
    private readonly OpenAiProxyService _proxyService;

    public OpenAiController(OpenAiProxyService proxyService)
    {
        _proxyService = proxyService;
    }

    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> ProxyAsync(string dynamicId)
    {
        await _proxyService.ProxyOpenAiRequestAsync(HttpContext, dynamicId);
        return new EmptyResult();
    }
}