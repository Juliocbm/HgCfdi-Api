using System;
using System.Collections.Generic;

namespace CFDI.Data.Entities;

public partial class cartaPorteDetalle
{
    public int id { get; set; }

    public int no_guia { get; set; }

    public string compania { get; set; } = null!;

    public decimal? cantidad { get; set; }

    public decimal? importe { get; set; }

    public string? descripcion { get; set; }

    public decimal? montoIvaOtro { get; set; }

    public decimal? factorIva { get; set; }

    public decimal? montoRetencion { get; set; }

    public decimal? factorRetencion { get; set; }

    public string? claveProdServ { get; set; }

    public string? cpeNoIdentificador { get; set; }

    public DateTime? fechaInsert { get; set; }

    public int? idConceptolis { get; set; }

    public string? objetoImp { get; set; }

    public virtual cartaPorteCabecera cartaPorteCabecera { get; set; } = null!;
}
