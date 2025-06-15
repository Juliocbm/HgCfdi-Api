using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorteUbicacione
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string compania { get; set; } = null!;

    public string? tipoUbicacionOrigen { get; set; }

    public string? nombreRemitente { get; set; }

    public string? remitenteRfc { get; set; }

    public string? remitenteResidenciaFiscal { get; set; }

    public string? remitenteId { get; set; }

    public string? remitenteCp { get; set; }

    public string? remitenteEstado { get; set; }

    public string? remitentePais { get; set; }

    public string? remitenteMunicipio { get; set; }

    public string? remitenteNumRegIdTrib { get; set; }

    public string? fechaDespachoProgramado { get; set; }

    public string? tipoUbicacionDestino { get; set; }

    public string? nombreDestinatario { get; set; }

    public string? destinatarioRfc { get; set; }

    public string? destinatarioResidenciaFiscal { get; set; }

    public string? destinatarioId { get; set; }

    public string? destinatarioCp { get; set; }

    public string? destinatarioEstado { get; set; }

    public string? destinatarioPais { get; set; }

    public string? destinatarioMunicipio { get; set; }

    public string? destinatarioNumRegIdTrib { get; set; }

    public string? fechaArriboProgramado { get; set; }

    public decimal? distanciaRecorrida { get; set; }

    public DateTime? fechaInsert { get; set; }

    public string? remitenteLocalidad { get; set; }

    public string? destinatarioLocalidad { get; set; }

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
