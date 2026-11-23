using System.Text;
using Microsoft.Extensions.Logging;
using NexusFlow.Application.Common.Interfaces;

namespace NexusFlow.Infrastructure.Services;

public class WebhookDispatcher : IWebhookDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookDispatcher> _logger;

    public WebhookDispatcher(HttpClient httpClient, ILogger<WebhookDispatcher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> DispatchAsync(string targetUrl, string payloadJson, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Dispatching resilient webhook to: {Url}", targetUrl);
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(targetUrl, content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook successfully delivered to: {Url} (Status: {StatusCode})", targetUrl, response.StatusCode);
                return true;
            }

            _logger.LogWarning("Webhook delivery failed with HTTP {StatusCode} to: {Url}", response.StatusCode, targetUrl);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception encountered during webhook dispatch to: {Url}", targetUrl);
            return false;
        }
    }
}
