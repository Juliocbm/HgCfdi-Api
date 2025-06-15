using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class archivoCFDi
{
    public int idArchivoCFDi { get; set; }

    public int no_guia { get; set; }

    public string num_guia { get; set; } = null!;

    public string compania { get; set; } = null!;

    public byte[] xml { get; set; } = null!;

    public byte[] pdf { get; set; } = null!;

    public DateTime? fechaCreacion { get; set; }

    public string? uuid { get; set; }

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
