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
using CFDI.Data.Contexts;
using CFDI.Data.Entities;

namespace HG.CFDI.DATA.Repositories
{

    public class CartaPorteRepository : ICartaPorteRepository
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<CartaPorteRepository> _logger;

        public CartaPorteRepository(CfdiDbContext context, IDbContextFactory dbContextFactory, IMapper mapper, ILogger<CartaPorteRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task fechaSolicitudTimbradoAsync(int no_guia, string compania)
        {
            try
            {
                string server = string.Empty;
                server = "server2019";

                // Crear opciones de DbContext basado en el nombre de la cadena de conexión
                var options = _dbContextFactory.CreateDbContextOptions(server);

                // Crear instancia de LisContext con las opciones
                using (var context = new CfdiDbContext(options))
                {
                    var entidad = context.cartaPorteCabeceras.Find(no_guia, compania);

                    if (entidad != null)
                    {
                        // Actualiza solo la columna deseada
                        entidad.fechaSolicitudTimbrado = DateTime.Now;

                        // Guarda los cambios en la base de datos
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public async Task changeStatusCartaPorteAsync(int no_guia, string compania, int EstatusTimbrado, string mensajeTimbrado, int sistemaTimbrado)
        {
            try
            {
                string server = string.Empty;
                switch (compania)
                {
                    case "hg":
                        server = "server2019";
                        break;
                    default:
                        server = "server2019";
                        break;
                }

                // Crear opciones de DbContext basado en el nombre de la cadena de conexión
                var options = _dbContextFactory.CreateDbContextOptions(server);

                // Crear instancia de LisContext con las opciones
                using (var context = new CfdiDbContext(options))
                {
                    var entidad = context.cartaPorteCabeceras.Find(no_guia, compania);

                    if (entidad != null)
                    {
                        // Actualiza solo la columna deseada
                        entidad.estatusTimbrado = EstatusTimbrado;
                        entidad.mensajeTimbrado = mensajeTimbrado;
                        entidad.sistemaTimbrado = sistemaTimbrado;
                        if (EstatusTimbrado == 5)
                        {
                            entidad.fechaTimbrado = null;
                        }
                        else
                        {
                            entidad.fechaTimbrado = DateTime.Now;
                        }

                        // Guarda los cambios en la base de datos
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> TrySetTimbradoEnProcesoAsync(int no_guia, string compania)
        {
            string server = string.Empty;
            switch (compania)
            {
                case "hg":
                    server = "server2019";
                    break;
                default:
                    server = "server2019";
                    break;
            }

            var options = _dbContextFactory.CreateDbContextOptions(server);

            using (var context = new CfdiDbContext(options))
            {
                var noGuiaParam = new SqlParameter("@no_guia", no_guia);
                var companiaParam = new SqlParameter("@compania", compania);

                var rows = await context.Database.ExecuteSqlRawAsync(@"UPDATE cartaPorteCabeceras
                    SET estatusTimbrado = 1,
                        mensajeTimbrado = 'En proceso de timbrado.',
                        fechaSolicitudTimbrado = GETDATE()
                    WHERE no_guia = @no_guia
                      AND compania = @compania
                      AND estatusTimbrado NOT IN (1,3);", noGuiaParam, companiaParam);

                return rows > 0;
            }
        }

        public async Task<bool> putCartaPorte(string database, cartaPorteCabecera cp)
        {
            string server = string.Empty;
            string compania = string.Empty;
            switch (database)
            {
                case "hgdb_lis":
                    server = "server2019";
                    compania = "hg";
                    break;
                case "rldb_lis":
                    server = "server2019";
                    compania = "rl";
                    break;
                case "chdb_lis":
                    server = "server2019";
                    compania = "ch";
                    break;
                case "lindadb":
                    server = "server2019";
                    compania = "ld";
                    break;
            }

            // Crear opciones de DbContext basado en el nombre de la cadena de conexión
            var options = _dbContextFactory.CreateDbContextOptions(server);

            // Crear instancia de LisContext con las opciones
            using (var context = new CfdiDbContext(options))
            {
                var entidad = context.cartaPorteCabeceras.Find(cp.no_guia, compania);


                cp.idTipoOperacionLis = entidad.idTipoOperacionLis;
                cp.tipoOperacion = entidad.tipoOperacion;
                cp.idTipoServicioLis = entidad.idTipoServicioLis;
                cp.tipoServicio = entidad.tipoServicio;
                cp.numCartaPorteLis = entidad.numCartaPorteLis;
                cp.idClienteRemitente = entidad.idClienteRemitente;
                cp.idClienteDestinatario = entidad.idClienteDestinatario;
                cp.idOperador = entidad.idOperador;
                cp.idRuta = entidad.idRuta;
                cp.idPlazaOrigen = entidad.idPlazaOrigen;
                cp.idPlazaDestino = entidad.idPlazaDestino;


                if (entidad != null)
                {
                    // Actualizar todos los campos de la entidad principal con los valores de cp
                    context.Entry(entidad).CurrentValues.SetValues(cp);

                    // Actualizar las mercancías
                    foreach (var mercancia in cp.cartaPorteMercancia)
                    {
                        if (mercancia.id == 0)
                        {
                            // Si es una nueva mercancía, agregarla
                            entidad.cartaPorteMercancia.Add(mercancia);
                        }
                        else
                        {
                            // Buscar la mercancía en la lista de mercancías rastreadas
                            var trackedMercancia = entidad.cartaPorteMercancia.FirstOrDefault(m => m.id == mercancia.id);

                            if (trackedMercancia != null)
                            {
                                // Si la mercancía ya está rastreada, actualizar sus propiedades directamente
                                context.Entry(trackedMercancia).CurrentValues.SetValues(mercancia);
                            }
                            else
                            {
                                // Si la mercancía no está rastreada, adjuntarla
                                context.Entry(mercancia).State = EntityState.Modified;
                            }
                        }
                    }

                    // Actualizar los conceptos
                    foreach (var concepto in cp.cartaPorteDetalles)
                    {
                        if (concepto.id == 0)
                        {
                            // Si es una nueva mercancía, agregarla
                            entidad.cartaPorteDetalles.Add(concepto);
                        }
                        else
                        {
                            // Buscar la mercancía en la lista de mercancías rastreadas
                            var trackedConcepto = entidad.cartaPorteDetalles.FirstOrDefault(m => m.id == concepto.id);

                            if (trackedConcepto != null)
                            {
                                // Si la mercancía ya está rastreada, actualizar sus propiedades directamente
                                context.Entry(trackedConcepto).CurrentValues.SetValues(concepto);
                            }
                            else
                            {
                                // Si la mercancía no está rastreada, adjuntarla
                                context.Entry(concepto).State = EntityState.Modified;
                            }
                        }
                    }

                    // Actualizar los regimenes aduaneros
                    foreach (var regimen in cp.cartaPorteRegimenAduaneros)
                    {                           
                        if (regimen.id == 0)
                        {
                            entidad.cartaPorteRegimenAduaneros.Add(regimen);
                        }
                        else
                        {
                            var trackedRegimen = entidad.cartaPorteRegimenAduaneros.FirstOrDefault(m => m.id == regimen.id);

                            if (trackedRegimen != null)
                            {
                                context.Entry(trackedRegimen).CurrentValues.SetValues(regimen);
                            }
                            else
                            {
                                context.Entry(regimen).State = EntityState.Modified;
                            }
                        }
                    }

                    // Actualizar FACTURAR A SUSTITUIR
                    foreach (var sustitucion in cp.cartaPorteSustitucions)
                    {
                        if (sustitucion.id == 0)
                        {
                            entidad.cartaPorteSustitucions.Add(sustitucion);
                        }
                        else
                        {
                            var trackedSustitucion = entidad.cartaPorteSustitucions.FirstOrDefault(m => m.id == sustitucion.id);

                            if (trackedSustitucion != null)
                            {
                                context.Entry(trackedSustitucion).CurrentValues.SetValues(sustitucion);
                            }
                            else
                            {
                                context.Entry(sustitucion).State = EntityState.Modified;
                            }
                        }
                    }

                    // Actualizar las ubicaciones
                    foreach (var ubicacion in cp.cartaPorteUbicaciones)
                    {
                        if (ubicacion.id == 0)
                        {
                            // Si es una nueva mercancía, agregarla
                            entidad.cartaPorteUbicaciones.Add(ubicacion);
                        }
                        else
                        {
                            // Buscar la mercancía en la lista de ubicacion rastreada
                            var trackedUbicacion = entidad.cartaPorteUbicaciones.FirstOrDefault(m => m.id == ubicacion.id);

                            if (trackedUbicacion != null)
                            {
                                // Si la mercancía ya está rastreada, actualizar sus propiedades directamente
                                context.Entry(trackedUbicacion).CurrentValues.SetValues(ubicacion);
                            }
                            else
                            {
                                // Si la mercancía no está rastreada, adjuntarla
                                context.Entry(ubicacion).State = EntityState.Modified;
                            }
                        }
                    }
                }
                else
                {
                    // Agregar nueva entidad si no existe
                    context.cartaPorteCabeceras.Add(cp);
                }

                // Guardar cambios en la base de datos
                await context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<GeneralResponse<cartaPorteCabecera>> getCartaPorte(string database, string guia)
        {
            string server = string.Empty;
            string compania = string.Empty;
            switch (database)
            {
                case "hgdb_lis":
                    server = "server2019";
                    compania = "hg";
                    break;
                case "rldb_lis":
                    server = "server2019";
                    compania = "rl";
                    break;
                case "chdb_lis":
                    server = "server2019";
                    compania = "ch";
                    break;
                case "lindadb":
                    server = "server2019";
                    compania = "ld";
                    break;
            }

            // Crear opciones de DbContext basado en el nombre de la cadena de conexión
            var options = _dbContextFactory.CreateDbContextOptions(server);

            // Crear instancia de LisContext con las opciones
            using (var context = new CfdiDbContext(options))
            {
                var response = await context.cartaPorteCabeceras
                    .Where(cph =>
                    cph.compania == compania
                    && cph.num_guia == guia
                    )
                     .Include(cph => cph.cartaPorteDetalles)
                    .Include(cph => cph.cartaPorteMercancia)
                    .Include(cph => cph.cartaPorteUbicaciones)
                    .Include(cph => cph.cartaPorteRegimenAduaneros)
                    .Include(cph => cph.cartaPorteOperacionRyders)
                    .Include(cph => cph.cartaPorteAddenda)
                    .Include(cph => cph.cartaPorteSustitucions)
                    .Include(cph => cph.errorTimbradoGenerals)
                    .FirstAsync();

                var archivos = await getArchivosTimbrado(response.no_guia, compania);

                response.archivoCFDi = archivos;

                return new GeneralResponse<cartaPorteCabecera>()
                {
                    Data = response,
                    IsSuccess = true,
                    Message = "Guia encontrada en repositorio de timbrado."
                };
            }
        }

        public async Task<archivoCFDi> getArchivosTimbrado(int no_guia, string compania)
        {
            try
            {
                string server = "server2019";
                //string compania = "hg";                  

                // Crear opciones de DbContext basado en el nombre de la cadena de conexión
                var options = _dbContextFactory.CreateDbContextOptions(server);

                // Crear instancia de LisContext con las opciones
                using (var context = new CfdiDbContext(options))
                {
                    return await context.archivoCFDis.FindAsync(no_guia, compania);
                }
            }
            catch (Exception err)
            {
                return null;
            }
        }

        public async Task<List<cartaPorteCabecera>> getCartasPortePendiente(string compañia)
        {
            string server = string.Empty;

            switch (compañia)
            {
                case "hg":
                    server = "server2019";
                    break;
                default:
                    server = "server2019";
                    break;
            }
            // Crear opciones de DbContext basado en el nombre de la cadena de conexión
            var options = _dbContextFactory.CreateDbContextOptions(server);

            // Crear instancia de LisContext con las opciones
            using (var context = new CfdiDbContext(options))
            {
                return await context.cartaPorteCabeceras
                    .Where(cph =>
                    cph.estatusTimbrado == 0 &&
                    cph.compania == compañia
                    )
                     .Include(cph => cph.cartaPorteDetalles)
                    .Include(cph => cph.cartaPorteMercancia)
                    .Include(cph => cph.cartaPorteUbicaciones)
                    .Include(cph => cph.cartaPorteRegimenAduaneros)
                    .Include(cph => cph.cartaPorteSustitucions)
                    .Include(cph => cph.cartaPorteAddenda)
                    .ToListAsync();
            }
        }

        private async Task ImportarCartaPorte(string serverOrigen, string serverDestino, cartaPorteCabecera cartaPorte)
        {
            var optionsOrigen = _dbContextFactory.CreateDbContextOptions(serverOrigen);
            var optionsDestino = _dbContextFactory.CreateDbContextOptions(serverDestino);

            var executionStrategy = new CfdiDbContext(optionsDestino).Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using (var contextOrigen = new CfdiDbContext(optionsOrigen))
                using (var contextDestino = new CfdiDbContext(optionsDestino))
                {
                    using (var transaction = await contextDestino.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var nuevaCartaPorte = _mapper.Map<cartaPorteCabecera>(cartaPorte);
                            await contextDestino.cartaPorteCabeceras.AddAsync(nuevaCartaPorte);

                            await contextDestino.SaveChangesAsync();
                            await transaction.CommitAsync();

                            //cartaPorte.EstatusTimbrado = 3;
                            //cartaPorte.MensajeTimbrado = "trasladado a server 2019";

                            //contextOrigen.Entry(cartaPorte).State = EntityState.Modified;
                            //await contextOrigen.SaveChangesAsync();
                        }
                        catch (Exception err)
                        {
                            await transaction.RollbackAsync();
                        }
                    }
                }
            });
        }

        public async Task<ResponseImportacion> importarCartaPorteServer2008(string num_guia, string compania)
        {
            ResponseImportacion responseImportacion = new ResponseImportacion();

            var optionsOrigen = _dbContextFactory.CreateDbContextOptions("server2008");

            using (var contextOrigen = new CfdiDbContext(optionsOrigen))
            {
                var cartaPorte = await contextOrigen.cartaPorteCabeceras
                .Where(cph => cph.num_guia == num_guia && cph.compania == compania)
                .Include(cph => cph.cartaPorteDetalles)
                .Include(cph => cph.cartaPorteMercancia)
                .Include(cph => cph.cartaPorteUbicaciones)
                .Include(cph => cph.cartaPorteRegimenAduaneros)
                .Include(cph => cph.cartaPorteSustitucions)
                .Include(cph => cph.archivoCFDi)
                .Include(cph => cph.cartaPorteAddenda)
                .SingleOrDefaultAsync();

                if (cartaPorte == null)
                {
                    responseImportacion.Mensaje = $"No se encontro carta porte para la guía {num_guia} y compañía {compania}.";
                    responseImportacion.MigrationIsSuccess = false;
                    return responseImportacion;
                }

                // Validación específica para los detalles
                if (cartaPorte.cartaPorteDetalles == null || !cartaPorte.cartaPorteDetalles.Any())
                {
                    responseImportacion.Mensaje = $"La validación de los detalles de la Carta Porte ha fallado";
                    responseImportacion.MigrationIsSuccess = false;
                    return responseImportacion;
                }

                // Validación específica para las mercancías
                if (cartaPorte.cartaPorteMercancia == null || !cartaPorte.cartaPorteMercancia.Any())
                {
                    responseImportacion.Mensaje = $"La validación de las mercancías de la Carta Porte ha fallado";
                    responseImportacion.MigrationIsSuccess = false;
                    return responseImportacion;
                }

                // Validación específica para las ubicaciones
                if (cartaPorte.cartaPorteUbicaciones == null || !cartaPorte.cartaPorteUbicaciones.Any())
                {
                    responseImportacion.Mensaje = $"La validación de las ubicaciones de la Carta Porte ha fallado";
                    responseImportacion.MigrationIsSuccess = false;
                    return responseImportacion;
                }

                await ImportarCartaPorte("server2008", "server2019", cartaPorte);
                responseImportacion.Mensaje = $"La importacion se ejecuto";
                responseImportacion.MigrationIsSuccess = true;
                return responseImportacion;

            }
        }

        public async Task deleteErrors(int no_guia, string compania)
        {
            string server = string.Empty;
            switch (compania)
            {
                case "hg":
                    server = "server2019";
                    break;
                default:
                    server = "server2019";
                    break;
            }

            // Crear opciones de DbContext basado en el nombre de la cadena de conexión

            var options = _dbContextFactory.CreateDbContextOptions(server);

            // Crear instancia de LisContext con las opciones
            using (var context = new CfdiDbContext(options))
            {
                // Eliminar los errores existentes para la guía especificada
                var erroresExistentes = context.errorTimbradoGenerals
                                                .Where(e => e.no_guia == no_guia && e.compania == compania);
                context.errorTimbradoGenerals.RemoveRange(erroresExistentes);
                await context.SaveChangesAsync();
            }
        }

        public async Task insertError(int no_guia, string num_guia, string compania, string error, int? idOperador_Lis, string? idUnidad_Lis, string? idRemolque_Lis)
        {
            string server = string.Empty;

            switch (compania)
            {
                case "hg":
                    server = "server2019";
                    break;
                default:
                    server = "server2019";
                    break;
            }

            // Crear opciones de DbContext basado en el nombre de la cadena de conexión

            var options = _dbContextFactory.CreateDbContextOptions(server);

            // Crear instancia de LisContext con las opciones
            using (var context = new CfdiDbContext(options))
            {
                errorTimbradoGeneral errorT = new errorTimbradoGeneral()
                {
                    no_guia = no_guia,
                    compania = compania,
                    num_guia = num_guia,
                    fechaInsert = DateTime.Now,
                    error = error,
                    idOperadorLis = idOperador_Lis,
                    idUnidadLis = idUnidad_Lis,
                    idRemolqueLis = idRemolque_Lis
                };

                // Esperar a que la operación AddAsync termine
                await context.errorTimbradoGenerals.AddAsync(errorT);
                // Esperar a que la operación SaveChangesAsync termine
                await context.SaveChangesAsync();
            }
        }

        public string GetServer(string compania)
        {
            Dictionary<string, string> Servidores = new Dictionary<string, string>
            {
                { "hg", "server2019TrucksHg" },
                { "ch", "server2008TrucksCh" },
                { "rl", "server2008TrucksRl" },
                { "ld", "server2008TrucksLd" }
            };

            if (Servidores.TryGetValue(compania, out var server))
                return server;

            throw new Exception($"No se encontró servidor para la compañía: {compania}");
        }

        public async Task<bool> InsertDocumentosTimbrados(archivoCFDi archivos, string server)
        {
            if (archivos.compania.Equals("hg") && server.Equals("server2008"))
                return true;

            var options = _dbContextFactory.CreateDbContextOptions(server);

            using (var context = new CfdiDbContext(options))
            {
                if (archivos == null)
                    throw new ArgumentNullException(nameof(archivos), "El objeto archivoCFDi no puede ser nulo.");
                archivos.idArchivoCFDi = null;

                await context.archivoCFDis.AddAsync(archivos);
                var result = await context.SaveChangesAsync();

                if (result > 0)
                    _logger.LogInformation($"Se inserto archivos cfdi de {archivos.num_guia} en {server}");

                return result > 0;
            }
        }

        public async Task patchPdfAsync(int no_guia, string compania, byte[] pdf)
        {
            string server = string.Empty;
            switch (compania)
            {
                case "hg":
                    server = "server2019";
                    break;
                default:
                    server = "server2019";
                    break;
            }

            // Crear opciones de DbContext basado en el nombre de la cadena de conexión
            var options = _dbContextFactory.CreateDbContextOptions(server);

            // Crear instancia de LisContext con las opciones
            using (var context = new CfdiDbContext(options))
            {
                var entidad = context.archivoCFDis.Find(no_guia, compania);

                if (entidad != null)
                {
                    // Actualiza solo la columna deseada
                    entidad.pdf = pdf;

                    // Guarda los cambios en la base de datos
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task actualizaEstatusEnvioRyderAsync(int id, bool estatus)
        {
            string server = "server2019";

            // Crear opciones de DbContext basado en el nombre de la cadena de conexión
            var options = _dbContextFactory.CreateDbContextOptions(server);

            // Crear instancia de LisContext con las opciones
            using (var context = new CfdiDbContext(options))
            {
                var entidad = context.cartaPorteOperacionRyders.Find(id);

                if (entidad != null)
                {
                    // Actualiza solo la columna deseada
                    entidad.estatusEnvio = estatus;

                    // Guarda los cambios en la base de datos
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<bool> trasladaUuidToTrucks(archivoCFDi archivos)
        {
            try
            {
                string server = GetServer(archivos.compania);

                var options = _dbContextFactory.CreateDbContextOptions(server);

                using (var context = new CfdiDbContext(options))
                {
                    // Asumiendo que tu SP se llama sp_trasladaUuidToTrucks y tiene parámetros
                    var result = await context.Database.ExecuteSqlRawAsync(
                        "EXEC trasladaFolioFiscalToTrucks @p0, @p1, @p2",
                        archivos.uuid, archivos.no_guia, archivos.num_guia);

                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                return false;
            }            
        }

        public async Task<ResponseImportacion> reinsertaCartaPorteRepositorio(string database, string guia)
        {
            string server = string.Empty;
            string compania = string.Empty;

            switch (database)
            {
                case "hgdb_lis":
                    server = "server2019";
                    compania = "hg";
                    break;
                case "chdb_lis":
                    server = "server2008";
                    compania = "ch";
                    break;
                case "rldb_lis":
                    server = "server2008";
                    compania = "rl";
                    break;
                case "lindadb":
                    server = "server2008";
                    compania = "ld";
                    break;
                default:
                    server = "server2019";
                    break;
            }

            var numGuiaParam = new SqlParameter("@num_guia", guia);
            var databaseParam = new SqlParameter("@database", database);

            var options = _dbContextFactory.CreateDbContextOptions(server);

            using (var context = new CfdiDbContext(options))
            {
                await context.Database.ExecuteSqlRawAsync("EXEC [dbo].[INSERT_CARTA_PORTE_REPOSITORY] @num_guia, @database", numGuiaParam, databaseParam);


                //si la carta porte se inserto en server 2008, ejecutamos importacion hacia el 2019
                if (server.Equals("server2008"))
                {
                    try
                    {
                        return await importarCartaPorteServer2008(guia, compania);

                    }
                    catch (Exception err)
                    {
                        return new ResponseImportacion() { MigrationIsSuccess = false, Mensaje = err.Message };
                    }
                }
                else
                {
                    return new ResponseImportacion() { MigrationIsSuccess = true, Mensaje = "Carta porte no requiere importacion" };
                }
            }
        }
    }
}
