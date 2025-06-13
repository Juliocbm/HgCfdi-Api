using System;
using System.Collections.Generic;
using CFDI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CFDI.Data.Contexts;

public partial class CfdiDbContext : DbContext
{
    public CfdiDbContext()
    {
    }

    public CfdiDbContext(DbContextOptions<CfdiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; }

    public virtual DbSet<Counter> Counters { get; set; }

    public virtual DbSet<Hash> Hashes { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobParameter> JobParameters { get; set; }

    public virtual DbSet<JobQueue> JobQueues { get; set; }

    public virtual DbSet<List> Lists { get; set; }

    public virtual DbSet<Schema> Schemas { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<VwOrdenTrasladoCabecera> VwOrdenTrasladoCabeceras { get; set; }

    public virtual DbSet<VwOrdenTrasladoMercancia> VwOrdenTrasladoMercancias { get; set; }

    public virtual DbSet<VwOrdenTrasladoUbicacione> VwOrdenTrasladoUbicaciones { get; set; }

    public virtual DbSet<anticipoDetailLi> anticipoDetailLis { get; set; }

    public virtual DbSet<anticipoError> anticipoErrors { get; set; }

    public virtual DbSet<anticipoHeaderLi> anticipoHeaderLis { get; set; }

    public virtual DbSet<archivoCFDi> archivoCFDis { get; set; }

    public virtual DbSet<cartaPorte> cartaPortes { get; set; }

    public virtual DbSet<cartaPorteAddendum> cartaPorteAddenda { get; set; }

    public virtual DbSet<cartaPorteCabecera> cartaPorteCabeceras { get; set; }

    public virtual DbSet<cartaPorteDetalle> cartaPorteDetalles { get; set; }

    public virtual DbSet<cartaPorteMercancium> cartaPorteMercancia { get; set; }

    public virtual DbSet<cartaPorteOperacionRyders> cartaPorteOperacionRyders { get; set; }

    public virtual DbSet<cartaPorteRegimenAduanero> cartaPorteRegimenAduaneros { get; set; }

    public virtual DbSet<cartaPorteSustitucion> cartaPorteSustitucions { get; set; }

    public virtual DbSet<cartaPorteUbicacione> cartaPorteUbicaciones { get; set; }

    public virtual DbSet<catalogoCodigoPostal> catalogoCodigoPostals { get; set; }

    public virtual DbSet<clienteFtp> clienteFtps { get; set; }

    public virtual DbSet<clienteFtpDetalle> clienteFtpDetalles { get; set; }

    public virtual DbSet<errorTimbradoGeneral> errorTimbradoGenerals { get; set; }

    public virtual DbSet<historicoEDIFactura> historicoEDIFacturas { get; set; }

    public virtual DbSet<liquidacionDetailLi> liquidacionDetailLis { get; set; }

    public virtual DbSet<liquidacionError> liquidacionErrors { get; set; }

    public virtual DbSet<liquidacionHeaderLi> liquidacionHeaderLis { get; set; }

    public virtual DbSet<liquidacionViajeDetailLi> liquidacionViajeDetailLis { get; set; }

    public virtual DbSet<ordenTrasladoCabecera> ordenTrasladoCabeceras { get; set; }

    public virtual DbSet<ordenTrasladoMercancia> ordenTrasladoMercancias { get; set; }

    public virtual DbSet<ordenTrasladoUbicacione> ordenTrasladoUbicaciones { get; set; }

    public virtual DbSet<tipoCambio> tipoCambios { get; set; }

    public virtual DbSet<vwTipoCambio> vwTipoCambios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AggregatedCounter>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PK_HangFire_CounterAggregated");

            entity.ToTable("AggregatedCounter", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_AggregatedCounter_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.id }).HasName("PK_HangFire_Counter");

            entity.ToTable("Counter", "HangFire");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Hash>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Field }).HasName("PK_HangFire_Hash");

            entity.ToTable("Hash", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Hash_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Field).HasMaxLength(100);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK_HangFire_Job");

            entity.ToTable("Job", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Job_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => e.StateName, "IX_HangFire_Job_StateName").HasFilter("([StateName] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.StateName).HasMaxLength(20);
        });

        modelBuilder.Entity<JobParameter>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Name }).HasName("PK_HangFire_JobParameter");

            entity.ToTable("JobParameter", "HangFire");

            entity.Property(e => e.Name).HasMaxLength(40);

            entity.HasOne(d => d.Job).WithMany(p => p.JobParameters)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_JobParameter_Job");
        });

        modelBuilder.Entity<JobQueue>(entity =>
        {
            entity.HasKey(e => new { e.Queue, e.id }).HasName("PK_HangFire_JobQueue");

            entity.ToTable("JobQueue", "HangFire");

            entity.Property(e => e.Queue).HasMaxLength(50);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.FetchedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<List>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.id }).HasName("PK_HangFire_List");

            entity.ToTable("List", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_List_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("PK_HangFire_Schema");

            entity.ToTable("Schema", "HangFire");

            entity.Property(e => e.Version).ValueGeneratedNever();
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK_HangFire_Server");

            entity.ToTable("Server", "HangFire");

            entity.HasIndex(e => e.LastHeartbeat, "IX_HangFire_Server_LastHeartbeat");

            entity.Property(e => e.id).HasMaxLength(200);
            entity.Property(e => e.LastHeartbeat).HasColumnType("datetime");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Value }).HasName("PK_HangFire_Set");

            entity.ToTable("Set", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Set_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => new { e.Key, e.Score }, "IX_HangFire_Set_Score");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(256);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.id }).HasName("PK_HangFire_State");

            entity.ToTable("State", "HangFire");

            entity.HasIndex(e => e.CreatedAt, "IX_HangFire_State_CreatedAt");

            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(100);

            entity.HasOne(d => d.Job).WithMany(p => p.States)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_State_Job");
        });

        modelBuilder.Entity<VwOrdenTrasladoCabecera>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VwOrdenTrasladoCabecera");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.domicilioEmisorEstado)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.domicilioReceptorCp)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.exportacion)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.fechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.formaDePago)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.lugarDeExpedicion)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.medioTransporte)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.mensajeTrasladoOT).IsUnicode(false);
            entity.Property(e => e.metodoPago)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.moneda)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.nombreEmisor).IsUnicode(false);
            entity.Property(e => e.numOrdenOT)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.plaza)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.razonSocialReceptor)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.regimenFiscal)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.rfcEmisor)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.rfcReceptor)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ruta)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.tipoCFD)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.tipoComprobante)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.totalDistanciaRecorrida).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.transportista)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.usoCfdiReceptor)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.version)
                .HasMaxLength(3)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwOrdenTrasladoMercancia>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VwOrdenTrasladoMercancias");

            entity.Property(e => e.claveMaterialPeligroso)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.claveProductoSat)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.claveUnidad)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.descripcionProducto)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.esMaterialPeligroso)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.nombreMercancia)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.pesoKg).HasColumnType("decimal(16, 2)");
        });

        modelBuilder.Entity<VwOrdenTrasladoUbicacione>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VwOrdenTrasladoUbicaciones");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.cpDestinatario)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.cpRemitente)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.estadoDestinatario)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.estadoRemitente)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.fechaLlegada)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.fechaSalida)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.idUbicacionDestinatario)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.idUbicacionRemitente)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.nombreDestinatario)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.nombreRemitente)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.paisDestinatario)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.paisRemitente)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<anticipoDetailLi>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__anticipo__3213E83FBF912AE5");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.descripcion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.importe).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.anticipoHeaderLi).WithMany(p => p.anticipoDetailLis)
                .HasForeignKey(d => new { d.idAnticipo, d.compania })
                .HasConstraintName("FK_anticipoDetailLis_Header");
        });

        modelBuilder.Entity<anticipoError>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__anticipo__3213E83F6B75D9FE");

            entity.ToTable("anticipoError");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.error).IsUnicode(false);
            entity.Property(e => e.fechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.origen)
                .HasMaxLength(25)
                .IsUnicode(false);
        });

        modelBuilder.Entity<anticipoHeaderLi>(entity =>
        {
            entity.HasKey(e => new { e.idAnticipo, e.compania }).HasName("PK__anticipo__CEDAE6A1BBECBDCB");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.fechaAnticipo).HasColumnType("datetime");
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.num_guia)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.observaciones)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<archivoCFDi>(entity =>
        {
            entity.HasKey(e => new { e.no_guia, e.compania });

            entity.ToTable("archivoCFDi", "cfdi");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.fechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.idArchivoCFDi).ValueGeneratedOnAdd();
            entity.Property(e => e.num_guia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.uuid)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.cartaPorteCabecera).WithOne(p => p.archivoCFDi)
                .HasForeignKey<archivoCFDi>(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchivoCFDi_CartaPorteCabecera");
        });

        modelBuilder.Entity<cartaPorte>(entity =>
        {
            entity.HasKey(e => e.idCartaPorte).HasName("pkConsultaArchivoCFDi");

            entity.ToTable("cartaPorte", "iLertek");

            entity.Property(e => e.fechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.fechaGeneracionArchivo).HasColumnType("datetime");
            entity.Property(e => e.fechaModificacion).HasColumnType("datetime");
        });

        modelBuilder.Entity<cartaPorteAddendum>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cartaPor__3213E83F46E54960");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.fechaInsert)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.valor)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.cartaPorteCabecera).WithMany(p => p.cartaPorteAddenda)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cartaPorteAddend__4830B400");
        });

        modelBuilder.Entity<cartaPorteCabecera>(entity =>
        {
            entity.HasKey(e => new { e.no_guia, e.compania }).HasName("PK__cartaPor__EB0589E7091C4863");

            entity.ToTable("cartaPorteCabecera");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.aseguradora)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.claveRegimenAduanero)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.claveTipoPermiso)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.codigoTransportista)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.configVehicular)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorCalle)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorColonia)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorCp)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorLocalidad)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorNoExterior)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorNombre)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorPais)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorRegimenFiscal)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.cteEmisorRfc)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.cteReceptorCp)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.cteReceptorDomicilio)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.cteReceptorNombre)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.cteReceptorRegimenFiscal)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.cteReceptorRfc)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.cteReceptorTipoCambio).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.cteReceptorUsoCFDI)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.entSalMercancia)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.esTransporteInternacional)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.estatusTimbrado).HasDefaultValueSql("((0))");
            entity.Property(e => e.estatusTrasladoLis).HasDefaultValueSql("((0))");
            entity.Property(e => e.estatusTrasladoOT).HasDefaultValueSql("((0))");
            entity.Property(e => e.fechaInsert)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.fechaSolicitudTimbrado).HasColumnType("datetime");
            entity.Property(e => e.fechaTimbrado).HasColumnType("datetime");
            entity.Property(e => e.fechaTrasladoLis).HasColumnType("datetime");
            entity.Property(e => e.formaPago)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.idLineaRem1Lis)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.idLineaRem2Lis)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.idRemolque)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.idRemolque2Lis)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.idRemolqueLis)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.idTipoServicioLis)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.idUnidad)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.idUnidadLis)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.licenciaOperador)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.mensajeTimbrado).HasColumnType("text");
            entity.Property(e => e.mensajeTrasladoLis).IsUnicode(false);
            entity.Property(e => e.mensajeTrasladoOT).IsUnicode(false);
            entity.Property(e => e.metodoPago)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.modeloUnidad)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.moneda)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.noViaje)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.nombreTransportista).IsUnicode(false);
            entity.Property(e => e.numCartaPorteLis)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.numTipoPermiso)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.observacionesPedido)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.operador)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.pedimento)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.pesoBrutoVehicular).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.placaRemolque1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.placaRemolque2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.placaUnidad)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.polizaUnidad)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.regimenfiscalTransportista)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.rfcImpo)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.rfcOperador)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.rfcTransportista)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.shipment)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.shipperAccount).IsUnicode(false);
            entity.Property(e => e.statusGuia)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.subtipoRemolque1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.subtipoRemolque2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.tipoOperacion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.tipoServicio)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.totalDistanciaRec).HasColumnType("decimal(18, 6)");
        });

        modelBuilder.Entity<cartaPorteDetalle>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cartaPor__3213E83FC635EDCC");

            entity.ToTable("cartaPorteDetalle");

            entity.Property(e => e.cantidad).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.claveProdServ)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.cpeNoIdentificador)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.descripcion)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.factorIva).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.factorRetencion).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.fechaInsert)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.importe).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.montoIvaOtro).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.montoRetencion).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.objetoImp)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.cartaPorteCabecera).WithMany(p => p.cartaPorteDetalles)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cartaPorteDetall__1D7B6025");
        });

        modelBuilder.Entity<cartaPorteMercancium>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cartaPor__3213E83F7015BAC2");

            entity.Property(e => e.cantidad).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.claveProdServ)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.claveUnidad)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.claveUnidadPeso)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.cveMaterialPeligroso)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.descripcionMateria)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.esMaterialPeligroso)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.fechaInsert)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.fraccionArancelaria)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.pedimento)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.peso).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.tipoMateria)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.valorMercancia).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.cartaPorteCabecera).WithMany(p => p.cartaPorteMercancia)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cartaPorteMercan__214BF109");
        });

        modelBuilder.Entity<cartaPorteOperacionRyders>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cartaPor__3213E83F73BE3E68");

            entity.ToTable("cartaPorteOperacionRyder");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.fechaInsert)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.cartaPorteCabecera).WithMany(p => p.cartaPorteOperacionRyders)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cartaPorteOperac__43F60EC8");
        });

        modelBuilder.Entity<cartaPorteRegimenAduanero>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cartaPor__3213E83F152434E5");

            entity.ToTable("cartaPorteRegimenAduanero");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.fechaInsert)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.regimenAduanero)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.cartaPorteCabecera).WithMany(p => p.cartaPorteRegimenAduaneros)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cartaPorteRegime__731B1205");
        });

        modelBuilder.Entity<cartaPorteSustitucion>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cartaPor__3213E83F9DE166CA");

            entity.ToTable("cartaPorteSustitucion");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.motivoRelacion)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.uuid)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.cartaPorteCabecera).WithMany(p => p.cartaPorteSustitucions)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cartaPorteSustit__0504B816");
        });

        modelBuilder.Entity<cartaPorteUbicacione>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cartaPor__3213E83FD0DDD8D3");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioCp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioEstado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioLocalidad)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioMunicipio)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioNumRegIdTrib)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioPais)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioResidenciaFiscal)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.destinatarioRfc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.distanciaRecorrida).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.fechaArriboProgramado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.fechaDespachoProgramado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.fechaInsert)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.nombreDestinatario)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.nombreRemitente)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.remitenteCp)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.remitenteEstado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.remitenteId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.remitenteLocalidad)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.remitenteMunicipio)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.remitenteNumRegIdTrib)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.remitentePais)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.remitenteResidenciaFiscal)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.remitenteRfc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.tipoUbicacionDestino)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.tipoUbicacionOrigen)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.cartaPorteCabecera).WithMany(p => p.cartaPorteUbicaciones)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cartaPorteUbicac__28ED12D1");
        });

        modelBuilder.Entity<catalogoCodigoPostal>(entity =>
        {
            entity.HasKey(e => e.cp).HasName("PK__catalogo__3213667A77FB71DA");

            entity.ToTable("catalogoCodigoPostal", "sat");

            entity.Property(e => e.cp)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.estado)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.localidad)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.municipio)
                .HasMaxLength(3)
                .IsUnicode(false);
        });

        modelBuilder.Entity<clienteFtp>(entity =>
        {
            entity.HasKey(e => e.idClienteFtp).HasName("pkConfiguracionCliente");

            entity.ToTable("clienteFtp", "eInvoicing");

            entity.Property(e => e.carpetaRemota)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.carpetaRepositorio)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.compania)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.contrasenia)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.fechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.nombreCliente)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.scacCliente)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.scacPropio)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.servidor)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.usuario)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<clienteFtpDetalle>(entity =>
        {
            entity.HasKey(e => e.idClienteFtpDetalle).HasName("pkClienteFtpDetalle");

            entity.ToTable("clienteFtpDetalle", "eInvoicing");

            entity.Property(e => e.fechaCreacion).HasColumnType("datetime");

            entity.HasOne(d => d.idClienteFtpNavigation).WithMany(p => p.clienteFtpDetalles)
                .HasForeignKey(d => d.idClienteFtp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkClienteFtpDetalleIdClienteFtp");
        });

        modelBuilder.Entity<errorTimbradoGeneral>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__errorTim__3213E83F458E5A0A");

            entity.ToTable("errorTimbradoGeneral");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.error).IsUnicode(false);
            entity.Property(e => e.fechaInsert)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.idRemolqueLis)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.idUnidadLis)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.cartaPorteCabecera).WithMany(p => p.errorTimbradoGenerals)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__errorTimbradoGen__251C81ED");
        });

        modelBuilder.Entity<historicoEDIFactura>(entity =>
        {
            entity.HasKey(e => e.idHistorico).HasName("pkHistoricoFacturasEnviadasIdHistorico");

            entity.ToTable("historicoEDIFacturas", "eInvoicing");

            entity.Property(e => e.compania)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.fechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.fechaTimbrado).HasColumnType("datetime");
            entity.Property(e => e.mensaje).IsUnicode(false);
        });

        modelBuilder.Entity<liquidacionDetailLi>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__liquidac__3213E83FA7659164");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.descripcion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.importe).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.unitPrice).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.liquidacionHeaderLi).WithMany(p => p.liquidacionDetailLis)
                .HasForeignKey(d => new { d.idLiquidacion, d.compania })
                .HasConstraintName("FK_liquidacionDetailLis_Header");
        });

        modelBuilder.Entity<liquidacionError>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__liquidac__3213E83F1A896300");

            entity.ToTable("liquidacionError");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.error).IsUnicode(false);
            entity.Property(e => e.fechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.origen)
                .HasMaxLength(25)
                .IsUnicode(false);
        });

        modelBuilder.Entity<liquidacionHeaderLi>(entity =>
        {
            entity.HasKey(e => new { e.idLiquidacion, e.compania }).HasName("PK__liquidac__85812F1B3CC0C0F3");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.claveLiquidacion)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.fechaLiquidacion).HasColumnType("datetime");
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.id_unidad)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.status)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.total).HasColumnType("decimal(18, 6)");
        });

        modelBuilder.Entity<liquidacionViajeDetailLi>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__liquidac__3213E83F353A8D77");

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.sueldoOperador).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.liquidacionHeaderLi).WithMany(p => p.liquidacionViajeDetailLis)
                .HasForeignKey(d => new { d.idLiquidacion, d.compania })
                .HasConstraintName("FK_liquidacionViajeDetailLis_Header");
        });

        modelBuilder.Entity<ordenTrasladoCabecera>(entity =>
        {
            entity.HasKey(e => new { e.no_guia, e.compania });

            entity.ToTable("ordenTrasladoCabecera", tb =>
                {
                    tb.HasTrigger("insertOrdenTrasladoMercancias");
                    tb.HasTrigger("insertOrdenTrasladoUbicaciones");
                });

            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.comentarioTraslado).IsUnicode(false);
            entity.Property(e => e.domicilioEmisorEstado)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.domicioReceptorCp)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.exportacion)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.fechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.fechaTraslado).HasColumnType("datetime");
            entity.Property(e => e.formaDePago)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.lugarDeExpedicion)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.medioTransporte)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.metodoPago)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.moneda)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.nombreEmisor)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.numOrdenOT)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.plaza)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.razonSocialReceptor)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.regimenFiscal)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.rfcEmisor)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.rfcReceptor)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ruta)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.tipoCFD)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.tipoComprobante)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.totalDistanciaRecorrida).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.transportista)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.usoCfdiReceptor)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.version)
                .HasMaxLength(5)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ordenTrasladoMercancia>(entity =>
        {
            entity.Property(e => e.claveMaterialPeligroso)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.claveProductoSat)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.claveUnidad)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.descripcionProducto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.esMaterialPeligroso)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.nombreMercancia)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.pesoKg).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ordenTrasladoCabecera).WithMany(p => p.ordenTrasladoMercancia)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .HasConstraintName("FK_Mercancias_Cabecera");
        });

        modelBuilder.Entity<ordenTrasladoUbicacione>(entity =>
        {
            entity.Property(e => e.compania)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.cpDestinatario)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.cpRemitente)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.estadoDestinatario)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.estadoRemitente)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.fechaLlegada)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.fechaSalida)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.idUbicacionDestinatario)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.idUbicacionRemitente)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.nombreDestinatario)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.nombreRemitente)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.num_guia)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.paisDestinatario)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.paisRemitente)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.rfcDestinatario)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.rfcRemitente)
                .HasMaxLength(13)
                .IsUnicode(false);

            entity.HasOne(d => d.ordenTrasladoCabecera).WithMany(p => p.ordenTrasladoUbicaciones)
                .HasForeignKey(d => new { d.no_guia, d.compania })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Ubicaciones_Cabecera");
        });

        modelBuilder.Entity<tipoCambio>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__tipoCamb__3213E83FC7C7CF29");

            entity.ToTable("tipoCambio");

            entity.Property(e => e.activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.fecha).HasColumnType("date");
            entity.Property(e => e.fechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.fechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.valor).HasColumnType("decimal(10, 4)");
        });

        modelBuilder.Entity<vwTipoCambio>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwTipoCambio");

            entity.Property(e => e.fecha).HasColumnType("date");
            entity.Property(e => e.fechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.fechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.usuarioCreadoPor)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.usuarioModificadoPor)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.valor).HasColumnType("decimal(10, 4)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
