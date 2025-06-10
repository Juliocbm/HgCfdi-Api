using CFDI.Data.Entities;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Models;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using HG.CFDI.CORE.Models.LisApi.ModelResponseLis;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ICartaPorteService
    {
        public Task<List<cartaPorteCabecera>> getCartasPortePendiente(string compañia);
        public Task<GeneralResponse<cartaPorteCabecera>> getCartaPorte(string database, string guia);
        public Task<bool> putCartaPorte(string database, cartaPorteCabecera cp);
        public Task<GeneralResponse<string>> changeStatusCartaPorteAsync(int no_guia, string num_guia, string compania, int EstatusTimbrado, string mensajeTimbrado, int sistemaTimbrado);
        public Task fechaSolicitudTimbradoAsync(int no_guia, string compania);
        public Task<GeneralResponse<string>> insertError(int no_guia, string num_guia, string compania, string error, int? idOperador_Lis, string? idUnidad_Lis, string? idRemolque_Lis);
        public Task<UniqueResponse> timbrarConLis(ICartaPorteServiceApi cartaPorteServiceApi, cartaPorteCabecera cartaPorte);
        public Task<UniqueResponse> timbrarConInvoiceOne(cartaPorteCabecera ccps, string database);
        public Task<UniqueResponse> timbrarConBuzonE(cartaPorteCabecera cartaPorte, string database);
        public Task deleteErrors(int no_guia, string compania);
    }
}
