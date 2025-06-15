using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorteOperacionRyder
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string compania { get; set; } = null!;

    public int idOperacionRyder { get; set; }

    public int idViajeRyder { get; set; }

    public DateTime? fechaInsert { get; set; }

    public bool estatusEnvio { get; set; }

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
