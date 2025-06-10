using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class liquidacionViajeDetailLi
{
    public int id { get; set; }

    public int idLiquidacion { get; set; }

    public string compania { get; set; } = null!;

    public int no_viaje { get; set; }

    public string num_guia { get; set; } = null!;

    public decimal? sueldoOperador { get; set; }

    public virtual liquidacionHeaderLi liquidacionHeaderLi { get; set; } = null!;
}
