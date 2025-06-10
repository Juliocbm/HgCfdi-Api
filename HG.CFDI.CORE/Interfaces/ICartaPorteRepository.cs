using CFDI.Data.Entities;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Models;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ICartaPorteRepository
    {
        public Task<List<cartaPorteCabecera>> getCartasPortePendiente(string compañia);
        public Task<GeneralResponse<cartaPorteCabecera>> getCartaPorte(string database, string guia);
        public Task<bool> putCartaPorte(string database, cartaPorteCabecera cp);
        public Task<ResponseImportacion> reinsertaCartaPorteRepositorio(string database, string guia);
        public Task changeStatusCartaPorteAsync(int no_guia, string compania, int EstatusTimbrado, string mensajeTimbrado, int sistemaTimbrado);
        public Task fechaSolicitudTimbradoAsync(int no_guia, string compania);
        public Task<bool> TrySetTimbradoEnProcesoAsync(int no_guia, string compania);
        public Task insertError(int no_guia, string num_guia, string compania, string error, int? idOperador_Lis, string? idUnidad_Lis, string? idRemolque_Lis);
        public Task<bool> InsertDocumentosTimbrados(archivoCFDi archivos, string server);
        public Task patchPdfAsync(int no_guia, string compania, byte[] pdf);
        public Task actualizaEstatusEnvioRyderAsync(int id, bool estatus);
        public Task deleteErrors(int no_guia, string compania);
        public Task<archivoCFDi> getArchivosTimbrado(int no_guia, string compania);
        public Task<bool> trasladaUuidToTrucks(archivoCFDi archivos);
    }
}
