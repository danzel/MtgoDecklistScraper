namespace MtgoDecklistScraperNet.Services;

public class MtgoClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.mtgo.com";

    public MtgoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> FetchIndexAsync(int? year = null, int? month = null, CancellationToken ct = default)
    {
        string url;
        if (year.HasValue && month.HasValue)
        {
            url = $"{BaseUrl}/decklists/{year.Value}/{month.Value:D2}";
        }
        else
        {
            url = $"{BaseUrl}/decklists";
        }

        return await _httpClient.GetStringAsync(url, ct);
    }

    public async Task<string> FetchEventPageAsync(string relativeUrl, CancellationToken ct = default)
    {
        var url = BaseUrl + relativeUrl;
        return await _httpClient.GetStringAsync(url, ct);
    }
}
