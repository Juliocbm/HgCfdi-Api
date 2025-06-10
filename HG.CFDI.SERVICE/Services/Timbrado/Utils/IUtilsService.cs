using Azure.Core;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Models;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using HG.CFDI.DATA.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Interfaces
{
    public interface IUtilsService
    {
        
        bool ValidaStringVariable(string cadena);
        List<string> GetAllExceptionMessages(System.Exception ex);
    }
}
