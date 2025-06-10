using Azure;
using BuzonE;
using CFDI.Data.Entities;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
//using HG.CFDI.CORE.Models.TipoCambioEF;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HG.CFDI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoCambioController : ControllerBase
    {
        private readonly ILogger<TipoCambioController> _logger;
        private readonly ITipoCambioService _tipoCambioService;

        public TipoCambioController(ILogger<TipoCambioController> logger, ITipoCambioService tipoCambioService)
        {
            _logger = logger;
            _tipoCambioService = tipoCambioService;
        }

        /// <summary>
        /// Obtiene una lista paginada de tipos de cambio según los filtros y la compañía especificada.
        /// </summary>
        /// <param name="idCompania">
        /// Identificador de la compañía para la cual se desean obtener los tipos de cambio.
        /// </param>
        /// <param name="parametros">
        /// Parámetros de consulta que pueden incluir filtros, ordenamiento y paginación:
        /// - `Activos`: Obtener solo registros activos.
        /// - `OrdenarPor`: Campo de ordenamiento.
        /// - `Descending`: Orden descendente si es true.
        /// - `FiltrosIniciales` y `FiltrosPorColumna`: Búsqueda global y por campo.
        /// - `NoPagina`: Número de página actual.
        /// - `TamanoPagina`: Tamaño de página.
        /// </param>
        /// <returns>
        /// Retorna un objeto que contiene:
        /// - `TotalRecords`: Número total de registros que coinciden.
        /// - `Items`: Lista de tipos de cambio paginada.
        /// </returns>

        [HttpGet]
        public async Task<ActionResult<GeneralResponse<vwTipoCambio>>> GetTiposCambio([FromQuery] ParametrosGenerales parametros)
        {
            var response = new GeneralResponse<vwTipoCambio>();

            response.IsSuccess = true;

            try
            {
                response = await _tipoCambioService.GetTiposCambio(parametros);
            }
            catch (Exception ex)
            {
                response.AddError($"Error al obtener los clientes: {ex.Message}");
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Obtiene los detalles de un tipo de cambio específico por id y compañía.
        /// </summary>
        /// <param name="idCompania">id de la compañía a la que pertenece el tipo de cambio.</param>
        /// <param name="idTipoCambio">id del tipo de cambio a consultar.</param>
        /// <returns>
        /// Retorna el objeto `tipoCambio` si se encuentra. De lo contrario, retorna 404 (Not Found).
        /// </returns>

        [HttpGet("getById/{idTipoCambio}")]
        public async Task<ActionResult<GeneralResponse<vwTipoCambio>>> GetTipoCambioById(int idTipoCambio)
        {
            var result = await _tipoCambioService.GetTipoCambioById(idTipoCambio);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo registro de tipo de cambio para una compañía.
        /// </summary>
        /// <param name="tipoCambio">Modelo con la información del nuevo tipo de cambio.</param>
        /// <returns>
        /// Retorna el registro creado si la operación fue exitosa.
        /// </returns>

        [HttpPost("create")]
        public async Task<ActionResult<GeneralResponse<vwTipoCambio>>> PostTipoCambio([FromBody] tipoCambio tipoCambio)
        {
            try
            {
                var response = await _tipoCambioService.PostTipoCambio(tipoCambio);

                if (!response.IsSuccess)
                {
                    return Conflict(response); // HTTP 409
                }

                // Todo bien, regresamos 200
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new GeneralResponse<vwTipoCambio>
                {
                    IsSuccess = false
                };

                response.AddError($"Error al crear tipo de cambio: {ex.Message}");
                return StatusCode(500, response);
            }
        }


        /// <summary>
        /// Actualiza un tipo de cambio existente por su id.
        /// </summary>
        /// <param name="idTipoCambio">id del tipo de cambio a actualizar.</param>
        /// <param name="tipoCambio">Datos actualizados del tipo de cambio.</param>
        /// <returns>
        /// Retorna el tipo de cambio actualizado si la operación fue exitosa.
        /// </returns>

        [HttpPut]
        public async Task<ActionResult<GeneralResponse<vwTipoCambio>>> PutTipoCambio([FromBody] tipoCambio tipoCambio)
        {   
            try
            {
                var response = await _tipoCambioService.PutTipoCambio(tipoCambio);

                if (!response.IsSuccess)
                {
                    return Conflict(response); // HTTP 409
                }

                // Todo bien, regresamos 200
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new GeneralResponse<vwTipoCambio>
                {
                    IsSuccess = false
                };

                response.AddError($"Error al actualizar tipo de cambio: {ex.Message}");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Elimina lógicamente un tipo de cambio por su id.
        /// Solo se permite eliminar el tipo de cambio correspondiente al día actual.
        /// </summary>
        /// <param name="tipoCambio">Objeto del tipo de cambio a eliminar.</param>
        /// <returns>
        /// Retorna true si la eliminación fue exitosa; de lo contrario, false.
        /// </returns>

        [HttpPut("eliminar")]
        public async Task<ActionResult<GeneralResponse<tipoCambio>>> DeleteTipoCambio([FromBody] tipoCambio tipoCambio)
        {
            try
            {
                var response = await _tipoCambioService.DeleteTipoCambio(tipoCambio);

                if (!response.IsSuccess)
                {
                    return Conflict(response); // HTTP 409
                }

                // Todo bien, regresamos 200
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new GeneralResponse<tipoCambio>
                {
                    IsSuccess = false
                };

                response.AddError($"Error al actualizar tipo de cambio: {ex.Message}");
                return StatusCode(500, response);
            }
        }
    }
}
