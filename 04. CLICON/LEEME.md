# 04. CLICON — Cliente de consola

Aplicación de consola en .NET Framework 4.7.2 que consume el servicio SOAP `WSConversion`.

## Estructura (MVC)

```
CONUNI_SOAP_DOTNET_CLICON/
├── Modelo/
│   ├── Conversion.cs            una conversión: categoría, unidades, operación del servicio
│   └── CatalogoConversiones.cs  las 16 conversiones agrupadas en Temperatura, Longitud y Masa
├── Servicio/
│   ├── IWSConversion.cs         copia del contrato del servidor + ErrorServicio
│   └── ClienteConversion.cs     ChannelFactory; traduce fallas a mensajes claros
├── Vista/
│   └── VistaConsola.cs          menús, lectura de datos, clave con asteriscos, resultados
├── Controlador/
│   └── ControladorConsola.cs    flujo: login (3 intentos) → categoría → conversión
├── Program.cs
└── App.config                   URL del servidor
```

## Cómo se conecta

No usa "Agregar referencia de servicio". El contrato está copiado en `Servicio/IWSConversion.cs`, con el mismo `Name` y `Namespace` que el servidor, y la conexión se abre con `ChannelFactory`, que toma la URL de `App.config`:

```xml
<endpoint name="WSConversion" address="http://localhost:8085/WSConversion.svc" ... />
```

Para conectarse a otra máquina basta cambiar esa dirección; no hay que recompilar. Después de compilar, el archivo editable es `bin\Debug\CONUNI_SOAP_DOTNET_CLICON.exe.config`.

## Funcionalidad

- Login contra el servidor, con la clave oculta con asteriscos y máximo 3 intentos.
- Menú de 3 categorías; cada una tiene sus conversiones (temperatura 6, longitud 6, masa 4).
- Acepta punto o coma decimal (`12.5` o `12,5`).
- Muestra el resultado con hasta 6 decimales.
- Mensajes claros cuando:
  - el valor es inválido (lo rechaza el servidor, por ejemplo -300 °C);
  - el servidor está apagado o la URL está mal;
  - el servidor tarda demasiado en responder.

## Ejecutar

1. Levantar el SERVIDOR (F5 en su solución, o IIS).
2. Abrir `CONUNI_SOAP_DOTNET_CLICON.sln` y presionar F5.
3. Usuario `monster`, clave `monster`.
