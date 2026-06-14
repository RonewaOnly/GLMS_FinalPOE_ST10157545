namespace GLMS.Web.Models
{
    public class CurrencyRateResponse { public decimal Rate { get; set; } public string Base { get; set; } = "USD"; public string Target { get; set; } = "ZAR"; public DateTime FetchedAt { get; set; } }

}
