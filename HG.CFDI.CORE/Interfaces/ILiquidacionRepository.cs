using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface ILiquidacionRepository
    {
        Task<string?> ObtenerDatosNominaJson(string database, int noLiquidacion);
    }
}
