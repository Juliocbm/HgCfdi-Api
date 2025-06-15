using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class liquidacionOperadorHistError
{
    public long IdError { get; set; }

    public long IdHistorico { get; set; }

    public int IdLiquidacion { get; set; }

    public int IdCompania { get; set; }

    public short NumeroIntento { get; set; }

    public string TextoError { get; set; } = null!;
}
