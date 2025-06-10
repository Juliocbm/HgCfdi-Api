using Azure.Core;
using BuzonE;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using HG.CFDI.DATA.LisApi;
using HG.CFDI.DATA.Repositories;
using HG.CFDI.SERVICE.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HG.CFDI.API.Jobs.TransferenciaCartaPorte
{
    public class ImportData
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _userZam;
        private readonly string _passZam;
        private readonly ILogger<CartaPorteServiceApi> _logger;
        private readonly List<compania> _companias;


        public ImportData(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<CartaPorteServiceApi> logger, IOptions<List<compania>> companiaOptions)
        {
            _userZam = configuration.GetValue<string>("LisApi:user");
            _passZam = configuration.GetValue<string>("LisApi:password");
            _companias = companiaOptions.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        public async Task ImportarCartasPorte()
        {
            _logger.LogInformation("entre al job de importacion de datos");

            Task.Run(async () =>
            {       
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var cartaPorteService = scope.ServiceProvider.GetRequiredService<ICartaPorteService>();
                        //await cartaPorteService.importarCartasPorteServer2008();
                    }                
            }).GetAwaiter().GetResult();

        }
    }
}
