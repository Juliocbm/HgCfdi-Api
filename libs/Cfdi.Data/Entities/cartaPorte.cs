using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorte
{
    public int idCartaPorte { get; set; }

    public int idArchivoCFDi { get; set; }

    public int noViaje { get; set; }

    public bool recibido { get; set; }

    public DateTime? fechaCreacion { get; set; }

    public DateTime? fechaModificacion { get; set; }

    public DateTime? fechaGeneracionArchivo { get; set; }
}
