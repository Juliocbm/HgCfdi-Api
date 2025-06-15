using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class liquidacionOperadorHist
{
    public long IdHistorico { get; set; }

    public int IdLiquidacion { get; set; }

    public int IdCompania { get; set; }

    public DateTime FechaIntento { get; set; }

    public short NumeroIntento { get; set; }

    public string? SnapshotData { get; set; }

    public string EstadoIntento { get; set; } = null!;
}
