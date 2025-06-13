using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.Models.DtoLiquidacionCfdi
{

    public class CfdiNomina
    {
        public string Version { get; set; }
        public string Serie { get; set; }
        public int Folio { get; set; }
        public DateTime Fecha { get; set; }
        public string Moneda { get; set; }
        public string Exportacion { get; set; }
        public int LugarExpedicion { get; set; }
        public string MetodoPago { get; set; }
        public string TipoDeComprobante { get; set; }

        public Emisor Emisor { get; set; }
        public Receptor Receptor { get; set; }
        public Nomina Nomina { get; set; }
        public Percepcion[] Percepciones { get; set; }
        public TotalesPercepcion TotalesPercepciones { get; set; }
        public Deduccion[] Deducciones { get; set; }
        public TotalesDeduccion TotalesDeducciones { get; set; }
        public ComplementoReceptor ComplementoReceptor { get; set; }
    }
    public class Emisor
    {
        public string rfc { get; set; }
        public string nombre { get; set; }
        public string claveSAT { get; set; }
    }

    public class Receptor
    {
        public string rfc { get; set; }
        public string nombre { get; set; }
        public string cp { get; set; }
        public string DomicilioFiscalReceptor { get; set; }
        public string RegimenFiscalReceptor { get; set; }
        public string UsoCFDI { get; set; }
    }

    public class Nomina
    {
        public string Version { get; set; }
        public string TipoNomina { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaInicialPago { get; set; }
        public DateTime FechaFinalPago { get; set; }
        public int NumDiasPagados { get; set; }
    }

    public class Percepcion
    {
        public string TipoPercepcion { get; set; }
        public string Clave { get; set; }
        public string Concepto { get; set; }
        public decimal ImporteGravado { get; set; }
        public decimal ImporteExento { get; set; }
    }

    public class TotalesPercepcion
    {
        public decimal TotalSueldos { get; set; }
        public decimal TotalGravado { get; set; }
        public decimal TotalExento { get; set; }
        public decimal TotalPercepciones { get; set; }
    }

    public class Deduccion
    {
        public string TipoDeduccion { get; set; }
        public int Clave { get; set; }
        public string Concepto { get; set; }
        public decimal Importe { get; set; }
    }

    public class TotalesDeduccion
    {
        public decimal TotalOtrasDeducciones { get; set; }
        public decimal TotalImpuestosRetenidos { get; set; }
        public decimal TotalDeducciones { get; set; }
    }

    public class ComplementoReceptor
    {
        public string Curp { get; set; }
        public string NumSeguridadSocial { get; set; }
        public DateTime FechaInicioRelLaboral { get; set; }
        public string Antiguedad { get; set; }
        public string TipoContrato { get; set; }
        public string TipoRegimen { get; set; }
        public string num_empleado { get; set; }
        public string Departamento { get; set; }
        public string Puesto { get; set; }
        public string RiesgoPuesto { get; set; }
        public string PeriodicidadPago { get; set; }
        public string Banco { get; set; }
        public decimal SalarioBaseCotApor { get; set; }
        public decimal SalarioDiarioIntegrado { get; set; }
        public string ClaveEntFed { get; set; }
    }
}
