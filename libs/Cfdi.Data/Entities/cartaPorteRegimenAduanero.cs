using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorteRegimenAduanero
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string compania { get; set; } = null!;

    public string regimenAduanero { get; set; } = null!;

    public DateTime? fechaInsert { get; set; }

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
