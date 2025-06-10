using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class ordenTrasladoMercancia
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string? num_guia { get; set; }

    public string compania { get; set; } = null!;

    public string? claveProductoSat { get; set; }

    public string? descripcionProducto { get; set; }

    public int? cantidad { get; set; }

    public string? claveUnidad { get; set; }

    public string? esMaterialPeligroso { get; set; }

    public string? claveMaterialPeligroso { get; set; }

    public decimal? pesoKg { get; set; }

    public string? nombreMercancia { get; set; }

    public virtual ordenTrasladoCabecera ordenTrasladoCabecera { get; set; } = null!;
}
