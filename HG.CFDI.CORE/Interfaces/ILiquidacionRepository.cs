using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionRepository
    {
        Task<string?> ObtenerDatosNominaJson(string database, int idLiquidacion);

        Task ActualizarEstatusAsync(string database, int idLiquidacion, byte estatus);

        Task InsertarDocTimbradoLiqAsync(string database, int idLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid);

        Task InsertarHistoricoAsync(string database, int idLiquidacion, string liquidacionJson);
    }
}
