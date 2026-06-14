using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GLMS.Web.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
        public ApiClient(HttpClient http, IHttpContextAccessor ctx) { _http = http; _ctx = ctx; }
        private void AttachToken()
        {
            var token = _ctx.HttpContext?.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        public async Task<T?> GetAsync<T>(string path)
        {
            AttachToken();
            var res = await _http.GetAsync(path);
            if (!res.IsSuccessStatusCode) return default;
            return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(), _json);
        }
        public async Task<(bool ok, T? data, string? error)> PostAsync<T>(string path, object body)
        {
            AttachToken();
            var res = await _http.PostAsync(path, new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            if (res.IsSuccessStatusCode)
                return (true, JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(), _json), null);
            return (false, default, await res.Content.ReadAsStringAsync());
        }
        public async Task<(bool ok, string? error)> PatchAsync(string path, object body)
        {
            AttachToken();
            var req = new HttpRequestMessage(HttpMethod.Patch, path) { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode ? (true, null) : (false, await res.Content.ReadAsStringAsync());
        }
        public async Task<(bool ok, string? error)> PutAsync(string path, object body)
        {
            AttachToken();
            var res = await _http.PutAsync(path, new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            return res.IsSuccessStatusCode ? (true, null) : (false, await res.Content.ReadAsStringAsync());
        }
        public async Task<bool> DeleteAsync(string path) { AttachToken(); return (await _http.DeleteAsync(path)).IsSuccessStatusCode; }
        public async Task<(bool ok, string? error)> UploadFileAsync(string path, IFormFile file)
        {
            AttachToken();
            await using var stream = file.OpenReadStream();
            using var form = new MultipartFormDataContent();
            using var fc = new StreamContent(stream);
            fc.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fc, "file", file.FileName);
            var res = await _http.PostAsync(path, form);
            return res.IsSuccessStatusCode ? (true, null) : (false, await res.Content.ReadAsStringAsync());
        }
        public async Task<(Stream? stream, string? contentType, string? fileName)> DownloadFileAsync(string path)
        {
            AttachToken();
            var res = await _http.GetAsync(path);
            if (!res.IsSuccessStatusCode) return (null, null, null);
            return (await res.Content.ReadAsStreamAsync(),
                    res.Content.Headers.ContentType?.MediaType ?? "application/pdf",
                    res.Content.Headers.ContentDisposition?.FileNameStar ?? res.Content.Headers.ContentDisposition?.FileName ?? "agreement.pdf");
        }
    }
}
