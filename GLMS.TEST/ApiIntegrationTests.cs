using GLMS.API.Models.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GLMS.TEST
{
    public class ApiIntegrationTests : IClassFixture<GlmsApiFactory>
    {
        private readonly HttpClient _client;
        private string? _token;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };


        public ApiIntegrationTests(GlmsApiFactory factory)
        {
            _client = factory.CreateClient();
        }


        private async Task AuthAsync()
        {
            if (_token != null) return;
            var res = await _client.PostAsJsonAsync("/api/auth/login", new { Username = "admin", Password = "Admin@1234" });
            var body = await res.Content.ReadFromJsonAsync<LoginResponse>(_json);
            _token = body?.Token;
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        }


        // ── Auth ──────────────────────────────────────────────────────────────────
        [Fact]
        public async Task Login_ValidCredentials_Returns200WithToken()
        {
            var res = await _client.PostAsJsonAsync("/api/auth/login", new { Username = "admin", Password = "Admin@1234" });
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var body = await res.Content.ReadFromJsonAsync<LoginResponse>(_json);
            Assert.False(string.IsNullOrWhiteSpace(body?.Token));
        }


        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            var res = await _client.PostAsJsonAsync("/api/auth/login", new { Username = "admin", Password = "wrong" });
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }


        [Fact]
        public async Task ProtectedEndpoint_NoToken_Returns401()
        {
            var fresh = new GlmsApiFactory().CreateClient();
            var res = await fresh.PostAsJsonAsync("/api/clients", new { Name = "X", ContractDetails = "y", Region = "ZA" });
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }


        // ── Dashboard ─────────────────────────────────────────────────────────────
        [Fact]
        public async Task GetDashboard_Returns200AndNonNullBody()
        {
            var res = await _client.GetAsync("/api/dashboard");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var body = await res.Content.ReadFromJsonAsync<DashboardDto>(_json);
            Assert.NotNull(body);
        }


        // ── Clients ───────────────────────────────────────────────────────────────
        [Fact]
        public async Task GetClients_Returns200AndArray()
        {
            var res = await _client.GetAsync("/api/clients");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var body = await res.Content.ReadFromJsonAsync<List<ClientDto>>(_json);
            Assert.NotNull(body);
        }


        [Fact]
        public async Task CreateClient_Returns201WithLocationHeader()
        {
            await AuthAsync();
            var res = await _client.PostAsJsonAsync("/api/clients",
                new { Name = "Integration Test Client", ContractDetails = "Test", Region = "ZA" });
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
            Assert.NotNull(res.Headers.Location);
        }


        // ── Contracts ─────────────────────────────────────────────────────────────
        [Fact]
        public async Task GetContracts_Returns200AndArray()
        {
            var res = await _client.GetAsync("/api/contracts");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }


        [Fact]
        public async Task GetContracts_FilterByActive_ReturnsOnlyActive()
        {
            var res = await _client.GetAsync("/api/contracts?status=Active");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var list = await res.Content.ReadFromJsonAsync<List<ContractDto>>(_json);
            Assert.NotNull(list);
            Assert.All(list!, c => Assert.Equal("Active", c.Status));
        }


        [Fact]
        public async Task CreateContract_ValidData_Returns201()
        {
            await AuthAsync();
            var cRes = await _client.PostAsJsonAsync("/api/clients", new { Name = "Ct Client", ContractDetails = "d", Region = "ZA" });
            var c = await cRes.Content.ReadFromJsonAsync<ClientDto>(_json);
            var res = await _client.PostAsJsonAsync("/api/contracts", new
            {
                ClientId = c!.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Status = "Active",
                ServiceLevel = "Integration"
            });
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }


        [Fact]
        public async Task PatchContractStatus_Returns200WithUpdatedStatus()
        {
            await AuthAsync();
            var cRes = await _client.PostAsJsonAsync("/api/clients", new { Name = "Patch Client", ContractDetails = "d", Region = "ZA" });
            var c = await cRes.Content.ReadFromJsonAsync<ClientDto>(_json);
            var ctRes = await _client.PostAsJsonAsync("/api/contracts", new { ClientId = c!.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1), Status = "Draft", ServiceLevel = "Patch SLA" });
            var ct = await ctRes.Content.ReadFromJsonAsync<ContractDto>(_json);
            var req = new HttpRequestMessage(HttpMethod.Patch, $"/api/contracts/{ct!.Id}/status") { Content = JsonContent.Create(new { Status = "Active" }) };
            var res = await _client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var updated = await res.Content.ReadFromJsonAsync<ContractDto>(_json);
            Assert.Equal("Active", updated?.Status);
        }


        // ── Service Requests ──────────────────────────────────────────────────────
        [Fact]
        public async Task GetServiceRequests_Returns200()
        {
            var res = await _client.GetAsync("/api/servicerequests");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }


        [Fact]
        public async Task CreateSR_ExpiredContract_Returns400()
        {
            await AuthAsync();
            var cRes = await _client.PostAsJsonAsync("/api/clients", new { Name = "Exp Client", ContractDetails = "d", Region = "ZA" });
            var c = await cRes.Content.ReadFromJsonAsync<ClientDto>(_json);
            var ctRes = await _client.PostAsJsonAsync("/api/contracts", new
            {
                ClientId = c!.Id,
                StartDate = DateTime.Today.AddYears(-2),
                EndDate = DateTime.Today.AddYears(-1),
                Status = "Expired",
                ServiceLevel = "Expired SLA"
            });
            var ct = await ctRes.Content.ReadFromJsonAsync<ContractDto>(_json);
            var res = await _client.PostAsJsonAsync("/api/servicerequests", new { ContractId = ct!.Id, Description = "Should fail", CostUsd = 100m });
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }


        [Fact]
        public async Task CreateSR_ActiveContract_Returns201()
        {
            await AuthAsync();
            var cRes = await _client.PostAsJsonAsync("/api/clients", new { Name = "SR Active Client", ContractDetails = "d", Region = "ZA" });
            var c = await cRes.Content.ReadFromJsonAsync<ClientDto>(_json);
            var ctRes = await _client.PostAsJsonAsync("/api/contracts", new
            {
                ClientId = c!.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Status = "Active",
                ServiceLevel = "Active SLA"
            });
            var ct = await ctRes.Content.ReadFromJsonAsync<ContractDto>(_json);
            var res = await _client.PostAsJsonAsync("/api/servicerequests", new { ContractId = ct!.Id, Description = "Valid SR", CostUsd = 500m });
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
            var sr = await res.Content.ReadFromJsonAsync<ServiceRequestDto>(_json);
            Assert.True(sr?.CostZar > 0);
            Assert.True(sr?.ExchangeRateUsed > 0);
        }


        // Currency
        [Fact]
        public async Task GetCurrencyRate_Returns200AndPositiveRate()
        {
            var res = await _client.GetAsync("/api/currency/rate");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var body = await res.Content.ReadFromJsonAsync<CurrencyRateResponse>(_json);
            Assert.True(body?.Rate > 0);
        }


        // Swagger
        [Fact]
        public async Task SwaggerJson_Returns200()
        {
            var res = await _client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }
    }


}
