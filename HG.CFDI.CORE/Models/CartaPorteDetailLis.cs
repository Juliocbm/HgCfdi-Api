using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HG.LIS_INTERFACE.GENERAL.CORE.Models
{
    public partial class CartaPorteDetailLis
    {
        public int id { get; set; }
        public decimal? Quantity { get; set; }
        public string? Warehouse { get; set; }
        public decimal? Flete { get; set; }
        public decimal? Subtotal { get; set; }
        public string? Embalaje { get; set; }
        public string? ProductoDes { get; set; }
        public string? InventoryId { get; set; }
        public int no_guia { get; set; }
        //public string Ciaref { get; set; } = null!;
        public string compania { get; set; } = null!;
        public DateTime? DateInsert { get; set; }
        public string? TaxCategory { get; set; }
        public int? idConcepto_lis { get; set; }

        [JsonIgnore]
        public virtual CartaPorteHeaderLis? CartaPorteHeader { get; set; } = null!;
    }
}
