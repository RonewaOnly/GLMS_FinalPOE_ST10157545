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
    public class CurrencySerivceTests
    {
        public class CurrencyServiceTests
        {
            private readonly GLMS.Web.Services.CurrencyService _svc;
            public CurrencyServiceTests()
            {
                var httpClient = new System.Net.Http.HttpClient();
                var logger = Mock.Of<ILogger<CurrencyService>>();
                _svc = new CurrencyService(httpClient, logger);
            }
            [Fact]
            public void ConvertUsdToZar_CorrectMath_ReturnsExpectedAmount()
            {
                // Arrange
                decimal usd = 100m;
                decimal rate = 18.50m;
                // Act
                decimal result = _svc.ConvertUsdToZar(usd, rate);
                // Assert
                Assert.Equal(1850.00m, result);
            }
            [Fact]
            public void ConvertUsdToZar_SmallAmount_RoundsToTwoDecimals()
            {
                decimal result = _svc.ConvertUsdToZar(1m, 18.756m);
                Assert.Equal(18.76m, result); // rounds half-up
            }
            [Fact]
            public void ConvertUsdToZar_ZeroUsd_ReturnsZero()
            {
                decimal result = _svc.ConvertUsdToZar(0m, 18.50m);
                Assert.Equal(0.00m, result);
            }
            [Fact]
            public void ConvertUsdToZar_LargeAmount_ReturnsCorrectResult()
            {
                decimal result = _svc.ConvertUsdToZar(50_000m, 19.25m);
                Assert.Equal(962_500.00m, result);
            }
            [Theory]
            [InlineData(10, 18.50, 185.00)]
            [InlineData(250, 17.80, 4450.00)]
            [InlineData(1000, 20.00, 20000.00)]
            [InlineData(0.01, 18.50, 0.19)]  // rounds 0.1850 → 0.19
            public void ConvertUsdToZar_TheoryData_MatchesExpected(
                double usd, double rate, double expected)
            {
                decimal result = _svc.ConvertUsdToZar((decimal)usd, (decimal)rate);
                Assert.Equal((decimal)expected, result);
            }
            [Fact]
            public void ConvertUsdToZar_ZeroRate_ThrowsArgumentException()
            {
                Assert.Throws<ArgumentException>(() =>
                    _svc.ConvertUsdToZar(100m, 0m));
            }
            [Fact]
            public void ConvertUsdToZar_NegativeRate_ThrowsArgumentException()
            {
                Assert.Throws<ArgumentException>(() =>
                    _svc.ConvertUsdToZar(100m, -1m));
            }
            [Fact]
            public void ConvertUsdToZar_NegativeUsd_ThrowsArgumentException()
            {
                Assert.Throws<ArgumentException>(() =>
                    _svc.ConvertUsdToZar(-50m, 18.50m));
            }
        }
    }
}
