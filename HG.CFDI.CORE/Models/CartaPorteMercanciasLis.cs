using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HG.LIS_INTERFACE.GENERAL.CORE.Models
{
    public partial class CartaPorteMercanciasLis
    {
        public int id { get; set; }
        public int no_guia { get; set; }
        //public string? Ciaref { get; set; } = null!;
        //public string Database { get; set; } = null!;
        public string compania { get; set; } = null!;
        public decimal? Cantidad { get; set; }
        public decimal? Peso { get; set; }
        public int? ClaveProductoSat { get; set; }
        public string? desc_producto { get; set; }
        public string? clave_unidad { get; set; }
        public string? clave_unidad_peso { get; set; }
        public string? MaterialPeligroso { get; set; }
        public string? CveMaterialPeligroso { get; set; }
        public decimal? cpe_valorMercancia { get; set; }
        public string? cpe_descripcionMateria { get; set; }
        public string? cpe_tipoMateria { get; set; }

        [JsonIgnore]
        public virtual CartaPorteHeaderLis? CartaPorteHeader { get; set; } = null!;
    }
}
