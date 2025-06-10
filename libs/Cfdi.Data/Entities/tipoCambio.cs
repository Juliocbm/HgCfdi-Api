using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class tipoCambio
{
    public int id { get; set; }

    public DateTime fecha { get; set; }

    public decimal valor { get; set; }

    public bool? activo { get; set; }

    public Guid? creadoPor { get; set; }

    public DateTime fechaCreacion { get; set; }

    public Guid? modificadoPor { get; set; }

    public DateTime? fechaModificacion { get; set; }
}
