using Azure.Core;
using CFDI.Data.Entities;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Models;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.SERVICE.Services.Timbrado.ValidacionesSat
{
    public interface IValidacionesSatService
    {
        Task<CartaPortePendiente<FacturaCartaPorte>> getCartaPorteRequestLis(cartaPorteCabecera cp);
        GeneralResponse<string> getCartaPorteRequestInvoiceOne(cartaPorteCabecera ccps, string database);
        Task<UniqueRequest<BuzonE.RequestBE>> getCartaPorteRequestBuzonE(cartaPorteCabecera ccps, string database);
    }
}
