using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryder.Api.Client.Configuration
{
    public class RyderApiOptions
    {
        public string BaseUrl { get; set; } = default!;
        public string AccessKey { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string SubscriptionKey { get; set; } = default!;
    }
}
