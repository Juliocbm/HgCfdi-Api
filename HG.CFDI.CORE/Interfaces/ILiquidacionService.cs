using System.Threading.Tasks;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using HG.CFDI.CORE.Models;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionService
    {
        Task<CfdiNomina?> ObtenerLiquidacion(int idCompania, int noLiquidacion);
        Task<UniqueResponse> TimbrarLiquidacionAsync(int idCompania, int noLiquidacion);
    }
}
