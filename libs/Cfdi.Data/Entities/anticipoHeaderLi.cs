using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class anticipoHeaderLi
{
    public int id { get; set; }

    public int idAnticipo { get; set; }

    public string compania { get; set; } = null!;

    public DateTime? fechaAnticipo { get; set; }

    public string? numGuia { get; set; }

    public int? noViaje { get; set; }

    public int? idOperadorLis { get; set; }

    public string? observaciones { get; set; }

    public int? estatusTraslado { get; set; }

    public int? idArea { get; set; }

    public int tipoAnticipo { get; set; }

    public string? idUnidadLis { get; set; }

    public virtual ICollection<anticipoDetailLi> anticipoDetailLis { get; set; } = new List<anticipoDetailLi>();
}
