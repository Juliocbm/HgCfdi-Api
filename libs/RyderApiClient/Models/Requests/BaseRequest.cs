using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryder.Api.Client.Models.Requests
{
    /// <summary>
    /// Propiedades comunes a todos los requests de Ryder.
    /// </summary>
    public abstract class BaseRequest
    {
        /// <summary>
        /// Correo del usuario que consume la API.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// AccessKey proporcionada por Ryder para autenticar la petición.
        /// </summary>
        public string AccessKey { get; set; } = default!;
    }
}
