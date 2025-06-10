using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorteAddendum
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string compania { get; set; } = null!;

    public int? idClienteLis { get; set; }

    public string? descripcion { get; set; }

    public string? valor { get; set; }

    public DateTime? fechaInsert { get; set; }

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
