# CLIMOV — Cliente móvil (.NET MAUI, sucesor de Xamarin.Forms)

Aplicación Android en C# que consume el servicio SOAP `WSConversion`. Se abre y ejecuta en **Visual Studio 2022**.

## Estructura (MVC + Singleton)

```
CONUNI_SOAP_DOTNET_CLIMOV.sln
Directory.Build.props        (obj en ruta corta: Android falla con rutas > 260 caracteres)
CONUNI_SOAP_DOTNET_CLIMOV/
├── Modelo/Catalogo.cs                    Categoria, Unidad, Conversion y las 16 conversiones
├── Servicio/ClienteSoap.cs               SOAP 1.1 con HttpClient · SINGLETON (ClienteSoap.Instancia)
├── Controlador/ControladorConversion.cs  login, conversión, formato, historial · SINGLETON
├── Vista/LoginPage.xaml                  CU01 login (3 intentos) + CU08 URL del servidor
├── Vista/PrincipalPage.xaml              CU02–CU07: categorías, De/A, intercambiar, historial, salir
└── Platforms/Android/AndroidManifest.xml permiso INTERNET + usesCleartextTraffic (HTTP sin TLS)
```

## Cómo funciona la llamada SOAP

1. Arma el sobre: `<s:Envelope><s:Body><celsiusAFahrenheit xmlns="http://espe.edu.ec/conuni/"><valor>100</valor></celsiusAFahrenheit></s:Body></s:Envelope>`
2. Lo envía por POST con la cabecera `SOAPAction: "http://espe.edu.ec/conuni/WSConversion/celsiusAFahrenheit"`.
3. Lee `<celsiusAFahrenheitResult>`; si llega un SOAP Fault muestra el `<Mensaje>` de `ErrorServicio`.

## Ejecutar en un celular real

1. Celular: Ajustes → Acerca del teléfono → tocar 7 veces **Número de compilación** → Opciones de desarrollador → activar **Depuración USB**.
2. Conectar por USB y aceptar "¿Permitir depuración USB?".
3. Visual Studio: abrir `CONUNI_SOAP_DOTNET_CLIMOV.sln`, elegir el celular en la lista de dispositivos y **Depurar → Iniciar sin depurar**.
4. En la app, campo **Servidor SOAP**:
   - Por USB (prueba rápida, con `adb reverse tcp:8085 tcp:8085`): `http://localhost:8085/WSConversion.svc`
   - Por WiFi (servidor publicado en IIS): `http://IP-DE-LA-PC:8085/WSConversion.svc`
   - Emulador: `http://10.0.2.2:8085/WSConversion.svc`
5. Usuario `monster`, clave `monster`.
