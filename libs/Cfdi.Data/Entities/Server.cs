using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class Server
{
    public string id { get; set; } = null!;

    public string? Data { get; set; }

    public DateTime LastHeartbeat { get; set; }
}
