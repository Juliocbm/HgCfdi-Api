using CFDI.Data.Contexts;
//using HG.CFDI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.ContextFactory
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly IConfiguration _configuration;

        public DbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbContextOptions<CfdiDbContext> CreateDbContextOptions(string connectionStringName)
        {
            var connectionString = _configuration.GetConnectionString(connectionStringName);
            var optionsBuilder = new DbContextOptionsBuilder<CfdiDbContext>();
            optionsBuilder.UseSqlServer(connectionString, providerOptions => providerOptions.EnableRetryOnFailure());

            return optionsBuilder.Options;
        }
    }
}
