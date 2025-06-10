
using HG.CFDI.CORE.Interfaces;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using HG.CFDI.DATA.LisApi;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using HG.CFDI.CORE.Models;
using GeneraPdfBuzonE;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
using BuzonE;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using System.Globalization;
using HG.CFDI.CORE.Models.LisApi.ModelResponseLis;
using XSDToXML.Utils;
using System.Data.SqlTypes;
using System.IO;
using System.Xml.Xsl;
using HG.CFDI.DATA.Repositories;
using System.Drawing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using InvoiceOne;
using Azure;
using System.ServiceModel.Channels;
using Microsoft.Extensions.Logging;
using System.ServiceModel;
//using HG.CFDI.CORE.Models.TipoCambioEF;
using CFDI.Data.Entities;

namespace HG.CFDI.SERVICE.Services
{
    public class TipoCambioService : ITipoCambioService
    {
        private readonly ITipoCambioRepository _tipoCambioRepository;       
        private readonly IConfiguration _configuration;
        private readonly ILogger<CartaPorteService> _logger;      

        public TipoCambioService(ITipoCambioRepository tipoCambioRepository,
            IConfiguration configuration,
            ILogger<CartaPorteService> logger)
        {
            _configuration = configuration;
            _tipoCambioRepository = tipoCambioRepository;
            _logger = logger;
        }

        public Task<GeneralResponse<tipoCambio>> DeleteTipoCambio(tipoCambio tipoCambio)
        {
            return _tipoCambioRepository.DeleteTipoCambio(tipoCambio);
        }

        public Task<GeneralResponse<vwTipoCambio?>> GetTipoCambioById(int idTipoCambio)
        {
            return _tipoCambioRepository.GetTipoCambioById(idTipoCambio);
        }

        public Task<GeneralResponse<vwTipoCambio>> GetTiposCambio(ParametrosGenerales parametros)
        {
            return _tipoCambioRepository.GetTiposCambio(parametros);
        }

        public Task<GeneralResponse<tipoCambio>> PostTipoCambio(tipoCambio tipoCambio)
        {
            tipoCambio.fechaCreacion = DateTime.Now;
            tipoCambio.fechaModificacion = DateTime.Now;
            tipoCambio.fecha = DateTime.Now.Date;
            tipoCambio.activo = true;

            return _tipoCambioRepository.PostTipoCambio(tipoCambio);
        }

        public Task<GeneralResponse<tipoCambio>> PutTipoCambio(tipoCambio tipoCambio)
        {
            return _tipoCambioRepository.PutTipoCambio(tipoCambio);
        }


    }
}
