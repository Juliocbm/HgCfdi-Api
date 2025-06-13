using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionRepository
    {
        Task<string?> ObtenerDatosNominaJson(string database, int noLiquidacion);

        Task ActualizarEstatusAsync(string database, int noLiquidacion, int estatus);

        Task InsertarHistoricoAsync(string database, int noLiquidacion, int estatus, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid);
    }
}
