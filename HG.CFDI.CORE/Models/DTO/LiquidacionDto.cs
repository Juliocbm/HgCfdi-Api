using System;

namespace HG.CFDI.CORE.Models.DtoLiquidacionCfdi
{
    public class LiquidacionDto
    {
        public int IdLiquidacion { get; set; }
        public string Nombre { get; set; }
        public string Rfc { get; set; }
        public DateTime Fecha { get; set; }
        public short Intentos { get; set; }
        public DateTime? ProximoIntento { get; set; }
        public string? Xml { get; set; }
        public string? Pdf { get; set; }
        public string? Uuid { get; set; }
    }
}
