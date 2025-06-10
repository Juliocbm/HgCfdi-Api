using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class errorTimbradoGeneral
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string num_guia { get; set; } = null!;

    public string compania { get; set; } = null!;

    public string? error { get; set; }

    public int? idOperadorLis { get; set; }

    public string? idUnidadLis { get; set; }

    public string? idRemolqueLis { get; set; }

    public DateTime? fechaInsert { get; set; }

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
