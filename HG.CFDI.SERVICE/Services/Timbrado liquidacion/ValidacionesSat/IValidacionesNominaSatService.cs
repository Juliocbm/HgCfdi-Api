using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;

namespace HG.CFDI.SERVICE.Services.Timbrado_liquidacion.ValidacionesSat
{
    public interface IValidacionesNominaSatService
    {
        Task<BuzonE.RequestBE> ConstruirRequestBuzonEAsync(CfdiNomina liquidacion, string database);
    }
}
