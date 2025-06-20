﻿
using HG.CFDI.CORE.Interfaces;
//using HG.CFDI.API.Models;
using HG.CFDI.DATA.LisApi;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using HG.CFDI.CORE.Models;
using BuzonE;
using Microsoft.Extensions.Options;
using Polly;
using System.Xml.Linq;
using XSDToXML.Utils;
using Microsoft.Extensions.Logging;
using HG.CFDI.SERVICE.Utils;

//VERSION PROD
using static InvoiceOne.ioTimbreCFDISoapClient;
using InvoiceOne;
using HG.CFDI.SERVICE.Services.Timbrado.ValidacionesSat;
using HG.CFDI.SERVICE.Services.Timbrado.Ryder;
using CFDI.Data.Entities;
//VERSION PROD

//VERSION TEST
//using static InvoiceOneTest.TimbreCFDISoapClient;
//VERSION TEST
//using InvoiceOneTest;

namespace HG.CFDI.SERVICE.Services
{
    public class CartaPorteService : ICartaPorteService
    {
        private readonly ICartaPorteRepository _cartaPorteRepository;
        private readonly string _sufijoArchivoCfdi;
        private readonly ILogger<CartaPorteService> _logger;
        private readonly List<FirmaDigitalOptions> _firmaDigitalOptions;
        private readonly InvoiceOneApiOptions _invoiceOneOptions;
        private readonly LisApiOptions _lisApiOptions;
        private readonly RyderApiOptions _ryderApiOptions;
        private readonly RetryOptions _retryOptions;
        private readonly IValidacionesSatService _validacionesSat;
        private readonly IRyderService _ryderService;
        private readonly IUtilsService _utilsService;
        private readonly ITrucksService _trucksService;
        private readonly IDocumentosService _documentosService;

        public CartaPorteService(ICartaPorteRepository cartaPorteRepository,
            IConfiguration configuration,
            IOptions<List<FirmaDigitalOptions>> firmaDigitalOptions,
            IOptions<InvoiceOneApiOptions> invoiceOneOptions,
            IOptions<List<BuzonEApiCredential>> buzonEOptions,
            IOptions<LisApiOptions> lisApiOptions,
            IOptions<RyderApiOptions> ryderApiOptions,
            IOptions<RetryOptions> retryOptions,
            IOptions<List<compania>> companiaOptions,
            //IApiCcpRyder apiCcpRyder,

            IValidacionesSatService validacionesSat,
            IRyderService ryderService,
            IUtilsService utilsService,
            ITrucksService trucksService,
            IDocumentosService documentosService,

        ILogger<CartaPorteService> logger)
        {
            _cartaPorteRepository = cartaPorteRepository;
            _firmaDigitalOptions = firmaDigitalOptions.Value;
            _invoiceOneOptions = invoiceOneOptions.Value;
            _lisApiOptions = lisApiOptions.Value;
            _ryderApiOptions = ryderApiOptions.Value;
            _retryOptions = retryOptions.Value;
            _sufijoArchivoCfdi = configuration.GetValue<string>("SufijoNombreCfdi");
            _logger = logger;
            _ryderService = ryderService;
            _validacionesSat = validacionesSat;
            _utilsService = utilsService;
            _trucksService = trucksService;
            _documentosService = documentosService;
        }

        #region Crud carta porte
        public async Task<bool> putCartaPorte(string database, cartaPorteCabecera cp)
        {
            try
            {
                return await _cartaPorteRepository.putCartaPorte(database, cp);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
        public async Task<GeneralResponse<string>> changeStatusCartaPorteAsync(int no_guia, string num_guia, string compania, int EstatusTimbrado, string mensajeTimbrado, int sistemaTimbrado)
        {
            try
            {
                await _cartaPorteRepository.changeStatusCartaPorteAsync(no_guia, compania, EstatusTimbrado, mensajeTimbrado, sistemaTimbrado);
                _logger.LogInformation($"Se actualizo estatus de timbrado a {num_guia}: estatus -> {EstatusTimbrado}");

                return new GeneralResponse<string>() { Data = null, IsSuccess = true, Message = "Estatus actualizado correctamente", ErrorList = null };
            }
            catch (System.Exception err)
            {
                _logger.LogInformation($"Fallo al actualizar estatus de timbrado a {num_guia}: estatus -> {EstatusTimbrado}");
                return new GeneralResponse<string>() { Data = null, IsSuccess = false, Message = "Fallo al actualizar estatus", ErrorList = _utilsService.GetAllExceptionMessages(err) };
            }
        }
        public async Task<GeneralResponse<cartaPorteCabecera>> getCartaPorte(string database, string guia)
        {
            try
            {
                var result = await _cartaPorteRepository.reinsertaCartaPorteRepositorio(database, guia);

                if (!result.MigrationIsSuccess)
                {
                    return new GeneralResponse<cartaPorteCabecera>() { IsSuccess = false, Message = result.Mensaje };
                }

                var cp = await _cartaPorteRepository.getCartaPorte(database, guia);

                return cp;
            }
            catch (System.Exception err)
            {
                return new GeneralResponse<cartaPorteCabecera>() { IsSuccess = false, Message = err.Message };
            }
        }
        public async Task<List<cartaPorteCabecera>> getCartasPortePendiente(string compañia)
        {
            try
            {
                var cartasPorte = await _cartaPorteRepository.getCartasPortePendiente(compañia);
                return cartasPorte;
            }
            catch (System.Exception err)
            {
                return new List<cartaPorteCabecera>();
            }            
        }
        #endregion

        #region timbrado con lis e invoiceOne
        public async Task<UniqueResponse> timbrarConLis(ICartaPorteServiceApi cartaPorteServiceApi, cartaPorteCabecera cartaPorte)
        {
            var authService = new AuthenticationService(new HttpClient());
            var authResponse = await authService.AuthenticateAsync(_lisApiOptions.User, _lisApiOptions.Password);
            UniqueResponse respuesta = new UniqueResponse();

            if (authResponse != null)
            {
                var cp = await _validacionesSat.getCartaPorteRequestLis(cartaPorte);
                var response = await cartaPorteServiceApi.SendCartaPorteAsync(authResponse.access_token, cp.facturaCartaPorte);

                if ((response.IM_PDF != null && response.IM_PDF != string.Empty || response.IM_XML != null && response.IM_XML != string.Empty) || (response.CompletedSuccessfully != null && response.CompletedSuccessfully.Equals("S")))
                {

                    respuesta.XmlByteArray = System.Convert.FromBase64String(response.IM_XML);
                    respuesta.PdfByteArray = System.Convert.FromBase64String(response.IM_PDF);
                    respuesta.IsSuccess = true;
                    respuesta.Mensaje = "Timbrado exitoso";
                    string xmlString = Encoding.UTF8.GetString(respuesta.XmlByteArray);

                    await changeStatusCartaPorteAsync(cp.no_guia, cp.num_guia, cp.compania, 3, "Timbrado exitoso", cartaPorte.sistemaTimbrado.Value);


                    var archivo = _documentosService.CreateArchivoCFDi(cartaPorte, respuesta.XmlByteArray, respuesta.PdfByteArray, await _documentosService.getUuidFromXml(xmlString));

                    // Guardar archivos en el servidor
                    await _documentosService.GuardarArchivosEnServidor(cartaPorte, respuesta.XmlByteArray, respuesta.PdfByteArray);

                    await _documentosService.SaveFile(respuesta.XmlByteArray, cp.compania, _sufijoArchivoCfdi + cp.num_guia + ".xml");

                    await _documentosService.SaveFile(respuesta.PdfByteArray, cp.compania, _sufijoArchivoCfdi + cp.num_guia + ".pdf");

                }
                else
                {
                    if (response.Errors != null)
                    {
                        await changeStatusCartaPorteAsync(cp.no_guia, cp.num_guia, cp.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value);
                        foreach (var error in response.Errors)
                        {
                            foreach (var errorMessage in error.Value)
                            {
                                await insertError(cp.no_guia, cp.num_guia, cp.compania, error.Key + " - " + errorMessage, cp.facturaCartaPorte.IdOperador, cp.facturaCartaPorte.IdUnidad, cp.facturaCartaPorte.IdRemolque);
                                respuesta.IsSuccess = false;
                                respuesta.Mensaje = "Contiene errores de timbrado";
                                respuesta.Errores.Add(error.Key + " - " + errorMessage);
                            }
                        }
                    }

                    if (response.Mensajes != null)
                    {
                        foreach (var mensaje in response.Mensajes)
                        {

                            if (_utilsService.ValidaStringVariable(mensaje.Descripcion))
                            {
                                await changeStatusCartaPorteAsync(cp.no_guia, cp.num_guia, cp.compania, 3, "Timbrado exitoso", cartaPorte.sistemaTimbrado.Value);
                                respuesta.IsSuccess = true;
                                respuesta.Mensaje = mensaje.Descripcion;
                            }
                            else
                            {
                                await changeStatusCartaPorteAsync(cp.no_guia, cp.num_guia, cp.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value);
                                await insertError(cp.no_guia, cp.num_guia, cp.compania, mensaje.Descripcion, cp.facturaCartaPorte.IdOperador, cp.facturaCartaPorte.IdUnidad, cp.facturaCartaPorte.IdRemolque);
                                respuesta.IsSuccess = false;
                                respuesta.Mensaje = "Contiene errores de timbrado";
                                respuesta.Errores.Add(mensaje.Descripcion);
                            }
                        }
                    }
                }

                return respuesta;
            }
            else
            {
                await changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value);
                await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Fallo en login con zam.", null, null, null);

                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Fallo en login con zam.";

                return respuesta;
            }
        }
        public async Task<UniqueResponse> timbrarConInvoiceOne(cartaPorteCabecera cartaPorte, string database)
        {
            UniqueResponse respuesta = new UniqueResponse();
            _logger.LogWarning($"Timbrado InvoiceOne {cartaPorte.num_guia} ");

            try
            {
                //VERSION 3.0
                var client = new ioTimbreCFDISoapClient(EndpointConfiguration.ioTimbreCFDISoap);
                #region ambiente test
                //VERSION 3.1 TEST
                //var client = new TimbreCFDISoapClient(EndpointConfiguration.TimbreCFDISoap);
                #endregion
                CFDIHandler handler = new CFDIHandler();

                string xmlComprobante = string.Empty;
                var reqResponse = _validacionesSat.getCartaPorteRequestInvoiceOne(cartaPorte, database);

                if (!reqResponse.IsSuccess)
                {
                    await changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value);

                    respuesta.IsSuccess = reqResponse.IsSuccess;
                    respuesta.Mensaje = reqResponse.Message;
                    respuesta.Errores = reqResponse.ErrorList;
                    return respuesta;
                }

                xmlComprobante = reqResponse.Data;
                #region PROCESO NUEVO PARA FIRMA DIGITAL
                // Seleccionar la firma digital adecuada
                var firmaDigitalOption = _firmaDigitalOptions.FirstOrDefault(f => f.Empresa.Equals(cartaPorte.compania, StringComparison.OrdinalIgnoreCase));
                if (firmaDigitalOption == null)
                {
                    await changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value);

                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = "No se encontró configuración de firma digital para la empresa especificada.";
                    return respuesta;
                }

                var certificadoPath = Path.Combine(Directory.GetCurrentDirectory(), "archivosSat", firmaDigitalOption.NombreCarpeta, firmaDigitalOption.NombreArchivoCertificado);
                var privateKey = Path.Combine(Directory.GetCurrentDirectory(), "archivosSat", firmaDigitalOption.NombreCarpeta, firmaDigitalOption.NombreArchivoPrivateKey);
                var passPrivateKey = firmaDigitalOption.PassPrivateKey;
                var pathXslt = Path.Combine(Directory.GetCurrentDirectory(), "archivosSat", firmaDigitalOption.NombreCarpeta, firmaDigitalOption.NombreArchivoCadenaOriginal);

                //OBTENER NUMERO DE CERTIFICADO
                string numeroCertificado, aa, b, c;
                if (string.IsNullOrEmpty(firmaDigitalOption.NumeroDeCertificado))
                {
                    SelloDigital.leerCER(certificadoPath, out aa, out b, out c, out numeroCertificado);
                }

                numeroCertificado = firmaDigitalOption.NumeroDeCertificado;

                XDocument doc = XDocument.Parse(xmlComprobante);
                XElement comprobante = doc.Root;
                comprobante.SetAttributeValue("NoCertificado", numeroCertificado);

                xmlComprobante = doc.ToString();

                //GENERA CADENA ORIGINAL
                string cadenaOriginal = handler.GenerarCadenaOriginalConXSLT(xmlComprobante, pathXslt);

                //SELLAR
                SelloDigital oSelloDigital = new SelloDigital();
                XDocument doc2 = XDocument.Parse(xmlComprobante);
                XElement comprobante2 = doc2.Root;
                comprobante.SetAttributeValue("Certificado", oSelloDigital.Certificado(certificadoPath));
                comprobante.SetAttributeValue("Sello", oSelloDigital.Sellar(cadenaOriginal, privateKey, passPrivateKey));
                xmlComprobante = doc.ToString(SaveOptions.DisableFormatting);

                // Configuración para garantizar que el XML esté en UTF-8
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false), // Sin BOM
                    Indent = true,                      // Opcional, para formatear con indentación
                    OmitXmlDeclaration = false          // Aseguramos que se incluya la declaración XML
                };

                // Usamos MemoryStream para obtener el resultado en UTF-8
                using (var memoryStream = new MemoryStream())
                {
                    using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                    {
                        doc.WriteTo(xmlWriter);
                    }

                    // Convertimos el resultado en un string UTF-8
                    xmlComprobante = Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                //<?xml version="1.0" encoding="UTF-8"?>

                #endregion PROCESO NUEVO PARA FIRMA DIGITAL
                ObtenerCFDIResponse res = await client.ObtenerCFDIAsync(null, _invoiceOneOptions.User, _invoiceOneOptions.Password, xmlComprobante);
                byte[] xmlBytes = Encoding.UTF8.GetBytes(res.ObtenerCFDIResult.Xml);
                byte[] pdfBytes = await _documentosService.getPdfTimbrado(res.ObtenerCFDIResult.Xml, database);

                await changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 3, "Timbrado exitoso", cartaPorte.sistemaTimbrado.Value);
                #region ambiente test
                //ObtenerCFDIPruebaResponse res = await client.ObtenerCFDIPruebaAsync(null, _invoiceOneOptions.User, _invoiceOneOptions.Password, xmlComprobante);
                //byte[] xmlBytes = Encoding.UTF8.GetBytes(res.ObtenerCFDIPruebaResult.Xml);
                //byte[] pdfBytes = await getPdfTimbrado(res.ObtenerCFDIPruebaResult.Xml);
                #endregion

                var archivo = _documentosService.CreateArchivoCFDi(cartaPorte, xmlBytes, pdfBytes, await _documentosService.getUuidFromXml(res.ObtenerCFDIResult.Xml));
                // Insertar documentos en la base de datos
                await _documentosService.GuardarDocumentosTimbrados(cartaPorte, archivo);

                await _ryderService.ProcesarRyderAsync(cartaPorte, xmlBytes, pdfBytes);

                await _trucksService.trasladaUuidToTrucks(new archivoCFDi() { no_guia = cartaPorte.no_guia, num_guia = cartaPorte.num_guia, compania = cartaPorte.compania, xml = xmlBytes, pdf = pdfBytes, uuid = await _documentosService.getUuidFromXml(res.ObtenerCFDIResult.Xml), fechaCreacion = DateTime.Now });


                #region guardar archivos xml y pdf fisicamente en servidor
                //try
                //{
                //    //// Escribir el archivo XML
                //    await SaveFile(xmlBytes, cartaPorte.compania, _sufijoArchivoCfdi + cartaPorte.num_guia + ".xml");
                //}
                //catch (System.Exception err)
                //{
                //    await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Fallo al crear el archivo xml en la ubicacion asignada.", null, null, null);

                //    respuesta.Errores.Add("Fallo al crear el archivo xml en la ubicacion asignada.");

                //    await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, err.Message, null, null, null);

                //    respuesta.Errores.Add(err.Message);
                //}

                //try
                //{
                //    //// Escribir el archivo PDF
                //    await SaveFile(pdfBytes, cartaPorte.compania, _sufijoArchivoCfdi + cartaPorte.num_guia + ".pdf");
                //}
                //catch (System.Exception err)
                //{
                //    await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Fallo al crear el archivo pdf en la ubicacion asignada.", null, null, null);

                //    respuesta.Errores.Add("Fallo al crear el archivo pdf en la ubicacion asignada.");

                //    await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, err.Message, null, null, null);

                //    respuesta.Errores.Add(err.Message);
                //}
                #endregion
                respuesta.IsSuccess = true;
                respuesta.XmlByteArray = xmlBytes;
                respuesta.PdfByteArray = pdfBytes;
                respuesta.Mensaje = "Timbrado exitoso";

                return respuesta;
            }
            catch (System.Exception ex)
            {

                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Contiene errores de timbrado";
                respuesta.Errores.Add(ex.Message);
                await changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value);
                await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, ex.Message, null, null, null);

                return respuesta;
            }
        }
        #endregion

        public async Task<UniqueResponse> timbrarConBuzonE(cartaPorteCabecera cartaPorte, string database)
        {
            UniqueResponse respuesta = new UniqueResponse();
            _logger.LogInformation($"Pac utilizado para timbrar {cartaPorte.num_guia}: BuzonE");
            try
            {
                var requestUnique = await GenerarPeticionBuzonE(cartaPorte, database);

                if (!requestUnique.IsSuccess)
                {
                    TaskHelper.RunSafeAsync(() => changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value));
                    TaskHelper.RunSafeAsync(() => insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, requestUnique.Mensaje, null, null, null));

                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = "Contiene errores de timbrado";
                    respuesta.Errores.Add(requestUnique.Mensaje);
                    return respuesta;
                }

                BuzonE.responseBE responseServicio = new BuzonE.responseBE();

                using (var client = new EmisionServiceClient())
                {
                    var retryPolicy = Policy
                        .Handle<System.Exception>()
                        .WaitAndRetryAsync(_retryOptions.MaxEmitirFacturaRetries,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            (ex, ts, attempt, ctx) =>
                            {
                                _logger.LogWarning(ex, $"Retry {attempt} executing emitirFacturaAsync");
                            });

                    try
                    {
                        responseServicio = await retryPolicy.ExecuteAsync(() => client.emitirFacturaAsync(requestUnique.request));
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "Error al invocar emitirFacturaAsync");
                        TaskHelper.RunSafeAsync(() => changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value));
                        TaskHelper.RunSafeAsync(() => insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, ex.Message, null, null, null));
                        return new UniqueResponse
                        {
                            IsSuccess = false,
                            Mensaje = "No se obtuvo respuesta del servicio de timbrado.",
                            Errores = new List<string> { ex.Message }
                        };
                    }
                    finally
                    {
                        try { await client.CloseAsync(); } catch { client.Abort(); }
                    }
                }

                if (responseServicio == null || string.IsNullOrWhiteSpace(responseServicio.code))
                {
                    TaskHelper.RunSafeAsync(() => changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 5, "Sin respuesta de BuzonE", cartaPorte.sistemaTimbrado.Value));
                    TaskHelper.RunSafeAsync(() => insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Respuesta nula o inválida del servicio BuzónE.", null, null, null));
                    return new UniqueResponse
                    {
                        IsSuccess = false,
                        Mensaje = "No se recibió una respuesta válida del servicio de timbrado.",
                        Errores = new List<string> { "Respuesta nula o inválida del servicio BuzónE." }
                    };
                }

                respuesta = await EvaluarRespuestaBuzonE(responseServicio, cartaPorte, database);

                if (respuesta.IsSuccess)
                {
                    TaskHelper.RunSafeAsync(() => PersistirDocumentosAsync(cartaPorte,
                        respuesta.XmlByteArray,
                        respuesta.PdfByteArray,
                        responseServicio.uuid,
                        database));

                }

                return respuesta;
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation($"Timbrado fallido BuzonE {cartaPorte.num_guia} ");
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Timbrado fallido";
                respuesta.Errores.AddRange(_utilsService.GetAllExceptionMessages(ex));

                TaskHelper.RunSafeAsync(() => _cartaPorteRepository.changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value));

                foreach (var error in respuesta.Errores)
                {
                    TaskHelper.RunSafeAsync(() => insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, error, null, null, null));
                }
                return respuesta;
            }

        }

        private async Task<UniqueRequest<RequestBE>> GenerarPeticionBuzonE(cartaPorteCabecera cartaPorte, string database)
        {
            return await _validacionesSat.getCartaPorteRequestBuzonE(cartaPorte, database);
        }

        private async Task<UniqueResponse> EvaluarRespuestaBuzonE(responseBE responseServicio, cartaPorteCabecera cartaPorte, string database)
        {
            UniqueResponse respuesta = new UniqueResponse();

            switch (responseServicio.code)
            {
                case "BE-EMS.200":
                    _logger.LogInformation($"Timbrado exitoso {cartaPorte.num_guia} - {responseServicio.code}");

                    TaskHelper.RunSafeAsync(() => changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 3, "Timbrado exitoso", cartaPorte.sistemaTimbrado.Value));

                    byte[] xmlBytes = Encoding.UTF8.GetBytes(responseServicio.xmlCFDTimbrado);
                    byte[] pdfBytes = Array.Empty<byte>();
                    try
                    {
                        pdfBytes = await _documentosService.getPdfTimbrado(responseServicio.xmlCFDTimbrado, database);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, $"Error al obtener el PDF para {cartaPorte.num_guia}");
                        respuesta.Errores.Add("Fallo al generar el PDF");
                    }

                    respuesta.IsSuccess = true;
                    respuesta.XmlByteArray = xmlBytes;
                    respuesta.PdfByteArray = pdfBytes;
                    respuesta.Mensaje = "Timbrado exitoso";
                    break;
                default:
                    _logger.LogInformation($"Timbrado fallido BuzonE {cartaPorte.num_guia} ");
                    TaskHelper.RunSafeAsync(() => changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 2, "Contiene errores de timbrado", cartaPorte.sistemaTimbrado.Value));

                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = "Contiene errores de timbrado";
                    respuesta.Errores.Add(responseServicio.mensaje);
                    respuesta.Errores.Add(responseServicio.mensajeErrorTimbrado);

                    foreach (var error in responseServicio.errorList)
                    {
                        respuesta.Errores.Add(error.message + error.detail);
                    }
                    break;
            }

            return respuesta;
        }

        private async Task PersistirDocumentosAsync(cartaPorteCabecera cartaPorte, byte[] xmlBytes, byte[] pdfBytes, string uuid, string database)
        {
            try
            {
                var archivo = _documentosService.CreateArchivoCFDi(cartaPorte, xmlBytes, pdfBytes, uuid);

                var servidores = new[] { "server2019", "server2008" };

                var tareasInsercion = servidores.Select(async server =>
                {
                    bool success = false;
                    try
                    {
                        success = await _cartaPorteRepository.InsertDocumentosTimbrados(archivo, server);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, $"Error insertando documentos en {server}");
                    }
                    return (server, success);
                }).ToList();

                var resultados = await Task.WhenAll(tareasInsercion);

                bool successDocs2019 = resultados.First(r => r.server == "server2019").success;
                if (!successDocs2019)
                {
                    TaskHelper.RunSafeAsync(() => insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Fallo al insertar documentos timbrados a base de datos 2019", null, null, null));
                    TaskHelper.RunSafeAsync(() => changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 3, "Timbrado exitoso [SD2019]", cartaPorte.sistemaTimbrado.Value));
                }

                bool successDocs2008 = resultados.First(r => r.server == "server2008").success;
                if (!successDocs2008)
                {
                    TaskHelper.RunSafeAsync(() => insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Fallo al insertar documentos timbrados a base de datos 2008", null, null, null));
                    TaskHelper.RunSafeAsync(() => changeStatusCartaPorteAsync(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, 3, "Timbrado exitoso [SD2008]", cartaPorte.sistemaTimbrado.Value));
                }

                if (cartaPorte.cteReceptorId == _ryderApiOptions.IdClienteForUploadIngreso)
                {
                    try
                    {
                        var succesProcessRyder = await _ryderService.ProcesarRyderAsync(cartaPorte, xmlBytes, pdfBytes);
                        if (!succesProcessRyder)
                        {
                            TaskHelper.RunSafeAsync(() => insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Fallo al enviar el cfdi a Api Ryder", null, null, null));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "Error enviando cfdi a Api Ryder");
                    }
                }

                try
                {
                    await _trucksService.trasladaUuidToTrucks(new archivoCFDi() { no_guia = cartaPorte.no_guia, num_guia = cartaPorte.num_guia, compania = cartaPorte.compania, xml = xmlBytes, pdf = pdfBytes, uuid = uuid, fechaCreacion = DateTime.Now });
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error trasladando uuid a Trucks");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error general en PersistirDocumentosAsync");
            }
        }


        public async Task deleteErrors(int no_guia, string compania)
        {
            try
            {
                await _cartaPorteRepository.deleteErrors(no_guia, compania);
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation(ex.Message);
            }
        }
        public async Task<GeneralResponse<string>> insertError(int no_guia, string num_guia, string compania, string error, int? idOperador_Lis, string? idUnidad_Lis, string? idRemolque_Lis)
        {
            try
            {
                await _cartaPorteRepository.insertError(no_guia, num_guia, compania, error, idOperador_Lis, idUnidad_Lis, idRemolque_Lis);
                return new GeneralResponse<string>() { Data = null, IsSuccess = true, Message = "Registro de error guardado correctamente", ErrorList = null };
            }
            catch (System.Exception err)
            {
                return new GeneralResponse<string>() { Data = null, IsSuccess = false, Message = "Fallo al guardar registro de error", ErrorList = _utilsService.GetAllExceptionMessages(err) };
            }
        }
        public async Task fechaSolicitudTimbradoAsync(int no_guia, string compania)
        {
            await _cartaPorteRepository.fechaSolicitudTimbradoAsync(no_guia, compania);
        }

        public async Task<bool> TrySetTimbradoEnProcesoAsync(int no_guia, string compania)
        {
            try
            {
                return await _cartaPorteRepository.TrySetTimbradoEnProcesoAsync(no_guia, compania);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error setting timbrado en proceso");
                return false;
            }
        }
    }
}
