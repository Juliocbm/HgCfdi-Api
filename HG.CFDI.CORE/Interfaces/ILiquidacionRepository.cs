using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionRepository
    {
        Task<string?> ObtenerDatosNominaJson(string database, int noLiquidacion);

        Task ActualizarEstatusAsync(string database, int noLiquidacion, int estatus);

        Task InsertarDocTimbradoLiqAsync(string database, int noLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid);

        Task InsertarHistoricoAsync(string database, int noLiquidacion, string liquidacionJson);
    }
}
