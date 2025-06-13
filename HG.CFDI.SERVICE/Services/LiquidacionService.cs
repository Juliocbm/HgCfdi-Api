using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using HG.CFDI.CORE.Models;
using BuzonE;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HG.CFDI.SERVICE.Services
{
    public class LiquidacionService : ILiquidacionService
    {
        private readonly ILiquidacionRepository _repository;
        private readonly List<BuzonEApiCredential> _buzonEApiCredentials;

        public LiquidacionService(ILiquidacionRepository repository, IOptions<List<BuzonEApiCredential>> buzonEOptions)
        {
            _repository = repository;
            _buzonEApiCredentials = buzonEOptions.Value;
        }

        public async Task<CfdiNomina?> ObtenerLiquidacion(string database, int noLiquidacion)
        {
            var json = await _repository.ObtenerDatosNominaJson(database, noLiquidacion);
            if (string.IsNullOrWhiteSpace(json))
                return null;
            try
            {
                return JsonSerializer.Deserialize<CfdiNomina>(json);
            }
            catch
            {
                return null;
            }
        }

        public Task<RequestBE> ConstruirRequestBuzonEAsync(CfdiNomina liquidacion, string database)
        {
            var cred = _buzonEApiCredentials.FirstOrDefault(c => c.Database.Equals(database, StringComparison.OrdinalIgnoreCase));
            if (cred == null)
                throw new InvalidOperationException($"No se encontraron credenciales para {database}");

            var request = new RequestBE
            {
                usuario = cred.User,
                password = cred.Password,
                AdditionalInformation = new RequestBEAdditionalInformation
                {
                    fileType = cred.FileType,
                    titulo = $"{liquidacion.Serie}-{liquidacion.Folio}",
                    conector = "6094209",
                    comentario = "Liquidacion"
                }
            };

            var comprobante = new Comprobante
            {
                Version = liquidacion.Version,
                Serie = liquidacion.Serie,
                Folio = liquidacion.Folio.ToString(),
                FechaSpecified = true,
                Fecha = liquidacion.Fecha,
                Moneda = Enum.Parse<c_Moneda>(liquidacion.Moneda),
                Exportacion = (c_Exportacion)Enum.Parse(typeof(c_Exportacion), "Item" + liquidacion.Exportacion),
                SubTotal = liquidacion.TotalesPercepciones.TotalPercepciones,
                DescuentoSpecified = true,
                Descuento = liquidacion.TotalesDeducciones.TotalDeducciones,
                Total = liquidacion.TotalesPercepciones.TotalPercepciones - liquidacion.TotalesDeducciones.TotalDeducciones,
                LugarExpedicion = liquidacion.LugarExpedicion.ToString(),
                MetodoPagoSpecified = true,
                MetodoPago = Enum.Parse<c_MetodoPago>(liquidacion.MetodoPago),
                TipoDeComprobante = Enum.Parse<c_TipoDeComprobante>(liquidacion.TipoDeComprobante)
            };

            comprobante.Emisor = new ComprobanteEmisor
            {
                Rfc = liquidacion.Emisor.rfc,
                Nombre = liquidacion.Emisor.nombre,
                RegimenFiscal = (c_RegimenFiscal)Enum.Parse(typeof(c_RegimenFiscal), "Item" + liquidacion.Emisor.claveSAT)
            };

            comprobante.Receptor = new ComprobanteReceptor
            {
                Rfc = liquidacion.Receptor.rfc,
                Nombre = liquidacion.Receptor.nombre,
                DomicilioFiscalReceptor = liquidacion.Receptor.DomicilioFiscalReceptor,
                RegimenFiscalReceptor = (c_RegimenFiscal)Enum.Parse(typeof(c_RegimenFiscal), "Item" + liquidacion.Receptor.RegimenFiscalReceptor),
                UsoCFDI = (c_UsoCFDI)Enum.Parse(typeof(c_UsoCFDI), liquidacion.Receptor.UsoCFDI)
            };

            comprobante.Conceptos = new[]
            {
                new ComprobanteConcepto
                {
                    ClaveProdServ = "84111505",
                    Cantidad = 1m,
                    ClaveUnidad = c_ClaveUnidad.ACT,
                    Descripcion = "Pago de nómina",
                    ValorUnitario = liquidacion.TotalesPercepciones.TotalPercepciones,
                    Importe = liquidacion.TotalesPercepciones.TotalPercepciones,
                    DescuentoSpecified = true,
                    Descuento = liquidacion.TotalesDeducciones.TotalDeducciones,
                    ObjetoImp = c_ObjetoImp.Item01
                }
            };

            var complemento = new ComprobanteComplemento();
            var nomina = new Nomina
            {
                Version = liquidacion.Nomina.Version,
                TipoNomina = (c_TipoNomina)Enum.Parse(typeof(c_TipoNomina), liquidacion.Nomina.TipoNomina),
                FechaPago = liquidacion.Nomina.FechaPago,
                FechaInicialPago = liquidacion.Nomina.FechaInicialPago,
                FechaFinalPago = liquidacion.Nomina.FechaFinalPago,
                NumDiasPagados = liquidacion.Nomina.NumDiasPagados,
                TotalPercepcionesSpecified = true,
                TotalPercepciones = liquidacion.TotalesPercepciones.TotalPercepciones,
                TotalDeduccionesSpecified = true,
                TotalDeducciones = liquidacion.TotalesDeducciones.TotalDeducciones
            };

            nomina.Percepciones = new NominaPercepciones
            {
                Percepcion = liquidacion.Percepciones.Select(p => new NominaPercepcionesPercepcion
                {
                    TipoPercepcion = (c_TipoPercepcion)Enum.Parse(typeof(c_TipoPercepcion), "Item" + p.TipoPercepcion),
                    Clave = p.Clave,
                    Concepto = p.Concepto,
                    ImporteGravado = p.ImporteGravado,
                    ImporteExento = p.ImporteExento
                }).ToArray(),
                TotalSueldosSpecified = true,
                TotalSueldos = liquidacion.TotalesPercepciones.TotalSueldos,
                TotalGravado = liquidacion.TotalesPercepciones.TotalGravado,
                TotalExento = liquidacion.TotalesPercepciones.TotalExento
            };

            nomina.Deducciones = new NominaDeducciones
            {
                Deduccion = liquidacion.Deducciones.Select(d => new NominaDeduccionesDeduccion
                {
                    TipoDeduccion = (c_TipoDeduccion)Enum.Parse(typeof(c_TipoDeduccion), "Item" + d.TipoDeduccion.PadLeft(3, '0')),
                    Clave = d.Clave.ToString("000000"),
                    Concepto = d.Concepto,
                    Importe = d.Importe
                }).ToArray(),
                TotalOtrasDeduccionesSpecified = true,
                TotalOtrasDeducciones = liquidacion.TotalesDeducciones.TotalOtrasDeducciones,
                TotalImpuestosRetenidosSpecified = true,
                TotalImpuestosRetenidos = liquidacion.TotalesDeducciones.TotalImpuestosRetenidos
            };

            nomina.Emisor = new NominaEmisor
            {
                RegistroPatronal = "D3814590104"
            };

            nomina.Receptor = new NominaReceptor
            {
                Curp = liquidacion.ComplementoReceptor.Curp,
                NumSeguridadSocial = liquidacion.ComplementoReceptor.NumSeguridadSocial,
                FechaInicioRelLaboralSpecified = true,
                FechaInicioRelLaboral = liquidacion.ComplementoReceptor.FechaInicioRelLaboral,
                Antigüedad = liquidacion.ComplementoReceptor.Antiguedad,
                TipoContrato = (c_TipoContrato)Enum.Parse(typeof(c_TipoContrato), "Item" + liquidacion.ComplementoReceptor.TipoContrato),
                TipoRegimen = (c_TipoRegimen)Enum.Parse(typeof(c_TipoRegimen), "Item" + liquidacion.ComplementoReceptor.TipoRegimen),
                NumEmpleado = liquidacion.ComplementoReceptor.num_empleado,
                Departamento = liquidacion.ComplementoReceptor.Departamento,
                Puesto = liquidacion.ComplementoReceptor.Puesto,
                RiesgoPuestoSpecified = true,
                RiesgoPuesto = (c_RiesgoPuesto)Enum.Parse(typeof(c_RiesgoPuesto), "Item" + liquidacion.ComplementoReceptor.RiesgoPuesto),
                PeriodicidadPago = (c_PeriodicidadPago)Enum.Parse(typeof(c_PeriodicidadPago), "Item" + liquidacion.ComplementoReceptor.PeriodicidadPago),
                BancoSpecified = !string.IsNullOrEmpty(liquidacion.ComplementoReceptor.Banco),
                Banco = string.IsNullOrEmpty(liquidacion.ComplementoReceptor.Banco) ? c_Banco.Item002 : (c_Banco)Enum.Parse(typeof(c_Banco), "Item" + liquidacion.ComplementoReceptor.Banco),
                SalarioBaseCotAporSpecified = true,
                SalarioBaseCotApor = liquidacion.ComplementoReceptor.SalarioBaseCotApor,
                SalarioDiarioIntegradoSpecified = true,
                SalarioDiarioIntegrado = liquidacion.ComplementoReceptor.SalarioDiarioIntegrado,
                ClaveEntFed = (c_Estado)Enum.Parse(typeof(c_Estado), liquidacion.ComplementoReceptor.ClaveEntFed)
            };

            complemento.Nomina = new[] { nomina };
            comprobante.Complemento = new[] { complemento };
            request.Comprobante = comprobante;

            return Task.FromResult(request);
        }
    }
}
