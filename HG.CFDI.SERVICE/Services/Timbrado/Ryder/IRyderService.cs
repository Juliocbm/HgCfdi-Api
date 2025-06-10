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

namespace HG.CFDI.SERVICE.Services.Timbrado.Ryder
{
    public interface IRyderService
    {
        Task<UniqueResponse> enviaCfdiToRyder(cartaPorteCabecera cp);
        Task<bool> ProcesarRyderAsync(cartaPorteCabecera cartaPorte, byte[] xmlBytes, byte[] pdfBytes);
    }
}
