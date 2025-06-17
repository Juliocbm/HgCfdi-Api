using System.Threading.Tasks;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using HG.CFDI.CORE.Models;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionService
    {
        Task<CfdiNomina?> ObtenerLiquidacion(int idCompania, int noLiquidacion);
        Task<GeneralResponse<LiquidacionDto>> ObtenerLiquidacionesAsync(ParametrosGenerales parametros, string database);
        Task<UniqueResponse> TimbrarLiquidacionAsync(int idCompania, int noLiquidacion);
        Task<UniqueResponse> ObtenerDocumentosTimbradosAsync(int idCompania, int idLiquidacion);
    }
}
