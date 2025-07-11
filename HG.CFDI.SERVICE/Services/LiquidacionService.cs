using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using HG.CFDI.CORE.Models;
using BuzonE;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using HG.CFDI.SERVICE.Services.Timbrado_liquidacion.ValidacionesSat;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Xml.Serialization;
using System.Xml;

namespace HG.CFDI.SERVICE.Services
{
    public class LiquidacionService : ILiquidacionService
    {
        private readonly ILiquidacionRepository _repository;
        private readonly IValidacionesNominaSatService _validacionesNominaSat;
        private readonly IDocumentosService _documentosService;
        private readonly ILogger<LiquidacionService> _logger;

        public LiquidacionService(IValidacionesNominaSatService validacionesNominaSat, ILiquidacionRepository repository, IOptions<List<BuzonEApiCredential>> buzonEOptions, IDocumentosService documentosService, ILogger<LiquidacionService> logger)
        {
            _repository = repository;
            _validacionesNominaSat = validacionesNominaSat;
            _documentosService = documentosService;
            _logger = logger;
        }

        public async Task<CfdiNomina?> ObtenerLiquidacion(int idCompania, int noLiquidacion)
        {
            _logger.LogInformation("Inicio ObtenerLiquidacion Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
            string? database = ObtenerDatabase(idCompania);
            if (string.IsNullOrEmpty(database))
            {
                _logger.LogInformation("Fin ObtenerLiquidacion Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
                return null;
            }

            var json = await _repository.ObtenerDatosNominaJson(database, noLiquidacion);
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogInformation("Fin ObtenerLiquidacion Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
                return null;
            }
            try
            {
                var result = JsonSerializer.Deserialize<CfdiNomina>(json);
                _logger.LogInformation("Fin ObtenerLiquidacion Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
                return result;
            }
            catch
            {
                _logger.LogInformation("Fin ObtenerLiquidacion Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
                return null;
            }
        }

        public async Task<GeneralResponse<LiquidacionDto>> ObtenerLiquidacionesAsync(ParametrosGenerales parametros, string database)
        {
            _logger.LogInformation("Inicio ObtenerLiquidaciones Compania:{database}", database);
            //string? database = ObtenerDatabase(idCompania);
            //if (string.IsNullOrEmpty(database))
            //{
            //    _logger.LogInformation("Fin ObtenerLiquidaciones Compania:{IdCompania}", idCompania);
            //    return new List<LiquidacionDto>();
            //}

            var result = await _repository.ObtenerLiquidacionesAsync(parametros, database);
            _logger.LogInformation("Fin ObtenerLiquidaciones Compania:{database}", database);
            return result;
        }

        public async Task<UniqueResponse> TimbrarLiquidacionAsync(int idCompania, int noLiquidacion)
        {
            _logger.LogInformation("Inicio TimbrarLiquidacionAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
            var respuesta = new UniqueResponse();

            var cabecera = await _repository.ObtenerCabeceraAsync(idCompania, noLiquidacion);
            if (cabecera != null && (cabecera.Estatus == (byte)EstatusLiquidacion.Timbrado || cabecera.Estatus == (byte)EstatusLiquidacion.EnProceso || cabecera.XMLTimbrado != null || cabecera.PDFTimbrado != null || cabecera.UUID != null))
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "La liquidación ya fue timbrada";
                _logger.LogInformation("Fin TimbrarLiquidacionAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
                return respuesta;
            }

            string? database = ObtenerDatabase(idCompania);

            var liquidacion = await ObtenerLiquidacion(idCompania, noLiquidacion);
            if (liquidacion == null)
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Liquidación no encontrada";
                _logger.LogInformation("Fin TimbrarLiquidacionAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
                return respuesta;
            }

            string liquidacionJson = JsonSerializer.Serialize(liquidacion);
            await _repository.RegistrarInicioIntentoAsync(idCompania, noLiquidacion, (byte)EstatusLiquidacion.EnProceso, liquidacionJson, ObtenerMensajePorEstatus(EstatusLiquidacion.EnProceso));


            RequestBE request = new();

            try
            {
                request = await _validacionesNominaSat.ConstruirRequestBuzonEAsync(liquidacion, database);
                var xml = SerializeToString(request.Comprobante);
                Console.WriteLine(xml);
            }
            catch (Exception ex)
            {
                bool transitorio = ex is TimeoutException || ex is HttpRequestException;
                await RegistrarFalloDeTimbrado(idCompania, noLiquidacion, transitorio);

                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Ocurrió un error al timbrar";
                respuesta.Errores.Add(ex.Message);

                var cabeceraActual = await _repository.ObtenerCabeceraAsync(idCompania, noLiquidacion);
                if (cabeceraActual != null)
                    await _repository.RegistrarErrorIntentoAsync(idCompania, noLiquidacion, cabeceraActual.UltimoIntento, ex.Message);

                _logger.LogInformation("Fin TimbrarLiquidacionAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
                return respuesta;
            }

            try
            {
                // Consumir servicio Buzón E
                BuzonE.responseBE responseServicio;
                using (var client = new EmisionServiceClient())
                {
                    responseServicio = await client.emitirFacturaAsync(request);
                    await client.CloseAsync();
                }

                if (responseServicio?.code == "BE-EMS.200")
                {
                    // Timbrado exitoso
                    byte[] xmlBytes = Encoding.UTF8.GetBytes(responseServicio.xmlCFDTimbrado);
                    byte[] pdfBytes = await _documentosService.GetPdfNominaTimbrado(responseServicio.xmlCFDTimbrado, database);

                    await _repository.ActualizarResultadoIntentoAsync(idCompania, noLiquidacion, (byte)EstatusLiquidacion.Timbrado, null, ObtenerMensajePorEstatus(EstatusLiquidacion.Timbrado));
                    await _repository.InsertarDocTimbradoLiqAsync(idCompania, noLiquidacion, xmlBytes, pdfBytes, responseServicio.uuid);

                    respuesta.IsSuccess = true;
                    respuesta.Mensaje = "Timbrado exitoso";
                    respuesta.XmlByteArray = xmlBytes;
                    respuesta.PdfByteArray = pdfBytes; // si vas a generar PDF luego
                }
                else
                {
                    // Error en timbrado del PAC
                    await RegistrarFalloDeTimbrado(idCompania, noLiquidacion, false);

                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = responseServicio?.mensaje ?? "Error en timbrado";

                    string mensajeError = responseServicio?.mensajeErrorTimbrado ?? respuesta.Mensaje;
                    if (!string.IsNullOrWhiteSpace(responseServicio?.mensajeErrorTimbrado))
                        respuesta.Errores.Add(responseServicio.mensajeErrorTimbrado);

                    var cabeceraActual = await _repository.ObtenerCabeceraAsync(idCompania, noLiquidacion);
                    if (responseServicio?.errorList != null)
                    {
                        foreach (var error in responseServicio.errorList)
                        {
                            string err = $"{error.message}{error.detail}";
                            respuesta.Errores.Add(err);
                            mensajeError += $" | {err}";
                            if (cabeceraActual != null)
                                await _repository.RegistrarErrorIntentoAsync(idCompania, noLiquidacion, cabeceraActual.UltimoIntento, err);
                        }
                    }

                    if (cabeceraActual != null)
                        await _repository.RegistrarErrorIntentoAsync(idCompania, noLiquidacion, cabeceraActual.UltimoIntento, mensajeError);
                }
            }
            catch (Exception ex)
            {
                // Fallo inesperado
                bool transitorio = ex is TimeoutException || ex is HttpRequestException;
                await RegistrarFalloDeTimbrado(idCompania, noLiquidacion, transitorio);

                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Ocurrió un error al timbrar";
                respuesta.Errores.Add(ex.Message);

                var cabeceraActual = await _repository.ObtenerCabeceraAsync(idCompania, noLiquidacion);
                if (cabeceraActual != null)
                    await _repository.RegistrarErrorIntentoAsync(idCompania, noLiquidacion, cabeceraActual.UltimoIntento, ex.Message);
            }

            _logger.LogInformation("Fin TimbrarLiquidacionAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, noLiquidacion);
            return respuesta;
        }

        public static string SerializeToString<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var xmlSerializer = new XmlSerializer(typeof(T));

            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false // Puedes poner true si no quieres <?xml ... ?>
            };

            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, "http://www.sat.gob.mx/cfd/4"); // Usa el namespace correcto para CFDI 4.0

            using (var stringWriter = new Utf8StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, xmlSettings))
            {
                xmlSerializer.Serialize(xmlWriter, value, ns);
                return stringWriter.ToString();
            }
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }


        public async Task<UniqueResponse> ObtenerDocumentosTimbradosAsync(int idCompania, int idLiquidacion)
        {
            _logger.LogInformation("Inicio ObtenerDocumentosTimbradosAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
            var respuesta = new UniqueResponse();

            string? database = ObtenerDatabase(idCompania);
            if (string.IsNullOrEmpty(database))
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Base de datos no válida";
                _logger.LogInformation("Fin ObtenerDocumentosTimbradosAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
                return respuesta;
            }

            var registro = await _repository.ObtenerCabeceraAsync(idCompania, idLiquidacion);
            if (registro?.Estatus != (byte)EstatusLiquidacion.Timbrado)
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "La liquidación no está timbrada";
                _logger.LogInformation("Fin ObtenerDocumentosTimbradosAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
                return respuesta;
            }

            if (registro?.XMLTimbrado == null)
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "No se encontró el XML timbrado";
                _logger.LogInformation("Fin ObtenerDocumentosTimbradosAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
                return respuesta;
            }

            // Validar si el PDF existe y no es vacío
            bool pdfValido = registro?.PDFTimbrado != null && registro.PDFTimbrado.Length > 0;
            if (!pdfValido)
            {
                try
                {
                    // Generar el PDF usando el servicio de documentos
                    byte[] pdf = await _documentosService.GetPdfNominaTimbrado(Encoding.UTF8.GetString(registro.XMLTimbrado), database);
                    if (pdf != null && pdf.Length > 0)
                    {
                        // Actualizar el PDF en la base de datos usando InsertarDocTimbradoLiqAsync
                        await _repository.InsertarDocTimbradoLiqAsync(idCompania, idLiquidacion, null, pdf, null);
                        registro.PDFTimbrado = pdf;
                    }
                    else
                    {
                        respuesta.IsSuccess = false;
                        respuesta.Mensaje = "Error al generar el PDF";
                        _logger.LogError("Error al generar PDF para liquidación {IdCompania}-{IdLiquidacion}", idCompania, idLiquidacion);
                        return respuesta;
                    }
                }
                catch (Exception ex)
                {
                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = "Error al generar el PDF: " + ex.Message;
                    _logger.LogError(ex, "Error al generar PDF para liquidación {IdCompania}-{IdLiquidacion}", idCompania, idLiquidacion);
                    return respuesta;
                }
            }

            // Devolver los documentos
            respuesta.IsSuccess = true;
            respuesta.Mensaje = "Documentos obtenidos exitosamente";
            respuesta.XmlByteArray = registro.XMLTimbrado;
            respuesta.PdfByteArray = registro.PDFTimbrado;
            //respuesta.Uuid = registro.UUID;

            _logger.LogInformation("Fin ObtenerDocumentosTimbradosAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
            return respuesta;
        }

        private async Task RegistrarFalloDeTimbrado(int idCompania, int noLiquidacion, bool transitorio)
        {
            if (transitorio)
            {
                DateTime proximo = DateTime.UtcNow.AddHours(1);
                await _repository.ActualizarResultadoIntentoAsync(idCompania, noLiquidacion, (byte)EstatusLiquidacion.ErrorTransitorio, proximo, ObtenerMensajePorEstatus(EstatusLiquidacion.ErrorTransitorio));
            }
            else
            {
                await _repository.ActualizarResultadoIntentoAsync(idCompania, noLiquidacion, (byte)EstatusLiquidacion.RequiereRevision, null, ObtenerMensajePorEstatus(EstatusLiquidacion.RequiereRevision));
            }

            await _repository.InsertarDocTimbradoLiqAsync(idCompania, noLiquidacion, null, null, null);
        }

        private static string ObtenerMensajePorEstatus(EstatusLiquidacion estatus) => estatus switch
        {
            EstatusLiquidacion.Pendiente => "Pendiente",
            EstatusLiquidacion.EnProceso => "En proceso",
            EstatusLiquidacion.ErrorTransitorio => "Error transitorio",
            EstatusLiquidacion.RequiereRevision => "Requiere revisión",
            EstatusLiquidacion.Timbrado => "Timbrado exitoso",
            EstatusLiquidacion.Migrada => "Migrada",
            _ => "Desconocido"
        };


        private static string? ObtenerDatabase(int idCompania)
        {
            return idCompania switch
            {
                1 => "hgdb_lis",
                2 => "chdb_lis",
                3 => "rldb_lis",
                4 => "lindadb",
                _ => null
            };
        }

    }
}
