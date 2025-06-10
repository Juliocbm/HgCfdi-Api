using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class VwOrdenTrasladoUbicacione
{
    public int no_guia { get; set; }

    public string num_guia { get; set; } = null!;

    public string compania { get; set; } = null!;

    public string? idUbicacionRemitente { get; set; }

    public string? rfcRemitente { get; set; }

    public string? nombreRemitente { get; set; }

    public string? fechaSalida { get; set; }

    public string? paisRemitente { get; set; }

    public string? estadoRemitente { get; set; }

    public string? cpRemitente { get; set; }

    public string? idUbicacionDestinatario { get; set; }

    public string? rfcDestinatario { get; set; }

    public string? nombreDestinatario { get; set; }

    public string? fechaLlegada { get; set; }

    public string? paisDestinatario { get; set; }

    public string? estadoDestinatario { get; set; }

    public string? cpDestinatario { get; set; }
}
