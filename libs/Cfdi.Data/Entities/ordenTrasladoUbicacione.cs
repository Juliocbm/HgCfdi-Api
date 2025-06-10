using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class ordenTrasladoUbicacione
{
    public int id { get; set; }

    public int? no_guia { get; set; }

    public string? num_guia { get; set; }

    public string? compania { get; set; }

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

    public virtual ordenTrasladoCabecera? ordenTrasladoCabecera { get; set; }
}
