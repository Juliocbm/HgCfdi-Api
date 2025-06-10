using CFDI.Data.Contexts;
//using HG.CFDI.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.CFDI.CORE.ContextFactory
{
    public interface IDbContextFactory
    {
        DbContextOptions<CfdiDbContext> CreateDbContextOptions(string connectionStringName);
    }
}
