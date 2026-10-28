using System;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using ConversorUnidades.Servidor.Logica;

namespace ConversorUnidades.Servidor.Servicios
{
    /// <summary>
    /// Mide cada operación que llega por WSConversion.svc o WSConversion.asmx y la anota en MonitorServicio
    /// (IP del cliente, entrada, resultado o mensaje de error y tiempo). Los errores se vuelven a lanzar
    /// para que cada servicio los convierta en su SOAP Fault.
    /// </summary>
    internal static class Consumo
    {
        public static T Medir<T>(string tecnologia, string operacion, object entrada, Func<T> llamada)
        {
            var reloj = Stopwatch.StartNew();
            var registro = new RegistroLlamada
            {
                Hora = DateTime.Now,
                Cliente = IpCliente(),
                TipoCliente = TipoCliente(),
                Tecnologia = tecnologia,
                Operacion = operacion,
                Entrada = Convert.ToString(entrada, CultureInfo.InvariantCulture),
            };
            try
            {
                T resultado = llamada();
                registro.Exito = true;
                registro.Resultado = Convert.ToString(resultado, CultureInfo.InvariantCulture);
                return resultado;
            }
            catch (Exception ex)
            {
                registro.Exito = false;
                registro.Resultado = ex.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                throw;
            }
            finally
            {
                registro.Milisegundos = reloj.Elapsed.TotalMilliseconds;
                MonitorServicio.Instancia.Registrar(registro);
            }
        }

        // Cada cliente manda la cabecera HTTP "X-Cliente" (Consola, Escritorio, Web o Movil).
        // Sin cabecera: si viene de un navegador es la página de prueba del .asmx.
        private static string TipoCliente()
        {
            var peticion = HttpContext.Current?.Request;
            string tipo = peticion?.Headers["X-Cliente"];
            if (!string.IsNullOrWhiteSpace(tipo)) return tipo.Trim();
            return (peticion?.UserAgent ?? "").Contains("Mozilla") ? "Navegador" : "Otro";
        }

        // Web.config tiene aspNetCompatibilityEnabled="true", así que HttpContext existe también en WCF.
        private static string IpCliente()
        {
            string ip = HttpContext.Current?.Request.UserHostAddress ?? "desconocido";
            return ip == "::1" ? "127.0.0.1" : ip;
        }
    }
}
