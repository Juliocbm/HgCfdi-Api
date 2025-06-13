using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using Microsoft.AspNetCore.Mvc;

namespace HG.CFDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiquidacionController : ControllerBase
    {
        private readonly ILiquidacionService _liquidacionService;
        private readonly ILogger<LiquidacionController> _logger;

        public LiquidacionController(ILogger<LiquidacionController> logger, ILiquidacionService liquidacionService)
        {
            _logger = logger;
            _liquidacionService = liquidacionService;
        }

        [HttpGet("GetLiquidacion")]
        public async Task<IActionResult> GetLiquidacion(string database, int noLiquidacion)
        {
            try
            {
                var liquidacion = await _liquidacionService.ObtenerLiquidacion(database.ToLower(), noLiquidacion);
                if (liquidacion == null)
                {
                    return NotFound();
                }
                return Ok(liquidacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la liquidación");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("TimbrarLiquidacion")]
        public async Task<IActionResult> TimbrarLiquidacion(string database, int noLiquidacion)
        {
            try
            {
                var response = await _liquidacionService.TimbrarLiquidacionAsync(database.ToLower(), noLiquidacion);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al timbrar la liquidación");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
