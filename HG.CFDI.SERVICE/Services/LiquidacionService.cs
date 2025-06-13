using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using System.Text.Json;

namespace HG.CFDI.SERVICE.Services
{
    public class LiquidacionService : ILiquidacionService
    {
        private readonly ILiquidacionRepository _repository;

        public LiquidacionService(ILiquidacionRepository repository)
        {
            _repository = repository;
        }

        public async Task<CfdiNomina?> ObtenerLiquidacion(string database, int noLiquidacion)
        {
            var json = await _repository.ObtenerDatosNominaJson(database, noLiquidacion);
            if (string.IsNullOrWhiteSpace(json))
                return null;
            try
            {
                return JsonSerializer.Deserialize<CfdiNomina>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
