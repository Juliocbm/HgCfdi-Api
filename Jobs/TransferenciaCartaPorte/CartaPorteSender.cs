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
    public class CartaPorteSender
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _userZam;
        private readonly string _passZam;
        private readonly ILogger<CartaPorteServiceApi> _logger;
        private readonly List<compania> _companias;


        public CartaPorteSender(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<CartaPorteServiceApi> logger, IOptions<List<compania>> companiaOptions)
        {
            _userZam = configuration.GetValue<string>("LisApi:user");
            _passZam = configuration.GetValue<string>("LisApi:password");
            _companias = companiaOptions.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        public async Task EnviarCartasPorte()
        {
            _logger.LogInformation("entre al job de timbrado");

            Task.Run(async () =>
            {
                // Recorriendo cada empresa con foreach
                foreach (var compania in _companias)
                {
                    if (!compania.Timbrar)
                        continue; 

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var cartaPorteService = scope.ServiceProvider.GetRequiredService<ICartaPorteService>();
                        var cartaPorteServiceApi = scope.ServiceProvider.GetRequiredService<ICartaPorteServiceApi>();

                        var cartasPorte = await cartaPorteService.getCartasPortePendiente(compania.id);

                        if (cartasPorte.Any())
                        {
                            var tasks = cartasPorte.Select(async x =>
                            {
                                await cartaPorteService.TrySetTimbradoEnProcesoAsync(x.no_guia, x.compania);
                            });
                            await Task.WhenAll(tasks);
                        }

                        foreach (var cp in cartasPorte)
                        {
                            await cartaPorteService.deleteErrors(cp.no_guia, cp.compania);
                            try
                            {
                                switch ((SistemaTimbrado)cp.sistemaTimbrado)
                                {
                                    case SistemaTimbrado.Lis:
                                        await cartaPorteService.timbrarConLis(cartaPorteServiceApi, cp);
                                        break;
                                    case SistemaTimbrado.BuzonE:
                                        await cartaPorteService.timbrarConBuzonE(cp, compania.Database);
                                        break;
                                    case SistemaTimbrado.InvoiceOne:
                                        await cartaPorteService.timbrarConInvoiceOne(cp, compania.Database);
                                        break;
                                    default:
                                        await cartaPorteService.timbrarConLis(cartaPorteServiceApi, cp);
                                        break;
                                }
                               
                            }
                            catch (Exception err)
                            {
                                await cartaPorteService.changeStatusCartaPorteAsync(cp.no_guia, cp.num_guia, cp.compania, 2, "Contiene errores de timbrado", cp.sistemaTimbrado);
                                await cartaPorteService.insertError(cp.no_guia, cp.num_guia, cp.compania, err.Message, cp.idOperadorLis, cp.idUnidadLis, cp.idRemolqueLis);
                            }
                        }
                    }
                }
            }).GetAwaiter().GetResult();

        }
    }
}
