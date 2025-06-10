
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using HG.CFDI.CORE.Models.LisApi.ModelResponseLis;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ICartaPorteServiceApi
    {
        public Task<Response> SendCartaPorteAsync(string bearerToken, FacturaCartaPorte cartaPorte);
    }
}
