using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Models
{
    public class UniqueResponse
    {
        public UniqueResponse()
        {
            Errores = new List<string>();
        }
        public byte[] XmlByteArray { get; set; }
        public byte[] PdfByteArray { get; set; }
        public List<string> Errores { get; set; }
        public bool IsSuccess { get; set; }
        public string Mensaje { get; set; }

    }
}
