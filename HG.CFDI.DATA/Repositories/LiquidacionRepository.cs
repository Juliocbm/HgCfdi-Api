using HG.CFDI.CORE.ContextFactory;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CFDI.Data.Contexts;
using System.Data;
using System;
using CFDI.Data.Entities;

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

        public async Task RegistrarInicioIntentoAsync(int idCompania, int idLiquidacion, byte estatus, string liquidacionJson)
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

        public async Task ActualizarResultadoIntentoAsync(int idCompania, int idLiquidacion, byte estatus, DateTime? fechaProximoIntento = null)
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
                entidad.XMLTimbrado = xmlTimbrado;
                entidad.PDFTimbrado = pdfTimbrado;
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
