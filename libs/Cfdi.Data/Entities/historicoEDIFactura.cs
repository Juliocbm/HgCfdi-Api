using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class historicoEDIFactura
{
    public int idHistorico { get; set; }

    public string compania { get; set; } = null!;

    public int idCliente { get; set; }

    public int noGuia { get; set; }

    public bool enviado { get; set; }

    public string mensaje { get; set; } = null!;

    public DateTime fechaTimbrado { get; set; }

    public DateTime fechaCreacion { get; set; }
}
