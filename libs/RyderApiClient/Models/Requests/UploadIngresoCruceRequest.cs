using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryder.Api.Client.Models.Requests
{
    /// <summary>
    /// Request para UploadIngresoCruce (incluye datos de factura).
    /// </summary>
    public class UploadIngresoCruceRequest : BaseRequest
    {
        /// <summary>
        /// Identificador de la operación.
        /// </summary>
        public int OperacionID { get; set; }

        /// <summary>
        /// Identificador del viaje.
        /// </summary>
        public int ViajeID { get; set; }

        /// <summary>
        /// PDF de ingreso en Base64.
        /// </summary>
        public string PdfBase64 { get; set; } = default!;

        /// <summary>
        /// XML de ingreso en Base64.
        /// </summary>
        public string XmlBase64 { get; set; } = default!;

        /// <summary>
        /// Fecha de la factura (formato ISO 8601: "yyyy-MM-dd").
        /// </summary>
        public string FechaFactura { get; set; } = default!;

        /// <summary>
        /// Folio de la factura.
        /// </summary>
        public string FolioFactura { get; set; } = default!;
    }
}
