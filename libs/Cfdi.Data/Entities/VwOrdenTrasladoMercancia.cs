using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class VwOrdenTrasladoMercancia
{
    public int no_guia { get; set; }

    public string num_guia { get; set; } = null!;

    public string compania { get; set; } = null!;

    public string? claveProductoSat { get; set; }

    public string? descripcionProducto { get; set; }

    public int cantidad { get; set; }

    public string? claveUnidad { get; set; }

    public string? esMaterialPeligroso { get; set; }

    public string claveMaterialPeligroso { get; set; } = null!;

    public decimal? pesoKg { get; set; }

    public string? nombreMercancia { get; set; }
}
