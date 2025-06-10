using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class anticipoDetailLi
{
    public int id { get; set; }

    public int idAnticipo { get; set; }

    public string compania { get; set; } = null!;

    public string? descripcion { get; set; }

    public int? cantidad { get; set; }

    public decimal? importe { get; set; }

    public int? idConceptoLis { get; set; }

    public virtual anticipoHeaderLi anticipoHeaderLi { get; set; } = null!;
}
