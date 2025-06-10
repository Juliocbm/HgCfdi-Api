using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class catalogoCodigoPostal
{
    public int id { get; set; }

    public string cp { get; set; } = null!;

    public string? estado { get; set; }

    public string? municipio { get; set; }

    public string? localidad { get; set; }
}
