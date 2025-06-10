using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorteSustitucion
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string compania { get; set; } = null!;

    public string motivoRelacion { get; set; } = null!;

    public string num_guia { get; set; } = null!;

    public string uuid { get; set; } = null!;

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
