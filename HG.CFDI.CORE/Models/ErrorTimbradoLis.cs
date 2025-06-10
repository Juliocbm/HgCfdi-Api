using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.LIS_INTERFACE.GENERAL.CORE.Models
{
    public class ErrorTimbradoLis
    {
        public int id { get; set; }
        public DateTime fecha_insert { get; set; }
        public string Num_guia { get; set; }
        public string Error { get; set; }
        public int? idOperador_Lis { get; set; }
        public string idUnidad_Lis { get; set; }
        public string idRemolque_Lis { get; set; }



        
    }

}
