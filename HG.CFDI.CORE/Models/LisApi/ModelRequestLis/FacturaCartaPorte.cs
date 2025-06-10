

namespace HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte
{
    public class CartaPortePendiente<T>
    {
        public int no_guia { get; set; }
        public string num_guia { get; set; } = null!;
        public string? StatusGuia { get; set; } = null!;
        public string compania { get; set; } = null!;
        public int sistemaTimbrado { get; set; }
        public T facturaCartaPorte { get; set; }
    }
    public class FacturaCartaPorte
    {
        public string Serie { get; set; }
        public int Folio { get; set; }
        public DateTime? FechaDoc { get; set; }
        public int? IdCliente { get; set; }
        public DateTime FechaCita { get; set; }
        public string? IdRemolque { get; set; }
        public string? IdDolly { get; set; }
        public string? IdRemolque2 { get; set; }
        public int IdTipoOperacion { get; set; }
        public string NumeroOperacion { get; set; }
        public int? IdOperador { get; set; }
        public int? idRuta { get; set; }
        public int? idArea { get; set; }
        public string montoFlete { get; set; }
        public string IdLineaRem1 { get; set; }
        public string IdLineaRem2 { get; set; }
        public string observacionesCartaPorte { get; set; } //nuevo campo 22/mar/2024
        public string referenciaCliente { get; set; } //nuevo campo 22/mar/2024
        public string? IdUnidad { get; set; }
        public bool es_LogisticaInversa { get; set; }



        public decimal? pesoBrutoVehicular { get; set; }
        public string? claveRegimenAduanero { get; set; }
        public List<Addenda>? Addenda { get; set; }
        public List<Concepto> Conceptos { get; set; }
        public List<Ubicacion>? Ubicaciones { get; set; }
        public List<Mercancia> Mercancias { get; set; }
        public List<DocumentoAduana> DocumentoAduana { get; set; }
    }

    public class DocumentoAduana
    {
        public string? numPedimento { get; set; }
        public string? numDocAduanero { get; set; }
        public string? rfcImportador { get; set; }
    }

    public class Ubicacion
    {
        public int? DistanciaRecorrida { get; set; }
        public int? IdRemitente { get; set; }
        public DateTime? FechaHoraSalidaRemitente { get; set; }
        public DateTime? FechaHoraLlegadaRemitente { get; set; }
        public int? IdDestinatario { get; set; }
        public DateTime? FechaHoraSalidaDestinatario { get; set; }
        public DateTime? FechaHoraLlegadaDestinatario { get; set; }

    }

    public class Mercancia
    {
        public int IdMercancia { get; set; }
        public string? BienesTransp { get; set; }
        public string? Descripcion { get; set; }
        public int? Cantidad { get; set; }
        public string? ClaveUnidad { get; set; }
        public string? ClaveUnidadPeso { get; set; }
        public string? MaterialPeligroso { get; set; }
        public string? CveMaterialPeligroso { get; set; }
        //public string IdEmbalaje { get; set; }
        public string? Embalaje { get; set; }
        public string? DescripEmbalaje { get; set; }
        public decimal? PesoEnKg { get; set; } // Usar double si puede haber decimales
        public string? FraccionArancelaria { get; set; }
        public Guid UUIDComercioExt { get; set; }
        public int? IDOrigen { get; set; }
        public int? IDDestino { get; set; }
        public string? claveTipoMateria { get; set; }

    }

    public class Concepto
    {
        public int? IdConcepto { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal? Importe { get; set; } // Usar decimal para valores monetarios
        public string? descripcion { get; set; }
        public string? NoIdentificacion { get; set; }
        public string? referenciaConcepto { get; set; } //nuevo campo 22/mar/2024
    }

    public class Addenda
    {
        public int? IdCliente { get; set; }
        public int Consecutivo { get; set; }
        public string Descripcion { get; set; }
        public string Valor { get; set; } // Considera el tipo adecuado, si es un valor numérico o textual
    }

}
