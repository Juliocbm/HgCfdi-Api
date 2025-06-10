using Azure;
using CFDI.Data.Entities;

//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Models;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
//using HG.CFDI.CORE.Models.TipoCambioEF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ITipoCambioRepository
    {
        /// <summary>
        /// Obtiene una lista paginada de tipos de cambio para una compañía, con filtros.
        /// </summary>
        /// <param name="parametros">Parámetros de búsqueda, ordenamiento y paginación.</param>
        /// <param name="idCompania">id de la compañía.</param>
        /// <returns>Lista paginada de tipos de cambio.</returns>
        Task<GeneralResponse<vwTipoCambio>> GetTiposCambio(ParametrosGenerales parametros);

        /// <summary>
        /// Obtiene un tipo de cambio por su id.
        /// </summary>
        /// <param name="idCompania">id de la compañía.</param>
        /// <param name="idTipoCambio">id del tipo de cambio.</param>
        /// <returns>Objeto tipo de cambio si existe, null si no.</returns>
        Task<GeneralResponse<vwTipoCambio?>> GetTipoCambioById(int idTipoCambio);

        /// <summary>
        /// Crea un nuevo tipo de cambio.
        /// </summary>
        /// <param name="tipoCambio">Datos del tipo de cambio a crear.</param>
        /// <returns>Tipo de cambio creado.</returns>
        Task<GeneralResponse<tipoCambio>> PostTipoCambio(tipoCambio tipoCambio);

        /// <summary>
        /// Actualiza un tipo de cambio existente.
        /// </summary>
        /// <param name="tipoCambio">Datos actualizados del tipo de cambio.</param>
        /// <returns>Tipo de cambio actualizado.</returns>
        Task<GeneralResponse<tipoCambio>> PutTipoCambio(tipoCambio tipoCambio);

        /// <summary>
        /// Realiza una eliminación lógica del tipo de cambio (marca como inactivo).
        /// </summary>
        /// <param name="idTipoCambio">id del tipo de cambio a eliminar.</param>
        /// <returns>True si se eliminó correctamente, false si no.</returns>
        Task<GeneralResponse<tipoCambio>> DeleteTipoCambio(tipoCambio tipoCambio);
    }
}
