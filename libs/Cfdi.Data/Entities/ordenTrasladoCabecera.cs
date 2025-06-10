using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class ordenTrasladoCabecera
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string? num_guia { get; set; }

    public string compania { get; set; } = null!;

    public int? estatusTraslado { get; set; }

    public DateTime? fechaCreacion { get; set; }

    public DateTime? fechaTraslado { get; set; }

    public string? numOrdenOT { get; set; }

    public string? transportista { get; set; }

    public string? rfcEmisor { get; set; }

    public string? nombreEmisor { get; set; }

    public string? regimenFiscal { get; set; }

    public string? formaDePago { get; set; }

    public string? rfcReceptor { get; set; }

    public string? razonSocialReceptor { get; set; }

    public string? usoCfdiReceptor { get; set; }

    public double? subTotal { get; set; }

    public double? total { get; set; }

    public string? tipoComprobante { get; set; }

    public string? tipoCFD { get; set; }

    public string? moneda { get; set; }

    public string? lugarDeExpedicion { get; set; }

    public string? metodoPago { get; set; }

    public double? totalImpuestosRetenidos { get; set; }

    public double? totalImpuestosTrasladados { get; set; }

    public string? domicilioEmisorEstado { get; set; }

    public string? domicioReceptorCp { get; set; }

    public decimal? totalDistanciaRecorrida { get; set; }

    public string? medioTransporte { get; set; }

    public string? exportacion { get; set; }

    public string? version { get; set; }

    public string? ruta { get; set; }

    public string? plaza { get; set; }

    public string? comentarioTraslado { get; set; }

    public virtual ICollection<ordenTrasladoMercancia> ordenTrasladoMercancia { get; set; } = new List<ordenTrasladoMercancia>();

    public virtual ICollection<ordenTrasladoUbicacione> ordenTrasladoUbicaciones { get; set; } = new List<ordenTrasladoUbicacione>();
}
