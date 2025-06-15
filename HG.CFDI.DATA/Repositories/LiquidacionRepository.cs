using HG.CFDI.CORE.ContextFactory;
using HG.CFDI.CORE.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CFDI.Data.Contexts;
using System.Data;
using System;

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

        public async Task<string?> ObtenerDatosNominaJson(string database, int noLiquidacion)
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
            command.Parameters.Add(new SqlParameter("@NoLiquidacion", noLiquidacion));

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

        public async Task ActualizarEstatusAsync(string database, int noLiquidacion, int estatus)
        {
            string server = database switch
            {
                "hgdb_lis" => "server2019",
                "chdb_lis" => "server2019",
                "rldb_lis" => "server2019",
                "lindadb" => "server2019",
                _ => "server2019"
            };

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            string compania = database.Substring(0, 2);
            var entidad = await context.liquidacionHeaderLis.FirstOrDefaultAsync(l => l.idLiquidacion == noLiquidacion && l.compania == compania);
            if (entidad != null)
            {
                entidad.estatusTraslado = estatus;
                await context.SaveChangesAsync();
            }
        }

        public async Task InsertarDocTimbradoLiqAsync(string database, int noLiquidacion, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid)
        {
            string server = database switch
            {
                "hgdb_lis" => "server2019",
                "chdb_lis" => "server2019",
                "rldb_lis" => "server2019",
                "lindadb" => "server2019",
                _ => "server2019"
            };

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            string compania = database.Substring(0, 2);
            var parametros = new[]
            {
                new SqlParameter("@NoLiquidacion", noLiquidacion),
                new SqlParameter("@Compania", compania),
                new SqlParameter("@Xml", SqlDbType.VarBinary) { Value = (object?)xmlTimbrado ?? DBNull.Value },
                new SqlParameter("@Pdf", SqlDbType.VarBinary) { Value = (object?)pdfTimbrado ?? DBNull.Value },
                new SqlParameter("@Uuid", SqlDbType.VarChar) { Value = (object?)uuid ?? DBNull.Value }
            };

            string sql = @"UPDATE cfdi.liquidacionHeaderLi
                                SET xmlTimbrado = @Xml,
                                    pdfTimbrado = @Pdf,
                                    uuid = @Uuid
                              WHERE idLiquidacion = @NoLiquidacion AND compania = @Compania";

            await context.Database.ExecuteSqlRawAsync(sql, parametros);
        }

        public async Task InsertarHistoricoAsync(string database, int noLiquidacion, string liquidacionJson)
        {
            string server = database switch
            {
                "hgdb_lis" => "server2019",
                "chdb_lis" => "server2019",
                "rldb_lis" => "server2019",
                "lindadb" => "server2019",
                _ => "server2019"
            };

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);

            string compania = database.Substring(0, 2);
            var parametros = new[]
            {
                new SqlParameter("@NoLiquidacion", noLiquidacion),
                new SqlParameter("@Compania", compania),
                new SqlParameter("@Json", liquidacionJson)
            };

            string sql = @"INSERT INTO cfdi.liquidacionHistorico(idLiquidacion, compania, jsonSnapshot, fecha)
                             VALUES(@NoLiquidacion, @Compania, @Json, GETDATE())";

            await context.Database.ExecuteSqlRawAsync(sql, parametros);
        }
    }
}
