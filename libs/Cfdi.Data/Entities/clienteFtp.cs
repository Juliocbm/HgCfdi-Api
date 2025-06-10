using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class clienteFtp
{
    public int idClienteFtp { get; set; }

    public string compania { get; set; } = null!;

    public int idCliente { get; set; }

    public string nombreCliente { get; set; } = null!;

    public string scacCliente { get; set; } = null!;

    public string scacPropio { get; set; } = null!;

    public bool productivo { get; set; }

    public bool SSH { get; set; }

    public string servidor { get; set; } = null!;

    public int puerto { get; set; }

    public string usuario { get; set; } = null!;

    public string contrasenia { get; set; } = null!;

    public string carpetaRepositorio { get; set; } = null!;

    public string carpetaRemota { get; set; } = null!;

    public bool activo { get; set; }

    public DateTime fechaCreacion { get; set; }

    public virtual ICollection<clienteFtpDetalle> clienteFtpDetalles { get; set; } = new List<clienteFtpDetalle>();
}
