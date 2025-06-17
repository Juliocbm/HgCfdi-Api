using System.Threading.Tasks;
using CFDI.Data.Entities;
using HG.CFDI.CORE.Models;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionRepository
    {
        Task<string?> ObtenerDatosNominaJson(string database, int idLiquidacion);
        Task<List<LiquidacionDto>> ObtenerLiquidacionesAsync(ParametrosGenerales parametros, string database);

        Task InsertarDocTimbradoLiqAsync(int idCompania, int idLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid);

        Task<liquidacionOperador?> ObtenerCabeceraAsync(int idCompania, int idLiquidacion);

        Task RegistrarInicioIntentoAsync(int idCompania, int idLiquidacion, byte estatus, string liquidacionJson, string? mensajeCorto = null);

        Task ActualizarResultadoIntentoAsync(int idCompania, int idLiquidacion, byte estatus, DateTime? fechaProximoIntento = null, string? mensajeCorto = null);

        Task RegistrarErrorIntentoAsync(int idCompania, int idLiquidacion, short numeroIntento, string error);
    }
}
