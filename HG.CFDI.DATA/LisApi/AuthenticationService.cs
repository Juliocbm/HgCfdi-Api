using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using HG.CFDI.CORE.Models.LisApi.ModelResponseLis;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis;

namespace HG.CFDI.DATA.LisApi
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint = "https://hg2.midireccion.com/ZamApi/api/security/authenticate";

        public AuthenticationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(string usuario, string password)
        {
            var authRequest = new AuthenticationRequest
            {
                Usuario = usuario,
                Password = password
            };


            var requestContent = new StringContent(JsonConvert.SerializeObject(authRequest), Encoding.UTF8, "application/json"); // Asegúrate de que el tipo MIME sea correcto

            var response = await _httpClient.PostAsync(_endpoint, requestContent);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            }

            return null;
        }
    }

}
