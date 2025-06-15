using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorteMercancium
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string compania { get; set; } = null!;

    public decimal cantidad { get; set; }

    public decimal? peso { get; set; }

    public string? claveProdServ { get; set; }

    public string? fraccionArancelaria { get; set; }

    public string? pedimento { get; set; }

    public string? descripcion { get; set; }

    public string? claveUnidad { get; set; }

    public string? claveUnidadPeso { get; set; }

    public string? esMaterialPeligroso { get; set; }

    public string? cveMaterialPeligroso { get; set; }

    public decimal? valorMercancia { get; set; }

    public string? descripcionMateria { get; set; }

    public string? tipoMateria { get; set; }

    public DateTime? fechaInsert { get; set; }

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
