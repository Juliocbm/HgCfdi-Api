using System.Threading.Tasks;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionService
    {
        Task<CfdiNomina?> ObtenerLiquidacion(string database, int noLiquidacion);
    }
}
