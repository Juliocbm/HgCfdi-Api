using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using HG.CFDI.CORE.Models.LisApi.ModelResponseLis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace HG.CFDI.DATA.LisApi
{
    public class CartaPorteServiceApi:ICartaPorteServiceApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrlZam;
        private readonly string _endpoint;
        private readonly ILogger<CartaPorteServiceApi> _logger;


        public CartaPorteServiceApi(HttpClient httpClient, IConfiguration configuration, ILogger<CartaPorteServiceApi> logger)
        {
            _httpClient = httpClient;
            _baseUrlZam = configuration.GetValue<string>("LisApi:BaseUrl");
            _endpoint = configuration.GetValue<string>("LisApi:EndpointCartaPorte");
            _logger = logger;
        }

        public async Task<Response> SendCartaPorteAsync(string bearerToken, FacturaCartaPorte cartaPorte)
        {
            try
            {
                // Serializa el objeto cartaPorte a JSON, omitiendo los valores nulos
                var requestJson = JsonConvert.SerializeObject(cartaPorte, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                _logger.LogInformation(requestJson);

                //var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                //var uniqueFileName = $"requestJson{cartaPorte.Serie}-{cartaPorte.Folio}-{timestamp}.txt";
                //var filePath = Path.Combine(AppContext.BaseDirectory, uniqueFileName);
                //await File.WriteAllTextAsync(filePath, requestJson);

                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(5)); // Establece un tiempo de espera de 5 minutos

                var request = new HttpRequestMessage(HttpMethod.Post, _baseUrlZam + _endpoint)
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };

                // Agrega el token Bearer al encabezado de autorización
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

               

                //var response = await _httpClient.SendAsync(request);
                var response = await _httpClient.SendAsync(request, cancellationTokenSource.Token);

                Response respuesta = new Response();
                if (response.IsSuccessStatusCode)
                {           
                    // Deserializa la respuesta JSON a la clase RespuestaCartaPorte
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    respuesta = JsonConvert.DeserializeObject<Response>(jsonResponse);
                    var responseJson = JsonConvert.SerializeObject(respuesta, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    _logger.LogInformation(responseJson);

                    return respuesta;
                }
                else
                {
                    // Manejo de diferentes códigos de estado
                    var statusCode = (int)response.StatusCode;
                    switch (statusCode)
                    {
                        case 413:
                            // Payload Too Large
                            _logger.LogInformation($"The request entity is too large, status code: {statusCode}");
                            throw new HttpRequestException($"The request entity is too large, status code: {statusCode}");
                        case 400:
                            // Bad Request
                            _logger.LogInformation($"Bad request. Please check the request data, status code: {statusCode}");
                            throw new HttpRequestException($"Bad request. Please check the request data, status code: {statusCode}");
                        case 401:
                            // Unauthorized
                            _logger.LogInformation($"Unauthorized. Please check your credentials, status code: {statusCode}");
                            throw new HttpRequestException($"Unauthorized. Please check your credentials, status code: {statusCode}");
                        case 403:
                            // Forbidden
                            _logger.LogInformation($"Forbidden. You do not have permission to access this resource, status code: {statusCode}");
                            throw new HttpRequestException($"Forbidden. You do not have permission to access this resource, status code: {statusCode}");
                        case 404:
                            // Not Found
                            _logger.LogInformation($"Not Found. The requested resource could not be found, status code: {statusCode}");
                            throw new HttpRequestException($"Not Found. The requested resource could not be found, status code: {statusCode}");
                        default:
                            _logger.LogInformation($"Unexpected status code: {statusCode}");
                            throw new HttpRequestException($"Unexpected status code: {statusCode}");
                    }
                }               
            }
            catch (TaskCanceledException ex)
            {
                // Si se cancela la tarea debido al tiempo de espera, puedes manejarlo aquí
                Console.WriteLine($"La solicitud se canceló debido al tiempo de espera: {ex.Message}");
                return new Response() { Mensajes = new List<Mensaje>() { new Mensaje() { Descripcion = "La solicitud se canceló debido al tiempo de espera." } } };
            }
            catch (Exception err)
            {
                Console.WriteLine($"Error al enviar la carta porte, error: {err}");
                return new Response() { Mensajes = new List<Mensaje>() { new Mensaje() { Descripcion = err.Message } } }; 
            }
        }

    }

}
