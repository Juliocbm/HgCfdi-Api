using CFDI.Data.Entities;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HG.CFDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CfdiLiquidacionController : ControllerBase
    {
        private readonly ILiquidacionService _liquidacionService;
        private readonly ILogger<CfdiLiquidacionController> _logger;

        public CfdiLiquidacionController(ILogger<CfdiLiquidacionController> logger, ILiquidacionService liquidacionService)
        {
            _logger = logger;
            _liquidacionService = liquidacionService;
        }

        [HttpGet("GetLiquidacion")]
        public async Task<IActionResult> GetLiquidacion(string database, int noLiquidacion)
        {
            try
            {
                int? idCompania = ObtenerIdCompania(database);
                if (idCompania is null)
                {
                    return BadRequest("Base de datos no válida");
                }

                var liquidacion = await _liquidacionService.ObtenerLiquidacion(idCompania.Value, noLiquidacion);
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

        [HttpGet("GetLiquidaciones")]
        public async Task<ActionResult<GeneralResponse<LiquidacionDto>>> GetLiquidaciones([FromQuery] ParametrosGenerales parametros, string database)
        {
            try
            {
                var liquidaciones = await _liquidacionService.ObtenerLiquidacionesAsync(parametros, database);
                if (liquidaciones == null || liquidaciones.Items == null || !liquidaciones.Items.Any())
                {
                    return NotFound();
                }
                return Ok(liquidaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las liquidaciones");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("TimbrarLiquidacion")]
        public async Task<IActionResult> TimbrarLiquidacion(int idCompania, int noLiquidacion)
        {
            try
            {
                string? database = ObtenerDatabase(idCompania);
                if (string.IsNullOrEmpty(database))
                {
                    return BadRequest("idCompania no válido");
                }

                var response = await _liquidacionService.TimbrarLiquidacionAsync(idCompania, noLiquidacion);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al timbrar la liquidación");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetDocumentosLiquidacion")]
        public async Task<IActionResult> GetDocumentosLiquidacion(int idCompania, int idLiquidacion)
        {
            try
            {
                var response = await _liquidacionService.ObtenerDocumentosTimbradosAsync(idCompania, idLiquidacion);
                if (!response.IsSuccess)
                {
                    return NotFound(response.Mensaje);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos de la liquidación");
                return StatusCode(500, "Internal server error");
            }
        }

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

        private static int? ObtenerIdCompania(string database)
        {
            database = database.ToLower();
            return database switch
            {
                "hgdb_lis" => 1,
                "chdb_lis" => 2,
                "rldb_lis" => 3,
                "lindadb" => 4,
                _ => null
            };
        }
    }
}
