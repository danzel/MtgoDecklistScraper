using Microsoft.Extensions.Logging;

namespace MtgoDecklistScraperNet.Services;

public class RetryHandler : DelegatingHandler
{
	private const int MaxRetries = 30;
	private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(5);

	private readonly ILogger<RetryHandler> _logger;

	public RetryHandler(ILogger<RetryHandler> logger, HttpMessageHandler innerHandler)
		: base(innerHandler)
	{
		_logger = logger;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var delay = DefaultRetryDelay;
		for (var attempt = 0; ; attempt++)
		{
			try
			{
				using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1));
				using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
				var response = await base.SendAsync(request, linked.Token);
				if (!response.IsSuccessStatusCode && attempt < MaxRetries)
				{
					_logger.LogWarning("Request to {Url} failed with status {Status}, retrying in {Delay}s ({Attempt}/{Max})",
						request.RequestUri, (int)response.StatusCode, delay.TotalSeconds, attempt + 1, MaxRetries);
					response.Dispose();
					await Task.Delay(delay, cancellationToken);
					continue;
				}
				return response;
			}
			catch (Exception ex) when (!cancellationToken.IsCancellationRequested && attempt < MaxRetries)
			{
				_logger.LogWarning(ex, "Request to {Url} failed, retrying in {Delay}s ({Attempt}/{Max})",
					request.RequestUri, delay.TotalSeconds, attempt + 1, MaxRetries);
				await Task.Delay(delay, cancellationToken);
			}

			//Backoff
			delay *= 2;
			if (delay > TimeSpan.FromMinutes(5))
				delay = TimeSpan.FromMinutes(5);
		}
	}
}
