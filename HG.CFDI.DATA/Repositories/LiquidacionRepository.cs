using HG.CFDI.CORE.ContextFactory;
using HG.CFDI.CORE.Interfaces;
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
            command.Parameters.Add(new SqlParameter("@IdLiquidacion", idLiquidacion));

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

        public async Task ActualizarEstatusAsync(int idCompania, int idLiquidacion, byte estatus)
        {
            string server = "server2019";

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Buscar registro principal
                var entidad = await context.liquidacionOperadors
                    .FirstOrDefaultAsync(l => l.IdLiquidacion == idLiquidacion && l.IdCompania == idCompania);

                if (entidad is null)
                    throw new InvalidOperationException("Liquidación no encontrada.");

                // Incrementar número de intento
                short nuevoIntento = (short)(entidad.Intentos + 1);

                // Actualización de campos en la tabla principal
                entidad.Estatus = estatus;
                entidad.Intentos = nuevoIntento;
                entidad.UltimoIntento = nuevoIntento;
                entidad.FechaProximoIntento = null; // si aplica lógica de reintento

                // Guardar cambios en tabla principal
                await context.SaveChangesAsync();

                // Insertar histórico
                var historico = new liquidacionOperadorHist
                {
                    IdLiquidacion = idLiquidacion,
                    IdCompania = idCompania,
                    NumeroIntento = nuevoIntento,
                    EstadoIntento = ObtenerNombreEstado(estatus), // puedes mapear el estatus a texto
                    SnapshotData = null, // o serialización si tienes una fuente
                    FechaIntento = DateTime.UtcNow
                };

                context.liquidacionOperadorHists.Add(historico);
                await context.SaveChangesAsync();

                // Confirmar transacción
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task InsertarDocTimbradoLiqAsync(int idCompania, int idLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid)
        {
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
        }

        public async Task InsertarHistoricoAsync(int idCompania, int idLiquidacion, string liquidacionJson)
        {
            string server = "server2019";

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            var nuevoHistorial = new liquidacionOperadorHist
            {
                IdLiquidacion = idLiquidacion,
                IdCompania = idCompania,
                SnapshotData = liquidacionJson,
                FechaIntento = DateTime.UtcNow
            };

            context.liquidacionOperadorHists.Add(nuevoHistorial);
            await context.SaveChangesAsync();
        }

        public string ObtenerNombreEstado(byte estatus) => estatus switch
        {
            0 => "Pendiente",
            1 => "EnProceso",
            2 => "RequiereRevision",
            3 => "Timbrado",
            4 => "ErrorTransitorio",
            5 => "Migrada",
            _ => "Desconocido"
        };
    }
}
