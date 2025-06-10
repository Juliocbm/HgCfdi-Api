using CFDI.BuildPdf.Service;
using CFDI.Data.Entities;
using GeneraPdfBuzonE;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
using HG.CFDI.SERVICE.Services.ValidacionesSat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HG.CFDI.SERVICE.Services.Timbrado.Documentos
{
    public class DocumentosService : IDocumentosService
    {
        private readonly ICartaPorteRepository _cartaPorteRepository;
        //private readonly IApiCcpRyder _apiCcpRyder;
        private readonly string _sufijoArchivoCfdi;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CartaPorteService> _logger;
        //private readonly FirmaDigitalOptions _firmaDigitalOptions;
        private readonly List<FirmaDigitalOptions> _firmaDigitalOptions;
        private readonly InvoiceOneApiOptions _invoiceOneOptions;
        private readonly List<BuzonEApiCredential> _buzonEApiCredentials;
        private readonly LisApiOptions _lisApiOptions;
        private readonly RyderApiOptions _ryderApiOptions;
        private readonly List<compania> _companias;

        public DocumentosService(ICartaPorteRepository cartaPorteRepository,
            IConfiguration configuration,
            IOptions<List<FirmaDigitalOptions>> firmaDigitalOptions,
            IOptions<InvoiceOneApiOptions> invoiceOneOptions,
            IOptions<List<BuzonEApiCredential>> buzonEOptions,
            IOptions<LisApiOptions> lisApiOptions,
            IOptions<RyderApiOptions> ryderApiOptions,
            IOptions<List<compania>> companiaOptions,
            //IApiCcpRyder apiCcpRyder,
            ILogger<CartaPorteService> logger)
        {
            _configuration = configuration;
            _cartaPorteRepository = cartaPorteRepository;
            _firmaDigitalOptions = firmaDigitalOptions.Value;
            _invoiceOneOptions = invoiceOneOptions.Value;
            _buzonEApiCredentials = buzonEOptions.Value;
            _lisApiOptions = lisApiOptions.Value;
            _ryderApiOptions = ryderApiOptions.Value;
            _companias = companiaOptions.Value;
            _sufijoArchivoCfdi = configuration.GetValue<string>("SufijoNombreCfdi");
            //_apiCcpRyder = apiCcpRyder;
            _logger = logger;
        }

        public archivoCFDi CreateArchivoCFDi(cartaPorteCabecera cartaPorte, byte[] xml, byte[] pdf, string uuid)
        {
            return new archivoCFDi
            {
                no_guia = cartaPorte.no_guia,
                num_guia = cartaPorte.num_guia,
                compania = cartaPorte.compania,
                xml = xml == null ? new byte[0] : xml,
                pdf = pdf == null ? new byte[0] : pdf,
                uuid = uuid == null ? string.Empty : uuid,
                fechaCreacion = DateTime.Now
            };
        }
        public async Task GuardarDocumentosTimbrados(cartaPorteCabecera cartaPorte, archivoCFDi archivo)
        {
            try
            {
                var servidores = new[] { "server2019", "server2008" };

                foreach (var server in servidores)
                {
                    var success = await _cartaPorteRepository.InsertDocumentosTimbrados(archivo, server);

                    if (!success)
                    {
                        var msg = $"Fallo al insertar documentos timbrados en la base de datos {server} para {cartaPorte.num_guia}";
                        _logger.LogError(msg);

                        //await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, msg, null, null, null);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar documentos timbrados para {num_guia}", cartaPorte.num_guia);

                //await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, ex.Message, null, null, null);
            }
        }

        public async Task<string> getUuidFromXml(string xmlComprobante)
        {
            try
            {
                // Removemos la parte no relacionada con XML
                int index = xmlComprobante.IndexOf("<cfdi:Comprobante");
                if (index > -1)
                {
                    xmlComprobante = xmlComprobante.Substring(index);
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlComprobante);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
                nsmgr.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");

                XmlNode node = xmlDoc.SelectSingleNode("//tfd:TimbreFiscalDigital[@UUID]", nsmgr);
                if (node != null && node.Attributes["UUID"] != null)
                {
                    string uuidValue = node.Attributes["UUID"].Value;
                    return uuidValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (System.Exception err)
            {
                return string.Empty;
            }
        }

        public async Task patchPdfFromXml(cartaPorteCabecera cp, byte[] pdfBytes)
        {
            try
            {
                await _cartaPorteRepository.patchPdfAsync(cp.no_guia, cp.compania, pdfBytes);
            }
            catch (System.Exception)
            {
                _logger.LogInformation($"Error al tratar de recuperar el pdf de la remision {cp.num_guia}: BuzonE");
            }
        }

        public async Task<byte[]> getPdfTimbrado(string xmlCFDTimbrado, string database)
        {
            //throw new System.Exception("💥 Error simulado en getPdfTimbrado ###");

            // Utilizar UTF-8 para codificar el XML y evitar perdida de datos
            byte[] xmlbytes = Encoding.UTF8.GetBytes(xmlCFDTimbrado);

            using (WsVerificaXmlRiClient cliente = new WsVerificaXmlRiClient())
            {
                var res = await cliente.revisaXmlValidoSATPDFAsync(xmlbytes, "usrWS_506028", "U%i6w%6b0%YW", "RI_Generica_CFDI40", true, false, false, true);

                byte[] pdfbytes = System.Convert.FromBase64String(res.@return.respuestageneracionpdf.pdf.documentobase64);
                return pdfbytes;
            }
        }

        private readonly Dictionary<string, string> _mapaBasesDatos = new()
        {
            { "hgdb_lis", "hgTransportaciones.png" },
            { "chdb_lis", "chTransportaciones.png" },
            { "rldb_lis", "rlTransportaciones.png" },
            { "lindadb",  "lindaTransportaciones.png" }
        };

        public string ObtenerLogotipoEnBase64(string database)
        {
            try
            {
                if (!_mapaBasesDatos.TryGetValue(database.ToLower(), out string? nombreArchivo))
                    throw new ArgumentException($"Base de datos no reconocida: {database}");

                string resourceName = $"HG.CFDI.SERVICE.logotipos.{nombreArchivo}"; // Ajusta a tu namespace real

                var assembly = Assembly.GetExecutingAssembly();
                using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                    throw new FileNotFoundException($"No se encontró el recurso embebido: {resourceName}");

                using MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                byte[] imageBytes = ms.ToArray();

                return Convert.ToBase64String(imageBytes);
            }
            catch (System.Exception)
            {
                return null;
            }          
        }


        public async Task<byte[]> FallbackGetPdfTimbrado(string xmlCFDTimbrado, string database)
        {
            try
            {
                var logoBase64 = ObtenerLogotipoEnBase64(database);

                if (string.IsNullOrEmpty(logoBase64))
                {
                    return await CfdiPdf.DesdeXmlStringAsync(xmlCFDTimbrado, true, false);
                }

                return await CfdiPdf.DesdeXmlStringAsync(xmlCFDTimbrado, true, false, logoBase64);
                //return new byte[0];
            }
            catch (System.Exception ex)
            {
                return new byte[0];
            }
        }

        public async Task<bool> SaveFile(byte[] data, string compania, string filename)
        {
            try
            {
                string yearFolder = DateTime.Now.Year.ToString();
                string monthFolder = CultureInfo.CreateSpecificCulture("es").DateTimeFormat.GetMonthName(DateTime.Now.Month);
                monthFolder = monthFolder.ToLower();
                string dayFolder = DateTime.Now.Day.ToString("D2");

                // Construir la ruta completa con subcarpetas para año, mes y día
                string fullFolderPath = Path.Combine(GetXmlPathForCompany(compania), compania, yearFolder, monthFolder, dayFolder);

                // Verificar si la carpeta existe, si no, crearla
                if (!Directory.Exists(fullFolderPath))
                {
                    Directory.CreateDirectory(fullFolderPath);
                }

                // Crear el path completo del archivo
                string filePath = Path.Combine(fullFolderPath, filename);

                // Escribir el archivo
                await File.WriteAllBytesAsync(filePath, data);

                _logger.LogInformation($"Se creo {filename} exitosamente en el servidor");

                return true;
            }
            catch (System.Exception)
            {
                _logger.LogInformation($"Fallo la creación de {filename} en el servidor");
                return false;
            }
        }

        public async Task GuardarArchivosEnServidor(cartaPorteCabecera cartaPorte, byte[] xmlBytes, byte[] pdfBytes)
        {
            bool xmlGuardado = await SaveFile(xmlBytes, cartaPorte.compania, $"{_sufijoArchivoCfdi}{cartaPorte.num_guia}.xml");
            if (!xmlGuardado)
            {
                _logger.LogError($"Fallo al escribir el archivo XML en el servidor para {cartaPorte.num_guia}");
                //await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Fallo al escribir el archivo XML en el servidor", null, null, null);
            }

            bool pdfGuardado = await SaveFile(pdfBytes, cartaPorte.compania, $"{_sufijoArchivoCfdi}{cartaPorte.num_guia}.pdf");
            if (!pdfGuardado)
            {
                _logger.LogError($"Fallo al escribir el archivo PDF en el servidor para {cartaPorte.num_guia}");
                //await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Fallo al escribir el archivo PDF en el servidor", null, null, null);
            }
        }

        public string GetXmlPathForCompany(string companyId)
        {
            // Busca la compañía específica por su id
            var company = _companias.FirstOrDefault(c => c.id == companyId);
            if (company != null)
            {
                return company.XmlPath;
            }
            else
            {
                throw new InvalidOperationException($"No se encontró la compañía con id: {companyId}");
            }
        }
        public string GetPdfPathForCompany(string companyId)
        {
            // Busca la compañía específica por su id
            var company = _companias.FirstOrDefault(c => c.id == companyId);
            if (company != null)
            {
                return company.PdfPath;
            }
            else
            {
                throw new InvalidOperationException($"No se encontró la compañía con id: {companyId}");
            }
        }

    }
}
