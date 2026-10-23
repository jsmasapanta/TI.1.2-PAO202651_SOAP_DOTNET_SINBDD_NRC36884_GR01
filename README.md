# TI.1.2 · Conversor de Unidades — SOAP .NET sin BDD

Proyecto de **Arquitectura de Software (ESPE)**: un **servicio web SOAP** hecho en .NET (WCF) que convierte unidades de **temperatura, longitud y masa**, y **cuatro clientes** que lo consumen: **consola, escritorio, web y móvil**.

No usa base de datos: las conversiones se calculan con fórmulas y factores exactos, y el usuario y la clave están en el `Web.config` del servidor.

| | |
|---|---|
| **Usuario** | `monster` |
| **Clave** | `monster` |
| **Servidor SOAP** | `http://localhost:8085/WSConversion.svc` (WSDL: `?wsdl`) |
| **Cliente web** | `http://localhost:5085` |
| **IDE** | Visual Studio 2022 Community |
| **Patrones** | MVC en los clientes · Singleton en servidor y clientes |

---

## Índice

1. [Arquitectura](#1-arquitectura)
2. [Estructura del repositorio](#2-estructura-del-repositorio)
3. [Operaciones del servicio](#3-operaciones-del-servicio)
4. [Patrones de diseño: MVC y Singleton](#4-patrones-de-diseño-mvc-y-singleton)
5. [Requisitos previos](#5-requisitos-previos)
6. [Paso 1 — Servidor SOAP](#paso-1--servidor-soap-04-servidor)
7. [Paso 2 — Cliente consola](#paso-2--cliente-consola-04-clicon)
8. [Paso 3 — Cliente escritorio](#paso-3--cliente-escritorio-04-cliesc)
9. [Paso 4 — Cliente web](#paso-4--cliente-web-04-cliweb)
10. [Paso 5 — Cliente móvil](#paso-5--cliente-móvil-04-climov)
11. [Paso 6 — Trabajo en red (cada integrante en su máquina)](#paso-6--trabajo-en-red-cada-integrante-en-su-máquina)
12. [Casos de prueba](#casos-de-prueba)
13. [Solución de problemas](#solución-de-problemas)
14. [Documentación del proyecto](#documentación-del-proyecto)

---

## 1. Arquitectura

```
                         ┌──────────────────────────────────────────┐
  CLICON (consola)  ───► │                                          │
  CLIESC (escritorio) ─► │   Servidor IIS :8085                     │
  CLIWEB (navegador) ──► │   WSConversion.svc  (WCF · SOAP 1.1)     │
     └ ASP.NET MVC :5085 │     ├── Contratos   IWSConversion        │
  CLIMOV (Android) ────► │     ├── Servicios   WSConversion         │
                         │     └── Lógica      Convertidores,       │
          SOAP / HTTP    │                     Validación,          │
                         │                     Autenticador (Web.config)
                         └──────────────────────────────────────────┘
```

- **Protocolo:** SOAP 1.1 sobre HTTP (`basicHttpBinding`).
- **Namespace XML:** `http://espe.edu.ec/conuni/`.
- **Errores:** cuando un valor es físicamente imposible (por ejemplo, menos que el cero absoluto o una longitud negativa), el servidor responde con un **SOAP Fault** (`ErrorServicio` con `Codigo` y `Mensaje`), y cada cliente muestra ese mensaje en rojo.

## 2. Estructura del repositorio

```
01. UML/
    01. ERS/        Especificación de requisitos (IEEE 830)
    02. ECUD/       Especificación de casos de uso CU01–CU08
    03. DIAGRAMAS/  Casos de uso, actividad, secuencia, clases y arquitectura (.puml + .png)
02. MER/            No aplica (sin base de datos)
03. BDD/            No aplica (sin base de datos)
04. SERVIDOR/       Servicio WCF (.NET Framework 4.7.2)
04. CLICON/         Cliente consola (.NET Framework 4.7.2)
04. CLIESC/         Cliente escritorio Windows Forms (.NET Framework 4.7.2)
04. CLIWEB/         Cliente web ASP.NET Core MVC (.NET 8)
04. CLIMOV/         Cliente móvil Android .NET MAUI (sucesor de Xamarin.Forms, .NET 9)
05. DOCUMENTACIÓN/  Manual técnico (Word), presentación (PowerPoint) y capturas
```

Cada carpeta `04. …` tiene su propio `LEEME.md` con más detalle.

## 3. Operaciones del servicio

| Categoría | Operación SOAP | Fórmula / factor |
|---|---|---|
| Autenticación | `login(usuario, clave)` → `bool` | Compara con `Web.config` |
| Temperatura | `celsiusAFahrenheit`, `fahrenheitACelsius` | °F = °C × 9/5 + 32 |
| | `celsiusAKelvin`, `kelvinACelsius` | K = °C + 273,15 |
| | `fahrenheitAKelvin`, `kelvinAFahrenheit` | combinación de las anteriores |
| Longitud | `metrosAPies`, `piesAMetros` | 1 ft = 0,3048 m |
| | `metrosAPulgadas`, `pulgadasAMetros` | 1 in = 0,0254 m |
| | `kilometrosAMillas`, `millasAKilometros` | 1 mi = 1,609344 km |
| Masa | `kilogramosALibras`, `librasAKilogramos` | 1 lb = 0,45359237 kg |
| | `gramosAOnzas`, `onzasAGramos` | 1 oz = 28,349523125 g |

**Validaciones del servidor:**
- La temperatura no puede ser menor al cero absoluto: −273,15 °C, −459,67 °F o 0 K.
- La longitud y la masa no pueden ser negativas.

## 4. Patrones de diseño: MVC y Singleton

### MVC en los clientes

| Cliente | Modelo | Vista | Controlador | Servicio (acceso SOAP) |
|---|---|---|---|---|
| CLICON | `Modelo/Conversion.cs`, `CatalogoConversiones.cs` | `Vista/VistaConsola.cs` | `Controlador/ControladorConsola.cs` | `Servicio/ClienteConversion.cs` |
| CLIESC | `Modelo/CatalogoConversiones.cs` | `Vista/FormLogin`, `FormPrincipal`, `PanelConversion` | `Controlador/ControladorConversion.cs` | `Servicio/ClienteConversion.cs` |
| CLIWEB | `Modelo/Catalogo.cs`, `ViewModels.cs` | `Views/Cuenta`, `Views/Conversion` (.cshtml) | `Controllers/CuentaController`, `ConversionController` | `Servicio/ClienteConversion.cs` |
| CLIMOV | `Modelo/Catalogo.cs` | `Vista/LoginPage.xaml`, `PrincipalPage.xaml` | `Controlador/ControladorConversion.cs` | `Servicio/ClienteSoap.cs` |

El **servidor** está organizado en capas: `Contratos` (interfaz del servicio), `Servicios` (implementación) y `Logica` (cálculos, validación y autenticación).

### Singleton

Una sola instancia compartida por toda la aplicación. El constructor es privado y el acceso es por `Instancia`. Se usa `Lazy<T>` para que la creación sea segura entre hilos.

| Dónde | Clase | Cómo se usa |
|---|---|---|
| Servidor | `Logica/Autenticador.cs` | `Autenticador.Instancia.Validar(usuario, clave)`; lee `Web.config` una sola vez |
| CLICON | `Servicio/ClienteConversion.cs` | `ClienteConversion.Instancia` (una sola `ChannelFactory`) |
| CLIESC | `Servicio/ClienteConversion.cs` | `ClienteConversion.Instancia` |
| CLIWEB | `Servicio/ClienteConversion.cs` | `builder.Services.AddSingleton<ClienteConversion>()` en `Program.cs` (Singleton del contenedor de dependencias) |
| CLIMOV | `Servicio/ClienteSoap.cs` y `Controlador/ControladorConversion.cs` | `ClienteSoap.Instancia`, `ControladorConversion.Instancia` |

```csharp
public sealed class ClienteConversion
{
    private static readonly Lazy<ClienteConversion> _instancia =
        new Lazy<ClienteConversion>(() => new ClienteConversion());

    public static ClienteConversion Instancia => _instancia.Value;

    private ClienteConversion() { /* crea la ChannelFactory una sola vez */ }
}
```

## 5. Requisitos previos

| Herramienta | Para qué | Cómo instalar |
|---|---|---|
| **Visual Studio 2022 Community** | Abrir y ejecutar todo | Visual Studio Installer |
| ↳ carga *Desarrollo de ASP.NET y web* | Servidor y web | con el componente **Windows Communication Foundation** |
| ↳ carga *Desarrollo de escritorio de .NET* | Consola y escritorio | incluye .NET Framework 4.7.2 |
| ↳ carga *Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET (.NET MAUI)* | Móvil | incluye el SDK de Android y el JDK |
| **SDK de .NET 8 o 9** | Web y móvil | viene con Visual Studio |
| Celular o tablet Android 7.0+ | Móvil | con *Depuración USB* activada |

> **Importante:** la carpeta `C:\Program Files\dotnet\` debe estar en la variable **Path**. Si no está, la web y el móvil aparecen como **"(Descargado)"** en Visual Studio. Ver [Solución de problemas](#solución-de-problemas).

> **Regla de oro:** abre **cada solución en su propia ventana de Visual Studio**. Si abres otra solución en la ventana del servidor, Visual Studio cierra el servidor y los clientes ya no pueden conectarse.

---

## Paso 1 — Servidor SOAP (`04. SERVIDOR`)

1. Abre Visual Studio 2022 → **Abrir un proyecto o una solución** → `04. SERVIDOR\CONUNI_SOAP_DOTNET_SERVIDOR\CONUNI_SOAP_DOTNET_SERVIDOR.sln`.
2. En el **Explorador de soluciones**, haz clic derecho en `WSConversion.svc` → **Establecer como página principal**.
3. Comprueba que arriba diga **Debug** e **IIS Express**.
4. Ve a **Depurar → Iniciar sin depurar** (o `Ctrl + F5`; en laptops puede ser `Ctrl + Fn + F5`).
5. El navegador abre `http://localhost:8085/WSConversion.svc` y debe aparecer **"Servicio de WSConversion"**.
6. Para ver el contrato, entra a `http://localhost:8085/WSConversion.svc?wsdl`.
7. **Deja esa ventana de Visual Studio abierta.** Mientras esté abierta, el servidor sigue encendido (verás el ícono de IIS Express junto al reloj).

**Configuración** (`Web.config`):
```xml
<appSettings>
  <add key="UsuarioServicio" value="monster" />
  <add key="ClaveServicio" value="monster" />
</appSettings>
...
<serviceHostingEnvironment ... minFreeMemoryPercentageToActivateService="0" />
```
`minFreeMemoryPercentageToActivateService="0"` evita que WCF se niegue a arrancar cuando la PC tiene menos del 5 % de RAM libre (error 500 *"Error en la comprobación de las puertas de memoria"*).

## Paso 2 — Cliente consola (`04. CLICON`)

1. Con el servidor encendido, abre **otra ventana** de Visual Studio.
2. Abre `04. CLICON\CONUNI_SOAP_DOTNET_CLICON\CONUNI_SOAP_DOTNET_CLICON.sln`.
3. Ve a **Depurar → Iniciar sin depurar**.
4. En la ventana negra escribe Usuario `monster` y Clave `monster`. La clave se muestra con `*`.
5. Navega con los números del menú: **[1] Temperatura, [2] Longitud, [3] Masa → conversión → valor**. Con **[0]** vuelves o cierras sesión.

La URL del servidor está en `App.config`:
```xml
<endpoint address="http://localhost:8085/WSConversion.svc" ... name="WSConversion" />
```

## Paso 3 — Cliente escritorio (`04. CLIESC`)

1. En otra ventana de Visual Studio, abre `04. CLIESC\CONUNI_SOAP_DOTNET_CLIESC\CONUNI_SOAP_DOTNET_CLIESC.sln`.
2. Ve a **Depurar → Iniciar sin depurar**.
3. Ingresa con `monster` / `monster`.
4. En el menú verde de la izquierda elige **Temperatura**, **Longitud** o **Masa**.
5. Elige las unidades **De** y **A**, escribe el valor y presiona **CONVERTIR** (o Enter).
6. Con el botón **⇄** intercambias las unidades. A la derecha está el **Historial de la sesión**.
7. Abajo se muestra a qué servidor está conectado. La URL está en `App.config`, igual que en la consola.

## Paso 4 — Cliente web (`04. CLIWEB`)

1. En otra ventana de Visual Studio, abre `04. CLIWEB\CONUNI_SOAP_DOTNET_CLIWEB\CONUNI_SOAP_DOTNET_CLIWEB.sln`.
2. En el botón verde debe aparecer **CONUNI_SOAP_DOTNET_CLIWEB**. Si aparece "IIS Express" o "https", cámbialo con la flecha ▾.
3. Ve a **Depurar → Iniciar sin depurar**. La primera vez descarga los paquetes NuGet de WCF.
4. Si pregunta por un **certificado SSL de desarrollo**, responde **No**: el proyecto usa HTTP.
5. Se abre una **ventana negra** (servidor Kestrel), que no debes cerrar, y el navegador en `http://localhost:5085`.
6. Ingresa con `monster` / `monster` y convierte.
7. La página es *responsive*: también se ve bien en un celular.

La URL del servidor SOAP está en `appsettings.json`:
```json
"ServicioSoap": { "Url": "http://localhost:8085/WSConversion.svc" }
```

Seguridad incluida:
- Sesión con 3 intentos de login.
- Filtro `[RequiereSesion]`: sin sesión te devuelve al login.
- `ValidateAntiForgeryToken` en los formularios.

## Paso 5 — Cliente móvil (`04. CLIMOV`)

Aplicación **Android en C# con .NET MAUI**, el sucesor oficial de Xamarin.Forms; Xamarin dejó de tener soporte en mayo de 2024. Se ejecuta en un **celular o tablet real**.

### 5.1 Preparar el celular (una sola vez)
1. **Ajustes → Acerca del teléfono → Información de software** → toca **7 veces "Número de compilación"**.
2. **Ajustes → Opciones de desarrollador** → activa **Depuración USB**.
3. Conecta el celular por USB con un **cable de datos**. En el celular aparece **"¿Permitir depuración USB?"**: marca *Permitir siempre* y presiona **Permitir**.

### 5.2 Configurar Visual Studio (una sola vez)
**Herramientas → Opciones → Xamarin → Configuración de Android**:
- **SDK de Android:** `C:\Program Files (x86)\Android\android-sdk`
- **JDK:** un JDK 17, por ejemplo `C:\Program Files\Eclipse Adoptium\jdk-17.x` o el OpenJDK de Microsoft

Si aparece el **contrato de licencia de Android SDK**, lo aceptas solo si te faltan componentes.

### 5.3 Ejecutar
1. Con el servidor encendido, abre en otra ventana de Visual Studio `04. CLIMOV\CONUNI_SOAP_DOTNET_CLIMOV\CONUNI_SOAP_DOTNET_CLIMOV.sln`.
2. En la flecha ▾ del botón verde elige **Dispositivos Android locales → tu celular**.
3. Ve a **Depurar → Iniciar sin depurar**. La primera instalación tarda unos minutos.

### 5.4 Conectar la app con el servidor

El celular no conoce el `localhost` de tu PC. Hay dos formas de conectarlos.

**Opción A — Por cable USB (prueba rápida en tu PC).** Abre una terminal (PowerShell) y ejecuta:
```powershell
& "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" reverse tcp:8085 tcp:8085
```
En la app, en el cuadro **Servidor SOAP** (debajo del botón INGRESAR), escribe:
```
http://localhost:8085/WSConversion.svc
```
Si desconectas el cable, vuelve a ejecutar el comando `adb reverse`.

**Opción B — Por WiFi (presentación con roles en máquinas distintas).** Sigue el [Paso 6](#paso-6--trabajo-en-red-cada-integrante-en-su-máquina) y en la app escribe:
```
http://IP-DE-LA-PC-SERVIDOR:8085/WSConversion.svc
```

**Opción C — Emulador:** `http://10.0.2.2:8085/WSConversion.svc`. Es el valor por defecto de la app y **no funciona en un celular real**.

La app recuerda la última URL usada.

> **Ruta larga:** Android falla si la ruta de los archivos temporales supera 260 caracteres. Por eso `Directory.Build.props` envía la carpeta `obj` a `%LOCALAPPDATA%\CONUNI_CLIMOV\obj`. Si Visual Studio muestra el proyecto como "(Descargado)", borra la carpeta `obj` que esté junto al `.csproj` y recarga el proyecto.

---

## Paso 6 — Trabajo en red (cada integrante en su máquina)

Reparto de roles del grupo:

| Integrante | Rol | Qué ejecuta |
|---|---|---|
| 1 | Servidor + consola | Pasos 1, 2 y 6 en su PC |
| 2 | Web + escritorio | Pasos 3 y 4 en su PC, apuntando a la IP del integrante 1 |
| 3 | Móvil | Paso 5 en su celular real, apuntando a la IP del integrante 1 |

IIS Express **solo acepta conexiones desde la misma PC**. Para que los demás se conecten, el integrante 1 publica el servidor en **IIS**. Todos deben estar en **la misma red WiFi**.

1. **Activar IIS y WCF** (como administrador). En *Activar o desactivar las características de Windows* marca:
   - **Internet Information Services** → Servicios World Wide Web → Características de desarrollo de aplicaciones → **ASP.NET 4.8**.
   - **.NET Framework 4.8 Advanced Services** → Servicios WCF → **Activación HTTP**.
2. **Publicar el servidor.** En Visual Studio (como administrador), haz clic derecho en el proyecto del servidor → **Publicar** → **Carpeta** → por ejemplo `C:\inetpub\conuni` → **Publicar**.
3. **Crear el sitio.** En el **Administrador de IIS** → Sitios → **Agregar sitio web**:
   - Nombre `CONUNI`
   - Ruta física `C:\inetpub\conuni`
   - Puerto **8085**
   - Grupo de aplicaciones con **.NET CLR v4.0**
4. **Abrir el firewall** (PowerShell como administrador):
   ```powershell
   New-NetFirewallRule -DisplayName "CONUNI SOAP 8085" -Direction Inbound -Protocol TCP -LocalPort 8085 -Action Allow
   ```
5. **Averiguar la IP.** Ejecuta `ipconfig` y busca *Dirección IPv4* en el adaptador WiFi, por ejemplo `192.168.1.10`.
6. **Comprobar desde otra máquina o desde el celular:** abre `http://192.168.1.10:8085/WSConversion.svc` en el navegador.
7. **Apuntar cada cliente a esa IP:**
   - Consola y escritorio: en `App.config`, cambia `address="http://192.168.1.10:8085/WSConversion.svc"`.
   - Web: en `appsettings.json`, cambia `"Url": "http://192.168.1.10:8085/WSConversion.svc"`.
   - Móvil: en el cuadro **Servidor SOAP** de la app.

---

## Casos de prueba

| # | Caso | Entrada | Resultado esperado |
|---|---|---|---|
| 1 | Login correcto | monster / monster | Acceso al sistema |
| 2 | Login incorrecto | monster / otra | "Usuario o clave incorrectos" |
| 3 | 3 intentos fallidos | clave mala ×3 | Se bloquea / cierra |
| 4 | Celsius → Fahrenheit | 100 | 212 °F |
| 5 | Celsius → Kelvin | 25 | 298,15 K |
| 6 | Bajo el cero absoluto | −300 °C | SOAP Fault: *no puede ser menor al cero absoluto* |
| 7 | Kilómetros → Millas | 10 | 6,213712 mi |
| 8 | Millas → Kilómetros | 1 | 1,609344 km |
| 9 | Metros → Pies | 1 | 3,28084 ft |
| 10 | Longitud negativa | −5 m | SOAP Fault: valor inválido |
| 11 | Kilogramos → Libras | 1 | 2,204623 lb |
| 12 | Gramos → Onzas | 100 | 3,527396 oz |
| 13 | Valor no numérico | abc | "Ingrese un número válido" (no llama al servidor) |
| 14 | Coma decimal | 36,5 °C | 97,7 °F |
| 15 | Servidor apagado | — | "No se pudo conectar con el servidor…" |

## Solución de problemas

| Síntoma | Causa | Solución |
|---|---|---|
| Consola o escritorio: *"No se pudo conectar con el servidor"* | El servidor está apagado | Vuelve a ejecutar el servidor (Paso 1) y no abras otra solución en su ventana |
| Error 500 *"comprobación de las puertas de memoria"* | La PC tiene menos del 5 % de RAM libre | Ya está corregido en `Web.config`; también conviene cerrar programas |
| Web o móvil aparecen como **"(Descargado)"** | `C:\Program Files\dotnet\` no está en el **Path** | *Editar las variables de entorno de esta cuenta* → **Path** → Editar → Nuevo → `C:\Program Files\dotnet\` → súbelo al inicio → reinicia Visual Studio |
| Móvil "(Descargado)" con error de **260 caracteres** | Quedó una carpeta `obj` vieja junto al `.csproj` | Bórrala y recarga el proyecto |
| Móvil: *"El servidor tardó demasiado en responder"* | La app tiene la URL del emulador (`10.0.2.2`) | Cambia la URL en la app (Paso 5.4) |
| El celular no aparece en Visual Studio | No se aceptó la depuración USB | Desbloquea el celular, reconecta el cable y presiona **Permitir** |
| Visual Studio pide instalar un certificado SSL | Certificado de desarrollo de ASP.NET Core | Responde **No**: no hace falta |
| `Ctrl + F5` no hace nada | Teclado de laptop | Usa **Depurar → Iniciar sin depurar** o `Ctrl + Fn + F5` |

## Documentación del proyecto

- `01. UML/01. ERS` — Especificación de requisitos: 13 requisitos funcionales y 8 no funcionales.
- `01. UML/02. ECUD` — Especificación de los casos de uso CU01 a CU08.
- `01. UML/03. DIAGRAMAS` — Diagramas en PlantUML (`.puml`) con su imagen (`.png`).
- `05. DOCUMENTACIÓN/WS_CONUNI_SOAP_DOTNET_*.docx` — Manual técnico.
- `05. DOCUMENTACIÓN/MANUAL_SOAP_DOTNET_*.pptx` — Presentación.
- `05. DOCUMENTACIÓN/capturas` — Capturas de cada cliente.
