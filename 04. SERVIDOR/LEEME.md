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
│   └── WSConversion.svc.cs implementación: delega en Logica y convierte errores en Fault
├── Logica/             reglas de negocio (no saben nada de SOAP)
│   ├── ConvertidorTemperatura.cs
│   ├── ConvertidorLongitud.cs
│   ├── ConvertidorMasa.cs
│   ├── Validacion.cs
│   └── Autenticador.cs     lee usuario/clave de Web.config
├── WSConversion.svc
└── Web.config          endpoint basicHttpBinding + credenciales
```

## Operaciones

Todas reciben un parámetro `valor` (double) y devuelven double.

| Categoría | Operaciones |
|---|---|
| Autenticación | `login(usuario, clave)` → bool |
| Temperatura | `celsiusAFahrenheit`, `fahrenheitACelsius`, `celsiusAKelvin`, `kelvinACelsius`, `fahrenheitAKelvin`, `kelvinAFahrenheit` |
| Longitud | `metrosAPies`, `piesAMetros`, `metrosAPulgadas`, `pulgadasAMetros`, `kilometrosAMillas`, `millasAKilometros` |
| Masa | `kilogramosALibras`, `librasAKilogramos`, `gramosAOnzas`, `onzasAGramos` |

**Errores:** temperatura bajo el cero absoluto, o longitud/masa negativa → SOAP Fault con `Codigo = VALOR_INVALIDO` y un mensaje en español.

**Credenciales** (en `Web.config`, sección `appSettings`): usuario `monster`, clave `monster`. El usuario no distingue mayúsculas.

## Ejecutar en Visual Studio

1. Abrir `CONUNI_SOAP_DOTNET_SERVIDOR.sln`.
2. F5. Se abre en IIS Express en `http://localhost:8085/WSConversion.svc`.
3. WSDL: `http://localhost:8085/WSConversion.svc?wsdl`.
4. Para probar a mano: *Herramientas → WCF Test Client* (si no aparece, instalar el componente "Windows Communication Foundation" desde el Visual Studio Installer).

## Publicar en IIS (para que los clientes se conecten por red)

1. Activar en Windows: *Características de Windows → .NET Framework 4.8 Advanced Services → Servicios WCF → Activación HTTP*.
2. Compilar en Release.
3. En el Administrador de IIS: *Sitios → Agregar sitio web*
   - Nombre: `CONUNI_SOAP`
   - Ruta física: la carpeta del proyecto (donde están `WSConversion.svc`, `Web.config` y `bin\`)
   - Puerto: `8085`
4. Abrir el puerto 8085 en el Firewall de Windows.
5. Probar desde otra máquina: `http://<IP-del-servidor>:8085/WSConversion.svc?wsdl`.
