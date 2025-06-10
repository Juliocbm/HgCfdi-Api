using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class anticipoError
{
    public int id { get; set; }

    public int idAnticipo { get; set; }

    public string compania { get; set; } = null!;

    public string? origen { get; set; }

    public string? error { get; set; }

    public DateTime fechaCreacion { get; set; }
}
