using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Models
{
    public class FirmaDigitalOptions
    {
        public string Empresa { get; set; }
        public string NombreCarpeta { get; set; }
        public string NumeroDeCertificado { get; set; }
        public string PassCertificate { get; set; }
        public string PassPrivateKey { get; set; }
        public string NombreArchivoCadenaOriginal { get; set; }
        public string NombreArchivoCertificado { get; set; }
        public string NombreArchivoPrivateKey { get; set; }


    }

    public class InvoiceOneApiOptions
    {
        public string User { get; set; }
        public string Password { get; set; }
    }


    public class BuzonEApiCredential
    {
        public string compania { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string FileType { get; set; }
    }

    public class LisApiOptions
    {
        public string BaseUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string EndpointLiquidaciones { get; set; }
        public string EndpointCartaPorte { get; set; }
    }

    public class RyderApiOptions
    {
        public bool ActivarUploadIngreso { get; set; }
        public int IdClienteForUploadIngreso { get; set; }
        public string BaseUrl { get; set; }
        public string Email { get; set; }
        public string AccessKey { get; set; }
        public string EndpointUploadIngreso { get; set; }
        public string EndpointConsultaDatosCartaPorte { get; set; }
    }

    public class compania
    {
        public string id { get; set; }
        public string Database { get; set; }
        public bool Timbrar { get; set; }
        public string XmlPath { get; set; }
        public string PdfPath { get; set; }
    }


}
