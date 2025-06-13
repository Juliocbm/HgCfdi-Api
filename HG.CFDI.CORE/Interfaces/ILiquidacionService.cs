using System.Threading.Tasks;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using BuzonE;
using HG.CFDI.CORE.Models;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionService
    {
        Task<CfdiNomina?> ObtenerLiquidacion(string database, int noLiquidacion);
        Task<BuzonE.RequestBE> ConstruirRequestBuzonEAsync(CfdiNomina liquidacion, string database);

        Task<UniqueResponse> TimbrarLiquidacionAsync(string database, int noLiquidacion);
    }
}
