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

            var request = await _validacionesNominaSat.ConstruirRequestBuzonEAsync(liquidacion, database);

            await _repository.InsertarHistoricoAsync(database, noLiquidacion, liquidacionJson);

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
                    await _repository.InsertarDocTimbradoLiqAsync(database, noLiquidacion, xmlBytes, null, responseServicio.uuid);

                    respuesta.IsSuccess = true;
                    respuesta.Mensaje = "Timbrado exitoso";
                    respuesta.XmlByteArray = xmlBytes;
                    respuesta.PdfByteArray = Array.Empty<byte>();
                }
                else
                {
                    await _repository.ActualizarEstatusAsync(database, noLiquidacion, 2);
                    await _repository.InsertarDocTimbradoLiqAsync(database, noLiquidacion, null, null, null);

                    respuesta.IsSuccess = false;
                    respuesta.Mensaje = responseServicio?.mensaje ?? "Error en timbrado";
                    if (!string.IsNullOrWhiteSpace(responseServicio?.mensajeErrorTimbrado))
                        respuesta.Errores.Add(responseServicio.mensajeErrorTimbrado);
                }
            }
            catch (System.Exception ex)
            {
                await _repository.ActualizarEstatusAsync(database, noLiquidacion, 2);
                await _repository.InsertarDocTimbradoLiqAsync(database, noLiquidacion, null, null, null);

                respuesta.IsSuccess = false;
                respuesta.Mensaje = "Ocurrió un error al timbrar";
                respuesta.Errores.Add(ex.Message);
            }

            return respuesta;
        }
    }
}
