using Azure.Core;
using CFDI.Data.Entities;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Models;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using HG.CFDI.DATA.Repositories;
using Interceptor.AOP.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface IDocumentosService
    {
        Task<string> getUuidFromXml(string xmlComprobante);
        Task<bool> SaveFile(byte[] data, string compania, string filename);
        Task patchPdfFromXml(cartaPorteCabecera cp, byte[] pdfBytes);

        [Audit("Obtener pdf from XML", LogInput = true, LogOutput = false, LogError = true)]
        [Retry(2)]
        [Fallback("FallbackGetPdfTimbrado")]
        [MeasureTime]
        Task<byte[]> getPdfTimbrado(string xmlCFDTimbrado, string database);
        Task<byte[]> FallbackGetPdfTimbrado(string xmlCFDTimbrado, string database);     
        Task<byte[]> GetPdfNominaTimbrado(string xmlCFDTimbrado, string database);

        Task GuardarArchivosEnServidor(cartaPorteCabecera cartaPorte, byte[] xmlBytes, byte[] pdfBytes);
        archivoCFDi CreateArchivoCFDi(cartaPorteCabecera cartaPorte, byte[] xml, byte[] pdf, string uuid);
        Task GuardarDocumentosTimbrados(cartaPorteCabecera cartaPorte, archivoCFDi archivo);
        string GetPdfPathForCompany(string companyId);
    }
}
