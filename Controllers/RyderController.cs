using BuzonE;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.SERVICE.Services;
using HG.CFDI.SERVICE.Services.Timbrado.Ryder;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.Text;

namespace HG.CFDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RyderController : Controller
    {
        private readonly ILogger<CfdiController> _logger;
        //private readonly IApiCcpRyder _apiCcpRyder;
        private readonly ICartaPorteService _cartaPorteService;
        private readonly IRyderService _ryderService;

        public RyderController(ILogger<CfdiController> logger, 
            //IApiCcpRyder ApiCcpRyder, 
            ICartaPorteService cartaPorteService, 
            IRyderService ryderService)
        {
            _logger = logger;
            //_apiCcpRyder = ApiCcpRyder;
            _cartaPorteService = cartaPorteService;
            _ryderService = ryderService;
        }

        //[HttpGet("getCartaPorteJson/{idOperacion}/{idViaje}")]
        //public async Task<IActionResult> getCartaPorteJson(int idOperacion, int idViaje)//(int idOperacion = 1, int idViaje = 428396)
        //{
        //    var res = await _apiCcpRyder.GetCartaPorteJson(idOperacion, idViaje);

        //    try
        //    {
        //        // Intenta deserializar la respuesta y formatearla como JSON
        //        return Content(res, "application/json");
        //    }
        //    catch
        //    {
        //        // Si no se puede deserializar, devuelve el string original como está
        //        return BadRequest("La respuesta no es un JSON válido.");
        //    }
        //}


        [HttpPost("EnviaCfdiToRyder")]
        public async Task<IActionResult> EnviaCfdiToRyder(string database, string num_guia)
        {
            try
            {
                var cp = await _cartaPorteService.getCartaPorte(database.ToLower(), num_guia);

                var response = await _ryderService.enviaCfdiToRyder(cp.Item);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
