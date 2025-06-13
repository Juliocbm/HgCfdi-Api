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

            var comprobante = new BuzonE.Comprobante
            {
                Version = liquidacion.Version,
                Serie = liquidacion.Serie,
                Folio = liquidacion.Folio.ToString(),
                FechaSpecified = true,
                Fecha = liquidacion.Fecha,
                Moneda = Enum.Parse<BuzonE.c_Moneda>(liquidacion.Moneda),
                Exportacion = (BuzonE.c_Exportacion)Enum.Parse(typeof(c_Exportacion), "Item" + liquidacion.Exportacion),
                SubTotal = liquidacion.TotalesPercepciones.TotalPercepciones,
                DescuentoSpecified = true,
                Descuento = liquidacion.TotalesDeducciones.TotalDeducciones,
                Total = liquidacion.TotalesPercepciones.TotalPercepciones - liquidacion.TotalesDeducciones.TotalDeducciones,
                LugarExpedicion = liquidacion.LugarExpedicion.ToString(),
                MetodoPagoSpecified = true,
                MetodoPago = Enum.Parse<BuzonE.c_MetodoPago>(liquidacion.MetodoPago),
                TipoDeComprobante = Enum.Parse<BuzonE.c_TipoDeComprobante>(liquidacion.TipoDeComprobante)
            };

            comprobante.Emisor = new BuzonE.ComprobanteEmisor
            {
                Rfc = liquidacion.Emisor.rfc,
                Nombre = liquidacion.Emisor.nombre,
                RegimenFiscal = (BuzonE.c_RegimenFiscal)Enum.Parse(typeof(BuzonE.c_RegimenFiscal), "Item" + liquidacion.Emisor.claveSAT)
            };

            comprobante.Receptor = new BuzonE.ComprobanteReceptor
            {
                Rfc = liquidacion.Receptor.rfc,
                Nombre = liquidacion.Receptor.nombre,
                DomicilioFiscalReceptor = liquidacion.Receptor.DomicilioFiscalReceptor,
                RegimenFiscalReceptor = (BuzonE.c_RegimenFiscal)Enum.Parse(typeof(BuzonE.c_RegimenFiscal), "Item" + liquidacion.Receptor.RegimenFiscalReceptor),
                UsoCFDI = (BuzonE.c_UsoCFDI)Enum.Parse(typeof(BuzonE.c_UsoCFDI), liquidacion.Receptor.UsoCFDI)
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
                TipoNomina = (BuzonE.c_TipoNomina)Enum.Parse(typeof(BuzonE.c_TipoNomina), liquidacion.Nomina.TipoNomina),
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
                    TipoPercepcion = (BuzonE.c_TipoPercepcion)Enum.Parse(typeof(BuzonE.c_TipoPercepcion), "Item" + p.TipoPercepcion),
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
                    TipoDeduccion = (BuzonE.c_TipoDeduccion)Enum.Parse(typeof(BuzonE.c_TipoDeduccion), "Item" + d.TipoDeduccion.PadLeft(3, '0')),
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
                TipoContrato = (BuzonE.c_TipoContrato)Enum.Parse(typeof(BuzonE.c_TipoContrato), "Item" + liquidacion.ComplementoReceptor.TipoContrato),
                TipoRegimen = (BuzonE.c_TipoRegimen)Enum.Parse(typeof(BuzonE.c_TipoRegimen), "Item" + liquidacion.ComplementoReceptor.TipoRegimen),
                NumEmpleado = liquidacion.ComplementoReceptor.num_empleado,
                Departamento = liquidacion.ComplementoReceptor.Departamento,
                Puesto = liquidacion.ComplementoReceptor.Puesto,
                RiesgoPuestoSpecified = true,
                RiesgoPuesto = (BuzonE.c_RiesgoPuesto)Enum.Parse(typeof(BuzonE.c_RiesgoPuesto), "Item" + liquidacion.ComplementoReceptor.RiesgoPuesto),
                PeriodicidadPago = (BuzonE.c_PeriodicidadPago)Enum.Parse(typeof(BuzonE.c_PeriodicidadPago), "Item" + liquidacion.ComplementoReceptor.PeriodicidadPago),
                BancoSpecified = !string.IsNullOrEmpty(liquidacion.ComplementoReceptor.Banco),
                Banco = string.IsNullOrEmpty(liquidacion.ComplementoReceptor.Banco) ? BuzonE.c_Banco.Item002 : (BuzonE.c_Banco)Enum.Parse(typeof(BuzonE.c_Banco), "Item" + liquidacion.ComplementoReceptor.Banco),
                SalarioBaseCotAporSpecified = true,
                SalarioBaseCotApor = liquidacion.ComplementoReceptor.SalarioBaseCotApor,
                SalarioDiarioIntegradoSpecified = true,
                SalarioDiarioIntegrado = liquidacion.ComplementoReceptor.SalarioDiarioIntegrado,
                ClaveEntFed = (BuzonE.c_Estado)Enum.Parse(typeof(BuzonE.c_Estado), liquidacion.ComplementoReceptor.ClaveEntFed)
            };

            complemento.Nomina = new[] { nomina };
            comprobante.Complemento = new[] { complemento };
            request.Comprobante = comprobante;

            return Task.FromResult(request);
        }

        public async Task<UniqueResponse> TimbrarLiquidacionAsync(string database, int noLiquidacion)
        {
            UniqueResponse respuesta = new UniqueResponse();

            await _repository.ActualizarEstatusAsync(database, noLiquidacion, 1);

            var liquidacion = await ObtenerLiquidacion(database, noLiquidacion);
            if (liquidacion == null)
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Liquidación no encontrada";
                return respuesta;
            }

            var request = await ConstruirRequestBuzonEAsync(liquidacion, database);

            await _repository.InsertarHistoricoAsync(database, noLiquidacion, 1, null, null, null);

            try
            {
                BuzonE.responseBE responseServicio;
                using (var client = new EmisionServiceClient())
                {
                    responseServicio = await client.emitirFacturaAsync(request);
                    await client.CloseAsync();
                }

                if (responseServicio != null && responseServicio.code == "BE-EMS.200")
                {
                    byte[] xmlBytes = System.Text.Encoding.UTF8.GetBytes(responseServicio.xmlCFDTimbrado);
                    await _repository.ActualizarEstatusAsync(database, noLiquidacion, 3);
                    await _repository.InsertarHistoricoAsync(database, noLiquidacion, 3, xmlBytes, null, responseServicio.uuid);

                    respuesta.IsSuccess = true;
                    respuesta.Mensaje = "Timbrado exitoso";
                    respuesta.XmlByteArray = xmlBytes;
                    respuesta.PdfByteArray = Array.Empty<byte>();
                }
                else
                {
                    await _repository.ActualizarEstatusAsync(database, noLiquidacion, 2);
                    await _repository.InsertarHistoricoAsync(database, noLiquidacion, 2, null, null, null);

                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = responseServicio?.mensaje ?? "Error en timbrado";
                    if (!string.IsNullOrWhiteSpace(responseServicio?.mensajeErrorTimbrado))
                        respuesta.Errores.Add(responseServicio.mensajeErrorTimbrado);
                }
            }
            catch (System.Exception ex)
            {
                await _repository.ActualizarEstatusAsync(database, noLiquidacion, 2);
                await _repository.InsertarHistoricoAsync(database, noLiquidacion, 2, null, null, null);

                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Ocurrió un error al timbrar";
                respuesta.Errores.Add(ex.Message);
            }

            return respuesta;
        }
    }
}
