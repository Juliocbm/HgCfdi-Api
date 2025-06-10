using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class liquidacionError
{
    public int id { get; set; }

    public int idLiquidacion { get; set; }

    public string compania { get; set; } = null!;

    public string? error { get; set; }

    public DateTime fechaCreacion { get; set; }

    public string? origen { get; set; }
}
