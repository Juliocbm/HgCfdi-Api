using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryder.Api.Client.Models.Responses
{
    public class BaseResponse
    {
        public string Estatus { get; set; }
        public string Error { get; set; }
        public string CSVFileBase64 { get; set; }
        public string Datos { get; set; }
    }
}
