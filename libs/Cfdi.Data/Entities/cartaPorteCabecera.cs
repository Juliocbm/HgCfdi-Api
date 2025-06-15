using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorteCabecera
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string num_guia { get; set; } = null!;

    public string statusGuia { get; set; } = null!;

    public string compania { get; set; } = null!;

    public string? observacionesPedido { get; set; }

    public int? sistemaTimbrado { get; set; }

    public int? estatusTimbrado { get; set; }

    public string? mensajeTimbrado { get; set; }

    public DateTime? fechaInsert { get; set; }

    public string? shipment { get; set; }

    public string? shipperAccount { get; set; }

    public string? moneda { get; set; }

    public int? diasCredito { get; set; }

    public string? formaPago { get; set; }

    public string? metodoPago { get; set; }

    public string? cteEmisorNombre { get; set; }

    public string? cteEmisorRfc { get; set; }

    public string? cteEmisorCalle { get; set; }

    public string? cteEmisorNoExterior { get; set; }

    public string? cteEmisorColonia { get; set; }

    public string? cteEmisorLocalidad { get; set; }

    public string? cteEmisorPais { get; set; }

    public string? cteEmisorCp { get; set; }

    public string? cteEmisorRegimenFiscal { get; set; }

    public int? cteReceptorId { get; set; }

    public string? cteReceptorNombre { get; set; }

    public string? cteReceptorCp { get; set; }

    public string? cteReceptorRegimenFiscal { get; set; }

    public string? cteReceptorDomicilio { get; set; }

    public string? cteReceptorRfc { get; set; }

    public string? cteReceptorUsoCFDI { get; set; }

    public decimal? cteReceptorTipoCambio { get; set; }

    public string? idUnidad { get; set; }

    public string? modeloUnidad { get; set; }

    public string? placaUnidad { get; set; }

    public string? configVehicular { get; set; }

    public string? aseguradora { get; set; }

    public string? polizaUnidad { get; set; }

    public decimal? pesoBrutoVehicular { get; set; }

    public string? claveTipoPermiso { get; set; }

    public string? numTipoPermiso { get; set; }

    public string? operador { get; set; }

    public string? rfcOperador { get; set; }

    public string? licenciaOperador { get; set; }

    public string? noViaje { get; set; }

    public decimal? totalDistanciaRec { get; set; }

    public string? idRemolque { get; set; }

    public string? placaRemolque1 { get; set; }

    public string? subtipoRemolque1 { get; set; }

    public string? placaRemolque2 { get; set; }

    public string? subtipoRemolque2 { get; set; }

    public string? esTransporteInternacional { get; set; }

    public string? claveRegimenAduanero { get; set; }

    public string? entSalMercancia { get; set; }

    public int? viaEntradaSalida { get; set; }

    public string? pedimento { get; set; }

    public string? rfcImpo { get; set; }

    public int? idClienteLis { get; set; }

    public string? idUnidadLis { get; set; }

    public string? idRemolqueLis { get; set; }

    public string? idRemolque2Lis { get; set; }

    public int? idPlazaOrLis { get; set; }

    public int? idPlazaDeLis { get; set; }

    public int? idRutaLis { get; set; }

    public int? idOperadorLis { get; set; }

    public string? idLineaRem1Lis { get; set; }

    public string? idLineaRem2Lis { get; set; }

    public int? idSucursalLis { get; set; }

    public int? idClienteRemitenteLis { get; set; }

    public int? idClienteDestinatarioLis { get; set; }

    public DateTime? fechaTimbrado { get; set; }

    public int? estatusTrasladoLis { get; set; }

    public string? mensajeTrasladoLis { get; set; }

    public DateTime? fechaTrasladoLis { get; set; }

    public int? idTipoOperacionLis { get; set; }

    public string? tipoOperacion { get; set; }

    public string? idTipoServicioLis { get; set; }

    public string? tipoServicio { get; set; }

    public string? numCartaPorteLis { get; set; }

    public int? idClienteRemitente { get; set; }

    public int? idClienteDestinatario { get; set; }

    public int? idOperador { get; set; }

    public int? idRuta { get; set; }

    public int? idPlazaOrigen { get; set; }

    public int? idPlazaDestino { get; set; }

    public bool? esPermisionario { get; set; }

    public string? codigoTransportista { get; set; }

    public string? rfcTransportista { get; set; }

    public string? nombreTransportista { get; set; }

    public string? regimenfiscalTransportista { get; set; }

    public DateTime? fechaSolicitudTimbrado { get; set; }

    public Guid? modificadoPor { get; set; }

    public string? mensajeTrasladoOT { get; set; }

    public int? estatusTrasladoOT { get; set; }

    public virtual archivoCFDi? archivoCFDi { get; set; }

    public virtual ICollection<cartaPorteAddendum> cartaPorteAddenda { get; set; } = new List<cartaPorteAddendum>();

    public virtual ICollection<cartaPorteDetalle> cartaPorteDetalles { get; set; } = new List<cartaPorteDetalle>();

    public virtual ICollection<cartaPorteMercancium> cartaPorteMercancia { get; set; } = new List<cartaPorteMercancium>();

    public virtual ICollection<cartaPorteOperacionRyder> cartaPorteOperacionRyders { get; set; } = new List<cartaPorteOperacionRyder>();

    public virtual ICollection<cartaPorteRegimenAduanero> cartaPorteRegimenAduaneros { get; set; } = new List<cartaPorteRegimenAduanero>();

    public virtual ICollection<cartaPorteSustitucion> cartaPorteSustitucions { get; set; } = new List<cartaPorteSustitucion>();

    public virtual ICollection<cartaPorteUbicacione> cartaPorteUbicaciones { get; set; } = new List<cartaPorteUbicacione>();

    public virtual ICollection<errorTimbradoGeneral> errorTimbradoGenerals { get; set; } = new List<errorTimbradoGeneral>();
}
