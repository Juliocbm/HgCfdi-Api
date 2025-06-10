using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Models
{
    public class ResponseImportacion
    {
        public bool MigrationIsSuccess { get; set; }
        public string Mensaje { get; set; }

    }
}
