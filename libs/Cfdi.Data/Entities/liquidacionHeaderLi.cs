using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class liquidacionHeaderLi
{
    public int id { get; set; }

    public int idLiquidacion { get; set; }

    public string compania { get; set; } = null!;

    public string claveLiquidacion { get; set; } = null!;

    public int? id_operador { get; set; }

    public string? id_unidad { get; set; }

    public string? status { get; set; }

    public decimal? total { get; set; }

    public int? estatusTraslado { get; set; }

    public int tipoLiquidacion { get; set; }

    public string? referenciaExterna { get; set; }

    public string? idUnidadLis { get; set; }

    public int? idOperadorLis { get; set; }

    public DateTime? fechaLiquidacion { get; set; }

    public int? idArea { get; set; }

    public virtual ICollection<liquidacionDetailLi> liquidacionDetailLis { get; set; } = new List<liquidacionDetailLi>();

    public virtual ICollection<liquidacionViajeDetailLi> liquidacionViajeDetailLis { get; set; } = new List<liquidacionViajeDetailLi>();
}
