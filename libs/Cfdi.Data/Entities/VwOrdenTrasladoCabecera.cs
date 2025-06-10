using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class VwOrdenTrasladoCabecera
{
    public int no_guia { get; set; }

    public string num_guia { get; set; } = null!;

    public string compania { get; set; } = null!;

    public int? estatusTrasladoOT { get; set; }

    public string? mensajeTrasladoOT { get; set; }

    public bool? esPermisionario { get; set; }

    public DateTime fechaCreacion { get; set; }

    public string numOrdenOT { get; set; } = null!;

    public string? transportista { get; set; }

    public string? rfcEmisor { get; set; }

    public string? nombreEmisor { get; set; }

    public string? regimenFiscal { get; set; }

    public string formaDePago { get; set; } = null!;

    public string? rfcReceptor { get; set; }

    public string? razonSocialReceptor { get; set; }

    public string usoCfdiReceptor { get; set; } = null!;

    public string tipoComprobante { get; set; } = null!;

    public string tipoCFD { get; set; } = null!;

    public string moneda { get; set; } = null!;

    public string lugarDeExpedicion { get; set; } = null!;

    public string metodoPago { get; set; } = null!;

    public string domicilioEmisorEstado { get; set; } = null!;

    public string? domicilioReceptorCp { get; set; }

    public decimal? totalDistanciaRecorrida { get; set; }

    public string medioTransporte { get; set; } = null!;

    public string exportacion { get; set; } = null!;

    public string version { get; set; } = null!;

    public string ruta { get; set; } = null!;

    public string plaza { get; set; } = null!;

    public int? SubTotal { get; set; }

    public int? Total { get; set; }

    public int? TotalImpuestosRetenidos { get; set; }

    public int? TotalImpuestosTrasladados { get; set; }
}
