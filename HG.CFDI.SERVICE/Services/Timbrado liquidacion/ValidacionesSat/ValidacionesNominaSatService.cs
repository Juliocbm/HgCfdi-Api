using BuzonE;
using HG.CFDI.CORE.Models;
using Microsoft.Extensions.Options;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;

namespace HG.CFDI.SERVICE.Services.Timbrado_liquidacion.ValidacionesSat
{
    public class ValidacionesNominaSatService : IValidacionesNominaSatService
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

            // Validaciones previas
            if (liquidacion.Emisor == null || liquidacion.Receptor == null || liquidacion.Nomina == null ||
                liquidacion.ComplementoEmisor == null || liquidacion.ComplementoReceptor == null)
                throw new ArgumentException("Faltan datos obligatorios de emisor, receptor o complementos.");

            if (liquidacion.Nomina.NumDiasPagados <= 0 || liquidacion.Nomina.NumDiasPagados > 365)
                throw new ArgumentException("NumDiasPagados debe estar entre 1 y 365");

            // Validaciones de montos
            var totalPercepciones = decimal.Round(liquidacion.TotalesPercepciones.TotalPercepciones, 2);
            var totalDeducciones = decimal.Round(liquidacion.TotalesDeducciones.TotalDeducciones, 2);
            var subtotal = totalPercepciones;  // Solo usamos el total de percepciones

            if (subtotal <= 0)
                throw new ArgumentException("El subtotal debe ser mayor a 0");

            if (totalDeducciones > subtotal)
                throw new ArgumentException("Las deducciones no pueden ser mayores que el subtotal");

            var totalNeto = decimal.Round(subtotal - totalDeducciones, 2);
            if (totalNeto <= 0)
                throw new ArgumentException("El total neto debe ser mayor a 0");

            // Validación de percepciones
            var sumaPercepciones = liquidacion.Percepciones.Sum(p => p.ImporteGravado + p.ImporteExento);
            if (decimal.Round(sumaPercepciones, 2) != totalPercepciones)
                throw new ArgumentException($"TotalPercepciones no cuadra con la suma de percepciones. Esperado: {totalPercepciones}, Calculado: {sumaPercepciones}");

            // Validación de deducciones
            var sumaDeducciones = decimal.Round(liquidacion.Deducciones.Sum(d => decimal.Round(d.Importe, 2)), 2);
            if (sumaDeducciones != totalDeducciones)
                throw new ArgumentException($"TotalDeducciones no cuadra con la suma de deducciones. Esperado: {totalDeducciones}, Calculado: {sumaDeducciones}");

            // Validación de complementos
            if (string.IsNullOrEmpty(liquidacion.ComplementoReceptor.Curp))
                throw new ArgumentException("El CURP del receptor es requerido");

            if (string.IsNullOrEmpty(liquidacion.ComplementoReceptor.NumSeguridadSocial))
                throw new ArgumentException("El número de seguridad social es requerido");

            if (liquidacion.ComplementoReceptor.FechaInicioRelLaboral == default(DateTime))
                throw new ArgumentException("La fecha de inicio de relación laboral es requerida");

            // Validación de tipo de nómina
            if (!Enum.TryParse<BuzonE.c_TipoNomina>(liquidacion.Nomina.TipoNomina, true, out var tipoNomina))
                throw new ArgumentException($"TipoNomina '{liquidacion.Nomina.TipoNomina}' no es válido");

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
                Fecha = new DateTime(liquidacion.Fecha.Year, liquidacion.Fecha.Month, liquidacion.Fecha.Day,
                                     liquidacion.Fecha.Hour, liquidacion.Fecha.Minute, liquidacion.Fecha.Second,
                                     DateTimeKind.Unspecified),
                Moneda = ParseEnumSafe<BuzonE.c_Moneda>(liquidacion.Moneda, nameof(liquidacion.Moneda)),
                Exportacion = ParseEnumSafe<BuzonE.c_Exportacion>(liquidacion.Exportacion, nameof(liquidacion.Exportacion), true),
                SubTotal = subtotal,
                DescuentoSpecified = true,
                Descuento = sumaDeducciones,
                Total = totalNeto,
                LugarExpedicion = liquidacion.LugarExpedicion.ToString(),
                MetodoPagoSpecified = true,
                MetodoPago = ParseEnumSafe<BuzonE.c_MetodoPago>(liquidacion.MetodoPago, nameof(liquidacion.MetodoPago)),
                TipoDeComprobante = ParseEnumSafe<BuzonE.c_TipoDeComprobante>(liquidacion.TipoDeComprobante, nameof(liquidacion.TipoDeComprobante))                
            };

            // Validación de tipo de comprobante
            if (comprobante.TipoDeComprobante != BuzonE.c_TipoDeComprobante.N)
                throw new InvalidOperationException($"TipoDeComprobante debe ser 'N' para nómina, actual: '{comprobante.TipoDeComprobante}'");

            comprobante.Emisor = new BuzonE.ComprobanteEmisor
            {
                Rfc = liquidacion.Emisor.Rfc,
                Nombre = liquidacion.Emisor.Nombre,
                RegimenFiscal = ParseEnumSafe<BuzonE.c_RegimenFiscal>(liquidacion.Emisor.RegimenFiscal, nameof(liquidacion.Emisor.RegimenFiscal), true)
            };

            var nombre = liquidacion.Receptor.Nombre;
            comprobante.Receptor = new BuzonE.ComprobanteReceptor
            {
                Rfc = liquidacion.Receptor.Rfc,
                //Nombre = liquidacion.Receptor.Nombre,
                Nombre = "ROBERTO CARLOS HUERTA ADAME",
                DomicilioFiscalReceptor = "67190",
                //DomicilioFiscalReceptor = liquidacion.Receptor.DomicilioFiscalReceptor,

                //Nombre = "JULIO CESAR BAUTISTA MONSALVO",
                //Rfc = "BAMJ9209248V8",
                //DomicilioFiscalReceptor = "63175",
                RegimenFiscalReceptor = ParseEnumSafe<BuzonE.c_RegimenFiscal>(liquidacion.Receptor.RegimenFiscalReceptor, nameof(liquidacion.Receptor.RegimenFiscalReceptor), true),
                UsoCFDI = ParseEnumSafe<BuzonE.c_UsoCFDI>(liquidacion.Receptor.UsoCFDI, nameof(liquidacion.Receptor.UsoCFDI))
            };

            if (totalNeto <= 0)
                throw new ArgumentException("El total neto (subtotal - deducciones) debe ser mayor a 0");

            comprobante.Conceptos = new[]
            {
                new BuzonE.ComprobanteConcepto
                {
                    ClaveProdServ = "84111505",
                    Cantidad = 1m,
                    ClaveUnidad = BuzonE.c_ClaveUnidad.ACT,
                    Descripcion = "Pago de nómina",
                    ValorUnitario = subtotal,
                    Importe = subtotal,
                    DescuentoSpecified = true,
                    Descuento = totalDeducciones,
                    ObjetoImp = BuzonE.c_ObjetoImp.Item01
                }
            };

            comprobante.Total = totalNeto;  // Actualizar el total del comprobante con el valor neto

            if (comprobante.TipoDeComprobante != BuzonE.c_TipoDeComprobante.N)
                throw new InvalidOperationException($"TipoDeComprobante esperado 'N', actual '{comprobante.TipoDeComprobante}'.");

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
                TotalPercepciones = totalPercepciones,
                TotalDeduccionesSpecified = true,
                TotalDeducciones = totalDeducciones,
                TotalOtrosPagosSpecified = true,
                TotalOtrosPagos = 0
            };

            nomina.OtrosPagos = new BuzonE.NominaOtroPago[]
            {
                new NominaOtroPago()
                {
                    Clave = "000",
                    Concepto = "Subsidio para el Empleo",
                    Importe = 0,
                    TipoOtroPago = c_TipoOtroPago.Item002,
                    SubsidioAlEmpleo = new NominaOtroPagoSubsidioAlEmpleo()
                    {
                        SubsidioCausado = 0
                    }
                }
            };

            nomina.Percepciones = new BuzonE.NominaPercepciones
            {
                Percepcion = liquidacion.Percepciones.Select(p => new BuzonE.NominaPercepcionesPercepcion
                {
                    TipoPercepcion = ParseEnumSafe<BuzonE.c_TipoPercepcion>(p.TipoPercepcion, nameof(p.TipoPercepcion), true),
                    Clave = p.Clave,
                    Concepto = p.Concepto,
                    ImporteGravado = decimal.Round(p.ImporteGravado, 2),
                    ImporteExento = decimal.Round(p.ImporteExento, 2)
                }).ToArray(),
                TotalSueldosSpecified = true,
                TotalSueldos = decimal.Round(liquidacion.TotalesPercepciones.TotalSueldos, 2),
                TotalGravado = decimal.Round(liquidacion.TotalesPercepciones.TotalGravado, 2),
                TotalExento = decimal.Round(liquidacion.TotalesPercepciones.TotalExento, 2)
            };

            nomina.Deducciones = new BuzonE.NominaDeducciones
            {
                Deduccion = liquidacion.Deducciones.Select(d => new BuzonE.NominaDeduccionesDeduccion
                {
                    TipoDeduccion = ParseEnumSafe<BuzonE.c_TipoDeduccion>(d.TipoDeduccion.PadLeft(3, '0'), nameof(d.TipoDeduccion), true),
                    Clave = d.Clave,
                    Concepto = d.Concepto,
                    Importe = decimal.Round(d.Importe, 2)
                }).ToArray(),
                TotalOtrasDeduccionesSpecified = true,
                TotalOtrasDeducciones = decimal.Round(liquidacion.TotalesDeducciones.TotalOtrasDeducciones, 2),
                TotalImpuestosRetenidosSpecified = true,
                TotalImpuestosRetenidos = decimal.Round(liquidacion.TotalesDeducciones.TotalImpuestosRetenidos, 2)
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
                Banco = string.IsNullOrEmpty(liquidacion.ComplementoReceptor.ClaveBanco) ? BuzonE.c_Banco.Item058 :
                    ParseEnumSafe<BuzonE.c_Banco>(liquidacion.ComplementoReceptor.ClaveBanco, nameof(liquidacion.ComplementoReceptor.ClaveBanco), true),
                SalarioBaseCotAporSpecified = true,
                SalarioBaseCotApor = liquidacion.ComplementoReceptor.SalarioBaseCotApor,
                SalarioDiarioIntegradoSpecified = true,
                SalarioDiarioIntegrado = liquidacion.ComplementoReceptor.SalarioDiarioIntegrado
            };

            // Validación de receptor
            if (string.IsNullOrEmpty(nomina.Receptor.Curp) || nomina.Receptor.Curp.Length != 18)
                throw new ArgumentException($"El CURP '{nomina.Receptor.Curp}' no es válido");

            if (string.IsNullOrEmpty(nomina.Receptor.NumSeguridadSocial) || nomina.Receptor.NumSeguridadSocial.Length != 11)
                throw new ArgumentException($"El número de seguridad social '{nomina.Receptor.NumSeguridadSocial}' no es válido");

            if (nomina.Receptor.FechaInicioRelLaboral > DateTime.Now)
                throw new ArgumentException($"La fecha de inicio de relación laboral no puede ser futura");

            complemento.Nomina = new[] { nomina };
            comprobante.Complemento = new[] { complemento };
            request.Comprobante = comprobante;

            // Validación final del XML
            if (request.Comprobante == null || request.Comprobante.Version != "4.0")
                throw new InvalidOperationException("La versión del CFDI debe ser 4.0");

            if (request.Comprobante.Serie == null || request.Comprobante.Folio == null)
                throw new InvalidOperationException("Serie y Folio son requeridos");

            if (request.Comprobante.FechaSpecified == false)
                throw new InvalidOperationException("La fecha es requerida");

            if (request.Comprobante.Moneda == null)
                throw new InvalidOperationException("La moneda es requerida");

            if (request.Comprobante.SubTotal <= 0 || request.Comprobante.Total <= 0)
                throw new InvalidOperationException("Subtotal y total deben ser mayores a 0");

            if (request.Comprobante.LugarExpedicion == null)
                throw new InvalidOperationException("El lugar de expedición es requerido");

            if (request.Comprobante.MetodoPagoSpecified == false)
                throw new InvalidOperationException("El método de pago es requerido");

            if (request.Comprobante.TipoDeComprobante != BuzonE.c_TipoDeComprobante.N)
                throw new InvalidOperationException("El tipo de comprobante debe ser 'N' para nómina");

            if (request.Comprobante.Conceptos == null || !request.Comprobante.Conceptos.Any())
                throw new InvalidOperationException("Debe haber al menos un concepto");

            if (request.Comprobante.Complemento == null || !request.Comprobante.Complemento.Any())
                throw new InvalidOperationException("El complemento es requerido");

            if (nomina.Version != "1.2")
                throw new InvalidOperationException("La versión del complemento de nómina debe ser 1.2");

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
