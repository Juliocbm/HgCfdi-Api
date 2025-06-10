using BuzonE;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
using CFDI.Data.Entities;

namespace HG.CFDI.SERVICE.Services.ValidacionesSat
{
    public class TrucksService : ITrucksService
    {
        private readonly ICartaPorteRepository _cartaPorteRepository;
        //private readonly IApiCcpRyder _apiCcpRyder;
        private readonly string _sufijoArchivoCfdi;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CartaPorteService> _logger;
        //private readonly FirmaDigitalOptions _firmaDigitalOptions;
        private readonly List<FirmaDigitalOptions> _firmaDigitalOptions;
        private readonly InvoiceOneApiOptions _invoiceOneOptions;
        private readonly List<BuzonEApiCredential> _buzonEApiCredentials;
        private readonly LisApiOptions _lisApiOptions;
        private readonly RyderApiOptions _ryderApiOptions;
        private readonly List<compania> _companias;

        public TrucksService(ICartaPorteRepository cartaPorteRepository,
            IConfiguration configuration,
            IOptions<List<FirmaDigitalOptions>> firmaDigitalOptions,
            IOptions<InvoiceOneApiOptions> invoiceOneOptions,
            IOptions<List<BuzonEApiCredential>> buzonEOptions,
            IOptions<LisApiOptions> lisApiOptions,
            IOptions<RyderApiOptions> ryderApiOptions,
            IOptions<List<compania>> companiaOptions,
            //IApiCcpRyder apiCcpRyder,
            ILogger<CartaPorteService> logger)
        {
            _configuration = configuration;
            _cartaPorteRepository = cartaPorteRepository;
            _firmaDigitalOptions = firmaDigitalOptions.Value;
            _invoiceOneOptions = invoiceOneOptions.Value;
            _buzonEApiCredentials = buzonEOptions.Value;
            _lisApiOptions = lisApiOptions.Value;
            _ryderApiOptions = ryderApiOptions.Value;
            _companias = companiaOptions.Value;
            _sufijoArchivoCfdi = configuration.GetValue<string>("SufijoNombreCfdi");
            //_apiCcpRyder = apiCcpRyder;
            _logger = logger;
        }

        public async Task<bool> trasladaUuidToTrucks(archivoCFDi archivos)
        {
            try
            {
                return await _cartaPorteRepository.trasladaUuidToTrucks(archivos);
            }
            catch (System.Exception ex)
            {
                //await insertError(archivos.no_guia, archivos.num_guia, archivos.compania, "Fallo al trasladar el folio fiscal [modulo complemento de pago]", null, null, null);
                //await insertError(archivos.no_guia, archivos.num_guia, archivos.compania, ex.Message, null, null, null);
                return false;
            }
        }
    }
}
