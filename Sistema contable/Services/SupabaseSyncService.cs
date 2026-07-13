using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sistema_contable.Services
{
    public class SupabaseSyncService
    {
        private static readonly HttpClient _http = new HttpClient();
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public SupabaseSyncService()
        {
            _baseUrl = ConfiguracionApp.SupabaseUrl.TrimEnd('/') + "/rest/v1";
            _apiKey = ConfiguracionApp.SupabaseAnonKey;
        }

        public async Task<bool> UpsertAsync(string tabla, object payload)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_baseUrl))
                return false;

            try
            {
                var json = JsonSerializer.Serialize(payload);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{tabla}?on_conflict=sync_id")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("apikey", _apiKey);
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Headers.Add("Prefer", "resolution=merge-duplicates,return=minimal");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                var response = await _http.SendAsync(request, cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // Sin internet, timeout, servidor caído, etc. — se maneja en la cola.
                return false;
            }
        }
    }
}