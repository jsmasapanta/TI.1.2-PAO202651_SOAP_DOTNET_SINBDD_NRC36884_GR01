# 04. SERVIDOR — Servicio SOAP de conversión de unidades

Servicio WCF en .NET Framework 4.7.2, sin base de datos.

## Estructura

```
CONUNI_SOAP_DOTNET_SERVIDOR/
├── Contratos/          lo que ven los clientes
│   ├── IWSConversion.cs    contrato SOAP: login + 16 conversiones
│   ├── ErrorServicio.cs    detalle del SOAP Fault (Codigo, Mensaje)
│   └── Constantes.cs       namespace XML: http://espe.edu.ec/conuni/
├── Servicios/
│   ├── WSConversion.svc.cs implementación: delega en Logica y convierte errores en Fault
│   └── WSConversionAsmx.asmx.cs  las mismas operaciones como servicio ASMX (página con formularios de prueba)
├── Logica/             reglas de negocio (no saben nada de SOAP)
│   ├── ConvertidorTemperatura.cs
│   ├── ConvertidorLongitud.cs
│   ├── ConvertidorMasa.cs
│   ├── Validacion.cs
│   ├── Autenticador.cs     SINGLETON: lee usuario/clave de Web.config y cuenta los 3 intentos (bloqueo 2 min)
│   └── ReglaNegocioException.cs
├── WSConversion.svc
├── WSConversion.asmx
└── Web.config          endpoint basicHttpBinding + credenciales
```

## Operaciones

Todas reciben un parámetro `valor` (double) y devuelven double.

| Categoría | Operaciones |
|---|---|
| Autenticación | `login(usuario, clave)` → true, o SOAP Fault `REGLA_NEGOCIO` con los intentos restantes (al tercero bloquea 2 minutos) |
| Temperatura | `celsiusAFahrenheit`, `fahrenheitACelsius`, `celsiusAKelvin`, `kelvinACelsius`, `fahrenheitAKelvin`, `kelvinAFahrenheit` |
| Longitud | `metrosAPies`, `piesAMetros`, `metrosAPulgadas`, `pulgadasAMetros`, `kilometrosAMillas`, `millasAKilometros` |
| Masa | `kilogramosALibras`, `librasAKilogramos`, `gramosAOnzas`, `onzasAGramos` |

**Errores:** temperatura bajo el cero absoluto, o longitud/masa negativa → SOAP Fault con `Codigo = VALOR_INVALIDO` y un mensaje en español.

**Credenciales** (en `Web.config`, sección `appSettings`): usuario `monster`, clave `monster69`. El usuario no distingue mayúsculas.

**Intentos:** el servidor cuenta los intentos fallidos de cada usuario. Responde "Usuario o clave incorrectos. Intentos restantes: N." y, al tercero, "Se superaron los 3 intentos. El usuario queda bloqueado 2 minutos.". Los clientes solo muestran ese mensaje.

## Ejecutar en Visual Studio

1. Abrir `CONUNI_SOAP_DOTNET_SERVIDOR.sln`.
2. F5. Se abre en IIS Express en `http://localhost:8085/WSConversion.svc`.
3. WSDL: `http://localhost:8085/WSConversion.svc?wsdl`.
4. Para probar a mano: *Herramientas → WCF Test Client* (si no aparece, instalar el componente "Windows Communication Foundation" desde el Visual Studio Installer).

## Publicar en IIS a mano (como lo pide el ingeniero)

1. Visual Studio **como administrador** → abrir el servidor → configuración **Release** → clic derecho al proyecto → **Publicar** → perfil **FolderProfile** (carpeta `C:\inetpub\conuni`) → **Publicar**.
2. Administrador de IIS → **Sitios** → **CONUNI** (puerto 8085, ruta `C:\inetpub\conuni`) → **Reiniciar**.
3. Probar `http://IP:8085/WSConversion.svc` y `http://IP:8085/WSConversion.asmx` desde otro equipo de la misma WiFi.
4. Después de publicar, no ejecutar el servidor en Visual Studio: el puerto 8085 ya lo usa IIS.

## Publicar en IIS (forma manual anterior)

1. Activar en Windows: *Características de Windows → .NET Framework 4.8 Advanced Services → Servicios WCF → Activación HTTP*.
2. Compilar en Release.
3. En el Administrador de IIS: *Sitios → Agregar sitio web*
   - Nombre: `CONUNI_SOAP`
   - Ruta física: la carpeta del proyecto (donde están `WSConversion.svc`, `Web.config` y `bin\`)
   - Puerto: `8085`
4. Abrir el puerto 8085 en el Firewall de Windows.
5. Probar desde otra máquina: `http://<IP-del-servidor>:8085/WSConversion.svc?wsdl`.
