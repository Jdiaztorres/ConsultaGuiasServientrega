
using ProyectoServientrega.Models;
using ProyectoServientrega.Services;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Punto de entrada principal del programa.
/// Carga configuración, inicializa servicios y realiza una prueba de consulta de estado de guías a la API de Servientrega.
/// </summary>
class Program
{
    static async Task Main()
    {
        // ✅ Cargar configuración desde appsettings.json
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        var config = builder.Build();

        // 🔧 Extraer valores de conexión
        var conexion = config.GetConnectionString("MySqlConnection");
        var endpoint = config["Servientrega:SoapEndpoint"];

        // 🔌 Inicializar servicio de base de datos
        var dbService = new MySqlService(conexion);

         // 📦 Lista de ejemplo 
            var guiasValidas = new List<GuiaInfo>
            {
                new GuiaInfo { NumeroGuia = "2229199910" },
                new GuiaInfo { NumeroGuia = "2229205337" },
                new GuiaInfo { NumeroGuia = "2229206090" },

            };

            Console.WriteLine("🚀 Ejecutando prueba con 3 guías...");
            var api = new ServientregaClient(dbService, endpoint);
            await api.ConsultarGuiasEstado(guiasValidas);


        /* // 🔍 Obtener guías desde DB
         var (guiasValidas, guiasInvalidas) = dbService.ObtenerGuias();

              // 📤 Exportar inválidas
              File.WriteAllText("GuiasInvalidas.csv", "Guia,ODS,Envio,Llegada\n");
              foreach (var g in guiasInvalidas)
                  File.AppendAllText("GuiasInvalidas.csv", $"{g.NumeroGuia},{g.OrdenDeServicio},{g.FechaEnvio},{g.FechaLlegada}\n");

              Console.WriteLine($"✅ Guías válidas: {guiasValidas.Count}");
              Console.WriteLine($"⚠️ Guías inválidas: {guiasInvalidas.Count}");

              // 🚀 Ejecutar consulta a Servientrega
              var api = new ServientregaClient(dbService, endpoint);
              await api.ConsultarGuiasEstado(guiasValidas); */
    }
}
