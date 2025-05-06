# 📦 Proyecto - Consulta de Estado de Guías (Servientrega)

Este proyecto en C# (.NET 8) automatiza la consulta del estado de guías de Servientrega a través de su API SOAP. El flujo incluye la obtención de guías desde una base de datos MySQL, su consulta en lotes al servicio web, el análisis de la respuesta XML y la actualización de la fecha de entrega en la base de datos, si está disponible.

---

## 📁 Estructura del Proyecto

```
ProyectoServientrega/
│
├── Models/
│   └── GuiaInfo.cs               # Modelo de datos para representar la información de cada guía.
│
├── Services/
│   ├── MySqlService.cs           # Encapsula operaciones con la base de datos MySQL.
│   └── ServientregaClient.cs     # Se encarga del consumo del servicio SOAP y procesamiento de la respuesta.
│
├── Utils/
│   └── Loteador.cs               # Herramienta para dividir la lista de guías en lotes de 90.
│
├── Program.cs                    # Punto de entrada principal del programa.
├── appsettings.json              # Configuraciones externas: cadena de conexión y endpoint SOAP.
└── README.md                     # Documento actual con descripción del proyecto.
```

---

## ⚙️ Configuración

### appsettings.json

Este archivo permite configurar fácilmente parámetros del entorno:

```json
{
  "ConnectionStrings": {
    "MySqlConnection": "server=...;database=...;user=...;password=...;"
  },
  "Servientrega": {
    "SoapEndpoint": "http://sismilenio.servientrega.com.co/wsrastreoenvios/wsrastreoenvios.asmx"
  }
}
```

---

## 🚀 Flujo General

1. Se obtienen las guías válidas desde la base de datos o de forma manual en el `Program.cs`.
2. Se dividen en lotes de hasta 90 guías usando `Loteador.DividirEnLotes()`.
3. Se genera y envía una solicitud XML a la API SOAP de Servientrega por cada lote.
4. Se analiza la respuesta XML:
   - Si una guía tiene una **fecha de entrega**, se actualiza en la base de datos.
5. (Opcional) Las guías inválidas pueden exportarse a un archivo CSV para análisis.

---

## 🔍 Ejemplo de Uso (desde Program.cs)

```csharp
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

var config = builder.Build();

var conexion = config.GetConnectionString("MySqlConnection");
var endpoint = config["Servientrega:SoapEndpoint"];

var dbService = new MySqlService(conexion);


## 🧪 Clases Principales

### `MySqlService.cs`
- Se conecta a MySQL usando `MySql.Data.MySqlClient`.
- Ejecuta sentencias `SELECT` y `UPDATE` parametrizadas.
- Permite obtener guías y actualizar fechas de entrega.

### `ServientregaClient.cs`
- Genera el XML para consultar el estado de guías.
- Consume el servicio SOAP mediante `HttpClient` y `StringContent`.
- Analiza la respuesta XML usando `XmlDocument`.
- Extrae los nodos `<EstadoGuia>` y actualiza la base si corresponde.

### `Loteador.cs`
- Expone un método genérico `DividirEnLotes<T>()`.
- Evita sobrecargar el servicio consultando de a 90 guías por lote.

---

## ✅ Requisitos

- .NET 8 SDK
- Base de datos MySQL accesible y con tabla de guías
- Visual Studio 2022 o superior
- Acceso al endpoint SOAP de Servientrega

---

## 📄 Notas Finales

- Este proyecto está en versión inicial funcional.
- Se enfoca en eficiencia y automatización de la inserción de fechas de entrega.
- El envío de correos electrónicos fue descartado del alcance final.
- No se aplicaron sugerencias de refactorización para mantener el código original.

---

## 📍 Autor

Jefferson Diaz Torres
Desarrollado como parte de un reto profesional en el rol de **Desarrollador Junior**.  

