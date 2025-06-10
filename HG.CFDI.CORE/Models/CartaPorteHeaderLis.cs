using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HG.LIS_INTERFACE.GENERAL.CORE.Models
{
    public partial class CartaPorteHeaderLis
    {
        public CartaPorteHeaderLis()
        {
            CartaPorteDetails = new HashSet<CartaPorteDetailLis>();
            cartaPorteMercancia = new HashSet<CartaPorteMercanciasLis>();
        }

        public int id { get; set; }
        public int no_guia { get; set; }
        public string num_guia { get; set; } = null!;
        public string? StatusGuia { get; set; } = null!;
        //public string? Ciaref { get; set; } = null!;
        //public string Database { get; set; } = null!;
        public string compania { get; set; } = null!;
        public string? OrderNbr { get; set; }
        //public int? EstatusInterface { get; set; }
        public int? EstatusTimbrado { get; set; }
        public string? Shipment { get; set; }
        public string? Caja1 { get; set; }
        [JsonIgnore]
        public string? Caja2 { get; set; }
        public string? Linea1 { get; set; }
        [JsonIgnore]
        public string? Linea2 { get; set; }
        public string? RecojerEn { get; set; }
        public string? EntregarEn { get; set; }
        public string? TransporteInternacional { get; set; }
        public string? EntSalMerc { get; set; }
        public string? Operador { get; set; }
        public string? Pedimento { get; set; }
        public decimal? Peso { get; set; }
        public string? Sello { get; set; }
        public string? Tractor { get; set; }
        public string? IdUnidad { get; set; }
        public DateTime? FechaInsert { get; set; }
        [JsonIgnore]
        public DateTime? FechaEnvioAcumatica { get; set; }
        public int? IdCliente { get; set; }
        public string? NombreCtePaga { get; set; }
        public string? EsExterno { get; set; }
        [JsonIgnore]
        public string? VendorId { get; set; }
        [JsonIgnore]
        public string? Currency { get; set; }
        public string? Viaje { get; set; }
        public string? Customer { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public string? PlacaRemolque1 { get; set; }
        public string? SubtipodeRemolque1 { get; set; }
        public string? PlacaRemolque2 { get; set; }
        public string? SubtipodeRemolque2 { get; set; }
        [JsonIgnore]
        public string? SfaddressLine1 { get; set; }
        [JsonIgnore]
        public string? SfcompanyName { get; set; }
        [JsonIgnore]
        public string? Sfcountry { get; set; }
        [JsonIgnore]
        public string? Sfcity { get; set; }
        [JsonIgnore]
        public string? Sfemail { get; set; }
        [JsonIgnore]
        public string? Sfphone1 { get; set; }
        [JsonIgnore]
        public string? SfpostalCode { get; set; }
        [JsonIgnore]
        public string? Sfstate { get; set; }
        [JsonIgnore]
        public string? SffechaEstimadadeSalida { get; set; }
        [JsonIgnore]
        public string? SfhoraEstimadadeSalida { get; set; }
        [JsonIgnore]
        public string? Sfrfc { get; set; }
        public string? Sfmunicipio { get; set; }
        public string? SfnumeroExterior { get; set; }
        public string? SfnumeroInterior { get; set; }
        [JsonIgnore]
        public string? StaddressLine1 { get; set; }
        [JsonIgnore]
        public string? StcompanyName { get; set; }
        [JsonIgnore]
        public string? Stcountry { get; set; }
        [JsonIgnore]
        public string? Stcity { get; set; }
        [JsonIgnore]
        public string? Stemail { get; set; }
        [JsonIgnore]
        public string? Stphone1 { get; set; }
        [JsonIgnore]
        public string? StpostalCode { get; set; }
        [JsonIgnore]
        public string? Ststate { get; set; }
        [JsonIgnore]
        public string? StfechaEstimadadeLlegada { get; set; }
        [JsonIgnore]
        public string? SthoraEstimadadeLlegada { get; set; }
        [JsonIgnore]
        public string? Strfc { get; set; }
        [JsonIgnore]
        public string? Stmunicipio { get; set; }
        [JsonIgnore]
        public string? StnumeroExterior { get; set; }
        [JsonIgnore]
        public string? StnumeroInterior { get; set; }
        [JsonIgnore]
        public int? IdprovRef { get; set; }
        [JsonIgnore]
        public string? MsjError { get; set; }
        [JsonIgnore]
        public string? Sfestado { get; set; }
        [JsonIgnore]
        public string? Stestado { get; set; }
        public decimal? TotalDistanciaRec { get; set; }

        public int? idCliente_Lis { get; set; }
        public int? idClienteRemitente_Lis { get; set; }
        public int? idClienteDestinatario_Lis { get; set; }
        public string? idUnidad_Lis { get; set; }
        public string? idRemolque_Lis { get; set; }
        public string? idRemolque2_Lis { get; set; }
        public int? idPlazaOr_Lis { get; set; }
        public int? idPlazaDe_Lis { get; set; }
        public int? idRuta_Lis { get; set; }
        public int? idOperador_Lis { get; set; }
        public string? idLineaRem1_Lis { get; set; }
        public string? idLineaRem2_Lis { get; set; }
        public int? idSucursal_Lis { get; set; }
        public decimal? pesoBrutoVehicular { get; set; }
        public string? claveRegimenAduanero { get; set; }
        


public virtual ICollection<CartaPorteDetailLis> CartaPorteDetails { get; set; }
        public virtual ICollection<CartaPorteMercanciasLis> cartaPorteMercancia { get; set; }
    }
}
