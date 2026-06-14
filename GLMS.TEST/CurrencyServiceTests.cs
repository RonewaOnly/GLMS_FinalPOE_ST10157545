using GLMS.Web.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace GLMS.TEST
{
    public class CurrencyServiceTests
    {
        private readonly CurrencyService _svc = new(
            new System.Net.Http.HttpClient(),
            Mock.Of<ILogger<CurrencyService>>());


        [Fact] public void CorrectMath_100Usd_At18_50_Returns1850() => Assert.Equal(1850.00m, _svc.ConvertUsdToZar(100m, 18.50m));
        [Fact] public void RoundsToTwoDecimals() => Assert.Equal(18.76m, _svc.ConvertUsdToZar(1m, 18.756m));
        [Fact] public void ZeroAmount_ReturnsZero() => Assert.Equal(0.00m, _svc.ConvertUsdToZar(0m, 18.50m));
        [Fact] public void ZeroRate_Throws() => Assert.Throws<ArgumentException>(() => _svc.ConvertUsdToZar(100m, 0m));
        [Fact] public void NegativeRate_Throws() => Assert.Throws<ArgumentException>(() => _svc.ConvertUsdToZar(100m, -5m));
        [Fact] public void NegativeUsd_Throws() => Assert.Throws<ArgumentException>(() => _svc.ConvertUsdToZar(-50m, 18.50m));
        [Fact] public void LargeAmount_CorrectResult() => Assert.Equal(962500.00m, _svc.ConvertUsdToZar(50000m, 19.25m));


        [Theory]
        [InlineData(10, 18.50, 185.00)]
        [InlineData(250, 17.80, 4450.00)]
        [InlineData(1000, 20.00, 20000.00)]
        [InlineData(0.01, 18.50, 0.19)]
        public void TheoryData_MatchesExpected(double usd, double rate, double expected)
            => Assert.Equal((decimal)expected, _svc.ConvertUsdToZar((decimal)usd, (decimal)rate));
    }
}
