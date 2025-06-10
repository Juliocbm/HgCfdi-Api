using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class vwTipoCambio
{
    public int id { get; set; }

    public DateTime fecha { get; set; }

    public decimal valor { get; set; }

    public bool activo { get; set; }

    public DateTime fechaCreacion { get; set; }

    public DateTime? fechaModificacion { get; set; }

    public Guid? creadoPor { get; set; }

    public Guid? modificadoPor { get; set; }

    public string usuarioCreadoPor { get; set; } = null!;

    public string usuarioModificadoPor { get; set; } = null!;
}
