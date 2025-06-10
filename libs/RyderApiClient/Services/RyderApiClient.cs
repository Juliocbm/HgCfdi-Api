using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ryder.Api.Client.Configuration;
using Ryder.Api.Client.Exceptions;
using Ryder.Api.Client.Models.Requests;
using Ryder.Api.Client.Models.Requests.Ryder.Api.Client.Models.Requests;
using Ryder.Api.Client.Models.Responses;

namespace Ryder.Api.Client.Services
{
    public class RyderApiClient : IRyderApiClient
    {
        private readonly HttpClient _http;
        private readonly RyderApiOptions _opts;
        private readonly JsonSerializerOptions _jsonOptions;

        public RyderApiClient(HttpClient http, IOptions<RyderApiOptions> opt)
        {
            _http = http;
            _opts = opt.Value;
            _http.BaseAddress = new Uri(_opts.BaseUrl);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private async Task<TResponse> SendAsync<TRequest, TResponse>(
            string path,
            TRequest req,
            CancellationToken ct)
            where TRequest : BaseRequest
        {
            // Inyectar credenciales en el body
            req.Email = _opts.Email;
            req.AccessKey = _opts.AccessKey;

            // Enviar
            var resp = await _http.PostAsJsonAsync(path, req, _jsonOptions, ct);
            var content = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new RyderApiException(
                    $"Error al llamar {path}: {(int)resp.StatusCode} {resp.ReasonPhrase}",
                    resp.StatusCode,
                    content);

            // Deserializar
            return JsonSerializer
                .Deserialize<TResponse>(content, _jsonOptions)!
                ;
        }

        public Task<BaseResponse> GetDatosCartaPorteAsync(
            GetDatosCartaPorteRequest request,
            CancellationToken ct = default)
            => SendAsync<GetDatosCartaPorteRequest, BaseResponse>(
                "cfdi/v1/cfdi/api/GetDatosCartaPorte",
                request,
                ct);

        public Task<BaseResponse> GetCartaPorteAsync(
            GetCartaPorteRequest request,
            CancellationToken ct = default)
            => SendAsync<GetCartaPorteRequest, BaseResponse>(
                "cfdi/v1/cfdi/api/GetCartaPorte",
                request,
                ct);

        public Task<BaseResponse> GetViajesAsync(
            GetViajesRequest request,
            CancellationToken ct = default)
            => SendAsync<GetViajesRequest, BaseResponse>(
                "cfdi/v1/cfdi/api/GetViajes",
                request,
                ct);

        public Task<BaseResponse> UploadIngresoAsync(
            UploadIngresoRequest request,
            CancellationToken ct = default)
            => SendAsync<UploadIngresoRequest, BaseResponse>(
                "cfdi/v1/cfdi/api/UploadIngreso",
                request,
                ct);

        // Si luego agregas UploadIngresoCruce...
        public Task<BaseResponse> UploadIngresoCruceAsync(
            UploadIngresoCruceRequest request,
            CancellationToken ct = default)
            => SendAsync<UploadIngresoCruceRequest, BaseResponse>(
                "cfdi/v1/cfdi/api/UploadIngresoCruce",
                request,
                ct);
    }
}
