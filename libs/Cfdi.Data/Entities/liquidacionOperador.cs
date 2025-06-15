using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class liquidacionOperador
{
    public int IdLiquidacion { get; set; }

    public int IdCompania { get; set; }

    public DateTime FechaRegistro { get; set; }

    public byte Estatus { get; set; }

    public short Intentos { get; set; }

    public DateTime? FechaProximoIntento { get; set; }

    public string? UUID { get; set; }

    public byte[]? XMLTimbrado { get; set; }

    public byte[]? PDFTimbrado { get; set; }

    public string? MensajeCorto { get; set; }

    public short UltimoIntento { get; set; }
}
