using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Models
{
    public class UniqueRequest<T>
    {
        public UniqueRequest()
        {
            Errores = new List<string>();
        }
        public T request {  get; set; }
        public List<string> Errores { get; set; }
        public bool IsSuccess { get; set; }
        public string Mensaje { get; set; }

    }
}

