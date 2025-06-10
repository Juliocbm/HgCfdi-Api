using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class liquidacionDetailLi
{
    public int id { get; set; }

    public int idLiquidacion { get; set; }

    public string compania { get; set; } = null!;

    public string? descripcion { get; set; }

    public int? cantidad { get; set; }

    public decimal? unitPrice { get; set; }

    public decimal? importe { get; set; }

    public int? pk_concepto_ref { get; set; }

    public int id_area { get; set; }

    public int no_viaje { get; set; }

    public virtual liquidacionHeaderLi liquidacionHeaderLi { get; set; } = null!;
}
