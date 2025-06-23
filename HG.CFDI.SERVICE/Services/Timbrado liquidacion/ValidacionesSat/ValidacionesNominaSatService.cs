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

        //public Task<RequestBE> ConstruirRequestBuzonEAsync(CfdiNomina liquidacion, string database)
        //{
        //    if (liquidacion is null)
        //        throw new ArgumentNullException(nameof(liquidacion), "La liquidación es requerida");

        //    var cred = _buzonEApiCredentials.FirstOrDefault(c => c.Database.Equals(database, StringComparison.OrdinalIgnoreCase));
        //    if (cred == null)
        //        throw new InvalidOperationException($"No se encontraron credenciales configuradas para la base de datos '{database}'.");

        //    if (liquidacion.Emisor == null)
        //        throw new ArgumentException("La información del emisor es requerida.", nameof(liquidacion.Emisor));
        //    if (liquidacion.Receptor == null)
        //        throw new ArgumentException("La información del receptor es requerida.", nameof(liquidacion.Receptor));
        //    if (liquidacion.Nomina == null)
        //        throw new ArgumentException("La información de nómina es requerida.", nameof(liquidacion.Nomina));
        //    if (liquidacion.ComplementoEmisor == null)
        //        throw new ArgumentException("El complemento del emisor es requerido.", nameof(liquidacion.ComplementoEmisor));
        //    if (liquidacion.ComplementoReceptor == null)
        //        throw new ArgumentException("El complemento del receptor es requerido.", nameof(liquidacion.ComplementoReceptor));

        //    var request = new RequestBE
        //    {
        //        usuario = cred.User,
        //        password = cred.Password,
        //        AdditionalInformation = new RequestBEAdditionalInformation
        //        {
        //            fileType = cred.FileType,
        //            titulo = $"{liquidacion.Serie}-{liquidacion.Folio}",
        //            conector = "6094209",
        //            comentario = "Liquidacion"
        //        }
        //    };

        //    var subTotalRedondeado = decimal.Round(liquidacion.SubTotal, 2, MidpointRounding.AwayFromZero);
        //    var descuentoRedondeado = decimal.Round(liquidacion.Descuento, 2, MidpointRounding.AwayFromZero);
        //    var totalCalculado = decimal.Round(subTotalRedondeado - descuentoRedondeado, 2, MidpointRounding.AwayFromZero);

        //    var comprobante = new BuzonE.Comprobante
        //    {
        //        Version = liquidacion.Version,
        //        Serie = liquidacion.Serie,
        //        Folio = liquidacion.Folio.ToString(),
        //        FechaSpecified = true,
        //        Fecha = new DateTime(
        //            liquidacion.Fecha.Year,
        //            liquidacion.Fecha.Month,
        //            liquidacion.Fecha.Day,
        //            liquidacion.Fecha.Hour,
        //            liquidacion.Fecha.Minute,
        //            liquidacion.Fecha.Second,
        //            DateTimeKind.Unspecified
        //        ),

        //        Moneda = ParseEnumSafe<BuzonE.c_Moneda>(liquidacion.Moneda, nameof(liquidacion.Moneda)),
        //        Exportacion = ParseEnumSafe<BuzonE.c_Exportacion>(liquidacion.Exportacion, nameof(liquidacion.Exportacion), true),

        //        //SubTotal = decimal.Round(liquidacion.SubTotal, 2, MidpointRounding.AwayFromZero),
        //        //Descuento = decimal.Round(liquidacion.Descuento, 2, MidpointRounding.AwayFromZero),
        //        //Total = decimal.Round(liquidacion.Total, 2, MidpointRounding.AwayFromZero),

        //        SubTotal = subTotalRedondeado,
        //        Descuento = descuentoRedondeado,
        //        Total = totalCalculado,

        //        LugarExpedicion = liquidacion.LugarExpedicion.ToString(),
        //        MetodoPagoSpecified = true,
        //        MetodoPago = ParseEnumSafe<BuzonE.c_MetodoPago>(liquidacion.MetodoPago, nameof(liquidacion.MetodoPago)),
        //        TipoDeComprobante = ParseEnumSafe<BuzonE.c_TipoDeComprobante>(liquidacion.TipoDeComprobante, nameof(liquidacion.TipoDeComprobante))
        //    };

        //    comprobante.Emisor = new BuzonE.ComprobanteEmisor
        //    {
        //        Rfc = liquidacion.Emisor.rfc,
        //        Nombre = liquidacion.Emisor.nombre,
        //        RegimenFiscal = ParseEnumSafe<BuzonE.c_RegimenFiscal>(liquidacion.Emisor.claveSAT, nameof(liquidacion.Emisor.claveSAT), true)
        //    };

        //    comprobante.Receptor = new BuzonE.ComprobanteReceptor
        //    {
        //        Rfc = liquidacion.Receptor.rfc,
        //        Nombre = liquidacion.Receptor.nombre,
        //        DomicilioFiscalReceptor = liquidacion.Receptor.DomicilioFiscalReceptor,
        //        RegimenFiscalReceptor = ParseEnumSafe<BuzonE.c_RegimenFiscal>(liquidacion.Receptor.RegimenFiscalReceptor, nameof(liquidacion.Receptor.RegimenFiscalReceptor), true),
        //        UsoCFDI = ParseEnumSafe<BuzonE.c_UsoCFDI>(liquidacion.Receptor.UsoCFDI, nameof(liquidacion.Receptor.UsoCFDI))
        //    };

        //    comprobante.Conceptos = new[]
        //    {
        //        new BuzonE.ComprobanteConcepto
        //        {
        //            ClaveProdServ = "84111505",
        //            Cantidad = 1m,
        //            ClaveUnidad = BuzonE.c_ClaveUnidad.ACT,
        //            Descripcion = "Pago de nómina",
        //            ValorUnitario = decimal.Round(liquidacion.TotalesPercepciones.TotalPercepciones, 2, MidpointRounding.AwayFromZero),
        //            Importe = decimal.Round(liquidacion.TotalesPercepciones.TotalPercepciones, 2, MidpointRounding.AwayFromZero),
        //            DescuentoSpecified = false,
        //            //Descuento = decimal.Round(liquidacion.TotalesDeducciones.TotalDeducciones, 2, MidpointRounding.AwayFromZero),
        //            ObjetoImp = BuzonE.c_ObjetoImp.Item01
        //        }
        //    };

        //    var complemento = new BuzonE.ComprobanteComplemento();
        //    var nomina = new BuzonE.Nomina
        //    {
        //        Version = liquidacion.Nomina.Version,
        //        TipoNomina = ParseEnumSafe<BuzonE.c_TipoNomina>(liquidacion.Nomina.TipoNomina, nameof(liquidacion.Nomina.TipoNomina)),
        //        FechaPago = liquidacion.Nomina.FechaPago,
        //        FechaInicialPago = liquidacion.Nomina.FechaInicialPago,
        //        FechaFinalPago = liquidacion.Nomina.FechaFinalPago,
        //        NumDiasPagados = liquidacion.Nomina.NumDiasPagados,
        //        TotalPercepcionesSpecified = true,
        //        TotalPercepciones = decimal.Round(liquidacion.TotalesPercepciones.TotalPercepciones, 2, MidpointRounding.AwayFromZero),
        //        TotalDeduccionesSpecified = true,
        //        TotalDeducciones = decimal.Round(liquidacion.TotalesDeducciones.TotalDeducciones, 2, MidpointRounding.AwayFromZero)
        //    };

        //    nomina.Percepciones = new BuzonE.NominaPercepciones
        //    {
        //        Percepcion = liquidacion.Percepciones.Select(p => new BuzonE.NominaPercepcionesPercepcion
        //        {
        //            TipoPercepcion = ParseEnumSafe<BuzonE.c_TipoPercepcion>(p.TipoPercepcion, nameof(p.TipoPercepcion), true),
        //            Clave = p.Clave,
        //            Concepto = p.Concepto,
        //            ImporteGravado = decimal.Round(p.ImporteGravado, 2, MidpointRounding.AwayFromZero),
        //            ImporteExento = decimal.Round(p.ImporteExento, 2, MidpointRounding.AwayFromZero)
        //        }).ToArray(),
        //        TotalSueldosSpecified = true,
        //        TotalSueldos = decimal.Round(liquidacion.TotalesPercepciones.TotalSueldos, 2, MidpointRounding.AwayFromZero),
        //        TotalGravado = decimal.Round(liquidacion.TotalesPercepciones.TotalGravado, 2, MidpointRounding.AwayFromZero),
        //        TotalExento = decimal.Round(liquidacion.TotalesPercepciones.TotalExento, 2, MidpointRounding.AwayFromZero)
        //    };

        //    nomina.Deducciones = new BuzonE.NominaDeducciones
        //    {
        //        Deduccion = liquidacion.Deducciones.Select(d => new BuzonE.NominaDeduccionesDeduccion
        //        {
        //            TipoDeduccion = ParseEnumSafe<BuzonE.c_TipoDeduccion>(d.TipoDeduccion.PadLeft(3, '0'), nameof(d.TipoDeduccion), true),
        //            Clave = d.Clave,
        //            Concepto = d.Concepto,
        //            Importe = decimal.Round(d.Importe, 2, MidpointRounding.AwayFromZero)
        //        }).ToArray(),
        //        TotalOtrasDeduccionesSpecified = true,
        //        TotalOtrasDeducciones = decimal.Round(liquidacion.TotalesDeducciones.TotalOtrasDeducciones, 2, MidpointRounding.AwayFromZero),
        //        TotalImpuestosRetenidosSpecified = true,
        //        TotalImpuestosRetenidos = decimal.Round(liquidacion.TotalesDeducciones.TotalImpuestosRetenidos, 2, MidpointRounding.AwayFromZero)
        //    };

        //    nomina.Emisor = new BuzonE.NominaEmisor
        //    {
        //        RegistroPatronal = liquidacion.ComplementoEmisor.RegistroPatronal
        //    };

        //    nomina.Receptor = new BuzonE.NominaReceptor
        //    {
        //        Curp = liquidacion.ComplementoReceptor.Curp,
        //        NumSeguridadSocial = liquidacion.ComplementoReceptor.NumSeguridadSocial,
        //        FechaInicioRelLaboralSpecified = true,
        //        FechaInicioRelLaboral = liquidacion.ComplementoReceptor.FechaInicioRelLaboral,
        //        Antigüedad = liquidacion.ComplementoReceptor.Antiguedad,
        //        TipoContrato = ParseEnumSafe<BuzonE.c_TipoContrato>(liquidacion.ComplementoReceptor.TipoContrato, nameof(liquidacion.ComplementoReceptor.TipoContrato), true),
        //        TipoRegimen = ParseEnumSafe<BuzonE.c_TipoRegimen>(liquidacion.ComplementoReceptor.TipoRegimen, nameof(liquidacion.ComplementoReceptor.TipoRegimen), true),
        //        NumEmpleado = liquidacion.ComplementoReceptor.NumEmpleado,
        //        Departamento = liquidacion.ComplementoReceptor.Departamento,
        //        Puesto = liquidacion.ComplementoReceptor.Puesto,
        //        RiesgoPuestoSpecified = false,
        //        //RiesgoPuesto = ParseEnumSafe<BuzonE.c_RiesgoPuesto>(liquidacion.ComplementoReceptor.RiesgoPuesto, nameof(liquidacion.ComplementoReceptor.RiesgoPuesto), true),
        //        PeriodicidadPago = ParseEnumSafe<BuzonE.c_PeriodicidadPago>(liquidacion.ComplementoReceptor.PeriodicidadPago, nameof(liquidacion.ComplementoReceptor.PeriodicidadPago), true),
        //        BancoSpecified = !string.IsNullOrEmpty(liquidacion.ComplementoReceptor.ClaveBanco),
        //        Banco = string.IsNullOrEmpty(liquidacion.ComplementoReceptor.ClaveBanco) ? BuzonE.c_Banco.Item058 : ParseEnumSafe<BuzonE.c_Banco>(liquidacion.ComplementoReceptor.ClaveBanco, nameof(liquidacion.ComplementoReceptor.ClaveBanco), true),
        //        SalarioBaseCotAporSpecified = true,
        //        SalarioBaseCotApor = liquidacion.ComplementoReceptor.SalarioBaseCotApor,
        //        SalarioDiarioIntegradoSpecified = true,
        //        SalarioDiarioIntegrado = liquidacion.ComplementoReceptor.SalarioDiarioIntegrado,
        //        //ClaveEntFed = ParseEnumSafe<BuzonE.c_Estado>(liquidacion.ComplementoReceptor.ClaveEntFed, nameof(liquidacion.ComplementoReceptor.ClaveEntFed))
        //    };

        //    complemento.Nomina = new[] { nomina };
        //    comprobante.Complemento = new[] { complemento };
        //    request.Comprobante = comprobante;

        //    return Task.FromResult(request);
        //}

        public Task<RequestBE> ConstruirRequestBuzonEAsync(CfdiNomina liquidacion, string database)
        {
            if (liquidacion is null)
                throw new ArgumentNullException(nameof(liquidacion), "La liquidación es requerida");

            var cred = _buzonEApiCredentials.FirstOrDefault(c => c.Database.Equals(database, StringComparison.OrdinalIgnoreCase));
            if (cred == null)
                throw new InvalidOperationException($"No se encontraron credenciales configuradas para la base de datos '{database}'.");

            if (liquidacion.Emisor == null || liquidacion.Receptor == null || liquidacion.Nomina == null ||
                liquidacion.ComplementoEmisor == null || liquidacion.ComplementoReceptor == null)
                throw new ArgumentException("Faltan datos obligatorios de emisor, receptor o complementos.");

            // Cálculo del subtotal: suma de percepciones (gravado + exento)  
            var subTotal = liquidacion.Percepciones.Sum(p => p.ImporteGravado + p.ImporteExento);
            subTotal = decimal.Round(subTotal, 2);

            // El total del comprobante es igual al subtotal en CFDI de nómina
            var total = subTotal;

            // Validación de percepciones
            var sumaPercepciones = liquidacion.Percepciones.Sum(p => p.ImporteGravado + p.ImporteExento);

            var totalPercepciones = decimal.Round(liquidacion.TotalesPercepciones.TotalPercepciones, 2);
            if (decimal.Round(sumaPercepciones, 2) != totalPercepciones)
                throw new Exception($"TotalPercepciones no cuadra con la suma de percepciones. Esperado: {totalPercepciones}, Calculado: {sumaPercepciones}");

            // Validación de deducciones
            var sumaDeducciones = decimal.Round(liquidacion.Deducciones.Sum(d => decimal.Round(d.Importe, 2)), 2);
            var totalDeducciones = decimal.Round(liquidacion.TotalesDeducciones.TotalDeducciones, 2);
            if (sumaDeducciones != totalDeducciones)
                throw new Exception($"TotalDeducciones no cuadra con la suma de deducciones. Esperado: {totalDeducciones}, Calculado: {sumaDeducciones}");

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
                SubTotal = subTotal,
                DescuentoSpecified = false,
                Total = total,
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
                    ValorUnitario = subTotal,
                    Importe = subTotal,
                    DescuentoSpecified = false,
                    //Descuento = 0,
                    ObjetoImp = BuzonE.c_ObjetoImp.Item01
                }
            };

            if (comprobante.TipoDeComprobante != BuzonE.c_TipoDeComprobante.N)
                throw new InvalidOperationException($"TipoDeComprobante esperado 'N', actual '{comprobante.TipoDeComprobante}'.");
            if (comprobante.Conceptos.Any(c => c.DescuentoSpecified))
                throw new InvalidOperationException("No debe existir Descuento en conceptos de nómina.");

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
                TotalDeducciones = totalDeducciones
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
                RiesgoPuestoSpecified = false,
                PeriodicidadPago = ParseEnumSafe<BuzonE.c_PeriodicidadPago>(liquidacion.ComplementoReceptor.PeriodicidadPago, nameof(liquidacion.ComplementoReceptor.PeriodicidadPago), true),
                BancoSpecified = !string.IsNullOrEmpty(liquidacion.ComplementoReceptor.ClaveBanco),
                Banco = string.IsNullOrEmpty(liquidacion.ComplementoReceptor.ClaveBanco) ? BuzonE.c_Banco.Item058 :
                    ParseEnumSafe<BuzonE.c_Banco>(liquidacion.ComplementoReceptor.ClaveBanco, nameof(liquidacion.ComplementoReceptor.ClaveBanco), true),
                SalarioBaseCotAporSpecified = true,
                SalarioBaseCotApor = liquidacion.ComplementoReceptor.SalarioBaseCotApor,
                SalarioDiarioIntegradoSpecified = true,
                SalarioDiarioIntegrado = liquidacion.ComplementoReceptor.SalarioDiarioIntegrado
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
