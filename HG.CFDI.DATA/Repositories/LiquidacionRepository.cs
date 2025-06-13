using HG.CFDI.CORE.ContextFactory;
using HG.CFDI.CORE.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CFDI.Data.Contexts;
using System.Data;

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
                "chdb_lis" => "server2008",
                "rldb_lis" => "server2008",
                "lindadb" => "server2008",
                _ => "server2019"
            };

            var options = _dbContextFactory.CreateDbContextOptions(server);
            using var context = new CfdiDbContext(options);
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "cfdi.actualizaEstatusLiquidacionOperador";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Database", database));
            command.Parameters.Add(new SqlParameter("@NoLiquidacion", noLiquidacion));
            command.Parameters.Add(new SqlParameter("@Estatus", estatus));

            await context.Database.OpenConnectionAsync();
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        public async Task InsertarHistoricoAsync(string database, int noLiquidacion, int estatus, byte[]? xmlTimbrado, byte[]? pdfTimbrado, string? uuid)
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
            command.CommandText = "cfdi.insertLiquidacionOperadorHist";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Database", database));
            command.Parameters.Add(new SqlParameter("@NoLiquidacion", noLiquidacion));
            command.Parameters.Add(new SqlParameter("@Estatus", estatus));
            command.Parameters.Add(new SqlParameter("@XmlTimbrado", SqlDbType.VarBinary) { Value = (object?)xmlTimbrado ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@PdfTimbrado", SqlDbType.VarBinary) { Value = (object?)pdfTimbrado ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Uuid", SqlDbType.VarChar) { Value = (object?)uuid ?? DBNull.Value });

            await context.Database.OpenConnectionAsync();
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }
    }
}
