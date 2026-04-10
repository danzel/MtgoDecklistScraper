using Microsoft.Extensions.Logging;

namespace MtgoDecklistScraperNet.Services;

public class RetryHandler : DelegatingHandler
{
    private const int MaxRetries = 2;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly ILogger<RetryHandler> _logger;

    public RetryHandler(ILogger<RetryHandler> logger, HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode && attempt < MaxRetries)
                {
                    _logger.LogWarning("Request to {Url} failed with status {Status}, retrying in {Delay}s ({Attempt}/{Max})",
                        request.RequestUri, (int)response.StatusCode, RetryDelay.TotalSeconds, attempt + 1, MaxRetries);
                    response.Dispose();
                    await Task.Delay(RetryDelay, cancellationToken);
                    continue;
                }
                return response;
            }
            catch (Exception ex) when (ex is not OperationCanceledException && attempt < MaxRetries)
            {
                _logger.LogWarning(ex, "Request to {Url} failed, retrying in {Delay}s ({Attempt}/{Max})",
                    request.RequestUri, RetryDelay.TotalSeconds, attempt + 1, MaxRetries);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }
}
