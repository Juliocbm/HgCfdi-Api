using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class JobQueue
{
    public long id { get; set; }

    public long JobId { get; set; }

    public string Queue { get; set; } = null!;

    public DateTime? FetchedAt { get; set; }
}
