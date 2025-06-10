using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class clienteFtpDetalle
{
    public int idClienteFtpDetalle { get; set; }

    public int idClienteFtp { get; set; }

    public int idCliente { get; set; }

    public bool activo { get; set; }

    public DateTime fechaCreacion { get; set; }

    public virtual clienteFtp idClienteFtpNavigation { get; set; } = null!;
}
