using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;


namespace HG.CFDI.SERVICE
{

    public class CFDIHandler
    {
        public string GenerarCadenaOriginalConXSLT(string xmlComprobante, string xsltPath)
        {
            // Cargar el documento XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlComprobante);

            // Cargar la transformación XSLT
            XslCompiledTransform transform = new XslCompiledTransform();

            // Configurar los ajustes para permitir documentos y scripts externos
            XsltSettings settings = new XsltSettings(enableDocumentFunction: true, enableScript: true);

            // Usar XmlUrlResolver para resolver referencias externas
            XmlUrlResolver resolver = new XmlUrlResolver();

            // Cargar el XSLT con los ajustes y el resolver
            transform.Load(xsltPath, settings, resolver);

            // Procesar la transformación
            using (StringWriter stringWriter = new StringWriter())
            {
                // Crear un XmlWriter con el ConformanceLevel establecido en Fragment
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment // Permite fragmentos de XML o texto
                };

                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
                {
                    transform.Transform(xmlDoc, xmlWriter);
                }

                // Devolver la cadena original generada
                return stringWriter.ToString();
            }
        }
        //funcion funcional para versiones recientes de .net 
        public string GeneraCadenaOriginal(string xmlString, string xsltPath)
        {
            XslCompiledTransform transform = new XslCompiledTransform();

            // Configurar los ajustes para permitir documentos y scripts externos
            XsltSettings settings = new XsltSettings(enableDocumentFunction: true, enableScript: true);

            // Usar XmlUrlResolver para resolver referencias externas
            XmlUrlResolver resolver = new XmlUrlResolver();

            // Cargar el XSLT con los ajustes y el resolver
            transform.Load(xsltPath, settings, resolver);

            // Usar StringReader para leer la cadena XML
            using (StringReader stringReader = new StringReader(xmlString))
            using (XmlReader xmlReader = XmlReader.Create(stringReader))
            {
                StringWriter cadenaOriginal = new StringWriter();
                transform.Transform(xmlReader, null, cadenaOriginal);
                return cadenaOriginal.ToString();
            }
        }

        public string ObtenerNumeroDeCertificado(string rutaCertificado, string contraseña)
        {
            X509Certificate2 certificado = new X509Certificate2(rutaCertificado, contraseña, X509KeyStorageFlags.Exportable);
            string Serie = certificado.SerialNumber;

            string Numero;
            string tNumero = "", rNumero = "", tNumero2 = "";

            int X;
            if (Serie.Length < 2)
                Numero = "";
            else
            {
                foreach (char c in Serie)
                {
                    switch (c)
                    {
                        case '0': tNumero += c; break;
                        case '1': tNumero += c; break;
                        case '2': tNumero += c; break;
                        case '3': tNumero += c; break;
                        case '4': tNumero += c; break;
                        case '5': tNumero += c; break;
                        case '6': tNumero += c; break;
                        case '7': tNumero += c; break;
                        case '8': tNumero += c; break;
                        case '9': tNumero += c; break;
                    }
                }
                for (X = 1; X < tNumero.Length; X++)
                {
                    X += 1;
                    tNumero2 = tNumero.Substring(0, X);
                    rNumero = rNumero + tNumero2.Substring(tNumero2.Length - 1, 1);
                }
                Numero = rNumero;
            }

            return Numero;
        }
    }

}
