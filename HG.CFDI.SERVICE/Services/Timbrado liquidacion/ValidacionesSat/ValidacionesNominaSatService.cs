using BuzonE;
using HG.CFDI.CORE.Models;
using Microsoft.Extensions.Options;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;

namespace HG.CFDI.SERVICE.Services.Timbrado_liquidacion.ValidacionesSat
{
    public class ValidacionesNominaSatService: IValidacionesNominaSatService
    {

        private readonly List<BuzonEApiCredential> _buzonEApiCredentials;

        public ValidacionesNominaSatService(
            IOptions<List<BuzonEApiCredential>> buzonEOptions)
        {
            _buzonEApiCredentials = buzonEOptions.Value;
        }

        public Task<RequestBE> ConstruirRequestBuzonEAsync(CfdiNomina liquidacion, string database)
        {
            if (liquidacion is null)
                throw new ArgumentNullException(nameof(liquidacion), "La liquidación es requerida");

            var cred = _buzonEApiCredentials.FirstOrDefault(c => c.Database.Equals(database, StringComparison.OrdinalIgnoreCase));
            if (cred == null)
                throw new InvalidOperationException($"No se encontraron credenciales configuradas para la base de datos '{database}'.");

            if (liquidacion.Emisor == null)
                throw new ArgumentException("La información del emisor es requerida.", nameof(liquidacion.Emisor));
            if (liquidacion.Receptor == null)
                throw new ArgumentException("La información del receptor es requerida.", nameof(liquidacion.Receptor));
            if (liquidacion.Nomina == null)
                throw new ArgumentException("La información de nómina es requerida.", nameof(liquidacion.Nomina));
            if (liquidacion.ComplementoEmisor == null)
                throw new ArgumentException("El complemento del emisor es requerido.", nameof(liquidacion.ComplementoEmisor));
            if (liquidacion.ComplementoReceptor == null)
                throw new ArgumentException("El complemento del receptor es requerido.", nameof(liquidacion.ComplementoReceptor));

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

            var comprobante = new BuzonE.Comprobante
            {
                Version = liquidacion.Version,
                Serie = liquidacion.Serie,
                Folio = liquidacion.Folio.ToString(),
                FechaSpecified = true,
                Fecha = liquidacion.Fecha,
                Moneda = ParseEnumSafe<BuzonE.c_Moneda>(liquidacion.Moneda, nameof(liquidacion.Moneda)),
                Exportacion = ParseEnumSafe<BuzonE.c_Exportacion>(liquidacion.Exportacion, nameof(liquidacion.Exportacion), true),
                SubTotal = liquidacion.TotalesPercepciones.TotalPercepciones,
                DescuentoSpecified = true,
                Descuento = liquidacion.TotalesDeducciones.TotalDeducciones,
                Total = liquidacion.TotalesPercepciones.TotalPercepciones - liquidacion.TotalesDeducciones.TotalDeducciones,
                LugarExpedicion = liquidacion.LugarExpedicion.ToString(),
                MetodoPagoSpecified = true,
                MetodoPago = ParseEnumSafe<BuzonE.c_MetodoPago>(liquidacion.MetodoPago, nameof(liquidacion.MetodoPago)),
                TipoDeComprobante = ParseEnumSafe<BuzonE.c_TipoDeComprobante>(liquidacion.TipoDeComprobante, nameof(liquidacion.TipoDeComprobante))
            };

            comprobante.Emisor = new BuzonE.ComprobanteEmisor
            {
                Rfc = liquidacion.Emisor.rfc,
                Nombre = liquidacion.Emisor.nombre,
                RegimenFiscal = ParseEnumSafe<BuzonE.c_RegimenFiscal>(liquidacion.Emisor.claveSAT, nameof(liquidacion.Emisor.claveSAT), true)
            };

            comprobante.Receptor = new BuzonE.ComprobanteReceptor
            {
                Rfc = liquidacion.Receptor.rfc,
                Nombre = liquidacion.Receptor.nombre,
                DomicilioFiscalReceptor = liquidacion.Receptor.DomicilioFiscalReceptor,
                RegimenFiscalReceptor = ParseEnumSafe<BuzonE.c_RegimenFiscal>(liquidacion.Receptor.RegimenFiscalReceptor, nameof(liquidacion.Receptor.RegimenFiscalReceptor), true),
                UsoCFDI = ParseEnumSafe<BuzonE.c_UsoCFDI>(liquidacion.Receptor.UsoCFDI, nameof(liquidacion.Receptor.UsoCFDI))
            };

            comprobante.Conceptos = new[]
            {
                new BuzonE.ComprobanteConcepto
                {
                    ClaveProdServ = "84111505",
                    Cantidad = 1m,
                    ClaveUnidad = BuzonE.c_ClaveUnidad.ACT,
                    Descripcion = "Pago de nómina",
                    ValorUnitario = liquidacion.TotalesPercepciones.TotalPercepciones,
                    Importe = liquidacion.TotalesPercepciones.TotalPercepciones,
                    DescuentoSpecified = true,
                    Descuento = liquidacion.TotalesDeducciones.TotalDeducciones,
                    ObjetoImp = BuzonE.c_ObjetoImp.Item01
                }
            };

            var complemento = new BuzonE.ComprobanteComplemento();
            var nomina = new BuzonE.Nomina
            {
                Version = liquidacion.Nomina.Version,
                TipoNomina = ParseEnumSafe<BuzonE.c_TipoNomina>(liquidacion.Nomina.TipoNomina, nameof(liquidacion.Nomina.TipoNomina)),
                FechaPago = liquidacion.Nomina.FechaPago,
                FechaInicialPago = liquidacion.Nomina.FechaInicialPago,
                FechaFinalPago = liquidacion.Nomina.FechaFinalPago,
                NumDiasPagados = liquidacion.Nomina.NumDiasPagados,
                TotalPercepcionesSpecified = true,
                TotalPercepciones = liquidacion.TotalesPercepciones.TotalPercepciones,
                TotalDeduccionesSpecified = true,
                TotalDeducciones = liquidacion.TotalesDeducciones.TotalDeducciones
            };

            nomina.Percepciones = new BuzonE.NominaPercepciones
            {
                Percepcion = liquidacion.Percepciones.Select(p => new BuzonE.NominaPercepcionesPercepcion
                {
                    TipoPercepcion = ParseEnumSafe<BuzonE.c_TipoPercepcion>(p.TipoPercepcion, nameof(p.TipoPercepcion), true),
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

            nomina.Deducciones = new BuzonE.NominaDeducciones
            {
                Deduccion = liquidacion.Deducciones.Select(d => new BuzonE.NominaDeduccionesDeduccion
                {
                    TipoDeduccion = ParseEnumSafe<BuzonE.c_TipoDeduccion>(d.TipoDeduccion.PadLeft(3, '0'), nameof(d.TipoDeduccion), true),
                    Clave = d.Clave.ToString("000000"),
                    Concepto = d.Concepto,
                    Importe = d.Importe
                }).ToArray(),
                TotalOtrasDeduccionesSpecified = true,
                TotalOtrasDeducciones = liquidacion.TotalesDeducciones.TotalOtrasDeducciones,
                TotalImpuestosRetenidosSpecified = true,
                TotalImpuestosRetenidos = liquidacion.TotalesDeducciones.TotalImpuestosRetenidos
            };

            nomina.Emisor = new BuzonE.NominaEmisor
            {
                RegistroPatronal = liquidacion.ComplementoEmisor.RegistroPatronal
            };

            nomina.Receptor = new BuzonE.NominaReceptor
            {
                Curp = liquidacion.ComplementoReceptor.Curp,
                NumSeguridadSocial = liquidacion.ComplementoReceptor.NumSeguridadSocial,
                FechaInicioRelLaboralSpecified = true,
                FechaInicioRelLaboral = liquidacion.ComplementoReceptor.FechaInicioRelLaboral,
                Antigüedad = liquidacion.ComplementoReceptor.Antiguedad,
                TipoContrato = ParseEnumSafe<BuzonE.c_TipoContrato>(liquidacion.ComplementoReceptor.TipoContrato, nameof(liquidacion.ComplementoReceptor.TipoContrato), true),
                TipoRegimen = ParseEnumSafe<BuzonE.c_TipoRegimen>(liquidacion.ComplementoReceptor.TipoRegimen, nameof(liquidacion.ComplementoReceptor.TipoRegimen), true),
                NumEmpleado = liquidacion.ComplementoReceptor.NumEmpleado,
                Departamento = liquidacion.ComplementoReceptor.Departamento,
                Puesto = liquidacion.ComplementoReceptor.Puesto,
                RiesgoPuestoSpecified = true,
                RiesgoPuesto = ParseEnumSafe<BuzonE.c_RiesgoPuesto>(liquidacion.ComplementoReceptor.RiesgoPuesto, nameof(liquidacion.ComplementoReceptor.RiesgoPuesto), true),
                PeriodicidadPago = ParseEnumSafe<BuzonE.c_PeriodicidadPago>(liquidacion.ComplementoReceptor.PeriodicidadPago, nameof(liquidacion.ComplementoReceptor.PeriodicidadPago), true),
                BancoSpecified = !string.IsNullOrEmpty(liquidacion.ComplementoReceptor.ClaveBanco),
                Banco = string.IsNullOrEmpty(liquidacion.ComplementoReceptor.ClaveBanco) ? BuzonE.c_Banco.Item058 : ParseEnumSafe<BuzonE.c_Banco>(liquidacion.ComplementoReceptor.ClaveBanco, nameof(liquidacion.ComplementoReceptor.ClaveBanco), true),
                SalarioBaseCotAporSpecified = true,
                SalarioBaseCotApor = liquidacion.ComplementoReceptor.SalarioBaseCotApor,
                SalarioDiarioIntegradoSpecified = true,
                SalarioDiarioIntegrado = liquidacion.ComplementoReceptor.SalarioDiarioIntegrado,
                ClaveEntFed = ParseEnumSafe<BuzonE.c_Estado>(liquidacion.ComplementoReceptor.ClaveEntFed, nameof(liquidacion.ComplementoReceptor.ClaveEntFed))
            };
           
            complemento.Nomina = new[] { nomina };
            comprobante.Complemento = new[] { complemento };
            request.Comprobante = comprobante;

            return Task.FromResult(request);
        }

        private static T ParseEnumSafe<T>(string value, string fieldName, bool prefixItem = false) where T : struct
        {
            string toParse = prefixItem ? $"Item{value}" : value;
            if (!Enum.TryParse(toParse, out T result))
                throw new ArgumentException($"El valor '{value}' del campo '{fieldName}' es inválido.");
            return result;
        }

    }
}
