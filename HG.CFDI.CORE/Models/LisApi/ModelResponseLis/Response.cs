using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Models.LisApi.ModelResponseLis
{
    public class Response
    {
        public string NumFactura { get; set; }
        public string IM_XML { get; set; }
        public string IM_PDF { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }

        [JsonProperty("completed_successfully")]
        public string CompletedSuccessfully { get; set; }

        [JsonProperty("mensajes")]
        public List<Mensaje> Mensajes { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }

    }

    public class Mensaje
    {
        public string IdMessages { get; set; }
        public string Descripcion { get; set; }
        public string Referencia { get; set; }
    }

}
