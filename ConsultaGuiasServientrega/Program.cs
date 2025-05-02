
using ProyectoServientrega.Models;
using ProyectoServientrega.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main()
    {
        // ✅ Cargar configuración desde appsettings.json
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        var config = builder.Build();

        var conexion = config.GetConnectionString("MySqlConnection");
        var endpoint = config["Servientrega:SoapEndpoint"];

        var dbService = new MySqlService(conexion);

        // 🔍 Obtener guías desde DB
        var (guiasValidas, guiasInvalidas) = dbService.ObtenerGuias();

        // 📤 Exportar inválidas
        File.WriteAllText("GuiasInvalidas.csv", "Guia,ODS,Envio,Llegada\n");
        foreach (var g in guiasInvalidas)
            File.AppendAllText("GuiasInvalidas.csv", $"{g.NumeroGuia},{g.OrdenDeServicio},{g.FechaEnvio},{g.FechaLlegada}\n");

        Console.WriteLine($"✅ Guías válidas: {guiasValidas.Count}");
        Console.WriteLine($"⚠️ Guías inválidas: {guiasInvalidas.Count}");

        // 🚀 Ejecutar consulta a Servientrega
        var api = new ServientregaClient(dbService, endpoint);
        await api.ConsultarGuiasEstado(guiasValidas);
    }
}
