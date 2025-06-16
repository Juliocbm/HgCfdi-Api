using System.Threading.Tasks;
using CFDI.Data.Entities;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionRepository
    {
        Task<string?> ObtenerDatosNominaJson(string database, int idLiquidacion);

        Task InsertarDocTimbradoLiqAsync(int idCompania, int idLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid);

        Task<liquidacionOperador?> ObtenerCabeceraAsync(int idCompania, int idLiquidacion);

        Task RegistrarInicioIntentoAsync(int idCompania, int idLiquidacion, byte estatus, string liquidacionJson);

        Task ActualizarResultadoIntentoAsync(int idCompania, int idLiquidacion, byte estatus, DateTime? fechaProximoIntento = null);

        Task RegistrarErrorIntentoAsync(int idCompania, int idLiquidacion, short numeroIntento, string error);
    }
}
