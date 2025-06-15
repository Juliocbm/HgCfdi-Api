using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;
using HG.CFDI.CORE.Models;
using BuzonE;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using HG.CFDI.SERVICE.Services.Timbrado_liquidacion.ValidacionesSat;
using System.Text;

namespace HG.CFDI.SERVICE.Services
{
    public class LiquidacionService : ILiquidacionService
    {
        private readonly ILiquidacionRepository _repository;
        private readonly IValidacionesNominaSatService _validacionesNominaSat;
        private readonly List<BuzonEApiCredential> _buzonEApiCredentials;

        public LiquidacionService(IValidacionesNominaSatService validacionesNominaSat, ILiquidacionRepository repository, IOptions<List<BuzonEApiCredential>> buzonEOptions)
        {
            _repository = repository;
            _validacionesNominaSat = validacionesNominaSat;
            _buzonEApiCredentials = buzonEOptions.Value;
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

            string? database = ObtenerDatabase(idCompania);
            if (string.IsNullOrEmpty(database))
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "idCompania no válido";
                return respuesta;
            }

            // Estatus 1 = EnProceso
            await _repository.ActualizarEstatusAsync(database, noLiquidacion, (byte)EstatusLiquidacion.EnProceso);

            // Obtener datos de liquidación
            var liquidacion = await ObtenerLiquidacion(idCompania, noLiquidacion);
            if (liquidacion == null)
            {
                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Liquidación no encontrada";
                return respuesta;
            }

            // Generar el request para Buzón E
            var request = await _validacionesNominaSat.ConstruirRequestBuzonEAsync(liquidacion, database);

            // Guardar histórico de la liquidación
            string liquidacionJson = JsonSerializer.Serialize(liquidacion);
            await _repository.InsertarHistoricoAsync(database, noLiquidacion, liquidacionJson);

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

                    await _repository.ActualizarEstatusAsync(database, noLiquidacion, (byte)EstatusLiquidacion.Timbrado); // Estatus 5 = Timbrado
                    await _repository.InsertarDocTimbradoLiqAsync(database, noLiquidacion, xmlBytes, null, responseServicio.uuid);

                    respuesta.IsSuccess = true;
                    respuesta.Mensaje = "Timbrado exitoso";
                    respuesta.XmlByteArray = xmlBytes;
                    respuesta.PdfByteArray = Array.Empty<byte>(); // si vas a generar PDF luego
                }
                else
                {
                    // Error en timbrado del PAC
                    await RegistrarFalloDeTimbrado(database, noLiquidacion);

                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = responseServicio?.mensaje ?? "Error en timbrado";

                    if (!string.IsNullOrWhiteSpace(responseServicio?.mensajeErrorTimbrado))
                        respuesta.Errores.Add(responseServicio.mensajeErrorTimbrado);
                }
            }
            catch (Exception ex)
            {
                // Fallo inesperado
                await RegistrarFalloDeTimbrado(database, noLiquidacion);

                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Ocurrió un error al timbrar";
                respuesta.Errores.Add(ex.Message);
            }

            return respuesta;
        }

        private async Task RegistrarFalloDeTimbrado(string database, int noLiquidacion)
        {
            await _repository.ActualizarEstatusAsync(database, noLiquidacion, (byte)EstatusLiquidacion.RequiereRevision); // Estatus 2 = ErrorValidacion
            await _repository.InsertarDocTimbradoLiqAsync(database, noLiquidacion, null, null, null);
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
