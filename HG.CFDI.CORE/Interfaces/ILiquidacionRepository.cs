using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionRepository
    {
        Task<string?> ObtenerDatosNominaJson(string database, int idLiquidacion);

        Task ActualizarEstatusAsync(int idCompania, int idLiquidacion, byte estatus);

        Task InsertarDocTimbradoLiqAsync(int idCompania, int idLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid);

        Task InsertarHistoricoAsync(int idCompania, int idLiquidacion, string liquidacionJson);
    }
}
