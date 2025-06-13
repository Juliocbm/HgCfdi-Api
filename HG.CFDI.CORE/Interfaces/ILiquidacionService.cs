using System.Threading.Tasks;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using BuzonE;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionService
    {
        Task<CfdiNomina?> ObtenerLiquidacion(string database, int noLiquidacion);
        Task<BuzonE.RequestBE> ConstruirRequestBuzonEAsync(CfdiNomina liquidacion, string database);
    }
}
