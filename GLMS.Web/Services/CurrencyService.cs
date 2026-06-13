using System.Text.Json;

namespace GLMS.Web.Services
{
    //  Interface(enables unit-test mocking)
    public interface ICurrencyService
    {

        Task<decimal> GetUsdToZarRateAsync();
        decimal ConvertUsdToZar(decimal usdAmount, decimal rate);
    }

    //  Production implementation using ExchangeRate-API(free tier) 
    // Free endpoint: https://open.er-api.com/v6/latest/USD
    // No API key required for the open endpoint — safe for school subscriptions.
    public class CurrencyService : ICurrencyService
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        // Simple in-memory cache — refreshes every 60 minutes to respect free-tier limits
        private decimal _cachedRate = 0;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private const int CacheMinutes = 60;
        // Fallback rate used when the API is unreachable (keeps the app functional offline)
        private const decimal FallbackRate = 18.50m;
        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<decimal> GetUsdToZarRateAsync()
        {
            // Return cached value if still fresh
            if (_cachedRate > 0 && DateTime.UtcNow < _cacheExpiry)
                return _cachedRate;
            try
            {
                var response = await _httpClient
                    .GetAsync("https://open.er-api.com/v6/latest/USD");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<OpenErApiResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data?.Rates != null && data.Rates.TryGetValue("ZAR", out decimal zarRate) && zarRate > 0)
                {
                    _cachedRate = zarRate;
                    _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheMinutes);
                    _logger.LogInformation("USD/ZAR rate refreshed: {Rate}", zarRate);
                    return zarRate;
                }
                _logger.LogWarning("ZAR not found in API response. Using fallback rate.");
                return FallbackRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency API call failed. Using fallback rate {Rate}.", FallbackRate);
                return FallbackRate;
            }
        }
        /// <summary>
        /// Pure calculation — no I/O, directly unit-testable.
        /// </summary>
        public decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
        {
            if (rate <= 0)
                throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));
            if (usdAmount < 0)
                throw new ArgumentException("USD amount cannot be negative.", nameof(usdAmount));
            return Math.Round(usdAmount * rate, 2);
        }
        //  Private response shape matching open.er-api.com 
        private class OpenErApiResponse
        {
            public string? Base_Code { get; set; }
            public Dictionary<string, decimal>? Rates { get; set; }
        }
    }
}
