using HG.CFDI.CORE.ContextFactory;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using HG.CFDI.CORE.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CFDI.Data.Contexts;
using System.Data;
using System;
using CFDI.Data.Entities;
using HG.CFDI.CORE.Models.DtoLiquidacionCfdi;

namespace HG.CFDI.DATA.Repositories
{
    public class LiquidacionRepository : ILiquidacionRepository
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger<LiquidacionRepository> _logger;

        public LiquidacionRepository(IDbContextFactory dbContextFactory, ILogger<LiquidacionRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<string?> ObtenerDatosNominaJson(string database, int idLiquidacion)
        {
            string server = database switch
            {
                "hgdb_lis" => "server2019",
                "chdb_lis" => "server2008",
                "rldb_lis" => "server2008",
                "lindadb" => "server2008",
                _ => "server2019"
            };

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "cfdi.obtenerDatosNominaJSON";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Database", database));
            command.Parameters.Add(new SqlParameter("@NoLiquidacion", idLiquidacion));

            await context.Database.OpenConnectionAsync();
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return reader.IsDBNull(0) ? null : reader.GetString(0);
                }
                return null;
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        public async Task<GeneralResponse<LiquidacionDto>> ObtenerLiquidacionesAsync(ParametrosGenerales parametros, string database)
        {
            string server = database switch
            {
                "hgdb_lis" => "server2019",
                "chdb_lis" => "server2008",
                "rldb_lis" => "server2008",
                "lindadb" => "server2008",
                _ => "server2019"
            };

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "cfdi.obtenerLiquidaciones";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Database", database));

            await context.Database.OpenConnectionAsync();
            try
            {
                using var reader = await command.ExecuteReaderAsync();

                var result = new List<LiquidacionDto>();

                // Obt�n los �ndices una sola vez
                int ordIdLiquidacion = reader.GetOrdinal("IdLiquidacion");
                int ordNombre = reader.GetOrdinal("Nombre");
                int ordRfc = reader.GetOrdinal("rfc");
                int ordFecha = reader.GetOrdinal("Fecha");
                int ordEstatus = reader.GetOrdinal("estatus");
                int ordMensaje = reader.GetOrdinal("mensaje");        // <- aqu� est� la clave
                int ordIntentos = reader.GetOrdinal("Intentos");
                int ordProxIntento = reader.GetOrdinal("ProximoIntento");
                int ordXml = reader.GetOrdinal("Xml");
                int ordPdf = reader.GetOrdinal("Pdf");
                int ordUuid = reader.GetOrdinal("Uuid");

                while (await reader.ReadAsync())
                {
                    var dto = new LiquidacionDto
                    {
                        IdLiquidacion = reader.GetInt32(ordIdLiquidacion),
                        Nombre = reader.GetString(ordNombre),
                        Rfc = reader.GetString(ordRfc),
                        Fecha = reader.GetDateTime(ordFecha),
                        Estatus = reader.GetByte(ordEstatus),

                        // Evita romperte si viene NULL
                        Mensaje = reader.IsDBNull(ordMensaje) ? null : reader.GetString(ordMensaje),

                        Intentos = reader.GetFieldValue<short>(ordIntentos),
                        ProximoIntento = reader.IsDBNull(ordProxIntento) ? null : reader.GetDateTime(ordProxIntento),
                        Xml = reader.IsDBNull(ordXml) ? null : (byte[])reader[ordXml],
                        Pdf = reader.IsDBNull(ordPdf) ? null : (byte[])reader[ordPdf],
                        Uuid = reader.IsDBNull(ordUuid) ? null : reader.GetString(ordUuid)
                    };

                    result.Add(dto);
                }


                var query = result.AsQueryable();

                //query = query.OrderByDynamic(parametros.OrdenarPor, parametros.Descending, nameof(LiquidacionDto.IdLiquidacion), true);

                if (parametros.filtrosPorColumna != null)
                {
                    foreach (var filtro in parametros.filtrosPorColumna)
                    {
                        if (!string.IsNullOrEmpty(filtro.Value))
                        {
                            switch (filtro.Key.ToLower())
                            {
                                case "idliquidacion":
                                    if (int.TryParse(filtro.Value, out int idLiqu))
                                        query = query.Where(l => l.IdLiquidacion.ToString().Contains(idLiqu.ToString()));
                                    break;
                                case "nombre":
                                    query = query.Where(l => l.Nombre != null && l.Nombre.ToLower().Contains(filtro.Value.ToLower()));
                                    break;
                                case "rfc":
                                    query = query.Where(l => l.Rfc != null && l.Rfc.ToLower().Contains(filtro.Value.ToLower()));
                                    break;
                                case "uuid":
                                    query = query.Where(l => l.Uuid != null && l.Uuid.ToLower().Contains(filtro.Value.ToLower()));
                                    break;
                                case "fecha":
                                    var fechas = filtro.Value.Split('-');
                                    if (fechas.Length == 2 &&
                                        DateTime.TryParse(fechas[0], out DateTime fechaInicio) &&
                                        DateTime.TryParse(fechas[1], out DateTime fechaFin))
                                    {
                                        // Aseguramos que las fechas no est�n desordenadas
                                        if (fechaInicio > fechaFin)
                                        {
                                            var temp = fechaInicio;
                                            fechaInicio = fechaFin;
                                            fechaFin = temp;
                                        }

                                        // Filtramos por rango de fecha
                                        query = query.Where(l => l.Fecha.Date >= fechaInicio.Date && l.Fecha.Date <= fechaFin.Date);
                                    }
                                    else if (DateTime.TryParse(filtro.Value, out DateTime fechaUnica))
                                    {
                                        // Si no es un rango, pero es una sola fecha v�lida
                                        query = query.Where(l => l.Fecha.Date == fechaUnica.Date);
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }

                var totalRecords = query.Count();
                var items = query
                    .Skip((parametros.NoPagina - 1) * parametros.TamanoPagina)
                    .Take(parametros.TamanoPagina)
                    .ToList();

                return new GeneralResponse<LiquidacionDto>
                {
                    TotalRecords = totalRecords,
                    Items = items,
                    IsSuccess = true,
                    Message = "Liquidaciones consultadas correctamente."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<LiquidacionDto>
                {
                    IsSuccess = false,
                    Message = "Error al obtener las liquidaciones.",
                    ErrorList = GetAllExceptionMessages(ex)
                };
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        private List<string> GetAllExceptionMessages(System.Exception ex)
        {
            var messages = new List<string>();
            while (ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }
            return messages;
        }

        public async Task<liquidacionOperador?> ObtenerCabeceraAsync(int idCompania, int idLiquidacion)
        {
            _logger.LogInformation("Inicio ObtenerCabeceraAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
            string server = "server2019";

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            var result = await context.liquidacionOperadors
                .FirstOrDefaultAsync(l => l.IdLiquidacion == idLiquidacion && l.IdCompania == idCompania);
            _logger.LogInformation("Fin ObtenerCabeceraAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
            return result;
        }

        public async Task RegistrarInicioIntentoAsync(int idCompania, int idLiquidacion, byte estatus, string liquidacionJson, string? mensajeCorto = null)
        {
            _logger.LogInformation("Inicio RegistrarInicioIntentoAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
            string server = "server2019";

            var options = _dbContextFactory.CreateDbContextOptions(server);

            var executionStrategy = new CfdiDbContext(options).Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var context = new CfdiDbContext(options);
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var entidad = await context.liquidacionOperadors
                        .FirstOrDefaultAsync(l => l.IdLiquidacion == idLiquidacion && l.IdCompania == idCompania);

                    if (entidad is null)
                        throw new InvalidOperationException("Liquidacion no encontrada.");

                    short nuevoIntento = (short)(entidad.Intentos + 1);

                    entidad.Estatus = estatus;
                    entidad.Intentos = nuevoIntento;
                    entidad.UltimoIntento = nuevoIntento;
                    entidad.FechaProximoIntento = null;
                    if (!string.IsNullOrWhiteSpace(mensajeCorto))
                        entidad.MensajeCorto = mensajeCorto;

                    await context.SaveChangesAsync();

                    var historico = new liquidacionOperadorHist
                    {
                        IdLiquidacion = idLiquidacion,
                        IdCompania = idCompania,
                        NumeroIntento = nuevoIntento,
                        EstadoIntento = ObtenerNombreEstado((EstatusLiquidacion)estatus),
                        SnapshotData = liquidacionJson,
                        FechaIntento = DateTime.UtcNow
                    };

                    context.liquidacionOperadorHists.Add(historico);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error RegistrarInicioIntentoAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
                    throw;
                }
            });
            _logger.LogInformation("Fin RegistrarInicioIntentoAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
        }

        public async Task ActualizarResultadoIntentoAsync(int idCompania, int idLiquidacion, byte estatus, DateTime? fechaProximoIntento = null, string? mensajeCorto = null)
        {
            _logger.LogInformation("Inicio ActualizarResultadoIntentoAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
            string server = "server2019";

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            var entidad = await context.liquidacionOperadors
                .FirstOrDefaultAsync(l => l.IdLiquidacion == idLiquidacion && l.IdCompania == idCompania);

            if (entidad is null)
            {
                _logger.LogInformation("Fin ActualizarResultadoIntentoAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
                throw new InvalidOperationException("Liquidacion no encontrada.");
            }

            entidad.Estatus = estatus;
            entidad.FechaProximoIntento = fechaProximoIntento;
            if (!string.IsNullOrWhiteSpace(mensajeCorto))
                entidad.MensajeCorto = mensajeCorto;
            await context.SaveChangesAsync();

            var historico = await context.liquidacionOperadorHists
                .Where(h => h.IdLiquidacion == idLiquidacion && h.IdCompania == idCompania && h.NumeroIntento == entidad.UltimoIntento)
                .OrderByDescending(h => h.IdHistorico)
                .FirstOrDefaultAsync();

            if (historico != null)
            {
                historico.EstadoIntento = ObtenerNombreEstado((EstatusLiquidacion)estatus);
                await context.SaveChangesAsync();
            }
            _logger.LogInformation("Fin ActualizarResultadoIntentoAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
        }

        //public async Task InsertarDocTimbradoLiqAsync(int idCompania, int idLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid)
        //{
        //    _logger.LogInformation("Inicio InsertarDocTimbradoLiqAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
        //    string server = "server2019";

        //    var options = _dbContextFactory.CreateDbContextOptions(server);
        //    using var context = new CfdiDbContext(options);

        //    var entidad = await context.liquidacionOperadors
        //        .FirstOrDefaultAsync(l => l.IdLiquidacion == idLiquidacion && l.IdCompania == idCompania);

        //    if (entidad != null)
        //    {
        //        entidad.XMLTimbrado = xmlTimbrado;
        //        entidad.PDFTimbrado = pdfTimbrado;
        //        entidad.UUID = uuid;
        //        await context.SaveChangesAsync();
        //    }
        //    _logger.LogInformation("Fin InsertarDocTimbradoLiqAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
        //}

        public async Task InsertarDocTimbradoLiqAsync(int idCompania, int idLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid)
        {
            _logger.LogInformation("Inicio InsertarDocTimbradoLiqAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
            string server = "server2019";

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            var entidad = await context.liquidacionOperadors
                .FirstOrDefaultAsync(l => l.IdLiquidacion == idLiquidacion && l.IdCompania == idCompania);

            if (entidad != null)
            {
                // Solo actualizar los campos que no son nulos
                if (xmlTimbrado != null)
                    entidad.XMLTimbrado = xmlTimbrado;
                if (pdfTimbrado != null)
                    entidad.PDFTimbrado = pdfTimbrado;
                if (uuid != null)
                    entidad.UUID = uuid;

                await context.SaveChangesAsync();
            }
            _logger.LogInformation("Fin InsertarDocTimbradoLiqAsync Compania:{IdCompania} Liquidacion:{IdLiquidacion}", idCompania, idLiquidacion);
        }

        public async Task RegistrarErrorIntentoAsync(int idCompania, int idLiquidacion, short numeroIntento, string error)
        {
            string server = "server2019";

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            var historico = await context.liquidacionOperadorHists
                .Where(h => h.IdLiquidacion == idLiquidacion && h.IdCompania == idCompania && h.NumeroIntento == numeroIntento)
                .OrderByDescending(h => h.IdHistorico)
                .FirstOrDefaultAsync();

            var registro = new liquidacionOperadorHistError
            {
                IdHistorico = historico?.IdHistorico ?? 0,
                IdLiquidacion = idLiquidacion,
                IdCompania = idCompania,
                NumeroIntento = numeroIntento,
                TextoError = error
            };

            context.liquidacionOperadorHistErrors.Add(registro);
            await context.SaveChangesAsync();
        }


        public string ObtenerNombreEstado(EstatusLiquidacion estatus) => estatus switch
        {
            EstatusLiquidacion.Pendiente => "Pendiente",
            EstatusLiquidacion.EnProceso => "EnProceso",
            EstatusLiquidacion.RequiereRevision => "RequiereRevision",
            EstatusLiquidacion.Timbrado => "Timbrado",
            EstatusLiquidacion.ErrorTransitorio => "ErrorTransitorio",
            EstatusLiquidacion.Migrada => "Migrada",
            _ => "Desconocido"
        };
    }
}
