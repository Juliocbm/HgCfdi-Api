using System.Threading.Tasks;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using HG.CFDI.CORE.Models;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionService
    {
        Task<CfdiNomina?> ObtenerLiquidacion(string database, int noLiquidacion);
        Task<UniqueResponse> TimbrarLiquidacionAsync(string database, int noLiquidacion);
    }
}
