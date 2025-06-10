using BuzonE;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using System.Globalization;
using GeneraPdfBuzonE;

namespace HG.CFDI.SERVICE.Services.ValidacionesSat
{
    public class UtilsService : IUtilsService
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

        public UtilsService(ICartaPorteRepository cartaPorteRepository,
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



        public bool ValidaStringVariable(string cadena)
        {
            // Definir la expresión regular
            string pattern = @"La carta porte [A-Za-z0-9\-]+ ya existe en el sistema ZAM\.";


            // Crear una instancia de Regex
            Regex regex = new Regex(pattern);

            // Verificar si la cadena coincide con el patrón
            return regex.IsMatch(cadena);
        }

        public List<string> GetAllExceptionMessages(System.Exception ex)
        {
            var messages = new List<string>();
            while (ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }
            return messages;
        }
    }
}
