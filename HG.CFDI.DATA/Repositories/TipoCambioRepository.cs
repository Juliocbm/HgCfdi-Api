//using HG.CFDI.API.Models;
using HG.CFDI.CORE.ContextFactory;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
//using HG.CFDI.CORE.Models.DocumentoTimbradoEF;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System;
//using HG.CFDI.CORE.Models.CartaPorteEF;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Azure;
//using HG.CFDI.CORE.Models.TipoCambioEF;
using HG.CFDI.CORE.Utilities;
using CFDI.Data.Contexts;
using CFDI.Data.Entities;

namespace HG.CFDI.DATA.Repositories
{

    public class TipoCambioRepository : ITipoCambioRepository
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger<CartaPorteRepository> _logger;

        public TipoCambioRepository(IDbContextFactory dbContextFactory, IMapper mapper, ILogger<CartaPorteRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }


        public async Task<GeneralResponse<tipoCambio>> DeleteTipoCambio(tipoCambio tipoCambio)
        {
            try
            {
                var options = _dbContextFactory.CreateDbContextOptions("server2019");
                using (var context = new CfdiDbContext(options))
                {
                    var tipoCambioExistente = await context.tipoCambios.FindAsync(tipoCambio.id);
                    if (tipoCambioExistente == null)
                    {
                        return new GeneralResponse<tipoCambio>
                        {
                            Data = null,
                            IsSuccess = false,
                            Message = "Tipo de cambio no encontrado."
                        };
                    }

                    // Eliminación lógica
                    tipoCambioExistente.activo = false;
                    tipoCambioExistente.modificadoPor = tipoCambio.modificadoPor;
                    tipoCambioExistente.fechaModificacion = DateTime.Now;

                    context.tipoCambios.Update(tipoCambioExistente);
                    await context.SaveChangesAsync();

                    return new GeneralResponse<tipoCambio>
                    {
                        Data = tipoCambio,
                        IsSuccess = true,
                        Message = "Tipo de cambio eliminado correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                return new GeneralResponse<tipoCambio>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Error al eliminar el tipo de cambio.",
                    ErrorList = GetAllExceptionMessages(ex)
                };
            }
        }


        public async Task<GeneralResponse<vwTipoCambio>> GetTipoCambioById(int idTipoCambio)
        {
            try
            {
                // Crear opciones de DbContext basado en el nombre de la cadena de conexión
                var options = _dbContextFactory.CreateDbContextOptions("server2019");

                // Crear instancia de LisContext con las opciones
                using (var context = new CfdiDbContext(options))
                {
                    var response = await context.vwTipoCambios
                        .Where(cph => cph.id == idTipoCambio
                        ).FirstAsync();

                    return new GeneralResponse<vwTipoCambio>()
                    {
                        Data = response,
                        IsSuccess = true,
                        Message = "Consulta exitosa de tipo de cambio."
                    };
                }
            }
            catch (Exception ex)
            {
                return new GeneralResponse<vwTipoCambio>()
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Consulta fallida de tipo de cambio.",
                    ErrorList = GetAllExceptionMessages(ex)
                };
            }
        }

        public async Task<GeneralResponse<vwTipoCambio>> GetTiposCambio(ParametrosGenerales parametros)
        {
            try
            {
                var options = _dbContextFactory.CreateDbContextOptions("server2019");
                using (var context = new CfdiDbContext(options))
                {
                    var query = context.vwTipoCambios.AsQueryable();

                    query = query.OrderByDynamic(parametros.OrdenarPor, parametros.Descending, "activo", true); 

                    if (parametros.Activos)
                    {
                        query = query.Where(tc => tc.activo);
                    }

                    // Filtros por columna
                    if (parametros.filtrosPorColumna != null)
                    {
                        foreach (var filtro in parametros.filtrosPorColumna)
                        {
                            if (!string.IsNullOrEmpty(filtro.Value))
                            {
                                switch (filtro.Key.ToLower())
                                {
                                    case "valor":
                                        if (decimal.TryParse(filtro.Value, out decimal valor))
                                        {
                                            query = query.Where(tc => tc.valor == valor);
                                        }
                                        break;
                                    case "fecha":
                                        if (DateTime.TryParse(filtro.Value, out DateTime fechaCol))
                                        {
                                            query = query.Where(tc => tc.fecha.Date == fechaCol.Date);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    // Paginación
                    var totalRecords = await query.CountAsync();
                    var items = await query
                        .Skip((parametros.NoPagina - 1) * parametros.TamanoPagina)
                        .Take(parametros.TamanoPagina)
                        .ToListAsync();

                    return new GeneralResponse<vwTipoCambio>
                    {
                        TotalRecords = totalRecords,
                        Items = items,
                        IsSuccess = true,
                        Message = "Tipos de cambio consultados correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                return new GeneralResponse<vwTipoCambio>
                {
                    IsSuccess = false,
                    Message = "Error al obtener los tipos de cambio.",
                    ErrorList = GetAllExceptionMessages(ex)
                };
            }
        }


        public async Task<GeneralResponse<tipoCambio>> PostTipoCambio(tipoCambio tipoCambio)
        {
            try
            {
                var options = _dbContextFactory.CreateDbContextOptions("server2019");
                using (var context = new CfdiDbContext(options))
                {
                    var yaExiste = await context.tipoCambios
                        .AnyAsync(tc => tc.fecha.Date == tipoCambio.fecha.Date && tc.activo.Value);


                    if (yaExiste)
                    {
                        return new GeneralResponse<tipoCambio>
                        {
                            Data = null,
                            IsSuccess = false,
                            Message = "No se agrego el tipo de cambio.",
                            ErrorList = new List<string>() { "Ya existe un tipo de cambio registrado para esa fecha." }
                        };
                    }

                    await context.tipoCambios.AddAsync(tipoCambio);
                    await context.SaveChangesAsync();

                    return new GeneralResponse<tipoCambio>
                    {
                        Data = tipoCambio,
                        IsSuccess = true,
                        Message = "Tipo de cambio agregado correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                return new GeneralResponse<tipoCambio>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Error inesperado al agregar el tipo de cambio.",
                    ErrorList = GetAllExceptionMessages(ex)
                };
            }
        }



        public async Task<GeneralResponse<tipoCambio>> PutTipoCambio(tipoCambio tipoCambio)
        {
            try
            {
                var options = _dbContextFactory.CreateDbContextOptions("server2019");
                using (var context = new CfdiDbContext(options))
                {
                    var existente = await context.tipoCambios.FindAsync(tipoCambio.id);

                    if (existente == null)
                    {
                        return new GeneralResponse<tipoCambio>
                        {
                            Data = null,
                            IsSuccess = false,
                            Message = "Tipo de cambio no encontrado."
                        };
                    }

                    existente.fechaModificacion = DateTime.Now;
                    existente.valor = tipoCambio.valor;

                    context.tipoCambios.Update(existente);
                    await context.SaveChangesAsync();

                    return new GeneralResponse<tipoCambio>
                    {
                        Data = existente,
                        IsSuccess = true,
                        Message = "Tipo de cambio actualizado correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                return new GeneralResponse<tipoCambio>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Error al actualizar el tipo de cambio.",
                    ErrorList = GetAllExceptionMessages(ex)
                };
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
    }
}
