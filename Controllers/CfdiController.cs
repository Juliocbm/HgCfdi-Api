using BuzonE;
using CFDI.Data.Entities;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HG.CFDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CfdiController : ControllerBase
    {
        private readonly ILogger<CfdiController> _logger;
        private readonly ICartaPorteService _cartaPorteService;
        private readonly ICartaPorteServiceApi _cartaPorteServiceApi;

        private readonly IDocumentosService _documentosService;
       
        public CfdiController(ILogger<CfdiController> logger, ICartaPorteService cartaPorteService, ICartaPorteServiceApi cartaPorteServiceApi, IDocumentosService documentosService)
        {
            _logger = logger;
            _cartaPorteService = cartaPorteService;
            _cartaPorteServiceApi = cartaPorteServiceApi;
            _documentosService = documentosService;
        }

        [HttpPost("TimbraRemision")]
        public async Task<IActionResult> TimbraRemision(string database, string remision, int sistemaTimbrado = 2)
        {
            try
            {
                // Validación inicial: parámetros requeridos
                if (string.IsNullOrWhiteSpace(database) || string.IsNullOrWhiteSpace(remision))
                {
                    _logger.LogInformation("Parámetros inválidos: database o remisión vacíos.");
                    return BadRequest(new UniqueResponse()
                    {
                        IsSuccess = false,
                        Mensaje = "Parámetros inválidos. Por favor, verifica la base de datos y la remisión."
                    });
                }

                // Obtener Carta Porte
                var res = await _cartaPorteService.getCartaPorte(database.ToLower(), remision);

                // Validar respuesta de la consulta
                if (!res.IsSuccess || res.Data == null)
                {
                    _logger.LogInformation($"Error al obtener Carta Porte para la guía {remision}. Detalles: {string.Join(", ", res.ErrorList)}");
                    return Ok(new UniqueResponse()
                    {
                        IsSuccess = false,
                        Mensaje = $"No se pudo obtener la información de la guía {remision}.",
                        Errores = res.ErrorList
                    });
                }

                var cartaPorte = res.Data;

                if (sistemaTimbrado < 1 || sistemaTimbrado > 3)
                {
                    _logger.LogInformation($"Valor de sistemaTimbrado inválido: {sistemaTimbrado}.");

                    return Ok(new UniqueResponse()
                    {
                        IsSuccess = false,
                        Mensaje = "El sistema de timbrado proporcionado no es válido. Debe ser 1, 2 o 3."
                    });
                }

                // Si el parámetro es válido, se usa para decidir el PAC
                cartaPorte.sistemaTimbrado = sistemaTimbrado;


                var setResult = await _cartaPorteService.TrySetTimbradoEnProcesoAsync(cartaPorte.no_guia, cartaPorte.compania);

                if (!setResult)
                {
                    _logger.LogInformation($"Guía {remision} ya está en proceso o timbrada.");
                    return Conflict(new UniqueResponse()
                    {
                        IsSuccess = false,
                        Mensaje = $"Guía {remision} ya está en proceso de timbrado o timbrada."
                    });
                }

                if (cartaPorte.estatusTimbrado == 5)
                {
                    _logger.LogInformation($"Guía {remision} no obtuvo respuesta de BuzonE.");
                   
                    return Ok(new UniqueResponse()
                    {
                        IsSuccess = false,
                        Mensaje = $"Guía {remision} no obtuvo respuesta de BuzonE.",
                        XmlByteArray = new byte[0],
                        PdfByteArray = new byte[0]
                    });
                    
                }

                // Validar sistema de timbrado
                if (cartaPorte.sistemaTimbrado == 0 || cartaPorte.sistemaTimbrado == null)
                {
                    _logger.LogInformation($"El cliente de la guía {remision} no tiene configurado un sistema de timbrado válido.");
                    return Ok(new UniqueResponse()
                    {
                        IsSuccess = false,
                        Mensaje = $"El cliente de la guía {remision} no tiene configurado un sistema de timbrado."
                    });
                }

                // Actualizar estatus a "En proceso de timbrado"
                _logger.LogInformation($"Iniciando timbrado de la guía {remision}");

                // Eliminar posibles errores anteriores
                await _cartaPorteService.deleteErrors(cartaPorte.no_guia, cartaPorte.compania);

                // Procesar timbrado basado en sistema configurado
                var result = cartaPorte.sistemaTimbrado switch
                {
                    1 => await _cartaPorteService.timbrarConLis(_cartaPorteServiceApi, cartaPorte),
                    2 => await _cartaPorteService.timbrarConBuzonE(cartaPorte, database.ToLower()),
                    3 => await _cartaPorteService.timbrarConInvoiceOne(cartaPorte, database.ToLower()),
                    _ => throw new InvalidOperationException($"Sistema de timbrado no válido: {cartaPorte.sistemaTimbrado}")
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Manejo de errores inesperados
                _logger.LogError(ex, $"Error inesperado al timbrar la guía {remision}.");
                return StatusCode(500, new UniqueResponse()
                {
                    IsSuccess = false,
                    Mensaje = "Ocurrió un error inesperado al procesar la solicitud.",
                    Errores = new List<string> { ex.Message }
                });
            }
        }
 
        [HttpGet("GetCartaPorte")]
        public async Task<IActionResult> GetCartaPorte(string database, string num_guia)
        {
            try
            {
                var response = await _cartaPorteService.getCartaPorte(database.ToLower(), num_guia);

                if (response.Data != null && response.Data.archivoCFDi != null && response.Data.archivoCFDi.pdf.Length <= 0)
                {
                    // Convertir de byte[] a string utilizando UTF-8 para evitar perdida de informacion
                    string xmlCFDTimbrado = Encoding.UTF8.GetString(response.Data.archivoCFDi.xml);

                    byte[] pdfBytes = await _documentosService.getPdfTimbrado(xmlCFDTimbrado, database);

                    response.Data.archivoCFDi.pdf = pdfBytes;

                    await _documentosService.patchPdfFromXml(response.Data, pdfBytes);
                }  

                if (response.IsSuccess)
                {

                    return Ok(response.Data);
                }
                else
                {
                    return NotFound(response.Message);
                }
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }

        }

        [HttpPost("GetRepresentacionGraficaFromXml")]
        public async Task<IActionResult> GetRepresentacionGraficaFromXml([FromBody] string xml, string database)
        {
            try
            {
                if (string.IsNullOrEmpty(xml))
                {
                    return BadRequest("XML cannot be null or empty");
                }

                var response = await _documentosService.getPdfTimbrado(xml, database);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("PutRestauraPdfFromXml")]
        public async Task<IActionResult> PutRestauraPdfFromXml(string database, string num_guia)
        {
            try
            {

                var response = await _cartaPorteService.getCartaPorte(database.ToLower(), num_guia);

                // Convertir de byte[] a string utilizando UTF-8 para evitar perdida de informacion
                string xmlCFDTimbrado = Encoding.UTF8.GetString(response.Data.archivoCFDi.xml);

                byte[] pdfBytes = await _documentosService.getPdfTimbrado(xmlCFDTimbrado, database);

                await _documentosService.patchPdfFromXml(response.Data, pdfBytes);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("PutCartaPorte")]
        public async Task<IActionResult> PutCartaPorte(string database, cartaPorteCabecera cp)
        {
            var response = await _cartaPorteService.putCartaPorte(database, cp);

            return Ok(response);
        }
    }
}
