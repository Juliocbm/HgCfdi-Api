using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using HG.CFDI.CORE.Models;
using BuzonE;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using HG.CFDI.SERVICE.Services.Timbrado_liquidacion.ValidacionesSat;
using System.Text;

namespace HG.CFDI.SERVICE.Services
{
    public class LiquidacionService : ILiquidacionService
    {
        private readonly ILiquidacionRepository _repository;
        private readonly IValidacionesNominaSatService _validacionesNominaSat;

        public LiquidacionService(IValidacionesNominaSatService validacionesNominaSat, ILiquidacionRepository repository, IOptions<List<BuzonEApiCredential>> buzonEOptions)
        {
            _repository = repository;
            _validacionesNominaSat = validacionesNominaSat;
        }

        public async Task<CfdiNomina?> ObtenerLiquidacion(int idCompania, int noLiquidacion)
        {
            string? database = ObtenerDatabase(idCompania);
            if (string.IsNullOrEmpty(database))
                return null;

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
     
        public async Task<UniqueResponse> TimbrarLiquidacionAsync(int idCompania, int noLiquidacion)
        {
            var respuesta = new UniqueResponse();

            var cabecera = await _repository.ObtenerCabeceraAsync(idCompania, noLiquidacion);
            if (cabecera != null && cabecera.Estatus == (byte)EstatusLiquidacion.Timbrado)
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "La liquidación ya fue timbrada";
                return respuesta;
            }

            string? database = ObtenerDatabase(idCompania);

            var liquidacion = await ObtenerLiquidacion(idCompania, noLiquidacion);
            if (liquidacion == null)
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Liquidación no encontrada";
                return respuesta;
            }

            string liquidacionJson = JsonSerializer.Serialize(liquidacion);
            await _repository.RegistrarInicioIntentoAsync(idCompania, noLiquidacion, (byte)EstatusLiquidacion.EnProceso, liquidacionJson);

            var request = await _validacionesNominaSat.ConstruirRequestBuzonEAsync(liquidacion, database);

            try
            {
                // Consumir servicio Buzón E
                BuzonE.responseBE responseServicio;
                using (var client = new EmisionServiceClient())
                {
                    responseServicio = await client.emitirFacturaAsync(request);
                    await client.CloseAsync();
                }

                if (responseServicio?.code == "BE-EMS.200")
                {
                    // Timbrado exitoso
                    byte[] xmlBytes = Encoding.UTF8.GetBytes(responseServicio.xmlCFDTimbrado);

                    await _repository.ActualizarResultadoIntentoAsync(idCompania, noLiquidacion, (byte)EstatusLiquidacion.Timbrado);
                    await _repository.InsertarDocTimbradoLiqAsync(idCompania, noLiquidacion, xmlBytes, null, responseServicio.uuid);

                    respuesta.IsSuccess = true;
                    respuesta.Mensaje = "Timbrado exitoso";
                    respuesta.XmlByteArray = xmlBytes;
                    respuesta.PdfByteArray = Array.Empty<byte>(); // si vas a generar PDF luego
                }
                else
                {
                    // Error en timbrado del PAC
                    await RegistrarFalloDeTimbrado(idCompania, noLiquidacion, false);

                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = responseServicio?.mensaje ?? "Error en timbrado";

                    string mensajeError = responseServicio?.mensajeErrorTimbrado ?? respuesta.Mensaje;
                    if (!string.IsNullOrWhiteSpace(responseServicio?.mensajeErrorTimbrado))
                        respuesta.Errores.Add(responseServicio.mensajeErrorTimbrado);

                    var cabeceraActual = await _repository.ObtenerCabeceraAsync(idCompania, noLiquidacion);
                    if (cabeceraActual != null)
                        await _repository.RegistrarErrorIntentoAsync(idCompania, noLiquidacion, cabeceraActual.UltimoIntento, mensajeError);
                }
            }
            catch (Exception ex)
            {
                // Fallo inesperado
                bool transitorio = ex is TimeoutException || ex is HttpRequestException;
                await RegistrarFalloDeTimbrado(idCompania, noLiquidacion, transitorio);

                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Ocurrió un error al timbrar";
                respuesta.Errores.Add(ex.Message);

                var cabeceraActual = await _repository.ObtenerCabeceraAsync(idCompania, noLiquidacion);
                if (cabeceraActual != null)
                    await _repository.RegistrarErrorIntentoAsync(idCompania, noLiquidacion, cabeceraActual.UltimoIntento, ex.Message);
            }

            return respuesta;
        }

        private async Task RegistrarFalloDeTimbrado(int idCompania, int noLiquidacion, bool transitorio)
        {
            if (transitorio)
            {
                DateTime proximo = DateTime.UtcNow.AddHours(1);
                await _repository.ActualizarResultadoIntentoAsync(idCompania, noLiquidacion, (byte)EstatusLiquidacion.ErrorTransitorio, proximo);
            }
            else
            {
                await _repository.ActualizarResultadoIntentoAsync(idCompania, noLiquidacion, (byte)EstatusLiquidacion.RequiereRevision);
            }

            await _repository.InsertarDocTimbradoLiqAsync(idCompania, noLiquidacion, null, null, null);
        }

        public enum EstatusLiquidacion : byte
        {
            Pendiente = 0,
            EnProceso = 1,
            ErrorTransitorio = 4,
            RequiereRevision = 2,
            Timbrado = 3,
            Migrada = 5
        }

        private static string? ObtenerDatabase(int idCompania)
        {
            return idCompania switch
            {
                1 => "hgdb_lis",
                2 => "chdb_lis",
                3 => "rldb_lis",
                4 => "lindadb",
                _ => null
            };
        }

    }
}
