using System.Text.Json;

namespace GLMS.API.Services
{
        public interface ICurrencyService
        {
            Task<decimal> GetUsdToZarRateAsync();
            decimal ConvertUsdToZar(decimal usdAmount, decimal rate);
        }

        public class CurrencyService : ICurrencyService
        {
            private readonly HttpClient _http;
            private readonly ILogger<CurrencyService> _logger;
            private decimal _cachedRate = 0;
            private DateTime _cacheExpiry = DateTime.MinValue;
            private const decimal FallbackRate = 18.50m;
            private const int CacheMinutes = 60;

            public CurrencyService(HttpClient http, ILogger<CurrencyService> logger) { _http = http; _logger = logger; }

            public async Task<decimal> GetUsdToZarRateAsync()
            {
                if (_cachedRate > 0 && DateTime.UtcNow < _cacheExpiry) return _cachedRate;
                try
                {
                    var res = await _http.GetAsync("https://open.er-api.com/v6/latest/USD");
                    res.EnsureSuccessStatusCode();
                    var json = await res.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<ErApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (data?.Rates != null && data.Rates.TryGetValue("ZAR", out decimal r) && r > 0)
                    { _cachedRate = r; _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheMinutes); return r; }
                }
                catch (Exception ex) { _logger.LogWarning(ex, "Currency API failed, using fallback {Rate}", FallbackRate); }
                return FallbackRate;
            }

            public decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
            {
                if (rate <= 0) throw new ArgumentException("Rate must be > 0", nameof(rate));
                if (usdAmount < 0) throw new ArgumentException("Amount cannot be < 0", nameof(usdAmount));
                return Math.Round(usdAmount * rate, 2);
            }

            private class ErApiResponse { public Dictionary<string, decimal>? Rates { get; set; } }
        }
    

}
