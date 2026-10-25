# 04. CLIWEB — Cliente web

Aplicación ASP.NET Core MVC (.NET 8) que consume el servicio SOAP `WSConversion`.

## Estructura (MVC)

```
CONUNI_SOAP_DOTNET_CLIWEB/
├── ec/edu/monster/
│   ├── controlador/
│   │   ├── CuentaController.cs        login (mensaje del servidor), logout
│   │   ├── ConversionController.cs    pantalla por categoría, conversión, historial
│   │   └── RequiereSesionAttribute.cs sin sesión → redirige al login
│   ├── modelo/
│   │   ├── Conversion.cs              Categoria, Conversion, Unidad (solo get/set)
│   │   ├── ErrorServicio.cs           detalle del SOAP Fault
│   │   └── ViewModels.cs              datos que reciben las vistas
│   ├── servicio/
│   │   ├── IWSConversion.cs           contrato ASÍNCRONO
│   │   └── ClienteConversion.cs       ChannelFactory (singleton) + mensaje del Fault
│   └── vista/
│       ├── Cuenta/Login.cshtml        con la imagen de Monsters Inc.
│       ├── Conversion/Index.cshtml    una sola vista para las 3 categorías (imagen en el menú)
│       ├── Shared/_Layout.cshtml
│       ├── CatalogoConversiones.cs    opciones de las listas
│       └── Formato.cs                 número y texto de la unidad
├── wwwroot/
│   ├── css/site.css                   diseño propio, adaptable a celular
│   ├── img/monster.jpg
│   └── js/conversion.js               lista "A" dinámica y botón ⇄
├── Program.cs                         sesión, Singleton y vistas en ec/edu/monster/vista
└── appsettings.json                   URL del servidor SOAP
```

## Cómo se conecta

Paquetes NuGet `System.ServiceModel.Http` y `System.ServiceModel.Primitives`. El contrato está en versión asíncrona (`Task<double> CelsiusAFahrenheitAsync(...)`) con el mismo `Name` de operación que el servidor, así que el SOAPAction coincide.

La URL está en `appsettings.json`:

```json
"ServicioSoap": { "Url": "http://192.168.100.3:8085/WSConversion.svc" }
```

## Funcionalidad

- **Login** contra el servidor: imagen de Monsters Inc., botón "Ver" para mostrar la clave y el mensaje del servidor (intentos restantes o bloqueo de 2 minutos).
- **Protección de páginas:** si alguien escribe `/Conversion` sin haber iniciado sesión, el sitio lo manda al login.
- **Menú lateral** con Temperatura, Longitud y Masa, cada una con su color (los mismos del cliente de escritorio).
- **Pantalla de conversión:**
  - Listas "De" y "A"; "A" se actualiza sola según "De".
  - Botón **⇄** para intercambiar unidades.
  - Acepta coma o punto decimal.
  - Resultado grande con detalle.
- **Historial** de las últimas 10 conversiones por categoría, guardado en la sesión, con botón para limpiarlo.
- **Mensajes de error claros:** número inválido, valor imposible (lo rechaza el servidor), servidor apagado.
- **Diseño adaptable:** en celular el menú pasa arriba y los campos se apilan.
- **Seguridad:** todos los formularios llevan token antifalsificación (`ValidateAntiForgeryToken`).

## Ejecutar

1. Levantar el SERVIDOR.
2. Abrir `CONUNI_SOAP_DOTNET_CLIWEB.sln` y presionar F5. Se abre `http://localhost:5085`.
3. Usuario `monster`, clave `monster69`.

Las capturas están en `05. DOCUMENTACIÓN\capturas\CLIWEB`.
