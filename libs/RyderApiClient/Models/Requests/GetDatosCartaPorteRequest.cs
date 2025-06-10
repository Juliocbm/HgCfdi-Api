using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryder.Api.Client.Models.Requests
{
    namespace Ryder.Api.Client.Models.Requests
    {
        /// <summary>
        /// Request para GetDatosCartaPorte.
        /// </summary>
        public class GetDatosCartaPorteRequest : BaseRequest
        {
            /// <summary>
            /// Identificador de la operación.
            /// </summary>
            public int OperacionID { get; set; }

            /// <summary>
            /// Identificador del viaje.
            /// </summary>
            public int ViajeID { get; set; }
        }
    }
}
