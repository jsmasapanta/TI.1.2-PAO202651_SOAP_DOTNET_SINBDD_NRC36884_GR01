# ===================================================================
#  Publica el servidor SOAP WSConversion en IIS (puerto 8085) para que
#  los demás integrantes (web, escritorio, móvil) se conecten por WiFi.
#
#  EJECUTAR COMO ADMINISTRADOR:
#    clic derecho en el menú Inicio > "Terminal (Administrador)" y luego:
#    powershell -ExecutionPolicy Bypass -File ".\publicar_iis.ps1"
#
#  Antes: compile el servidor en Visual Studio (Compilar > Compilar solución)
#  y CIERRE la ventana de Visual Studio del servidor (IIS Express usa el 8085).
# ===================================================================

$ErrorActionPreference = 'Stop'
$Puerto   = 8085
$Sitio    = 'CONUNI'
$Destino  = 'C:\inetpub\conuni'
$Proyecto = Join-Path $PSScriptRoot 'CONUNI_SOAP_DOTNET_SERVIDOR\CONUNI_SOAP_DOTNET_SERVIDOR'

function Paso($texto) { Write-Host "`n>> $texto" -ForegroundColor Cyan }

$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Abra la terminal COMO ADMINISTRADOR y vuelva a ejecutar el script.' }

if (-not (Test-Path (Join-Path $Proyecto 'bin\ConversorUnidades.Servidor.dll'))) {
    throw "No se encontró bin\ConversorUnidades.Servidor.dll. Compile primero el servidor en Visual Studio."
}

# 1. Características de Windows: IIS + ASP.NET 4.8 + activación HTTP de WCF (.svc)
Paso '1/5  Activando IIS, ASP.NET 4.8 y WCF (Activación HTTP)...'
$caracteristicas = 'IIS-WebServerRole','IIS-WebServer','IIS-ManagementConsole','IIS-ASPNET45',
                   'IIS-NetFxExtensibility45','IIS-ISAPIExtensions','IIS-ISAPIFilter',
                   'NetFx4Extended-ASPNET45','WCF-Services45','WCF-HTTP-Activation45'
foreach ($c in $caracteristicas) {
    $estado = (Get-WindowsOptionalFeature -Online -FeatureName $c).State
    if ($estado -ne 'Enabled') {
        Write-Host "   activando $c"
        Enable-WindowsOptionalFeature -Online -FeatureName $c -All -NoRestart | Out-Null
    }
}

# 2. Copiar solo lo necesario para ejecutar: .svc, Web.config y bin\
Paso "2/5  Copiando el servidor a $Destino..."
New-Item -ItemType Directory -Force -Path $Destino, "$Destino\bin" | Out-Null
Copy-Item (Join-Path $Proyecto 'WSConversion.svc') $Destino -Force
if (Test-Path (Join-Path $Proyecto 'WSConversion.asmx')) { Copy-Item (Join-Path $Proyecto 'WSConversion.asmx') $Destino -Force }
if (Test-Path (Join-Path $Proyecto 'Monitor.ashx')) { Copy-Item (Join-Path $Proyecto 'Monitor.ashx') $Destino -Force }
Copy-Item (Join-Path $Proyecto 'Web.config') $Destino -Force
Copy-Item (Join-Path $Proyecto 'bin\*') "$Destino\bin" -Recurse -Force

# 3. Grupo de aplicaciones (.NET CLR v4.0) y sitio en el puerto 8085
Paso "3/5  Creando el sitio '$Sitio' en el puerto $Puerto..."
Import-Module WebAdministration
if (-not (Test-Path "IIS:\AppPools\$Sitio")) {
    New-WebAppPool -Name $Sitio | Out-Null
}
Set-ItemProperty "IIS:\AppPools\$Sitio" -Name managedRuntimeVersion -Value 'v4.0'
if (Test-Path "IIS:\Sites\$Sitio") { Remove-Website -Name $Sitio }
New-Website -Name $Sitio -Port $Puerto -PhysicalPath $Destino -ApplicationPool $Sitio | Out-Null
icacls $Destino /grant "IIS AppPool\$($Sitio):(OI)(CI)RX" /T /Q | Out-Null
Start-Website -Name $Sitio

# 4. Firewall: permitir conexiones entrantes al puerto 8085 (redes públicas y privadas)
Paso "4/5  Abriendo el puerto $Puerto en el firewall..."
if (-not (Get-NetFirewallRule -DisplayName "CONUNI SOAP $Puerto" -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName "CONUNI SOAP $Puerto" -Direction Inbound -Protocol TCP `
        -LocalPort $Puerto -Action Allow -Profile Any | Out-Null
}

# 5. Comprobación
Paso '5/5  Comprobando...'
$ips = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notmatch '^(127|169)\.' -and $_.InterfaceAlias -match 'Wi-?Fi|Ethernet' }
try {
    $r = Invoke-WebRequest "http://localhost:$Puerto/WSConversion.svc?wsdl" -UseBasicParsing -TimeoutSec 30
    Write-Host "   WSDL OK (HTTP $($r.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   No respondió: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nListo. Los demás integrantes usan una de estas URL (la del adaptador Wi-Fi):" -ForegroundColor Green
foreach ($ip in $ips) { Write-Host ("   {0,-12} http://{1}:{2}/WSConversion.svc" -f $ip.InterfaceAlias, $ip.IPAddress, $Puerto) }
Write-Host "`nMonitor de consumo: http://localhost:$Puerto/Monitor.ashx" -ForegroundColor Green
