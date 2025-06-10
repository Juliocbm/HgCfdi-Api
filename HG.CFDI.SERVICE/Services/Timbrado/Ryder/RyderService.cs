using BuzonE;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using CFDI.Data.Entities;
using Ryder.Api.Client.Services;
using Ryder.Api.Client.Models.Requests;

namespace HG.CFDI.SERVICE.Services.Timbrado.Ryder
{
    public class RyderService: IRyderService
    {
        private readonly ICartaPorteRepository _cartaPorteRepository;
        private readonly IRyderApiClient _apiCcpRyder;
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

        public RyderService(ICartaPorteRepository cartaPorteRepository,
            IConfiguration configuration,
            IOptions<List<FirmaDigitalOptions>> firmaDigitalOptions,
            IOptions<InvoiceOneApiOptions> invoiceOneOptions,
            IOptions<List<BuzonEApiCredential>> buzonEOptions,
            IOptions<LisApiOptions> lisApiOptions,
            IOptions<RyderApiOptions> ryderApiOptions,
            IOptions<List<compania>> companiaOptions,
            IRyderApiClient apiCcpRyder,
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
            _apiCcpRyder = apiCcpRyder;
            _logger = logger;
        }

        public async Task<UniqueResponse> enviaCfdiToRyder(cartaPorteCabecera cp)
        {
            UniqueResponse respuesta = new UniqueResponse();

            try
            {
                if (cp.cteReceptorId == _ryderApiOptions.IdClienteForUploadIngreso)
                {
                    if (_ryderApiOptions.ActivarUploadIngreso)
                    {
                        if (cp.cartaPorteOperacionRyders != null && cp.cartaPorteOperacionRyders.Any())
                        {
                            try
                            {

                                string xmlBase64 = Convert.ToBase64String(cp.archivoCFDi.xml);
                                string pdfBase64 = Convert.ToBase64String(cp.archivoCFDi.pdf);

                                //var response = await _apiCcpRyder.SendCFDI(xmlBase64, pdfBase64, cp.cartaPorteOperacionRyders.First().idOperacionRyder, cp.cartaPorteOperacionRyders.First().idViajeRyder);
                                var response = await _apiCcpRyder.UploadIngresoAsync(
                                    new UploadIngresoRequest() { 
                                        //FechaFactura = DateTime.Now, 
                                        OperacionID = cp.cartaPorteOperacionRyders.First().idOperacionRyder,
                                        ViajeID = cp.cartaPorteOperacionRyders.First().idViajeRyder,
                                        XmlBase64 = xmlBase64,
                                        PdfBase64 = pdfBase64
                                    });

                                if (response.Estatus.Equals("ERROR"))
                                {
                                    respuesta.IsSuccess = false;
                                    respuesta.Errores.Add("Se genero un error al intentar enviar los archivos cfdi a ryder.");
                                    respuesta.Errores.Add(response.Error);
                                    return respuesta;
                                }

                                await _cartaPorteRepository.actualizaEstatusEnvioRyderAsync(cp.cartaPorteOperacionRyders.FirstOrDefault().id, true);

                                respuesta.IsSuccess = true;
                                respuesta.Mensaje = "El cfdi se envio exitosamente a ryder.";
                                return respuesta;
                            }
                            catch (Exception err)
                            {
                                respuesta.IsSuccess = false;
                                respuesta.Errores.Add("Se genero un error al intentar enviar los archivos cfdi a ryder.");
                                respuesta.Errores.Add(err.Message);
                                return respuesta;
                            }
                        }
                        else
                        {
                            respuesta.IsSuccess = false;
                            respuesta.Errores.Add("No se tuvieron disponibles el idOperacion y el idViaje.");
                            return respuesta;
                        }
                    }
                    else
                    {
                        respuesta.IsSuccess = false;
                        respuesta.Errores.Add("No se envio el xml y pdf a Ryder, ya que dicho envio por api no esta activo.");
                        return respuesta;
                    }
                }
                else
                {
                    respuesta.IsSuccess = false;
                    respuesta.Errores.Add("La remision debe tener al cliente Ryder como receptor.");
                    return respuesta;
                }
            }
            catch (Exception err)
            {
                respuesta.IsSuccess = false;
                respuesta.Errores.Add("Se genero un error al intentar enviar los archivos cfdi a ryder.");
                respuesta.Errores.Add(err.Message);
                return respuesta;
            }
        }

        public async Task<bool> ProcesarRyderAsync(cartaPorteCabecera cartaPorte, byte[] xmlBytes, byte[] pdfBytes)
        {
            if (!_ryderApiOptions.ActivarUploadIngreso) return false;

            if (cartaPorte.cartaPorteOperacionRyders == null || !cartaPorte.cartaPorteOperacionRyders.Any())
            {                
                return false;
            }

            try
            {
                string xmlBase64 = Convert.ToBase64String(xmlBytes);
                string pdfBase64 = Convert.ToBase64String(pdfBytes);

                //var response = await _apiCcpRyder.SendCFDI(
                //    xmlBase64,
                //    pdfBase64,
                //    cartaPorte.cartaPorteOperacionRyders.First().idOperacionRyder,
                //    cartaPorte.cartaPorteOperacionRyders.First().idViajeRyder
                //);

                var response = await _apiCcpRyder.UploadIngresoAsync(
                                 new UploadIngresoRequest()
                                 {
                                     //FechaFactura = DateTime.Now, 
                                     OperacionID = cartaPorte.cartaPorteOperacionRyders.First().idOperacionRyder,
                                     ViajeID = cartaPorte.cartaPorteOperacionRyders.First().idViajeRyder,
                                     XmlBase64 = xmlBase64,
                                     PdfBase64 = pdfBase64
                                 });

                if (response.Estatus.Equals("ERROR"))
                {
                    //await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, "Error al enviar los archivos CFDI a Ryder.", null, null, null);
                    //await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, response.Error, null, null, null);
                    return false;
                }

                await _cartaPorteRepository.actualizaEstatusEnvioRyderAsync(cartaPorte.cartaPorteOperacionRyders.First().id, true);
                _logger.LogInformation($"El xml y pdf de {cartaPorte.num_guia} se envio exitosamente a API Ryder");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"El xml y pdf de {cartaPorte.num_guia} fallo al enviarse a API Ryder");
                //await insertError(cartaPorte.no_guia, cartaPorte.num_guia, cartaPorte.compania, $"Error inesperado en Ryder: {ex.Message}", null, null, null);
                return false;
            }
        }
    }
}
