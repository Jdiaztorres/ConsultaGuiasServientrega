
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

            // ✅ VALIDADOR DE GUÍAS
            private List<GuiaInfo> FiltrarGuiasValidas(List<GuiaInfo> guias)
            {
                return guias
                    .Where(g =>
                        !string.IsNullOrWhiteSpace(g.NumeroGuia) &&
                        g.NumeroGuia.All(char.IsDigit) &&
                        g.NumeroGuia.Length >= 9 &&
                        g.NumeroGuia.Length <= 13 &&
                        !g.NumeroGuia.Distinct().Take(1).All(c => g.NumeroGuia.All(x => x == c)) // evita secuencias repetitivas como 222222222
                    )
                    .ToList();
            }

            public async Task ConsultarGuiasEstado(List<GuiaInfo> guias)
            {
                if (guias == null || guias.Count == 0)
                {
                    Console.WriteLine("⚠️ No hay guías para consultar en la API.");
                    return;
                }

                guias = guias.DistinctBy(g => g.NumeroGuia).ToList();

                int originales = guias.Count;
                guias = FiltrarGuiasValidas(guias);
                int filtradas = guias.Count;
                //Console.WriteLine($"🧹 Guías válidas: {filtradas} / {originales}");

                if (!guias.Any())
                {
                    Console.WriteLine("⚠️ No hay guías válidas para consultar.");
                    return;
                }

                var lotes = Loteador.DividirEnLotes(guias, 90);
                var alertas = new List<string>();

                foreach (var lote in lotes)
                {
                    try
                    {
                        Console.WriteLine($"📦 Enviando lote de {lote.Count} guías...");
                        var xml = ConstruirRelacionGuiasXml(lote);

                       // Console.WriteLine("📤 XML enviado a la API:");
                       // Console.WriteLine(xml);

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
                  //  Console.WriteLine("📋 Resumen de alertas:");
                  //  Console.WriteLine(string.Join("\n", alertas));
                }
            }

            // 🧱 CONSTRUCCIÓN DEL XML DE GUIAS
            private string ConstruirRelacionGuiasXml(List<GuiaInfo> guias)
            {
                var xml = new XElement("guias",
                    guias.Select(g =>
                        new XElement("guia",
                            new XElement("numero_guia", g.NumeroGuia)
                        )
                    )
                );

                return xml.ToString(SaveOptions.DisableFormatting);
            }

            // 📦 PROCESAMIENTO DE RESPUESTA XML Y ACTUALIZACIÓN EN BD
            private List<string> ProcesarRespuestaXml(string respuestaXml)
            {
                var alertas = new List<string>();

               // Console.WriteLine("📨 XML de respuesta recibido:");
              //  Console.WriteLine(respuestaXml);
               // Console.WriteLine("🔍 LONGITUD DEL XML ▶ " + respuestaXml.Length);

                XDocument doc;
                try
                {
                    doc = XDocument.Parse(respuestaXml);
                   // Console.WriteLine("📄 XML de respuesta formateado:");
                    //Console.WriteLine(doc.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al parsear XML: {ex.Message}");
                    alertas.Add("❌ Error al procesar XML.");
                    return alertas;
                }

                var resultados = doc.Descendants().Where(x => x.Name.LocalName == "EstadosGuias").ToList();
                Console.WriteLine($"🔎 Guías encontradas en respuesta: {resultados.Count}");

                foreach (var info in resultados)
                {
                    var guia = info.Elements().FirstOrDefault(e => e.Name.LocalName == "Guia")?.Value;
                    var fecha = info.Elements().FirstOrDefault(e => e.Name.LocalName == "Fecha_Entrega")?.Value;

                    Console.WriteLine($"📬 Guia: {guia} — FechaEntrega: {fecha}");

                //insertar datos sin modificar fecha 
                /* if (!string.IsNullOrWhiteSpace(fecha))
                 {
                    // Console.WriteLine($"✅ Guía {guia} entregada el {fecha}");
                     _db.ActualizarFechaLlegada(guia, fecha);
                 }*/
                if (!string.IsNullOrWhiteSpace(fecha))
                {
                    if (DateTime.TryParse(fecha, out var fechaLlegada))
                    {
                        // ⏰ Si la hora de llegada es posterior a las 15:00:00
                        if (fechaLlegada.TimeOfDay > new TimeSpan(15, 0, 0))
                        {
                            // Ajustar al día siguiente a las 09:00:00
                            fechaLlegada = fechaLlegada.Date.AddDays(1).AddHours(9);
                            fecha = fechaLlegada.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }

             //       _db.ActualizarFechaLlegada(guia, fecha);
                }


                else
                {
                        string mensaje = $"❓ Guía {guia} sin fecha de entrega.";
                      Console.WriteLine(mensaje);
                        alertas.Add(mensaje);
                    }
                }

                return alertas;
            }
        }
    }


