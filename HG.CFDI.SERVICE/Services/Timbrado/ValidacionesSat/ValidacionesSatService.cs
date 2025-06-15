using BuzonE;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.CORE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using HG.CFDI.CORE.Models.LisApi.ModelRequestLis.CartaPorte;
using CFDI.Data.Entities;

namespace HG.CFDI.SERVICE.Services.Timbrado.ValidacionesSat
{
    public class ValidacionesSatService: IValidacionesSatService
    {
        private readonly ICartaPorteRepository _cartaPorteRepository;
        private readonly string _sufijoArchivoCfdi;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CartaPorteService> _logger;
        private readonly List<FirmaDigitalOptions> _firmaDigitalOptions;
        private readonly InvoiceOneApiOptions _invoiceOneOptions;
        private readonly List<BuzonEApiCredential> _buzonEApiCredentials;
        private readonly LisApiOptions _lisApiOptions;
        private readonly RyderApiOptions _ryderApiOptions;
        private readonly List<compania> _companias;

        public ValidacionesSatService(ICartaPorteRepository cartaPorteRepository,
            IConfiguration configuration,
            IOptions<List<FirmaDigitalOptions>> firmaDigitalOptions,
            IOptions<InvoiceOneApiOptions> invoiceOneOptions,
            IOptions<List<BuzonEApiCredential>> buzonEOptions,
            IOptions<LisApiOptions> lisApiOptions,
            IOptions<RyderApiOptions> ryderApiOptions,
            IOptions<List<compania>> companiaOptions,
            ILogger<CartaPorteService> logger)
        {
            _configuration = configuration;
            _cartaPorteRepository = cartaPorteRepository;
            _firmaDigitalOptions = firmaDigitalOptions.Value;
            _invoiceOneOptions = invoiceOneOptions.Value;
            _buzonEApiCredentials = buzonEOptions.Value;
            _lisApiOptions = lisApiOptions.Value;
            _ryderApiOptions = ryderApiOptions.Value;
            _companias = companiaOptions.Value;
            _sufijoArchivoCfdi = configuration.GetValue<string>("SufijoNombreCfdi");
            _logger = logger;
        }

        public async Task<CartaPortePendiente<FacturaCartaPorte>> getCartaPorteRequestLis(cartaPorteCabecera cp)
        {
            try
            {
                var facturaCCP = new FacturaCartaPorte();

                string[] remision = cp.num_guia.Split('-');

                facturaCCP.Serie = remision[0]; ;
                facturaCCP.Folio = Convert.ToInt32(remision[1]);
                facturaCCP.FechaDoc = cp.fechaInsert;
                facturaCCP.IdCliente = cp.idClienteLis;
                facturaCCP.FechaCita = DateTime.Now;
                facturaCCP.IdTipoOperacion = 5;
                facturaCCP.NumeroOperacion = "171023";
                facturaCCP.IdOperador = cp.idOperadorLis;
                facturaCCP.IdUnidad = cp.idUnidadLis;
                facturaCCP.IdRemolque = cp.idRemolqueLis == null ? string.Empty : cp.idRemolqueLis;
                facturaCCP.IdLineaRem1 = cp.idLineaRem1Lis == null ? string.Empty : cp.idLineaRem1Lis;
                facturaCCP.IdRemolque2 = cp.idRemolque2Lis == null ? string.Empty : cp.idRemolque2Lis;
                facturaCCP.IdLineaRem2 = cp.idLineaRem2Lis == null ? string.Empty : cp.idLineaRem2Lis;
                facturaCCP.IdDolly = string.Empty;
                facturaCCP.idRuta = cp.idRutaLis == null ? 0 : cp.idRutaLis;
                facturaCCP.montoFlete = "0.00";
                facturaCCP.es_LogisticaInversa = false;
                facturaCCP.referenciaCliente = "n/a";
                facturaCCP.observacionesCartaPorte = cp.observacionesPedido;
                facturaCCP.pesoBrutoVehicular = cp.pesoBrutoVehicular == null ? 100 : cp.pesoBrutoVehicular;

                facturaCCP.claveRegimenAduanero = cp.cartaPorteRegimenAduaneros != null && cp.cartaPorteRegimenAduaneros.Count > 0 ? cp.cartaPorteRegimenAduaneros.FirstOrDefault().regimenAduanero : "";
                facturaCCP.idArea = cp.idSucursalLis;
                facturaCCP.Mercancias = new List<Mercancia>();
                facturaCCP.Conceptos = new List<Concepto>();
                facturaCCP.Addenda = new List<Addenda>();

                facturaCCP.Ubicaciones = new List<Ubicacion>()
                    {
                        new Ubicacion() {
                            DistanciaRecorrida = 12,
                            IdRemitente = cp.idClienteRemitenteLis,
                            FechaHoraSalidaRemitente = DateTime.Now,
                            IdDestinatario = cp.idClienteDestinatarioLis,
                            FechaHoraLlegadaDestinatario = DateTime.Now
                        }
                    };

                int idIncrementalAddenda = 1;
                foreach (var addenda in cp.cartaPorteAddenda)
                {
                    var nuevaAddenda = new Addenda();
                    nuevaAddenda.IdCliente = addenda.idClienteLis;
                    nuevaAddenda.Consecutivo = idIncrementalAddenda++;
                    nuevaAddenda.Descripcion = addenda.descripcion;
                    nuevaAddenda.Valor = addenda.valor;

                    facturaCCP.Addenda.Add(nuevaAddenda);
                }

                facturaCCP.DocumentoAduana = new List<DocumentoAduana>()
                    {
                        new DocumentoAduana()
                        {
                            numDocAduanero = "",
                            numPedimento = "",
                            rfcImportador = ""
                        }
                    };

                int idIncremental = 1;
                foreach (var m in cp.cartaPorteMercancia)
                {

                    var merca = new Mercancia();
                    merca.IdMercancia = idIncremental++;
                    merca.BienesTransp = m.claveProdServ;
                    merca.Descripcion = m.descripcion;
                    merca.Cantidad = m.cantidad != null ? (int)m.cantidad : null;
                    merca.ClaveUnidadPeso = m.claveUnidadPeso;

                    if (m.claveUnidad != null)
                    {
                        merca.ClaveUnidad = m.claveUnidad.Contains("-") ? m.claveUnidad.Split('-')[0] : m.claveUnidad;
                    }
                    else
                    {
                        merca.ClaveUnidad = null;
                    }

                    //merca.MaterialPeligroso = m.EsMaterialPeligroso == null || m.EsMaterialPeligroso == "" ? "No" : m.EsMaterialPeligroso;
                    //merca.CveMaterialPeligroso = merca.MaterialPeligroso == "Sí" ? m.CveMaterialPeligroso : "";


                    if (m.esMaterialPeligroso.Equals("1"))
                    {
                        merca.MaterialPeligroso = "Sí";
                        merca.CveMaterialPeligroso = m.cveMaterialPeligroso;
                    }
                    else if (m.esMaterialPeligroso.Equals("0,1") || m.esMaterialPeligroso.Equals("0"))
                    {
                        merca.MaterialPeligroso = "No";
                        merca.CveMaterialPeligroso = string.Empty;
                    }



                    merca.claveTipoMateria = m.tipoMateria == null ? "" : m.tipoMateria;
                    merca.Embalaje = "Z01";
                    merca.DescripEmbalaje = "Cajas de Cartón";
                    merca.PesoEnKg = m.peso;
                    merca.FraccionArancelaria = "";
                    merca.UUIDComercioExt = new Guid();
                    merca.IDOrigen = cp.idPlazaOrLis;
                    merca.IDDestino = cp.idPlazaDeLis;



                    facturaCCP.Mercancias.Add(merca);
                }

                foreach (var detalle in cp.cartaPorteDetalles)
                {
                    var concepto = new Concepto();
                    concepto.IdConcepto = detalle.idConceptolis;
                    concepto.Cantidad = detalle.cantidad;
                    concepto.Importe = detalle.importe;
                    concepto.descripcion = detalle.descripcion;
                    concepto.NoIdentificacion = "n/a";
                    concepto.referenciaConcepto = "n/a";

                    facturaCCP.Conceptos.Add(concepto);
                }

                return new CartaPortePendiente<FacturaCartaPorte>() { no_guia = cp.no_guia, compania = cp.compania, StatusGuia = cp.statusGuia, num_guia = cp.num_guia, facturaCartaPorte = facturaCCP };

            }
            catch (Exception err)
            {
                return null;
            }
        }

        public GeneralResponse<string> getCartaPorteRequestInvoiceOne(cartaPorteCabecera ccps, string database)
        {
            try
            {
                #region
                //COMPROBANTE
                var comprobante = new Comprobante();

                if (ccps.cartaPorteSustitucions.Any())
                {
                    ComprobanteCfdiRelacionados cfdiRelacionados = new ComprobanteCfdiRelacionados();
                    cfdiRelacionados.TipoRelacion = c_TipoRelacion.Item04;

                    List<ComprobanteCfdiRelacionadosCfdiRelacionado> listaCfdiRelacionado = new List<ComprobanteCfdiRelacionadosCfdiRelacionado>();

                    foreach (var item in ccps.cartaPorteSustitucions)
                    {
                        ComprobanteCfdiRelacionadosCfdiRelacionado cfdiRelacionado = new ComprobanteCfdiRelacionadosCfdiRelacionado();
                        cfdiRelacionado.UUID = item.uuid;
                        listaCfdiRelacionado.Add(cfdiRelacionado);
                    }
                    cfdiRelacionados.CfdiRelacionado = listaCfdiRelacionado.ToArray();
                    comprobante.CfdiRelacionados = new ComprobanteCfdiRelacionados[] { cfdiRelacionados };
                }

                comprobante.Version = "4.0";

                comprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

                //JYMH Exportacion (En nuestro caso no hacemos las exportaciones)
                comprobante.Exportacion = "01";

                comprobante.MetodoPagoSpecified = true;  //REQUERIDO
                switch (ccps.metodoPago)
                {
                    case "PPD":
                        comprobante.MetodoPago = c_MetodoPago.PPD; //REQUERIDO //PAGO EN UNA SOLA EXHIBICION, FALTA RELACION TABLA from sat_metodo_pago CON TRAFICO_GUIA
                        break;
                    case "PUE":
                        comprobante.MetodoPago = c_MetodoPago.PUE; //REQUERIDO //PAGO EN UNA SOLA EXHIBICION, FALTA RELACION TABLA from sat_metodo_pago CON TRAFICO_GUIA
                        break;
                    default:
                        comprobante.MetodoPago = c_MetodoPago.PUE; //REQUERIDO //PAGO EN UNA SOLA EXHIBICION, FALTA RELACION TABLA from sat_metodo_pago CON TRAFICO_GUIA
                        break;
                }

                comprobante.FormaPagoSpecified = true;  //REQUERIDO
                switch (ccps.formaPago)
                {
                    case "99":
                        comprobante.FormaPago = c_FormaPago.Item99;  //REQUERIDO //99 ES POR DEFINIR, FALTA AGREGAR AL QUERY
                        break;
                    case "03":
                        comprobante.FormaPago = c_FormaPago.Item03;  //REQUERIDO //99 ES POR DEFINIR, FALTA AGREGAR AL QUERY
                        break;
                    case "02":
                        comprobante.FormaPago = c_FormaPago.Item02;  //REQUERIDO //99 ES POR DEFINIR, FALTA AGREGAR AL QUERY
                        break;
                    case "01":
                        comprobante.FormaPago = c_FormaPago.Item01;  //REQUERIDO //99 ES POR DEFINIR, FALTA AGREGAR AL QUERY
                        break;
                    case "12":
                        comprobante.FormaPago = c_FormaPago.Item12;  //REQUERIDO //99 ES POR DEFINIR, FALTA AGREGAR AL QUERY
                        break;
                    default:
                        comprobante.FormaPago = c_FormaPago.Item99;  //REQUERIDO //99 ES POR DEFINIR, FALTA AGREGAR AL QUERY
                        break;
                }

                //comprobante.CondicionesDePago = ccps.cabecera.dias_credito + " DIAS";
                if (database == "hgdb_lis" && ccps.cteReceptorId == 5999)
                {
                    if (string.IsNullOrEmpty(ccps.shipperAccount))
                    {
                        //        code = "AMAZON",
                        //        message = "Shipper Account vacio",
                        //        detail = "Favor de ingresarlo en el pedido, o en app soporteFacturas"

                        List<string> errors = new List<string>
                        {
                            "Favor de ingresar el shipperAccount en el pedido, o en app soporteFacturas"
                        };
                        return new GeneralResponse<string>() { IsSuccess = false, Message = "Shipper Account vacio", ErrorList = errors };
                    }
                    if (string.IsNullOrEmpty(ccps.shipment))
                    {
                        //        code = "AMAZON",
                        //        message = "shipment vacio",
                        //        detail = "Favor de ingresarlo en el pedido, o en app soporteFacturas"                  
                        List<string> errors = new List<string>
                        {
                            "Favor de ingresar el shipment en el pedido, o en app soporteFacturas"
                        };
                        return new GeneralResponse<string>() { IsSuccess = false, Message = "shipment vacio", ErrorList = errors };
                    }

                    comprobante.CondicionesDePago = $"TFSINV:AMUOU:{ccps.shipperAccount.ToUpper().Trim()}:{ccps.shipment.ToUpper().Trim()}";
                }
                else if (database == "lindadb" && ccps.cteReceptorId == 1874)
                {
                    if (string.IsNullOrEmpty(ccps.shipperAccount))
                    {
                        //        code = "AMAZON",
                        //        message = "Shipper Account vacio",
                        //        detail = "Favor de ingresarlo en el pedido, o en app soporteFacturas"

                        List<string> errors = new List<string>
                        {
                            "Favor de ingresar el shipperAccount en el pedido, o en app soporteFacturas"
                        };
                        return new GeneralResponse<string>() { IsSuccess = false, Message = "Shipper Account vacio", ErrorList = errors };
                    }
                    if (string.IsNullOrEmpty(ccps.shipment))
                    {
                        //        code = "AMAZON",
                        //        message = "shipment vacio",
                        //        detail = "Favor de ingresarlo en el pedido, o en app soporteFacturas"                  
                        List<string> errors = new List<string>
                        {
                            "Favor de ingresar el shipment en el pedido, o en app soporteFacturas"
                        };
                        return new GeneralResponse<string>() { IsSuccess = false, Message = "shipment vacio", ErrorList = errors };
                    }

                    comprobante.CondicionesDePago = $"TFSINV:LINDA:{ccps.shipperAccount.ToUpper().Trim()}:{ccps.shipment.ToUpper().Trim()}";
                }
                else
                {
                    comprobante.CondicionesDePago = ccps.diasCredito + " DIAS";
                }

                comprobante.Moneda = ccps.moneda;

                comprobante.tipoCambio = ccps.moneda.Equals("MXN") ? (int)ccps.cteReceptorTipoCambio.Value : ccps.cteReceptorTipoCambio.Value;
                comprobante.TipoCambioSpecified = true;
                comprobante.TipoDeComprobante = "I";
                comprobante.LugarExpedicion = ccps.cteEmisorCp;
                comprobante.Folio = ccps.num_guia;

                //EMISOR
                var emisor = new ComprobanteEmisor();
                emisor.Rfc = ccps.cteEmisorRfc.Trim();
                emisor.Nombre = ccps.cteEmisorNombre; //"HG TRANSPORTACIONES SA DE CV";
                emisor.RegimenFiscal = ccps.cteEmisorRegimenFiscal;
                comprobante.Emisor = emisor;

                //RECEPTOR
                var receptor = new ComprobanteReceptor();
                //RECEPTOR
                if (ccps.cteReceptorRfc == "XAXX010101000" || ccps.cteReceptorRfc == "XEXX010101000")
                {
                    receptor.Rfc = ccps.cteReceptorRfc.Trim();
                    receptor.Nombre = ccps.cteReceptorNombre;
                    receptor.DomicilioFiscalReceptor = string.IsNullOrEmpty(ccps.cteEmisorCp) ? "" : ccps.cteEmisorCp.Trim();
                    receptor.RegimenFiscalReceptor = "616";
                    receptor.UsoCFDI = c_UsoCFDI.S01;
                    comprobante.Receptor = receptor;
                }
                else
                {
                    receptor.Rfc = ccps.cteReceptorRfc.Trim();
                    receptor.Nombre = ccps.cteReceptorNombre;
                    receptor.DomicilioFiscalReceptor = string.IsNullOrEmpty(ccps.cteReceptorCp) ? "" : ccps.cteReceptorCp.Trim();
                    receptor.RegimenFiscalReceptor = ccps.cteReceptorRegimenFiscal;
                    receptor.UsoCFDI = (c_UsoCFDI)Enum.Parse(typeof(c_UsoCFDI), ccps.cteReceptorUsoCFDI == "P01" ? "G03" : ccps.cteReceptorUsoCFDI);
                    comprobante.Receptor = receptor;
                }


                var addenda = new ComprobanteAddenda();
                XmlDocument docAddenda = new XmlDocument();

                var partes = new List<string>();

                if (!string.IsNullOrEmpty(ccps.cteEmisorCalle))
                    partes.Add($"{ccps.cteEmisorCalle} No.");

                if (!string.IsNullOrEmpty(ccps.cteEmisorNoExterior))
                    partes.Add($"{ccps.cteEmisorNoExterior} COL.");

                if (!string.IsNullOrEmpty(ccps.cteEmisorColonia))
                    partes.Add(ccps.cteEmisorColonia);

                if (!string.IsNullOrEmpty(ccps.cteEmisorLocalidad))
                    partes.Add($"{ccps.cteEmisorLocalidad} NUEVO LEON,");

                if (!string.IsNullOrEmpty(ccps.cteEmisorPais))
                    partes.Add($"{ccps.cteEmisorPais} CP.");

                if (!string.IsNullOrEmpty(ccps.cteEmisorCp))
                    partes.Add(ccps.cteEmisorCp);

                string domicilioEmisorAddenda = string.Join(" ", partes);

                string noIdentificador = ccps.cartaPorteDetalles.Any() ? ccps.cartaPorteDetalles.FirstOrDefault().cpeNoIdentificador ?? "" : "";
                string addendaContent = string.Format("<InformacionAdicional xmlns=\"http://www.buzone.com.mx/XSD/ParserGenerico/Generico\" xsi:schemaLocation=\"http://www.buzone.com.mx/XSD/ParserGenerico/Generico schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">   <Conector>string</Conector>   <IdUnico>string</IdUnico>   <CadenaOriginal>string</CadenaOriginal>   <IdDoc>string</IdDoc>   <SenderID>string</SenderID>   <CFDI />   <lineaY />   <EMSR />   <EXP />   <R />   <PTDA />   <PTDA />   <PTDA />   <PTDAo />   <PTDAo />   <PTDAo />   <PTDAo />   <PTDAo />   <T etiqueta8T=\"Caja:\" valor8T=\"{0}\" etiqueta9T=\"shipment:\" valor9T=\"{1}\" etiqueta10T=\"No Identificacion:\" valor10T=\"{2}\" etiqueta11T=\"Observaciones:\" valor11T =\"{3}\" etiqueta12T=\"Dirección emisor:\" valor12T =\"{4}\" />   <OBS/>   <Documento/> </InformacionAdicional>",
                    ccps.idRemolque ?? "",
                    ccps.shipment ?? "",
                    noIdentificador,
                    string.IsNullOrEmpty(ccps.observacionesPedido) ? "SIN OBSERVACIONES" : ccps.observacionesPedido.Replace("\"", "'"),
                    domicilioEmisorAddenda
                    );

                docAddenda.LoadXml(addendaContent);
                addenda.Any = new XmlElement[] { docAddenda.DocumentElement };

                comprobante.Addenda = addenda;


                //CONCEPTOS
                List<ComprobanteConcepto> listaConceptos = new List<ComprobanteConcepto>();
                List<ComprobanteImpuestosTraslado> listaTraslados = new List<ComprobanteImpuestosTraslado>();
                List<ComprobanteImpuestosRetencion> listaRetenciones = new List<ComprobanteImpuestosRetencion>();

                foreach (var item in ccps.cartaPorteDetalles)
                {
                    var concepto = new ComprobanteConcepto();
                    concepto.ClaveProdServ = item.claveProdServ;
                    concepto.ClaveUnidad = "E48";
                    concepto.Cantidad = 1;
                    concepto.Descripcion = item.descripcion;
                    concepto.ValorUnitario = decimal.Round(item.importe.Value, 6);
                    concepto.Importe = decimal.Round(item.importe.Value, 6);
                    concepto.Unidad = "SERVICIO";
                    concepto.DescuentoSpecified = false;
                    concepto.Descuento = decimal.Parse("00.00");
                    concepto.ObjetoImp = (item.objetoImp ?? "").Equals("No Objeto")
                        ? c_ObjetoImp.Item01
                        : c_ObjetoImp.Item02;


                    concepto.NoIdentificacion = string.IsNullOrEmpty(item.cpeNoIdentificador) || string.IsNullOrWhiteSpace(item.cpeNoIdentificador) ? null : item.cpeNoIdentificador;

                    ComprobanteConceptoImpuestos ImpuestosConcepto = new ComprobanteConceptoImpuestos();

                    ImpuestosConcepto.Traslados = new ComprobanteConceptoImpuestosTraslado[]
                        {
                                new ComprobanteConceptoImpuestosTraslado()
                                {
                                    Base= decimal.Round(item.importe.Value,6),
                                    Impuesto= c_Impuesto.Item002,
                                    TipoFactor= c_TipoFactor.Tasa,
                                    TasaOCuota= decimal.Round(item.factorIva.Value,6),
                                    Importe= decimal.Round(item.importe.Value * item.factorIva.Value,6) ,
                                    ImporteSpecified = true,
                                    TasaOCuotaSpecified = true
                                }
                        };

                    if (item.factorRetencion > 0)
                    {
                        ImpuestosConcepto.Retenciones = new ComprobanteConceptoImpuestosRetencion[]
                        {
                                new ComprobanteConceptoImpuestosRetencion(){
                                    Base = decimal.Round(item.importe.Value,6),
                                    Impuesto = c_Impuesto.Item002,
                                    TipoFactor = c_TipoFactor.Tasa,
                                    TasaOCuota = decimal.Round(item.factorRetencion.Value,6),
                                    Importe = decimal.Round(item.importe.Value * item.factorRetencion.Value,6)
                                }
                        };
                    }

                    concepto.Impuestos = ImpuestosConcepto;

                    listaConceptos.Add(concepto);
                }

                ComprobanteImpuestos ComprobanteImpuestos = new ComprobanteImpuestos();

                var retencionesGenerales = listaConceptos.Where(x => x.Impuestos.Retenciones != null).Sum(x => x.Impuestos.Retenciones.First().Importe);

                var trasladosGenerales = from concepto in listaConceptos
                                         group concepto by concepto.Impuestos.Traslados.ToList().FirstOrDefault().TasaOCuota into g
                                         select new ComprobanteConceptoImpuestosTraslado()
                                         {
                                             Base = g.Sum(x => x.Impuestos.Traslados.ToList().FirstOrDefault().Base),
                                             TasaOCuota = g.Key,
                                             Importe = g.Sum(x => x.Impuestos.Traslados.ToList().FirstOrDefault().Importe)
                                         };


                if (retencionesGenerales > 0)
                {
                    listaRetenciones.Add
                    (
                        new ComprobanteImpuestosRetencion()
                        {
                            Importe = decimal.Round(retencionesGenerales, 2),
                            Impuesto = c_Impuesto.Item002
                        }
                    );

                    ComprobanteImpuestos.TotalImpuestosRetenidosSpecified = true;
                    ComprobanteImpuestos.TotalImpuestosRetenidos = decimal.Round(retencionesGenerales, 2);
                    ComprobanteImpuestos.Retenciones = listaRetenciones.ToArray();
                }
                else
                {
                    ComprobanteImpuestos.TotalImpuestosRetenidosSpecified = false;
                }

                foreach (var item in trasladosGenerales)
                {
                    listaTraslados.Add
                    (
                        new ComprobanteImpuestosTraslado()
                        {
                            Base = decimal.Round(item.Base, 2),
                            Impuesto = c_Impuesto.Item002,
                            TipoFactor = c_TipoFactor.Tasa,
                            TasaOCuota = item.TasaOCuota,
                            Importe = decimal.Round(item.Importe, 2),
                            ImporteSpecified = true,
                            TasaOCuotaSpecified = true
                        }
                    );
                }

                ComprobanteImpuestos.TotalImpuestosTrasladadosSpecified = true;
                ComprobanteImpuestos.TotalImpuestosTrasladados = decimal.Round(trasladosGenerales.Sum(x => x.Importe), 2);
                ComprobanteImpuestos.Traslados = listaTraslados.ToArray();

                comprobante.Impuestos = ComprobanteImpuestos;
                comprobante.SubTotal = decimal.Round(listaConceptos.Sum(x => x.Importe), 2);
                comprobante.Total = decimal.Round(listaConceptos.Sum(x => x.Importe) + comprobante.Impuestos.TotalImpuestosTrasladados - comprobante.Impuestos.TotalImpuestosRetenidos, 2);
                comprobante.DescuentoSpecified = false;
                comprobante.Descuento = decimal.Parse("00.000000");
                comprobante.Conceptos = listaConceptos.ToArray();

                //INTEGRACION DE COMPLEMENTO CARTA PORTE
                ComprobanteComplemento comprobanteComplemento = new ComprobanteComplemento();

                CartaPorte cartaPorte = new CartaPorte();

                cartaPorte.IdCCP = GenerateIdCCP();
                cartaPorte.Version = "3.1";

                cartaPorte.TranspInternac = ccps.esTransporteInternacional;

                if (cartaPorte.TranspInternac.Equals("Sí"))
                {
                    cartaPorte.EntradaSalidaMerc = ccps.entSalMercancia;
                    cartaPorte.EntradaSalidaMercSpecified = true;
                    cartaPorte.ViaEntradaSalida = "0" + ccps.viaEntradaSalida;//Auto transporte federal
                    cartaPorte.ViaEntradaSalidaSpecified = true;
                    cartaPorte.PaisOrigenDestino = "USA";
                    cartaPorte.PaisOrigenDestinoSpecified = true;



                    //cartaPorte.RegimenAduaneroSpecified = true;
                    //cartaPorte.RegimenAduanero = ccps.ClaveRegimenAduanero;

                    List<CartaPorteRegimenAduaneroCCP> regimenesAduaneros = new List<CartaPorteRegimenAduaneroCCP>();

                    if (ccps.cartaPorteRegimenAduaneros.Count > 0)
                    {
                        foreach (var item in ccps.cartaPorteRegimenAduaneros)
                        {

                            CartaPorteRegimenAduaneroCCP regimenAduanero = new CartaPorteRegimenAduaneroCCP();

                            if ((item.regimenAduanero == null || item.regimenAduanero.Equals("")) && cartaPorte.EntradaSalidaMerc.Equals("Salida"))
                            {
                                regimenAduanero.RegimenAduanero = "EXD";
                            }
                            else if (item.regimenAduanero == null || item.regimenAduanero.Equals("") && cartaPorte.EntradaSalidaMerc.Equals("Entrada"))
                            {
                                regimenAduanero.RegimenAduanero = "IMD";
                            }
                            else
                            {
                                regimenAduanero.RegimenAduanero = item.regimenAduanero;
                            }

                            regimenesAduaneros.Add(regimenAduanero);
                        }
                    }
                    else
                    {
                        CartaPorteRegimenAduaneroCCP regimenAduanero = new CartaPorteRegimenAduaneroCCP();

                        if (cartaPorte.EntradaSalidaMerc.Equals("Salida"))
                        {
                            regimenAduanero.RegimenAduanero = "EXD";
                        }

                        if (cartaPorte.EntradaSalidaMerc.Equals("Entrada"))
                        {
                            regimenAduanero.RegimenAduanero = "IMD";
                        }

                        regimenesAduaneros.Add(regimenAduanero);
                    }



                    cartaPorte.RegimenesAduaneros = regimenesAduaneros.ToArray();
                }

                //FIGURA TRANSPORTE
                List<CartaPorteTiposFigura> figurasTransporte = new List<CartaPorteTiposFigura>
                {
                    new CartaPorteTiposFigura()
                    {
                        TipoFigura = c_FiguraTransporte.Item01, //01 es operador, 02 es propietario, 03 es arrendador, 04 es notificado
                        NombreFigura = ccps.operador,
                        NumLicencia = ccps.licenciaOperador,
                        RFCFigura = ccps.rfcOperador.Trim()
                    }
                };

                //UBICACIONES   
                List<CartaPorteUbicacion> ubicaciones = new List<CartaPorteUbicacion>();

                foreach (var item in ccps.cartaPorteUbicaciones)
                {
                    CartaPorteUbicacion ubicacionOrigen = new CartaPorteUbicacion();
                    CartaPorteUbicacion ubicacionDestino = new CartaPorteUbicacion();

                    //UBICACION ORIGEN
                    if (!(string.IsNullOrWhiteSpace(item.remitenteResidenciaFiscal) || item.remitenteResidenciaFiscal.Equals("MEX")))
                    {
                        ubicacionOrigen.ResidenciaFiscal = "MEX";
                        ubicacionOrigen.NumRegIdTrib = string.IsNullOrWhiteSpace(item.remitenteNumRegIdTrib) ? null : item.remitenteNumRegIdTrib.Trim();
                    }

                    ubicacionOrigen.RFCRemitenteDestinatario = string.IsNullOrWhiteSpace(item.remitenteRfc) ? null : item.remitenteRfc.Trim();
                    ubicacionOrigen.TipoUbicacion = CartaPorteUbicacionTipoUbicacion.Origen;
                    ubicacionOrigen.IDUbicacion = string.IsNullOrWhiteSpace(item.remitenteId) ? "OR000001" : item.remitenteId.Trim();
                    ubicacionOrigen.NombreRemitenteDestinatario = string.IsNullOrWhiteSpace(item.nombreRemitente) ? null : item.nombreRemitente.Trim();

                    ubicacionOrigen.FechaHoraSalidaLlegada = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //item.FechaDespachoProgramado;                 

                    ubicacionOrigen.Domicilio = new CartaPorteUbicacionDomicilio()
                    {
                        CodigoPostal = item.remitenteCp,
                        Estado = item.remitenteEstado,
                        Pais = item.remitentePais,
                        Municipio = item.remitenteMunicipio,
                        Localidad = item.remitenteLocalidad
                    };

                    ubicaciones.Add(ubicacionOrigen);

                    //UBICACION DESTINO
                    if (!string.IsNullOrWhiteSpace(item.destinatarioResidenciaFiscal))
                    {
                        if (!item.destinatarioPais.Equals("MEX"))
                        {
                            ubicacionDestino.ResidenciaFiscal = item.destinatarioResidenciaFiscal.Trim();
                            ubicacionDestino.ResidenciaFiscalSpecified = true;

                            if (!string.IsNullOrWhiteSpace(item.destinatarioNumRegIdTrib))
                            {
                                ubicacionDestino.NumRegIdTrib = item.destinatarioNumRegIdTrib.Trim();
                            }
                        }

                    }

                    ubicacionDestino.RFCRemitenteDestinatario = string.IsNullOrWhiteSpace(item.destinatarioRfc) ? null : item.destinatarioRfc.Trim();
                    ubicacionDestino.TipoUbicacion = CartaPorteUbicacionTipoUbicacion.Destino;
                    ubicacionDestino.IDUbicacion = string.IsNullOrWhiteSpace(item.destinatarioId) ? "DE000001" : item.destinatarioId.Trim();
                    ubicacionDestino.NombreRemitenteDestinatario = string.IsNullOrWhiteSpace(item.nombreDestinatario) ? null : item.nombreDestinatario.Trim();

                    ubicacionDestino.FechaHoraSalidaLlegada = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //item.FechaArriboProgramado;                 
                    ubicacionDestino.DistanciaRecorridaSpecified = true;
                    ubicacionDestino.DistanciaRecorrida = item.distanciaRecorrida.Value;

                    ubicacionDestino.Domicilio = new CartaPorteUbicacionDomicilio()
                    {
                        CodigoPostal = item.destinatarioCp,
                        Estado = item.destinatarioEstado,
                        Pais = item.destinatarioPais,
                        Municipio = item.destinatarioMunicipio,
                        Localidad = item.destinatarioLocalidad
                    };

                    ubicaciones.Add(ubicacionDestino);

                }

                cartaPorte.TotalDistRecSpecified = true;
                //debe ser igual a la suma de "DistanciaRecorrida" de las ubicaciones de tipo "destino"
                cartaPorte.TotalDistRec = ubicaciones.Where(x => x.TipoUbicacion == CartaPorteUbicacionTipoUbicacion.Destino).Sum(y => y.DistanciaRecorrida);

                //CONFIGURACION VEHICULAR


                CartaPorteMercanciasAutotransporte autotransporte = new CartaPorteMercanciasAutotransporte();

                CartaPorteMercanciasAutotransporteIdentificacionVehicular identificacionVehicular = new CartaPorteMercanciasAutotransporteIdentificacionVehicular();
                identificacionVehicular.AnioModeloVM = Convert.ToInt32(ccps.modeloUnidad);
                identificacionVehicular.ConfigVehicular = ccps.configVehicular;
                identificacionVehicular.PlacaVM = Regex.Replace(ccps.placaUnidad, "[^a-zA-Z0-9]", "");
                identificacionVehicular.PesoBrutoVehicular = ccps.pesoBrutoVehicular.Value;
                autotransporte.IdentificacionVehicular = identificacionVehicular;
                autotransporte.PermSCT = ccps.claveTipoPermiso; //TPTA02;
                autotransporte.NumPermisoSCT = ccps.numTipoPermiso;

                CartaPorteMercanciasAutotransporteSeguros seguros = new CartaPorteMercanciasAutotransporteSeguros();
                seguros.AseguraRespCivil = ccps.aseguradora;
                seguros.PolizaRespCivil = "M128209"; //ccps.PolizaUnidad; 
                autotransporte.Seguros = seguros;

                //Proceso de limpiado al subtiporemolque
                if (!string.IsNullOrWhiteSpace(ccps.subtipoRemolque1))
                {
                    Regex regex = new Regex("([CTR0-9]{6})");
                    Match match = regex.Match(ccps.subtipoRemolque1);
                    if (match.Success)
                    {
                        ccps.subtipoRemolque1 = match.Value;
                    }
                }

                if (!string.IsNullOrWhiteSpace(ccps.placaRemolque1))
                {
                    ccps.placaRemolque1 = Regex.Replace(ccps.placaRemolque1, "[^a-zA-Z0-9]", "");
                }

                var configVehicularPermitidos = new HashSet<string>
                {
                    "VL", "C2", "C3", "OTROEVGP", "OTROSG",
                    "GPLUTA", "GPLUTB", "GPLUTC", "GPLUTD",
                    "GPLATA", "GPLATB", "GPLATC", "GPLATD"
                };

                if (!configVehicularPermitidos.Contains(identificacionVehicular.ConfigVehicular))
                {
                    autotransporte.Remolques = new CartaPorteMercanciasAutotransporteRemolque[]
                      {
                            new CartaPorteMercanciasAutotransporteRemolque()
                            {
                                Placa = ccps.placaRemolque1,
                                SubTipoRem = ccps.subtipoRemolque1
                            }
                      };
                }

                #endregion
                //MERCANCIAS            
                CartaPorteMercancias mercancias = new CartaPorteMercancias();
                List<CartaPorteMercanciasMercancia> listaMercancias = new List<CartaPorteMercanciasMercancia>();
                foreach (var item in ccps.cartaPorteMercancia)
                {
                    var merca = new CartaPorteMercanciasMercancia();
                    merca.Moneda = ccps.moneda;
                    merca.MonedaSpecified = true;

                    if (cartaPorte.TranspInternac.Equals("Sí"))
                    {
                        merca.ValorMercancia = Convert.ToDecimal(item.valorMercancia.ToString().Replace(",", "."));
                        merca.ValorMercanciaSpecified = true;

                        merca.TipoMateriaSpecified = true;
                        merca.TipoMateria = item.tipoMateria;
                        merca.DescripcionMateria = item.tipoMateria.Equals("05") ? item.descripcionMateria : null;

                        if (!string.IsNullOrEmpty(item.fraccionArancelaria))
                        {
                            item.fraccionArancelaria = Regex.Replace(item.fraccionArancelaria, "[^0-9]", "");
                            merca.FraccionArancelariaSpecified = true;
                            merca.FraccionArancelaria = item.fraccionArancelaria.Length < 10 ? item.fraccionArancelaria.PadRight(10, '0') : item.fraccionArancelaria;
                        }

                        if (!string.IsNullOrWhiteSpace(item.pedimento))
                        {
                            string pedimento = item.pedimento;
                            pedimento = Regex.Replace(pedimento, "[^0-9]", "");

                            if (pedimento.Length >= 15)
                            {
                                pedimento = pedimento.Substring(0, 2) + "  " + pedimento.Substring(2, 2) + "  " + pedimento.Substring(4, 4) + "  " + pedimento.Substring(8, 7);

                                item.pedimento = pedimento;
                            }
                        }

                        string tipoDocumento = cartaPorte.EntradaSalidaMerc.Equals("Salida") ? "20" : "01";
                        string rfcImpo = ccps.rfcImpo == null ? "XAXX010101000" : ccps.rfcImpo;

                        var documentacionAduanera = new CartaPorteMercanciasMercanciaDocumentacionAduanera()
                        {
                            TipoDocumento = tipoDocumento // pedimento
                        };

                        if (cartaPorte.EntradaSalidaMerc.Equals("Entrada") && tipoDocumento.Equals("01") && item.pedimento != null)
                        {
                            documentacionAduanera.NumPedimento = item.pedimento;
                            documentacionAduanera.RFCImpo = rfcImpo;
                        }

                        if (!tipoDocumento.Equals("01"))
                        {
                            Random random = new Random();
                            var codigo = random.Next(100000000, 1000000000);
                            string idecDocAduanero = string.Concat(DateTime.Now.Year, "-", codigo);
                            documentacionAduanera.IdentDocAduanero = idecDocAduanero;
                        }

                        merca.DocumentacionAduanera = new CartaPorteMercanciasMercanciaDocumentacionAduanera[]
                        {
                            documentacionAduanera
                        };
                    }

                    //merca.BienesTransp = item.ClaveProdServ;
                    merca.BienesTransp = item.claveProdServ == "1010101" ? "0" + item.claveProdServ.ToString() : item.claveProdServ.ToString();
                    merca.Descripcion = item.descripcion;
                    merca.Cantidad = item.cantidad;

                    if (!string.IsNullOrEmpty(item.claveUnidad))
                    {
                        Regex regex = new Regex("([a-zA-Z0-9]{2,3})");
                        Match match = regex.Match(item.claveUnidad);

                        if (match.Success)
                        {
                            merca.ClaveUnidad = match.Value;
                        }
                    }

                    if (item.esMaterialPeligroso.Equals("1"))
                    {
                        merca.MaterialPeligrosoSpecified = true;
                        merca.MaterialPeligroso = CartaPorteMercanciasMercanciaMaterialPeligroso.Sí;
                        merca.CveMaterialPeligrosoSpecified = true;
                        merca.CveMaterialPeligroso = item.cveMaterialPeligroso;
                    }
                    else if (item.esMaterialPeligroso.Equals("0,1"))
                    {
                        merca.MaterialPeligrosoSpecified = true;
                        merca.MaterialPeligroso = CartaPorteMercanciasMercanciaMaterialPeligroso.No;
                    }

                    merca.PesoEnKg = item.peso.Value;
                    merca.Moneda = ccps.moneda;
                    //if (ccps.cabecera.cte_paga_id == 76 && database == "chdb_lis")
                    //{
                    //merca.ValorMercancia = 100;
                    //}

                    if (ccps.cteReceptorId == 76 && database == "chdb_lis")
                    {
                        merca.ValorMercancia = 100;
                        merca.ValorMercanciaSpecified = true;
                        //merca.Moneda = ccps.Moneda;
                    }
                    listaMercancias.Add(merca);
                }
                mercancias.Autotransporte = autotransporte;
                mercancias.Mercancia = listaMercancias.ToArray();
                mercancias.NumTotalMercancias = ccps.cartaPorteMercancia.Count();
                mercancias.PesoBrutoTotal = ccps.cartaPorteMercancia.Sum(x => x.peso.Value);
                mercancias.UnidadPeso = "KGM";

                #region parte2
                //AÑADIR A CARTA PORTE
                cartaPorte.FiguraTransporte = figurasTransporte.ToArray();
                cartaPorte.Ubicaciones = ubicaciones.ToArray();
                cartaPorte.Mercancias = mercancias;




                comprobante.Complemento = new ComprobanteComplemento();

                XmlDocument docCartaPorte = new XmlDocument();
                XmlSerializerNamespaces xmlNameSpaceCartaPorte = new XmlSerializerNamespaces();
                xmlNameSpaceCartaPorte.Add("cartaporte31", "http://www.sat.gob.mx/CartaPorte31");

                XmlSerializer cartaPorteSerializar = new XmlSerializer(typeof(CartaPorte));

                using (XmlWriter writer = docCartaPorte.CreateNavigator().AppendChild())
                {
                    cartaPorteSerializar.Serialize(writer, cartaPorte, xmlNameSpaceCartaPorte);
                }

                comprobante.Complemento.Any = new XmlElement[1];
                comprobante.Complemento.Any[0] = docCartaPorte.DocumentElement;

                XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
                xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
                xmlNameSpace.Add("cartaporte31", "http://www.sat.gob.mx/CartaPorte31");
                xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance"); // Agregar el namespace para xsi

                XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

                string sXml = "";

                using (var sww = new StringWriterWithEncoding(Encoding.UTF8))
                {

                    using (XmlWriter writter = XmlWriter.Create(sww))
                    {

                        oXmlSerializar.Serialize(writter, comprobante, xmlNameSpace);
                        sXml = sww.ToString();
                    }

                }


                //otro metodo
                // Crear e inicializar los namespaces
                //XmlSerializerNamespaces xmlNameSpaceCartaPorte = new XmlSerializerNamespaces();
                //xmlNameSpaceCartaPorte.Add("cartaporte31", "http://www.sat.gob.mx/CartaPorte31");

                //XmlDocument docCartaPorte = new XmlDocument();
                //XmlSerializer cartaPorteSerializar = new XmlSerializer(typeof(CartaPorte));

                //// Serializar el complemento Carta Porte
                //using (XmlWriter writer = docCartaPorte.CreateNavigator().AppendChild())
                //{
                //    cartaPorteSerializar.Serialize(writer, cartaPorte, xmlNameSpaceCartaPorte);
                //}

                //// Asignar el complemento Carta Porte al comprobante
                //comprobante.Complemento.Any = new XmlElement[1];
                //comprobante.Complemento.Any[0] = docCartaPorte.DocumentElement;

                //// Crear e inicializar los namespaces del comprobante
                //XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
                //xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
                //xmlNameSpace.Add("cartaporte31", "http://www.sat.gob.mx/CartaPorte31");
                //xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance"); // Agregar el namespace para xsi

                //XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));
                //string sXml = "";

                //// Serializar el comprobante a string
                //using (var sww = new StringWriterWithEncoding(Encoding.UTF8))
                //{
                //    using (XmlWriter writter = XmlWriter.Create(sww))
                //    {
                //        oXmlSerializar.Serialize(writter, comprobante, xmlNameSpace);
                //        sXml = sww.ToString();
                //    }
                //}

                // Cargar el XML en un XmlDocument para agregar manualmente el xsi:schemaLocation
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sXml);

                // Convertir el documento a string nuevamente si es necesario
                string xmlConSchemaLocation = xmlDoc.OuterXml;

                return new GeneralResponse<string>() { IsSuccess = true, Message = "Success", Data = xmlConSchemaLocation };

                //return sXml;
                #endregion parte2
            }
            catch (Exception err)
            {
                List<string> errors = new List<string>
                {
                    err.Message
                };
                return new GeneralResponse<string>() { IsSuccess = false, Message = "Ocurrio una excepcion", ErrorList = errors };
            }
        }

        public async Task<UniqueRequest<RequestBE>> getCartaPorteRequestBuzonE(cartaPorteCabecera ccps, string database)
        {
            try
            {
                #region parte1
                //CREDENTIALS
                var request = new RequestBE();
                string fileType = string.Empty;

                //ADDITIONALINFORMATION
                var infoAd = new RequestBEAdditionalInformation();

                var credentials = _buzonEApiCredentials.Where(x => x.Database.Equals(database)).FirstOrDefault();

                request.usuario = credentials.User;
                request.password = credentials.Password;
                infoAd.fileType = credentials.FileType;

                infoAd.titulo = "108-4500324";
                infoAd.conector = "6094209";
                infoAd.comentario = ccps.num_guia;
                request.AdditionalInformation = infoAd;

                //COMPROBANTE
                var comprobante = new BuzonE.Comprobante();

                if (ccps.cartaPorteSustitucions.Any())
                {
                    BuzonE.ComprobanteCfdiRelacionados cfdiRelacionados = new BuzonE.ComprobanteCfdiRelacionados();
                    cfdiRelacionados.TipoRelacion = BuzonE.c_TipoRelacion.Item04;

                    List<BuzonE.ComprobanteCfdiRelacionadosCfdiRelacionado> listaCfdiRelacionado = new List<BuzonE.ComprobanteCfdiRelacionadosCfdiRelacionado>();

                    foreach (var item in ccps.cartaPorteSustitucions)
                    {
                        BuzonE.ComprobanteCfdiRelacionadosCfdiRelacionado cfdiRelacionado = new BuzonE.ComprobanteCfdiRelacionadosCfdiRelacionado();
                        cfdiRelacionado.UUID = item.uuid;
                        listaCfdiRelacionado.Add(cfdiRelacionado);
                    }
                    cfdiRelacionados.CfdiRelacionado = listaCfdiRelacionado.ToArray();
                    comprobante.CfdiRelacionados = new BuzonE.ComprobanteCfdiRelacionados[] { cfdiRelacionados };
                }

                comprobante.Version = "4.0";

                comprobante.MetodoPagoSpecified = true;
                switch (ccps.metodoPago)
                {
                    case "PPD":
                        comprobante.MetodoPago = BuzonE.c_MetodoPago.PPD;
                        break;
                    case "PUE":
                        comprobante.MetodoPago = BuzonE.c_MetodoPago.PUE;
                        break;
                    default:
                        comprobante.MetodoPago = BuzonE.c_MetodoPago.PUE;
                        break;
                }

                comprobante.FormaPagoSpecified = true;
                switch (ccps.formaPago)
                {
                    case "99":
                        comprobante.FormaPago = BuzonE.c_FormaPago.Item99;
                        break;
                    case "03":
                        comprobante.FormaPago = BuzonE.c_FormaPago.Item03;
                        break;
                    case "02":
                        comprobante.FormaPago = BuzonE.c_FormaPago.Item02;
                        break;
                    case "01":
                        comprobante.FormaPago = BuzonE.c_FormaPago.Item01;
                        break;
                    case "12":
                        comprobante.FormaPago = BuzonE.c_FormaPago.Item12;
                        break;
                    default:
                        comprobante.FormaPago = BuzonE.c_FormaPago.Item99;
                        break;
                }

                if (database == "hgdb_lis" && ccps.cteReceptorId == 5999)
                {
                    if (string.IsNullOrEmpty(ccps.shipperAccount))
                    {
                        return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = "El Shipper Account esta vacio, favor de llenarlo en el pedido." };
                    }
                    if (string.IsNullOrEmpty(ccps.shipment))
                    {
                        return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = "El shipment esta vacio, favor de llenarlo en el pedido." };
                    }

                    comprobante.CondicionesDePago = $"TFSINV:AMUOU:{ccps.shipperAccount.ToUpper().Trim()}:{ccps.shipment.ToUpper().Trim()}";
                }
                else if (database == "lindadb" && ccps.cteReceptorId == 1874)
                {
                    if (string.IsNullOrEmpty(ccps.shipperAccount))
                    {
                        return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = "El Shipper Account esta vacio, favor de llenarlo en el pedido." };
                    }
                    if (string.IsNullOrEmpty(ccps.shipment))
                    {
                        return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = "El shipment esta vacio, favor de llenarlo en el pedido." };
                    }

                    comprobante.CondicionesDePago = $"TFSINV:LINDA:{ccps.shipperAccount.ToUpper().Trim()}:{ccps.shipment.ToUpper().Trim()}";
                }
                else if (database == "chdb_lis" && ccps.cteReceptorId == 1313)
                {
                    if (string.IsNullOrEmpty(ccps.shipperAccount))
                    {
                        return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = "El Shipper Account esta vacio, favor de llenarlo en el pedido." };
                    }
                    if (string.IsNullOrEmpty(ccps.shipment))
                    {
                        return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = "El shipment esta vacio, favor de llenarlo en el pedido." };
                    }

                    comprobante.CondicionesDePago = $"TFSINV:CHARQ:{ccps.shipperAccount.ToUpper().Trim()}:{ccps.shipment.ToUpper().Trim()}";
                }
                else
                {
                    comprobante.CondicionesDePago = ccps.diasCredito + " DIAS";
                }

                switch (ccps.moneda)
                {
                    case "MXN":
                        comprobante.Moneda = c_Moneda.MXN;
                        //comprobante.tipoCambio = 1;
                        comprobante.TipoCambioSpecified = false;
                        break;
                    case "USD":
                        comprobante.Moneda = c_Moneda.USD;
                        comprobante.TipoCambio = ccps.cteReceptorTipoCambio.Value;
                        comprobante.TipoCambioSpecified = true;
                        break;
                    case "CAD":
                        comprobante.Moneda = c_Moneda.CAD;
                        comprobante.TipoCambio = ccps.cteReceptorTipoCambio.Value;
                        comprobante.TipoCambioSpecified = true;
                        break;
                    case "EUR":
                        comprobante.Moneda = c_Moneda.EUR;
                        comprobante.TipoCambio = ccps.cteReceptorTipoCambio.Value;
                        comprobante.TipoCambioSpecified = true;
                        break;
                    default:
                        comprobante.Moneda = c_Moneda.MXN;
                        //comprobante.TipoCambio = 1;
                        comprobante.TipoCambioSpecified = false;
                        break;
                }

                comprobante.TipoDeComprobante = BuzonE.c_TipoDeComprobante.I;
                comprobante.LugarExpedicion = ccps.cteEmisorCp;
                comprobante.Exportacion = BuzonE.c_Exportacion.Item01;
                comprobante.Folio = ccps.num_guia;

                //EMISOR
                var emisor = new BuzonE.ComprobanteEmisor();
                emisor.Rfc = ccps.cteEmisorRfc;
                emisor.Nombre = ccps.cteEmisorNombre;
                emisor.RegimenFiscal = (c_RegimenFiscal)Enum.Parse(typeof(c_RegimenFiscal),
                    "Item" +
                    ccps.cteEmisorRegimenFiscal);
                comprobante.Emisor = emisor;


                var receptor = new BuzonE.ComprobanteReceptor();
                //RECEPTOR
                if (ccps.cteReceptorRfc == "XAXX010101000" || ccps.cteReceptorRfc == "XEXX010101000")
                {
                    receptor.Rfc = ccps.cteReceptorRfc;
                    receptor.Nombre = ccps.cteReceptorNombre;
                    receptor.DomicilioFiscalReceptor = ccps.cteEmisorCp;
                    receptor.RegimenFiscalReceptor = c_RegimenFiscal.Item616;
                    receptor.UsoCFDI = BuzonE.c_UsoCFDI.S01;
                    comprobante.Receptor = receptor;
                }
                else
                {
                    receptor.Rfc = ccps.cteReceptorRfc;
                    receptor.Nombre = ccps.cteReceptorNombre;
                    receptor.DomicilioFiscalReceptor = ccps.cteReceptorCp;
                    receptor.RegimenFiscalReceptor = (c_RegimenFiscal)Enum.Parse(typeof(c_RegimenFiscal),
                        "Item" +
                        ccps.cteReceptorRegimenFiscal);
                    receptor.UsoCFDI = (BuzonE.c_UsoCFDI)Enum.Parse(typeof(BuzonE.c_UsoCFDI), ccps.cteReceptorUsoCFDI == "P01" ? "G03" : ccps.cteReceptorUsoCFDI);
                    comprobante.Receptor = receptor;
                }

                List<BuzonE.ComprobanteAddenda> addendas = new List<BuzonE.ComprobanteAddenda>();

                BuzonE.ComprobanteAddenda addenda = new BuzonE.ComprobanteAddenda();

                addenda.clave = "AP-GENERICA";

                var partes = new List<string>();

                if (!string.IsNullOrEmpty(ccps.cteEmisorCalle))
                    partes.Add($"{ccps.cteEmisorCalle} No.");

                if (!string.IsNullOrEmpty(ccps.cteEmisorNoExterior))
                    partes.Add($"{ccps.cteEmisorNoExterior} COL.");

                if (!string.IsNullOrEmpty(ccps.cteEmisorColonia))
                    partes.Add(ccps.cteEmisorColonia);

                if (!string.IsNullOrEmpty(ccps.cteEmisorLocalidad))
                    partes.Add($"{ccps.cteEmisorLocalidad} NUEVO LEON,");

                if (!string.IsNullOrEmpty(ccps.cteEmisorPais))
                    partes.Add($"{ccps.cteEmisorPais} CP.");

                if (!string.IsNullOrEmpty(ccps.cteEmisorCp))
                    partes.Add(ccps.cteEmisorCp);

                string domicilioEmisorAddenda = string.Join(" ", partes);

                string noIdentificador = string.Empty;
                if (ccps.cartaPorteDetalles.Any())
                {
                    noIdentificador = ccps.cartaPorteDetalles.FirstOrDefault().cpeNoIdentificador == null ? "" : ccps.cartaPorteDetalles.FirstOrDefault().cpeNoIdentificador;
                }

                //addenda.Value = "<InformacionAdicional xmlns=\"http://www.buzone.com.mx/XSD/ParserGenerico/Generico\" xsi:schemaLocation=\"http://www.buzone.com.mx/XSD/ParserGenerico/Generico schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">   <Conector>string</Conector>   <IdUnico>string</IdUnico>   <CadenaOriginal>string</CadenaOriginal>   <IdDoc>string</IdDoc>   <SenderID>string</SenderID>   <CFDI tipoDocumento=\"string\" addendaCode=\"string\" complementoCode=\"string\" numeroDocumentos=\"string\" etiqueta1C=\"string\" valor1C=\"string\" etiqueta2C=\"string\" valor2C=\"string\" etiqueta3C=\"string\" valor3C=\"string\" etiqueta4C=\"string\" valor4C=\"string\" etiqueta5C=\"string\" valor5C=\"string\" etiqueta6C=\"string\" valor6C=\"string\" etiqueta7C=\"string\" valor7C=\"string\" etiqueta8C=\"string\" valor8C=\"string\" etiqueta9C=\"string\" valor9C=\"string\" etiqueta10C=\"string\" valor10C=\"string\" />   <lineaY titulo=\"string\" comentario=\"string\" />   <EMSR calle=\"string\" noExterior=\"string\" noInterior=\"string\" colonia=\"string\" localidad=\"string\" referencia=\"string\" municipio=\"string\" estado=\"string\" pais=\"string\" codigoPostal=\"string\" etiqueta1E=\"string\" valor1E=\"string\" etiqueta2E=\"string\" valor2E=\"string\" etiqueta3E=\"string\" valor3E=\"string\" etiqueta4E=\"string\" valor4E=\"string\" etiqueta5E=\"string\" valor5E=\"string\" etiqueta6E=\"string\" valor6E=\"string\" etiqueta7E=\"string\" valor7E=\"string\" etiqueta8E=\"string\" valor8E=\"string\" etiqueta9E=\"string\" valor9E=\"string\" etiqueta10E=\"string\" valor10E=\"string\" />   <EXP calle=\"string\" noExterior=\"string\" noInterior=\"string\" colonia=\"string\" localidad=\"string\" referencia=\"string\" municipio=\"string\" estado=\"string\" pais=\"string\" codigoPostal=\"string\" etiqueta1EX=\"string\" valor1EX=\"string\" etiqueta2EX=\"string\" valor2EX=\"string\" etiqueta3EX=\"string\" valor3EX=\"string\" etiqueta4EX=\"string\" valor4EX=\"string\" etiqueta5EX=\"string\" valor5EX=\"string\" etiqueta6EX=\"string\" valor6EX=\"string\" etiqueta7EX=\"string\" valor7EX=\"string\" etiqueta8EX=\"string\" valor8EX=\"string\" etiqueta9EX=\"string\" valor9EX=\"string\" etiqueta10EX=\"string\" valor10EX=\"string\" />   <R calle=\"string\" noExterior=\"string\" noInterior=\"string\" colonia=\"string\" localidad=\"string\" referencia=\"string\" municipio=\"string\" estado=\"string\" pais=\"string\" codigoPostal=\"string\" conector=\"string\" etiqueta1R=\"string\" valor1R=\"string\" etiqueta2R=\"string\" valor2R=\"string\" etiqueta3R=\"string\" valor3R=\"string\" etiqueta4R=\"string\" valor4R=\"string\" etiqueta5R=\"string\" valor5R=\"string\" etiqueta6R=\"string\" valor6R=\"string\" etiqueta7R=\"string\" valor7R=\"string\" etiqueta8R=\"string\" valor8R=\"string\" etiqueta9R=\"string\" valor9R=\"string\" etiqueta10R=\"string\" valor10R=\"string\" etiqueta11R=\"string\" valor11R=\"string\" etiqueta12R=\"string\" valor12R=\"string\" etiqueta13R=\"string\" valor13R=\"string\" etiqueta14R=\"string\" valor14R=\"string\" etiqueta15R=\"string\" valor15R=\"string\" />   <PTDA idPartida=\"string\" partida=\"string\" etiqueta1P=\"string\" valor1P=\"string\" etiqueta2P=\"string\" valor2P=\"string\" etiqueta3P=\"string\" valor3P=\"string\" etiqueta4P=\"string\" valor4P=\"string\" etiqueta5P=\"string\" valor5P=\"string\" etiqueta6P=\"string\" valor6P=\"string\" etiqueta7P=\"string\" valor7P=\"string\" etiqueta8P=\"string\" valor8P=\"string\" etiqueta9P=\"string\" valor9P=\"string\" etiqueta10P=\"string\" valor10P=\"string\" etiqueta11P=\"string\" valor11P=\"string\" etiqueta12P=\"string\" valor12P=\"string\" etiqueta13P=\"string\" valor13P=\"string\" etiqueta14P=\"string\" valor14P=\"string\" etiqueta15P=\"string\" valor15P=\"string\" etiqueta16P=\"string\" valor16P=\"string\" etiqueta17P=\"string\" valor17P=\"string\" etiqueta18P=\"string\" valor18P=\"string\" etiqueta19P=\"string\" valor19P=\"string\" etiqueta20P=\"string\" valor20P=\"string\" etiqueta21P=\"string\" valor21P=\"string\" etiqueta22P=\"string\" valor22P=\"string\" etiqueta23P=\"string\" valor23P=\"string\" etiqueta24P=\"string\" valor24P=\"string\" etiqueta25P=\"string\" valor25P=\"string\" etiqueta26P=\"string\" valor26P=\"string\" etiqueta27P=\"string\" valor27P=\"string\" etiqueta28P=\"string\" valor28P=\"string\" etiqueta29P=\"string\" valor29P=\"string\" etiqueta30P=\"string\" valor30P=\"string\" etiqueta31P=\"string\" valor31P=\"string\" etiqueta32P=\"string\" valor32P=\"string\" etiqueta33P=\"string\" valor33P=\"string\" etiqueta34P=\"string\" valor34P=\"string\" etiqueta35P=\"string\" valor35P=\"string\" etiqueta36P=\"string\" valor36P=\"string\" etiqueta37P=\"string\" valor37P=\"string\" etiqueta38P=\"string\" valor38P=\"string\" etiqueta39P=\"string\" valor39P=\"string\" etiqueta40P=\"string\" valor40P=\"string\" etiqueta41P=\"string\" valor41P=\"string\" etiqueta42P=\"string\" valor42P=\"string\" etiqueta43P=\"string\" valor43P=\"string\" etiqueta44P=\"string\" valor44P=\"string\" etiqueta45P=\"string\" valor45P=\"string\" etiqueta46P=\"string\" valor46P=\"string\" etiqueta47P=\"string\" valor47P=\"string\" etiqueta48P=\"string\" valor48P=\"string\" etiqueta49P=\"string\" valor49P=\"string\" etiqueta50P=\"string\" valor50P=\"string\" />   <PTDA idPartida=\"string\" partida=\"string\" etiqueta1P=\"string\" valor1P=\"string\" etiqueta2P=\"string\" valor2P=\"string\" etiqueta3P=\"string\" valor3P=\"string\" etiqueta4P=\"string\" valor4P=\"string\" etiqueta5P=\"string\" valor5P=\"string\" etiqueta6P=\"string\" valor6P=\"string\" etiqueta7P=\"string\" valor7P=\"string\" etiqueta8P=\"string\" valor8P=\"string\" etiqueta9P=\"string\" valor9P=\"string\" etiqueta10P=\"string\" valor10P=\"string\" etiqueta11P=\"string\" valor11P=\"string\" etiqueta12P=\"string\" valor12P=\"string\" etiqueta13P=\"string\" valor13P=\"string\" etiqueta14P=\"string\" valor14P=\"string\" etiqueta15P=\"string\" valor15P=\"string\" etiqueta16P=\"string\" valor16P=\"string\" etiqueta17P=\"string\" valor17P=\"string\" etiqueta18P=\"string\" valor18P=\"string\" etiqueta19P=\"string\" valor19P=\"string\" etiqueta20P=\"string\" valor20P=\"string\" etiqueta21P=\"string\" valor21P=\"string\" etiqueta22P=\"string\" valor22P=\"string\" etiqueta23P=\"string\" valor23P=\"string\" etiqueta24P=\"string\" valor24P=\"string\" etiqueta25P=\"string\" valor25P=\"string\" etiqueta26P=\"string\" valor26P=\"string\" etiqueta27P=\"string\" valor27P=\"string\" etiqueta28P=\"string\" valor28P=\"string\" etiqueta29P=\"string\" valor29P=\"string\" etiqueta30P=\"string\" valor30P=\"string\" etiqueta31P=\"string\" valor31P=\"string\" etiqueta32P=\"string\" valor32P=\"string\" etiqueta33P=\"string\" valor33P=\"string\" etiqueta34P=\"string\" valor34P=\"string\" etiqueta35P=\"string\" valor35P=\"string\" etiqueta36P=\"string\" valor36P=\"string\" etiqueta37P=\"string\" valor37P=\"string\" etiqueta38P=\"string\" valor38P=\"string\" etiqueta39P=\"string\" valor39P=\"string\" etiqueta40P=\"string\" valor40P=\"string\" etiqueta41P=\"string\" valor41P=\"string\" etiqueta42P=\"string\" valor42P=\"string\" etiqueta43P=\"string\" valor43P=\"string\" etiqueta44P=\"string\" valor44P=\"string\" etiqueta45P=\"string\" valor45P=\"string\" etiqueta46P=\"string\" valor46P=\"string\" etiqueta47P=\"string\" valor47P=\"string\" etiqueta48P=\"string\" valor48P=\"string\" etiqueta49P=\"string\" valor49P=\"string\" etiqueta50P=\"string\" valor50P=\"string\" />   <PTDA idPartida=\"string\" partida=\"string\" etiqueta1P=\"string\" valor1P=\"string\" etiqueta2P=\"string\" valor2P=\"string\" etiqueta3P=\"string\" valor3P=\"string\" etiqueta4P=\"string\" valor4P=\"string\" etiqueta5P=\"string\" valor5P=\"string\" etiqueta6P=\"string\" valor6P=\"string\" etiqueta7P=\"string\" valor7P=\"string\" etiqueta8P=\"string\" valor8P=\"string\" etiqueta9P=\"string\" valor9P=\"string\" etiqueta10P=\"string\" valor10P=\"string\" etiqueta11P=\"string\" valor11P=\"string\" etiqueta12P=\"string\" valor12P=\"string\" etiqueta13P=\"string\" valor13P=\"string\" etiqueta14P=\"string\" valor14P=\"string\" etiqueta15P=\"string\" valor15P=\"string\" etiqueta16P=\"string\" valor16P=\"string\" etiqueta17P=\"string\" valor17P=\"string\" etiqueta18P=\"string\" valor18P=\"string\" etiqueta19P=\"string\" valor19P=\"string\" etiqueta20P=\"string\" valor20P=\"string\" etiqueta21P=\"string\" valor21P=\"string\" etiqueta22P=\"string\" valor22P=\"string\" etiqueta23P=\"string\" valor23P=\"string\" etiqueta24P=\"string\" valor24P=\"string\" etiqueta25P=\"string\" valor25P=\"string\" etiqueta26P=\"string\" valor26P=\"string\" etiqueta27P=\"string\" valor27P=\"string\" etiqueta28P=\"string\" valor28P=\"string\" etiqueta29P=\"string\" valor29P=\"string\" etiqueta30P=\"string\" valor30P=\"string\" etiqueta31P=\"string\" valor31P=\"string\" etiqueta32P=\"string\" valor32P=\"string\" etiqueta33P=\"string\" valor33P=\"string\" etiqueta34P=\"string\" valor34P=\"string\" etiqueta35P=\"string\" valor35P=\"string\" etiqueta36P=\"string\" valor36P=\"string\" etiqueta37P=\"string\" valor37P=\"string\" etiqueta38P=\"string\" valor38P=\"string\" etiqueta39P=\"string\" valor39P=\"string\" etiqueta40P=\"string\" valor40P=\"string\" etiqueta41P=\"string\" valor41P=\"string\" etiqueta42P=\"string\" valor42P=\"string\" etiqueta43P=\"string\" valor43P=\"string\" etiqueta44P=\"string\" valor44P=\"string\" etiqueta45P=\"string\" valor45P=\"string\" etiqueta46P=\"string\" valor46P=\"string\" etiqueta47P=\"string\" valor47P=\"string\" etiqueta48P=\"string\" valor48P=\"string\" etiqueta49P=\"string\" valor49P=\"string\" etiqueta50P=\"string\" valor50P=\"string\" />   <PTDAo etiqueta1PO=\"string\" valor1PO=\"string\" etiqueta2PO=\"string\" valor2PO=\"string\" etiqueta3PO=\"string\" valor3PO=\"string\" etiqueta4PO=\"string\" valor4PO=\"string\" etiqueta5PO=\"string\" valor5PO=\"string\" etiqueta6PO=\"string\" valor6PO=\"string\" etiqueta7PO=\"string\" valor7PO=\"string\" etiqueta8PO=\"string\" valor8PO=\"string\" etiqueta9PO=\"string\" valor9PO=\"string\" etiqueta10PO=\"string\" valor10PO=\"string\" etiqueta11PO=\"string\" valor11PO=\"string\" etiqueta12PO=\"string\" valor12PO=\"string\" etiqueta13PO=\"string\" valor13PO=\"string\" etiqueta14PO=\"string\" valor14PO=\"string\" etiqueta15PO=\"string\" valor15PO=\"string\" etiqueta16PO=\"string\" valor16PO=\"string\" etiqueta17PO=\"string\" valor17PO=\"string\" etiqueta18PO=\"string\" valor18PO=\"string\" etiqueta19PO=\"string\" valor19PO=\"string\" etiqueta20PO=\"string\" valor20PO=\"string\" etiqueta21PO=\"string\" valor21PO=\"string\" etiqueta22PO=\"string\" valor22PO=\"string\" etiqueta23PO=\"string\" valor23PO=\"string\" etiqueta24PO=\"string\" valor24PO=\"string\" etiqueta25PO=\"string\" valor25PO=\"string\" etiqueta26PO=\"string\" valor26PO=\"string\" etiqueta27PO=\"string\" valor27PO=\"string\" etiqueta28PO=\"string\" valor28PO=\"string\" etiqueta29PO=\"string\" valor29PO=\"string\" etiqueta30PO=\"string\" valor30PO=\"string\" etiqueta31PO=\"string\" valor31PO=\"string\" etiqueta32PO=\"string\" valor32PO=\"string\" etiqueta33PO=\"string\" valor33PO=\"string\" etiqueta34PO=\"string\" valor34PO=\"string\" etiqueta35PO=\"string\" valor35PO=\"string\" etiqueta36PO=\"string\" valor36PO=\"string\" etiqueta37PO=\"string\" valor37PO=\"string\" etiqueta38PO=\"string\" valor38PO=\"string\" etiqueta39PO=\"string\" valor39PO=\"string\" etiqueta40PO=\"string\" valor40PO=\"string\" etiqueta41PO=\"string\" valor41PO=\"string\" etiqueta42PO=\"string\" valor42PO=\"string\" etiqueta43PO=\"string\" valor43PO=\"string\" etiqueta44PO=\"string\" valor44PO=\"string\" etiqueta45PO=\"string\" valor45PO=\"string\" etiqueta46PO=\"string\" valor46PO=\"string\" etiqueta47PO=\"string\" valor47PO=\"string\" etiqueta48PO=\"string\" valor48PO=\"string\" etiqueta49PO=\"string\" valor49PO=\"string\" etiqueta50PO=\"string\" valor50PO=\"string\" />   <PTDAo etiqueta1PO=\"string\" valor1PO=\"string\" etiqueta2PO=\"string\" valor2PO=\"string\" etiqueta3PO=\"string\" valor3PO=\"string\" etiqueta4PO=\"string\" valor4PO=\"string\" etiqueta5PO=\"string\" valor5PO=\"string\" etiqueta6PO=\"string\" valor6PO=\"string\" etiqueta7PO=\"string\" valor7PO=\"string\" etiqueta8PO=\"string\" valor8PO=\"string\" etiqueta9PO=\"string\" valor9PO=\"string\" etiqueta10PO=\"string\" valor10PO=\"string\" etiqueta11PO=\"string\" valor11PO=\"string\" etiqueta12PO=\"string\" valor12PO=\"string\" etiqueta13PO=\"string\" valor13PO=\"string\" etiqueta14PO=\"string\" valor14PO=\"string\" etiqueta15PO=\"string\" valor15PO=\"string\" etiqueta16PO=\"string\" valor16PO=\"string\" etiqueta17PO=\"string\" valor17PO=\"string\" etiqueta18PO=\"string\" valor18PO=\"string\" etiqueta19PO=\"string\" valor19PO=\"string\" etiqueta20PO=\"string\" valor20PO=\"string\" etiqueta21PO=\"string\" valor21PO=\"string\" etiqueta22PO=\"string\" valor22PO=\"string\" etiqueta23PO=\"string\" valor23PO=\"string\" etiqueta24PO=\"string\" valor24PO=\"string\" etiqueta25PO=\"string\" valor25PO=\"string\" etiqueta26PO=\"string\" valor26PO=\"string\" etiqueta27PO=\"string\" valor27PO=\"string\" etiqueta28PO=\"string\" valor28PO=\"string\" etiqueta29PO=\"string\" valor29PO=\"string\" etiqueta30PO=\"string\" valor30PO=\"string\" etiqueta31PO=\"string\" valor31PO=\"string\" etiqueta32PO=\"string\" valor32PO=\"string\" etiqueta33PO=\"string\" valor33PO=\"string\" etiqueta34PO=\"string\" valor34PO=\"string\" etiqueta35PO=\"string\" valor35PO=\"string\" etiqueta36PO=\"string\" valor36PO=\"string\" etiqueta37PO=\"string\" valor37PO=\"string\" etiqueta38PO=\"string\" valor38PO=\"string\" etiqueta39PO=\"string\" valor39PO=\"string\" etiqueta40PO=\"string\" valor40PO=\"string\" etiqueta41PO=\"string\" valor41PO=\"string\" etiqueta42PO=\"string\" valor42PO=\"string\" etiqueta43PO=\"string\" valor43PO=\"string\" etiqueta44PO=\"string\" valor44PO=\"string\" etiqueta45PO=\"string\" valor45PO=\"string\" etiqueta46PO=\"string\" valor46PO=\"string\" etiqueta47PO=\"string\" valor47PO=\"string\" etiqueta48PO=\"string\" valor48PO=\"string\" etiqueta49PO=\"string\" valor49PO=\"string\" etiqueta50PO=\"string\" valor50PO=\"string\" />   <PTDAo etiqueta1PO=\"string\" valor1PO=\"string\" etiqueta2PO=\"string\" valor2PO=\"string\" etiqueta3PO=\"string\" valor3PO=\"string\" etiqueta4PO=\"string\" valor4PO=\"string\" etiqueta5PO=\"string\" valor5PO=\"string\" etiqueta6PO=\"string\" valor6PO=\"string\" etiqueta7PO=\"string\" valor7PO=\"string\" etiqueta8PO=\"string\" valor8PO=\"string\" etiqueta9PO=\"string\" valor9PO=\"string\" etiqueta10PO=\"string\" valor10PO=\"string\" etiqueta11PO=\"string\" valor11PO=\"string\" etiqueta12PO=\"string\" valor12PO=\"string\" etiqueta13PO=\"string\" valor13PO=\"string\" etiqueta14PO=\"string\" valor14PO=\"string\" etiqueta15PO=\"string\" valor15PO=\"string\" etiqueta16PO=\"string\" valor16PO=\"string\" etiqueta17PO=\"string\" valor17PO=\"string\" etiqueta18PO=\"string\" valor18PO=\"string\" etiqueta19PO=\"string\" valor19PO=\"string\" etiqueta20PO=\"string\" valor20PO=\"string\" etiqueta21PO=\"string\" valor21PO=\"string\" etiqueta22PO=\"string\" valor22PO=\"string\" etiqueta23PO=\"string\" valor23PO=\"string\" etiqueta24PO=\"string\" valor24PO=\"string\" etiqueta25PO=\"string\" valor25PO=\"string\" etiqueta26PO=\"string\" valor26PO=\"string\" etiqueta27PO=\"string\" valor27PO=\"string\" etiqueta28PO=\"string\" valor28PO=\"string\" etiqueta29PO=\"string\" valor29PO=\"string\" etiqueta30PO=\"string\" valor30PO=\"string\" etiqueta31PO=\"string\" valor31PO=\"string\" etiqueta32PO=\"string\" valor32PO=\"string\" etiqueta33PO=\"string\" valor33PO=\"string\" etiqueta34PO=\"string\" valor34PO=\"string\" etiqueta35PO=\"string\" valor35PO=\"string\" etiqueta36PO=\"string\" valor36PO=\"string\" etiqueta37PO=\"string\" valor37PO=\"string\" etiqueta38PO=\"string\" valor38PO=\"string\" etiqueta39PO=\"string\" valor39PO=\"string\" etiqueta40PO=\"string\" valor40PO=\"string\" etiqueta41PO=\"string\" valor41PO=\"string\" etiqueta42PO=\"string\" valor42PO=\"string\" etiqueta43PO=\"string\" valor43PO=\"string\" etiqueta44PO=\"string\" valor44PO=\"string\" etiqueta45PO=\"string\" valor45PO=\"string\" etiqueta46PO=\"string\" valor46PO=\"string\" etiqueta47PO=\"string\" valor47PO=\"string\" etiqueta48PO=\"string\" valor48PO=\"string\" etiqueta49PO=\"string\" valor49PO=\"string\" etiqueta50PO=\"string\" valor50PO=\"string\" />   <PTDAo etiqueta1PO=\"string\" valor1PO=\"string\" etiqueta2PO=\"string\" valor2PO=\"string\" etiqueta3PO=\"string\" valor3PO=\"string\" etiqueta4PO=\"string\" valor4PO=\"string\" etiqueta5PO=\"string\" valor5PO=\"string\" etiqueta6PO=\"string\" valor6PO=\"string\" etiqueta7PO=\"string\" valor7PO=\"string\" etiqueta8PO=\"string\" valor8PO=\"string\" etiqueta9PO=\"string\" valor9PO=\"string\" etiqueta10PO=\"string\" valor10PO=\"string\" etiqueta11PO=\"string\" valor11PO=\"string\" etiqueta12PO=\"string\" valor12PO=\"string\" etiqueta13PO=\"string\" valor13PO=\"string\" etiqueta14PO=\"string\" valor14PO=\"string\" etiqueta15PO=\"string\" valor15PO=\"string\" etiqueta16PO=\"string\" valor16PO=\"string\" etiqueta17PO=\"string\" valor17PO=\"string\" etiqueta18PO=\"string\" valor18PO=\"string\" etiqueta19PO=\"string\" valor19PO=\"string\" etiqueta20PO=\"string\" valor20PO=\"string\" etiqueta21PO=\"string\" valor21PO=\"string\" etiqueta22PO=\"string\" valor22PO=\"string\" etiqueta23PO=\"string\" valor23PO=\"string\" etiqueta24PO=\"string\" valor24PO=\"string\" etiqueta25PO=\"string\" valor25PO=\"string\" etiqueta26PO=\"string\" valor26PO=\"string\" etiqueta27PO=\"string\" valor27PO=\"string\" etiqueta28PO=\"string\" valor28PO=\"string\" etiqueta29PO=\"string\" valor29PO=\"string\" etiqueta30PO=\"string\" valor30PO=\"string\" etiqueta31PO=\"string\" valor31PO=\"string\" etiqueta32PO=\"string\" valor32PO=\"string\" etiqueta33PO=\"string\" valor33PO=\"string\" etiqueta34PO=\"string\" valor34PO=\"string\" etiqueta35PO=\"string\" valor35PO=\"string\" etiqueta36PO=\"string\" valor36PO=\"string\" etiqueta37PO=\"string\" valor37PO=\"string\" etiqueta38PO=\"string\" valor38PO=\"string\" etiqueta39PO=\"string\" valor39PO=\"string\" etiqueta40PO=\"string\" valor40PO=\"string\" etiqueta41PO=\"string\" valor41PO=\"string\" etiqueta42PO=\"string\" valor42PO=\"string\" etiqueta43PO=\"string\" valor43PO=\"string\" etiqueta44PO=\"string\" valor44PO=\"string\" etiqueta45PO=\"string\" valor45PO=\"string\" etiqueta46PO=\"string\" valor46PO=\"string\" etiqueta47PO=\"string\" valor47PO=\"string\" etiqueta48PO=\"string\" valor48PO=\"string\" etiqueta49PO=\"string\" valor49PO=\"string\" etiqueta50PO=\"string\" valor50PO=\"string\" />   <PTDAo etiqueta1PO=\"string\" valor1PO=\"string\" etiqueta2PO=\"string\" valor2PO=\"string\" etiqueta3PO=\"string\" valor3PO=\"string\" etiqueta4PO=\"string\" valor4PO=\"string\" etiqueta5PO=\"string\" valor5PO=\"string\" etiqueta6PO=\"string\" valor6PO=\"string\" etiqueta7PO=\"string\" valor7PO=\"string\" etiqueta8PO=\"string\" valor8PO=\"string\" etiqueta9PO=\"string\" valor9PO=\"string\" etiqueta10PO=\"string\" valor10PO=\"string\" etiqueta11PO=\"string\" valor11PO=\"string\" etiqueta12PO=\"string\" valor12PO=\"string\" etiqueta13PO=\"string\" valor13PO=\"string\" etiqueta14PO=\"string\" valor14PO=\"string\" etiqueta15PO=\"string\" valor15PO=\"string\" etiqueta16PO=\"string\" valor16PO=\"string\" etiqueta17PO=\"string\" valor17PO=\"string\" etiqueta18PO=\"string\" valor18PO=\"string\" etiqueta19PO=\"string\" valor19PO=\"string\" etiqueta20PO=\"string\" valor20PO=\"string\" etiqueta21PO=\"string\" valor21PO=\"string\" etiqueta22PO=\"string\" valor22PO=\"string\" etiqueta23PO=\"string\" valor23PO=\"string\" etiqueta24PO=\"string\" valor24PO=\"string\" etiqueta25PO=\"string\" valor25PO=\"string\" etiqueta26PO=\"string\" valor26PO=\"string\" etiqueta27PO=\"string\" valor27PO=\"string\" etiqueta28PO=\"string\" valor28PO=\"string\" etiqueta29PO=\"string\" valor29PO=\"string\" etiqueta30PO=\"string\" valor30PO=\"string\" etiqueta31PO=\"string\" valor31PO=\"string\" etiqueta32PO=\"string\" valor32PO=\"string\" etiqueta33PO=\"string\" valor33PO=\"string\" etiqueta34PO=\"string\" valor34PO=\"string\" etiqueta35PO=\"string\" valor35PO=\"string\" etiqueta36PO=\"string\" valor36PO=\"string\" etiqueta37PO=\"string\" valor37PO=\"string\" etiqueta38PO=\"string\" valor38PO=\"string\" etiqueta39PO=\"string\" valor39PO=\"string\" etiqueta40PO=\"string\" valor40PO=\"string\" etiqueta41PO=\"string\" valor41PO=\"string\" etiqueta42PO=\"string\" valor42PO=\"string\" etiqueta43PO=\"string\" valor43PO=\"string\" etiqueta44PO=\"string\" valor44PO=\"string\" etiqueta45PO=\"string\" valor45PO=\"string\" etiqueta46PO=\"string\" valor46PO=\"string\" etiqueta47PO=\"string\" valor47PO=\"string\" etiqueta48PO=\"string\" valor48PO=\"string\" etiqueta49PO=\"string\" valor49PO=\"string\" etiqueta50PO=\"string\" valor50PO=\"string\" />   <T etiqueta1T=\"string\" valor1T=\"string\" etiqueta2T=\"string\" valor2T=\"string\" etiqueta3T=\"string\" valor3T=\"string\" etiqueta4T=\"string\" valor4T=\"string\" etiqueta5T=\"string\" valor5T=\"string\" etiqueta6T=\"string\" valor6T=\"string\" etiqueta7T=\"string\" valor7T=\"string\" etiqueta8T=\"string\" valor8T=\"string\" etiqueta9T=\"string\" valor9T=\"string\" etiqueta10T=\"string\" valor10T=\"string\" etiqueta11T=\"string\" valor11T=\"string\" etiqueta12T=\"string\" valor12T=\"string\" etiqueta13T=\"string\" valor13T=\"string\" etiqueta14T=\"string\" valor14T=\"string\" etiqueta15T=\"string\" valor15T=\"string\" />   <OBS etiqueta1O=\"string\" valor1O=\"string\" etiqueta2O=\"string\" valor2O=\"string\" etiqueta3O=\"string\" valor3O=\"string\" etiqueta4O=\"string\" valor4O=\"string\" etiqueta5O=\"string\" valor5O=\"string\" etiqueta6O=\"string\" valor6O=\"string\" etiqueta7O=\"string\" valor7O=\"string\" etiqueta8O=\"string\" valor8O=\"string\" etiqueta9O=\"string\" valor9O=\"string\" etiqueta10O=\"string\" valor10O=\"string\" etiqueta11O=\"string\" valor11O=\"string\" etiqueta12O=\"string\" valor12O=\"string\" etiqueta13O=\"string\" valor13O=\"string\" etiqueta14O=\"string\" valor14O=\"string\" etiqueta15O=\"string\" valor15O=\"string\" etiqueta16O=\"string\" valor16O=\"string\" etiqueta17O=\"string\" valor17O=\"string\" etiqueta18O=\"string\" valor18O=\"string\" etiqueta19O=\"string\" valor19O=\"string\" etiqueta20O=\"string\" valor20O=\"string\" etiqueta21O=\"string\" valor21O=\"string\" etiqueta22O=\"string\" valor22O=\"string\" etiqueta23O=\"string\" valor23O=\"string\" etiqueta24O=\"string\" valor24O=\"string\" etiqueta25O=\"string\" valor25O=\"string\" etiqueta26O=\"string\" valor26O=\"string\" etiqueta27O=\"string\" valor27O=\"string\" etiqueta28O=\"string\" valor28O=\"string\" etiqueta29O=\"string\" valor29O=\"string\" etiqueta30O=\"string\" valor30O=\"string\" etiqueta31O=\"string\" valor31O=\"string\" etiqueta32O=\"string\" valor32O=\"string\" etiqueta33O=\"string\" valor33O=\"string\" etiqueta34O=\"string\" valor34O=\"string\" etiqueta35O=\"string\" valor35O=\"string\" etiqueta36O=\"string\" valor36O=\"string\" etiqueta37O=\"string\" valor37O=\"string\" etiqueta38O=\"string\" valor38O=\"string\" etiqueta39O=\"string\" valor39O=\"string\" etiqueta40O=\"string\" valor40O=\"string\" etiqueta41O=\"string\" valor41O=\"string\" etiqueta42O=\"string\" valor42O=\"string\" etiqueta43O=\"string\" valor43O=\"string\" etiqueta44O=\"string\" valor44O=\"string\" etiqueta45O=\"string\" valor45O=\"string\" etiqueta46O=\"string\" valor46O=\"string\" etiqueta47O=\"string\" valor47O=\"string\" etiqueta48O=\"string\" valor48O=\"string\" etiqueta49O=\"string\" valor49O=\"string\" etiqueta50O=\"string\" valor50O=\"string\" />   <Documento valorFE=\"string\" /> </InformacionAdicional>";
                addenda.Value = string.Format("<InformacionAdicional xmlns=\"http://www.buzone.com.mx/XSD/ParserGenerico/Generico\" xsi:schemaLocation=\"http://www.buzone.com.mx/XSD/ParserGenerico/Generico schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">   <Conector>string</Conector>   <IdUnico>string</IdUnico>   <CadenaOriginal>string</CadenaOriginal>   <IdDoc>string</IdDoc>   <SenderID>string</SenderID>   <CFDI />   <lineaY />   <EMSR />   <EXP />   <R />   <PTDA />   <PTDA />   <PTDA />   <PTDAo />   <PTDAo />   <PTDAo />   <PTDAo />   <PTDAo />   <T etiqueta8T=\"Caja:\" valor8T=\"{0}\" etiqueta9T=\"shipment:\" valor9T=\"{1}\" etiqueta10T=\"No Identificacion:\" valor10T=\"{2}\" etiqueta11T=\"Observaciones:\" valor11T =\"{3}\"  etiqueta12T=\"Dirección emisor:\" valor12T =\"{4}\" />   <OBS/>   <Documento/> </InformacionAdicional>",
                    ccps.idRemolque == null ? "" : ccps.idRemolque, ccps.shipment == null ? "" : ccps.shipment,
                    noIdentificador, string.IsNullOrEmpty(ccps.observacionesPedido) ? "SIN OBSERVACIONES" : ccps.observacionesPedido.Replace("\"", "'"), domicilioEmisorAddenda);

                addendas.Add(addenda);

                comprobante.Addendas = addendas.ToArray();

                //CONCEPTOS
                List<BuzonE.ComprobanteConcepto> listaConceptos = new List<BuzonE.ComprobanteConcepto>();
                List<BuzonE.ComprobanteImpuestosTraslado> listaTraslados = new List<BuzonE.ComprobanteImpuestosTraslado>();
                List<BuzonE.ComprobanteImpuestosRetencion> listaRetenciones = new List<BuzonE.ComprobanteImpuestosRetencion>();

                if (!ccps.cartaPorteDetalles.Any())
                {
                    return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = "No se encontraron conceptos de facturacion asociados a esta remision" };
                }

                foreach (var item in ccps.cartaPorteDetalles)
                {
                    var concepto = new BuzonE.ComprobanteConcepto();
                    concepto.ClaveProdServ = item.claveProdServ;
                    concepto.ClaveUnidad = c_ClaveUnidad.E48;
                    concepto.Cantidad = 1;
                    concepto.Descripcion = item.descripcion;
                    concepto.ValorUnitario = decimal.Round(item.importe.Value, 6);
                    concepto.Importe = decimal.Round(item.importe.Value, 6);
                    concepto.Unidad = "SERVICIO";
                    concepto.DescuentoSpecified = false;
                    concepto.Descuento = decimal.Parse("00.00");
                    concepto.ObjetoImp = (item.objetoImp ?? "").Equals("No Objeto")
                         ? BuzonE.c_ObjetoImp.Item01
                         : BuzonE.c_ObjetoImp.Item02;


                    concepto.NoIdentificacion = string.IsNullOrEmpty(item.cpeNoIdentificador) || string.IsNullOrWhiteSpace(item.cpeNoIdentificador) ? null : item.cpeNoIdentificador;


                    BuzonE.ComprobanteConceptoImpuestos ImpuestosConcepto = new BuzonE.ComprobanteConceptoImpuestos();

                    ImpuestosConcepto.Traslados = new BuzonE.ComprobanteConceptoImpuestosTraslado[]
                        {
                                new BuzonE.ComprobanteConceptoImpuestosTraslado()
                                {
                                    Base= decimal.Round(item.importe.Value,6),
                                    Impuesto= BuzonE.c_Impuesto.Item002,
                                    TipoFactor= BuzonE.c_TipoFactor.Tasa,
                                    TasaOCuota= decimal.Round(item.factorIva.Value,6),
                                    Importe= decimal.Round(item.importe.Value * item.factorIva.Value,6) ,
                                    ImporteSpecified = true,
                                    TasaOCuotaSpecified = true
                                }
                        };

                    if (item.factorRetencion > 0)
                    {
                        ImpuestosConcepto.Retenciones = new BuzonE.ComprobanteConceptoImpuestosRetencion[]
                        {
                                new BuzonE.ComprobanteConceptoImpuestosRetencion(){
                                    Base = decimal.Round(item.importe.Value,6),
                                    Impuesto = BuzonE.c_Impuesto.Item002,
                                    TipoFactor = BuzonE.c_TipoFactor.Tasa,
                                    TasaOCuota = decimal.Round(item.factorRetencion.Value,6),
                                    Importe = decimal.Round(item.importe.Value * item.factorRetencion.Value,6)
                                }
                        };
                    }

                    concepto.Impuestos = ImpuestosConcepto;

                    listaConceptos.Add(concepto);

                }

                BuzonE.ComprobanteImpuestos ComprobanteImpuestos = new BuzonE.ComprobanteImpuestos();

                var retencionesGenerales = listaConceptos.Where(x => x.Impuestos.Retenciones != null).Sum(x => x.Impuestos.Retenciones.First().Importe);

                var trasladosGenerales = from concepto in listaConceptos
                                         group concepto by concepto.Impuestos.Traslados.ToList().FirstOrDefault().TasaOCuota into g
                                         select new BuzonE.ComprobanteConceptoImpuestosTraslado()
                                         {
                                             Base = g.Sum(x => x.Impuestos.Traslados.ToList().FirstOrDefault().Base),
                                             TasaOCuota = g.Key,
                                             Importe = g.Sum(x => x.Impuestos.Traslados.ToList().FirstOrDefault().Importe)
                                         };

                if (retencionesGenerales > 0)
                {
                    listaRetenciones.Add
                                    (
                                        new BuzonE.ComprobanteImpuestosRetencion()
                                        {
                                            Importe = decimal.Round(retencionesGenerales, 2),
                                            Impuesto = BuzonE.c_Impuesto.Item002
                                        }
                                    );

                    ComprobanteImpuestos.TotalImpuestosRetenidosSpecified = true;
                    ComprobanteImpuestos.TotalImpuestosRetenidos = decimal.Round(retencionesGenerales, 2);
                    ComprobanteImpuestos.Retenciones = listaRetenciones.ToArray();
                }
                else
                {
                    ComprobanteImpuestos.TotalImpuestosRetenidosSpecified = false;
                }


                foreach (var item in trasladosGenerales)
                {
                    listaTraslados.Add
                    (
                        new BuzonE.ComprobanteImpuestosTraslado()
                        {
                            Base = decimal.Round(item.Base, 2),
                            Impuesto = BuzonE.c_Impuesto.Item002,
                            TipoFactor = BuzonE.c_TipoFactor.Tasa,
                            TasaOCuota = item.TasaOCuota,
                            Importe = decimal.Round(item.Importe, 2),
                            ImporteSpecified = true,
                            TasaOCuotaSpecified = true
                        }
                    );
                }

                ComprobanteImpuestos.TotalImpuestosTrasladadosSpecified = true;
                ComprobanteImpuestos.TotalImpuestosTrasladados = decimal.Round(trasladosGenerales.Sum(x => x.Importe), 2);
                ComprobanteImpuestos.Traslados = listaTraslados.ToArray();

                comprobante.Impuestos = ComprobanteImpuestos;

                comprobante.SubTotal = decimal.Round(listaConceptos.Sum(x => x.Importe), 2);
                comprobante.Total = decimal.Round(listaConceptos.Sum(x => x.Importe) + comprobante.Impuestos.TotalImpuestosTrasladados - comprobante.Impuestos.TotalImpuestosRetenidos, 2);
                comprobante.DescuentoSpecified = false;
                comprobante.Descuento = decimal.Parse("00.000000");
                comprobante.Conceptos = listaConceptos.ToArray();


                //INTEGRACION DE COMPLEMENTO CARTA PORTE
                BuzonE.ComprobanteComplemento comprobanteComplemento = new BuzonE.ComprobanteComplemento();

                CartaPorte3 cartaPorte = new CartaPorte3();
                cartaPorte.Version = "3.1";
                cartaPorte.TranspInternac = ccps.esTransporteInternacional.Equals("No") ? CartaPorteTranspInternac.No : CartaPorteTranspInternac.Sí;

                if (cartaPorte.TranspInternac == CartaPorteTranspInternac.Sí)
                {
                    cartaPorte.EntradaSalidaMerc = ccps.entSalMercancia.Equals("Entrada") ? CartaPorteEntradaSalidaMerc.Entrada : CartaPorteEntradaSalidaMerc.Salida;
                    cartaPorte.EntradaSalidaMercSpecified = true;
                    cartaPorte.ViaEntradaSalida = "01";
                    cartaPorte.PaisOrigenDestino = c_Pais.USA.ToString();


                    //VERSION 3.1

                    List<BuzonE.CartaPorteRegimenAduaneroCCP> regimenesAduaneros = new List<BuzonE.CartaPorteRegimenAduaneroCCP>();

                    if (ccps.cartaPorteRegimenAduaneros.Count > 0)
                    {
                        foreach (var item in ccps.cartaPorteRegimenAduaneros)
                        {

                            BuzonE.CartaPorteRegimenAduaneroCCP regimenAduanero = new BuzonE.CartaPorteRegimenAduaneroCCP();

                            if ((item.regimenAduanero == null || item.regimenAduanero.Equals("")) && cartaPorte.EntradaSalidaMerc == CartaPorteEntradaSalidaMerc.Salida)
                            {
                                regimenAduanero.RegimenAduanero = "EXD";
                            }
                            else if (item.regimenAduanero == null || item.regimenAduanero.Equals("") && cartaPorte.EntradaSalidaMerc == CartaPorteEntradaSalidaMerc.Entrada)
                            {
                                regimenAduanero.RegimenAduanero = "IMD";
                            }
                            else
                            {
                                regimenAduanero.RegimenAduanero = item.regimenAduanero;
                            }

                            regimenesAduaneros.Add(regimenAduanero);
                        }
                    }
                    else
                    {
                        BuzonE.CartaPorteRegimenAduaneroCCP regimenAduanero = new BuzonE.CartaPorteRegimenAduaneroCCP();

                        if (cartaPorte.EntradaSalidaMerc == CartaPorteEntradaSalidaMerc.Salida)
                        {
                            regimenAduanero.RegimenAduanero = "EXD";
                        }
                        else if (cartaPorte.EntradaSalidaMerc == CartaPorteEntradaSalidaMerc.Entrada)
                        {
                            regimenAduanero.RegimenAduanero = "IMD";
                        }

                        regimenesAduaneros.Add(regimenAduanero);
                    }

                    cartaPorte.RegimenesAduaneros = regimenesAduaneros.ToArray();
                }

                //FIGURA TRANSPORTE
                List<CartaPorteTiposFigura1> figurasTransporte = new List<CartaPorteTiposFigura1>();

                figurasTransporte.Add
                (
                    new CartaPorteTiposFigura1()
                    {
                        TipoFigura = "01", //01 es operador, 02 es propietario, 03 es arrendador, 04 es notificado
                        NombreFigura = ccps.operador,
                        NumLicencia = ccps.licenciaOperador,
                        RFCFigura = ccps.rfcOperador.Trim()
                    }
                );

                //UBICACIONES   

                if (!ccps.cartaPorteUbicaciones.Any())
                {
                    return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = "No se encontraron datos de ubicacion origen y destino (archivo ccp dummy)" };
                }

                List<CartaPorteUbicacion1> ubicaciones = new List<CartaPorteUbicacion1>();

                foreach (var item in ccps.cartaPorteUbicaciones)
                {
                    BuzonE.CartaPorteUbicacion1 ubicacionOrigen = new BuzonE.CartaPorteUbicacion1();
                    BuzonE.CartaPorteUbicacion1 ubicacionDestino = new BuzonE.CartaPorteUbicacion1();

                    //UBICACION ORIGEN
                    if (!(string.IsNullOrWhiteSpace(item.remitenteResidenciaFiscal) || item.remitenteResidenciaFiscal.Equals("MEX")))
                    {
                        ubicacionOrigen.ResidenciaFiscal = "MEX";
                        ubicacionOrigen.NumRegIdTrib = string.IsNullOrWhiteSpace(item.remitenteNumRegIdTrib) ? null : item.remitenteNumRegIdTrib.Trim();
                    }

                    ubicacionOrigen.RFCRemitenteDestinatario = string.IsNullOrWhiteSpace(item.remitenteRfc) ? null : item.remitenteRfc.Trim();
                    ubicacionOrigen.TipoUbicacion = BuzonE.CartaPorteUbicacionTipoUbicacion.Origen;
                    ubicacionOrigen.IDUbicacion = string.IsNullOrWhiteSpace(item.remitenteId) ? null : item.remitenteId.Trim();
                    ubicacionOrigen.NombreRemitenteDestinatario = string.IsNullOrWhiteSpace(item.nombreRemitente) ? null : item.nombreRemitente.Trim();
                    ubicacionOrigen.FechaHoraSalidaLlegada = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

                    ubicacionOrigen.Domicilio = new CartaPorteUbicacionDomicilio1()
                    {
                        CodigoPostal = item.remitenteCp,
                        Estado = item.remitenteEstado,
                        Pais = item.remitentePais,
                        Municipio = item.remitenteMunicipio
                    };

                    ubicaciones.Add(ubicacionOrigen);

                    //UBICACION DESTINO
                    if (!string.IsNullOrWhiteSpace(item.destinatarioResidenciaFiscal))
                    {
                        if (!item.destinatarioPais.Equals("MEX"))
                        {
                            ubicacionDestino.ResidenciaFiscal = item.destinatarioResidenciaFiscal.Trim();
                            //ubicacionDestino.ResidenciaFiscalSpecified = true;

                            if (!string.IsNullOrWhiteSpace(item.destinatarioNumRegIdTrib))
                            {
                                ubicacionDestino.NumRegIdTrib = item.destinatarioNumRegIdTrib.Trim();
                            }

                        }
                    }

                    ubicacionDestino.RFCRemitenteDestinatario = string.IsNullOrWhiteSpace(item.destinatarioRfc) ? null : item.destinatarioRfc.Trim();
                    ubicacionDestino.TipoUbicacion = BuzonE.CartaPorteUbicacionTipoUbicacion.Destino;
                    ubicacionDestino.IDUbicacion = string.IsNullOrWhiteSpace(item.destinatarioId) ? null : item.destinatarioId.Trim();
                    ubicacionDestino.NombreRemitenteDestinatario = string.IsNullOrWhiteSpace(item.nombreDestinatario) ? null : item.nombreDestinatario.Trim();

                    ubicacionDestino.FechaHoraSalidaLlegada = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); //item.FechaArriboProgramado;  
                    ubicacionDestino.DistanciaRecorrida = item.distanciaRecorrida.Value;
                    ubicacionDestino.DistanciaRecorridaSpecified = true;

                    ubicacionDestino.Domicilio = new CartaPorteUbicacionDomicilio1()
                    {
                        CodigoPostal = item.destinatarioCp,
                        Estado = item.destinatarioEstado,
                        Pais = item.destinatarioPais,
                        Municipio = item.destinatarioMunicipio
                    };

                    ubicaciones.Add(ubicacionDestino);
                }

                //debe ser igual a la suma de "DistanciaRecorrida" de las ubicaciones
                cartaPorte.TotalDistRec = ubicaciones.Where(x => x.TipoUbicacion == BuzonE.CartaPorteUbicacionTipoUbicacion.Destino).FirstOrDefault().DistanciaRecorrida;
                cartaPorte.TotalDistRecSpecified = true;

                //MERCANCIAS            
                CartaPorteMercancias1 mercancias = new CartaPorteMercancias1();

                //Proceso de limpiado al subtiporemolque
                if (!string.IsNullOrWhiteSpace(ccps.subtipoRemolque1))
                {
                    Regex regex = new Regex("([CTR0-9]{6})");
                    Match match = regex.Match(ccps.subtipoRemolque1);

                    if (match.Success)
                    {
                        ccps.subtipoRemolque1 = match.Value;
                    }
                }

                if (!string.IsNullOrWhiteSpace(ccps.placaRemolque1))
                {
                    ccps.placaRemolque1 = Regex.Replace(ccps.placaRemolque1, "[^a-zA-Z0-9]", "");
                }
                else
                {
                    ccps.placaRemolque1 = "AAA000";
                }


                CartaPorteMercanciasAutotransporte2 autotransporte = new CartaPorteMercanciasAutotransporte2();

                CartaPorteMercanciasAutotransporteIdentificacionVehicular1 identificacionVehicular = new CartaPorteMercanciasAutotransporteIdentificacionVehicular1();
                identificacionVehicular.AnioModeloVM = Convert.ToInt32(string.IsNullOrEmpty(ccps.modeloUnidad) ? 2023 : ccps.modeloUnidad);
                identificacionVehicular.ConfigVehicular = ccps.configVehicular;
                identificacionVehicular.PlacaVM = Regex.Replace(ccps.placaUnidad, "[^a-zA-Z0-9]", "");
                //identificacionVehicular.PesoBrutoVehicular = ccps.PesoBrutoVehicular.ToString().Replace(",", ".");
                identificacionVehicular.PesoBrutoVehicular = ccps.pesoBrutoVehicular.Value;
                autotransporte.IdentificacionVehicular = identificacionVehicular;
                autotransporte.PermSCT = ccps.claveTipoPermiso;
                autotransporte.NumPermisoSCT = ccps.numTipoPermiso.ToString();

                CartaPorteMercanciasAutotransporteSeguros1 seguros = new CartaPorteMercanciasAutotransporteSeguros1();
                seguros.AseguraRespCivil = ccps.aseguradora;
                seguros.PolizaRespCivil = "M128209";

                foreach (var item in ccps.cartaPorteMercancia)
                {
                    if (!string.IsNullOrWhiteSpace(item.esMaterialPeligroso))
                    {
                        if (item.esMaterialPeligroso.Equals("1"))
                        {
                            seguros.AseguraMedAmbiente = ccps.aseguradora;
                            seguros.PolizaMedAmbiente = "M128209";
                            break;
                        }
                    }
                }

                autotransporte.Seguros = seguros;

                var configVehicularPermitidos = new HashSet<string>
                {
                    "VL", "C2", "C3", "OTROEVGP", "OTROSG",
                    "GPLUTA", "GPLUTB", "GPLUTC", "GPLUTD",
                    "GPLATA", "GPLATB", "GPLATC", "GPLATD"
                };

                if (!configVehicularPermitidos.Contains(identificacionVehicular.ConfigVehicular))
                {
                    autotransporte.Remolques = new CartaPorteMercanciasAutotransporteRemolque1[]
                     {
                            new  CartaPorteMercanciasAutotransporteRemolque1()
                            {
                                Placa = ccps.placaRemolque1,
                                SubTipoRem = ccps.subtipoRemolque1
                            }
                     };
                }

                #endregion parte1
                List<CartaPorteMercanciasMercancia1> listaMercancias = new List<CartaPorteMercanciasMercancia1>();

                foreach (var item in ccps.cartaPorteMercancia)
                {
                    var merca = new CartaPorteMercanciasMercancia1();
                    merca.Moneda = ccps.moneda;

                    if (cartaPorte.TranspInternac == CartaPorteTranspInternac.Sí)
                    {
                        if (!string.IsNullOrEmpty(item.fraccionArancelaria))
                        {
                            merca.FraccionArancelaria = Regex.Replace(item.fraccionArancelaria, "[^0-9]", "");
                        }

                        if (item.tipoMateria == null || item.tipoMateria == "")
                        {
                            item.tipoMateria = "03";
                        }

                        if (item.tipoMateria.Length < 2)
                        {
                            item.tipoMateria = item.tipoMateria.Insert(0, "0");
                        }

                        if (string.IsNullOrEmpty(item.tipoMateria))
                        {
                            item.tipoMateria = "05";
                            item.descripcionMateria = item.descripcionMateria;
                        }

                        merca.TipoMateria = item.tipoMateria;
                        if (item.tipoMateria.Equals("05"))
                        {
                            merca.DescripcionMateria = item.descripcionMateria;
                        }
                        merca.ValorMercancia = item.valorMercancia.ToString().Replace(",", ".");

                        if (!string.IsNullOrWhiteSpace(item.pedimento))
                        {
                            string pedimento = item.pedimento;
                            pedimento = Regex.Replace(pedimento, "[^0-9]", "");

                            if (pedimento.Length >= 15)
                            {
                                pedimento = pedimento.Substring(0, 2) + "  " + pedimento.Substring(2, 2) + "  " + pedimento.Substring(4, 4) + "  " + pedimento.Substring(8, 7);

                                item.pedimento = pedimento;
                            }
                        }

                        string tipoDocumento = cartaPorte.EntradaSalidaMerc == CartaPorteEntradaSalidaMerc.Salida ? "20" : "01";
                        string rfcImpo = ccps.rfcImpo == null ? "XAXX010101000" : ccps.rfcImpo;
                        var documentacionAduanera = new CartaPorteMercanciasMercanciaDocumentacionAduanera1()
                        {
                            TipoDocumento = tipoDocumento
                        };

                        if (cartaPorte.EntradaSalidaMerc == CartaPorteEntradaSalidaMerc.Entrada && tipoDocumento.Equals("01") && item.pedimento != null)
                        {
                            documentacionAduanera.NumPedimento = item.pedimento;
                            documentacionAduanera.RFCImpo = rfcImpo;
                        }

                        if (!tipoDocumento.Equals("01"))
                        {
                            Random random = new Random();
                            var codigo = random.Next(100000000, 1000000000);
                            string idecDocAduanero = string.Concat(DateTime.Now.Year, "-", codigo);
                            documentacionAduanera.IdentDocAduanero = idecDocAduanero;
                        }

                        merca.DocumentacionAduanera = new CartaPorteMercanciasMercanciaDocumentacionAduanera1[]
                        {
                            documentacionAduanera
                        };
                    }

                    merca.BienesTransp = item.claveProdServ == "1010101" ? "0" + item.claveProdServ.ToString() : item.claveProdServ.ToString();
                    merca.Descripcion = item.descripcion;
                    //merca.Cantidad = item.Cantidad.ToString().Replace(",", ".");
                    merca.Cantidad = item.cantidad;
                    mercancias.UnidadPeso = "KGM";

                    if (!string.IsNullOrEmpty(item.claveUnidad))
                    {
                        Regex regex = new Regex("([a-zA-Z0-9]{2,3})");
                        Match match = regex.Match(item.claveUnidad);

                        if (match.Success)
                        {
                            merca.ClaveUnidad = match.Value;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(item.esMaterialPeligroso))
                    {
                        if (item.esMaterialPeligroso.Equals("1"))
                        {
                            merca.CveMaterialPeligroso = item.cveMaterialPeligroso;
                        }
                        else if (item.esMaterialPeligroso.Equals("0,1"))
                        {
                            merca.MaterialPeligrosoSpecified = true;
                            merca.MaterialPeligroso = BuzonE.CartaPorteMercanciasMercanciaMaterialPeligroso.No;
                        }
                    }
                    //merca.PesoEnKg = item.Peso.ToString().Replace(",", ".");
                    merca.PesoEnKg = item.peso.Value;

                    if (ccps.cteReceptorId == 76 && database == "chdb_lis")
                    {
                        merca.ValorMercancia = "100";
                        merca.Moneda = ccps.moneda;
                    }
                    listaMercancias.Add(merca);
                }

                mercancias.Autotransporte = autotransporte;
                mercancias.Mercancia = listaMercancias.ToArray();

                //mercancias.NumTotalMercancias = ccps.cartaPorteMercancia.Count().ToString();
                //mercancias.PesoBrutoTotal = ccps.cartaPorteMercancia.Sum(x => x.Peso).ToString().Replace(",", ".");
                //mercancias.PesoNetoTotal = ccps.cartaPorteMercancia.Sum(x => x.Peso).ToString().Replace(",", ".");

                mercancias.NumTotalMercancias = ccps.cartaPorteMercancia.Count();
                mercancias.PesoBrutoTotal = ccps.cartaPorteMercancia.Sum(x => x.peso.Value);
                mercancias.PesoNetoTotal = ccps.cartaPorteMercancia.Sum(x => x.peso.Value);

                //AÑADIR A CARTA PORTE
                cartaPorte.FiguraTransporte = figurasTransporte.ToArray();
                cartaPorte.Ubicaciones = ubicaciones.ToArray();
                cartaPorte.Mercancias = mercancias;

                comprobanteComplemento.CartaPorte1 = cartaPorte;

                //FIN INTEGRACION DE COMPLEMENTO CARTA PORTE
                comprobante.Complemento = new BuzonE.ComprobanteComplemento[] { comprobanteComplemento };
                request.Comprobante = comprobante;

                return new UniqueRequest<RequestBE>() { request = request, IsSuccess = true, Mensaje = "Generacion de solicitud correcta" };
            }
            catch (Exception err)
            {
                return new UniqueRequest<RequestBE>() { IsSuccess = false, Mensaje = err.Message };
            }
        }


        public static string GenerateIdCCP()
        {
            // Genera un nuevo GUID.
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString("N"); // Obtiene el GUID sin guiones.

            // Formatea la cadena para cumplir con el patrón específico.
            return $"CCC{guidString.Substring(0, 5)}-{guidString.Substring(5, 4)}-{guidString.Substring(9, 4)}-{guidString.Substring(13, 4)}-{guidString.Substring(17, 12)}";
        }
    }
}
