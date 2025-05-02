
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ProyectoServientrega.Models;
using ProyectoServientrega.Utils;
using ServientregaApi.ServiceReference;

namespace ProyectoServientrega.Services
{
    
    public class ServientregaClient
    {
        private readonly string IdCliente = "830129426";
        private readonly MySqlService _db;
        private readonly string _soapEndpoint;

        public ServientregaClient(MySqlService db, string soapEndpoint)
        {
            _db = db;
            _soapEndpoint = soapEndpoint;
           
        }

        public async Task ConsultarGuiasEstado(List<GuiaInfo> guias)
        {
            if (guias == null || guias.Count == 0)
            {
                Console.WriteLine("⚠️ No hay guías para consultar en la API.");
                return;
            }

            // ✅ Eliminar duplicados
            guias = guias.DistinctBy(g => g.NumeroGuia).ToList();

            var lotes = Loteador.DividirEnLotes(guias, 100);
            var alertas = new List<string>();

            foreach (var lote in lotes)
            {
                try
                {
                    Console.WriteLine($"📦 Enviando lote de {lote.Count} guías...");
                    var xml = ConstruirRelacionGuiasXml(lote);

                    Console.WriteLine("📤 XML enviado a la API:");
                    Console.WriteLine(xml);

                    var binding = new System.ServiceModel.BasicHttpBinding();
                    var endpoint = new System.ServiceModel.EndpointAddress(_soapEndpoint);
                    var cliente = new wsRastreoEnviosSoapClient(binding, endpoint);

                    var resultado = await cliente.EstadoGuiasAsync(IdCliente, xml);
                    var respuestaXml = new XElement("GuiasRespuesta", resultado.Nodes).ToString();

                    var alertasLote = ProcesarRespuestaXml(respuestaXml);
                    alertas.AddRange(alertasLote);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al consultar API: {ex.Message}");
                }
            }

            if (alertas.Any())
            {
                Console.WriteLine("📋 Resumen de alertas:");
                Console.WriteLine(string.Join("\n", alertas));
            }

        }





        // 🧱 CONSTRUCCIÓN DEL XML DE GUIAS
        private string ConstruirRelacionGuiasXml(List<GuiaInfo> guias)
        {
            var xml = new XElement("Guias",
                guias.Select(g => new XElement("Guia", g.NumeroGuia))
            );
            return xml.ToString();
        }

        // 📦 PROCESAMIENTO DE RESPUESTA XML Y ACTUALIZACIÓN EN BD
        /*  private List<string> ProcesarRespuestaXml(string respuestaXml)
         {
             var alertas = new List<string>();

             Console.WriteLine("📨 XML de respuesta recibido:");
             Console.WriteLine(respuestaXml);
             Console.WriteLine("🔍 LONGITUD DEL XML ▶ " + respuestaXml.Length);

             var doc = XDocument.Parse(respuestaXml);
             var resultados = doc.Descendants().Where(x => x.Name.LocalName == "EstadosGuias");

             foreach (var info in resultados)
             {
                 var guia = info.Element(info.GetDefaultNamespace() + "Guia")?.Value;
                 var fecha = info.Element(info.GetDefaultNamespace() + "Fecha_Entrega")?.Value;
                 var novedad = info.Element(info.GetDefaultNamespace() + "Novedad")?.Value;

                 if (!string.IsNullOrWhiteSpace(fecha))
                 {
                     Console.WriteLine($"✅ Guía {guia} entregada el {fecha}");
                     _db.ActualizarFechaLlegada(guia, fecha);
                 }
                 else if (!string.IsNullOrWhiteSpace(novedad))
                 {
                     string mensaje = $"📌 Guía {guia} sin entrega — Novedad: {novedad}";
                     Console.WriteLine(mensaje);
                     alertas.Add(mensaje);
                 }
                 else
                 {
                     string mensaje = $"❓ Guía {guia} sin entrega ni novedad.";
                     Console.WriteLine(mensaje);
                     alertas.Add(mensaje);
                 }
             }

             return alertas;
         } */

        // 📦 PROCESAMIENTO DE RESPUESTA XML Y ACTUALIZACIÓN EN BD
        private List<string> ProcesarRespuestaXml(string respuestaXml)
        {
            var alertas = new List<string>();

            Console.WriteLine("📨 XML de respuesta recibido:");
            Console.WriteLine(respuestaXml);
            Console.WriteLine("🔍 LONGITUD DEL XML ▶ " + respuestaXml.Length);

            var doc = XDocument.Parse(respuestaXml);
            var resultados = doc.Descendants().Where(x => x.Name.LocalName == "EstadosGuias").ToList();

            Console.WriteLine($"🔎 Guías encontradas en respuesta: {resultados.Count}");

            foreach (var info in resultados)
            {
                var guia = info.Element(info.GetDefaultNamespace() + "Guia")?.Value;
                var fecha = info.Element(info.GetDefaultNamespace() + "Fecha_Entrega")?.Value;
                var novedad = info.Element(info.GetDefaultNamespace() + "Novedad")?.Value;

                Console.WriteLine($"📬 Guia encontrada: {guia} — FechaEntrega: {fecha} — Novedad: {novedad}");

                if (!string.IsNullOrWhiteSpace(fecha))
                {
                    Console.WriteLine($"✅ Guía {guia} entregada el {fecha}");
                    _db.ActualizarFechaLlegada(guia, fecha);
                }
                else if (!string.IsNullOrWhiteSpace(novedad))
                {
                    string mensaje = $"📌 Guía {guia} sin entrega — Novedad: {novedad}";
                    Console.WriteLine(mensaje);
                    alertas.Add(mensaje);
                }
                else
                {
                    string mensaje = $"❓ Guía {guia} sin entrega ni novedad.";
                    Console.WriteLine(mensaje);
                    alertas.Add(mensaje);
                }
            }

            return alertas;
        }

    }
}

