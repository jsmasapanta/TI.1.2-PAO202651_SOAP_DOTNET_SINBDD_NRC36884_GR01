using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using ConversorUnidades.Servidor.Contratos;
using ConversorUnidades.Servidor.Logica;

namespace ConversorUnidades.Servidor.Servicios
{
    /// <summary>
    /// Las mismas operaciones de WSConversion.svc publicadas también como servicio web ASMX (WSConversion.asmx).
    /// ASMX es la tecnología SOAP anterior a WCF; su página en el navegador lista las operaciones
    /// y permite probarlas con un formulario. Usa la misma lógica (Autenticador y Convertidor*).
    /// </summary>
    [WebService(Name = "WSConversion", Namespace = Constantes.NamespaceServicio,
        Description = "Conversor de unidades · SOAP .NET sin base de datos. Usuario de prueba: monster / monster69. Temperatura, longitud y masa.")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class WSConversionAsmx : WebService
    {
        [WebMethod(Description = "Devuelve true si usuario y clave son correctos; si no, responde un error con los intentos restantes (bloqueo de 2 minutos al tercero).")]
        public bool Login(string usuario, string clave) => Ejecutar(() => Autenticador.Instancia.Validar(usuario, clave));

        // ---------- Temperatura ----------
        [WebMethod(Description = "°C → °F (no menor a -273.15 °C).")]
        public double CelsiusAFahrenheit(double valor) => Ejecutar(() => ConvertidorTemperatura.CelsiusAFahrenheit(valor));
        [WebMethod(Description = "°F → °C (no menor a -459.67 °F).")]
        public double FahrenheitACelsius(double valor) => Ejecutar(() => ConvertidorTemperatura.FahrenheitACelsius(valor));
        [WebMethod(Description = "°C → K.")]
        public double CelsiusAKelvin(double valor) => Ejecutar(() => ConvertidorTemperatura.CelsiusAKelvin(valor));
        [WebMethod(Description = "K → °C (no menor a 0 K).")]
        public double KelvinACelsius(double valor) => Ejecutar(() => ConvertidorTemperatura.KelvinACelsius(valor));
        [WebMethod(Description = "°F → K.")]
        public double FahrenheitAKelvin(double valor) => Ejecutar(() => ConvertidorTemperatura.FahrenheitAKelvin(valor));
        [WebMethod(Description = "K → °F.")]
        public double KelvinAFahrenheit(double valor) => Ejecutar(() => ConvertidorTemperatura.KelvinAFahrenheit(valor));

        // ---------- Longitud ----------
        [WebMethod(Description = "m → ft (no negativo).")]
        public double MetrosAPies(double valor) => Ejecutar(() => ConvertidorLongitud.MetrosAPies(valor));
        [WebMethod(Description = "ft → m (no negativo).")]
        public double PiesAMetros(double valor) => Ejecutar(() => ConvertidorLongitud.PiesAMetros(valor));
        [WebMethod(Description = "m → in (no negativo).")]
        public double MetrosAPulgadas(double valor) => Ejecutar(() => ConvertidorLongitud.MetrosAPulgadas(valor));
        [WebMethod(Description = "in → m (no negativo).")]
        public double PulgadasAMetros(double valor) => Ejecutar(() => ConvertidorLongitud.PulgadasAMetros(valor));
        [WebMethod(Description = "km → mi (no negativo).")]
        public double KilometrosAMillas(double valor) => Ejecutar(() => ConvertidorLongitud.KilometrosAMillas(valor));
        [WebMethod(Description = "mi → km (no negativo).")]
        public double MillasAKilometros(double valor) => Ejecutar(() => ConvertidorLongitud.MillasAKilometros(valor));

        // ---------- Masa ----------
        [WebMethod(Description = "kg → lb (no negativo).")]
        public double KilogramosALibras(double valor) => Ejecutar(() => ConvertidorMasa.KilogramosALibras(valor));
        [WebMethod(Description = "lb → kg (no negativo).")]
        public double LibrasAKilogramos(double valor) => Ejecutar(() => ConvertidorMasa.LibrasAKilogramos(valor));
        [WebMethod(Description = "g → oz (no negativo).")]
        public double GramosAOnzas(double valor) => Ejecutar(() => ConvertidorMasa.GramosAOnzas(valor));
        [WebMethod(Description = "oz → g (no negativo).")]
        public double OnzasAGramos(double valor) => Ejecutar(() => ConvertidorMasa.OnzasAGramos(valor));

        // Mismos códigos de error que WSConversion.svc: el detalle del Fault lleva <ErrorServicio><Codigo/><Mensaje/>.
        private T Ejecutar<T>(Func<T> operacion)
        {
            try
            {
                return operacion();
            }
            catch (ArgumentException ex)
            {
                throw Falla("VALOR_INVALIDO", ex.Message);
            }
            catch (ReglaNegocioException ex)
            {
                throw Falla("REGLA_NEGOCIO", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw Falla("ERROR_SERVIDOR", ex.Message);
            }
        }

        private SoapException Falla(string codigo, string mensaje)
        {
            // ArgumentOutOfRangeException agrega "Nombre del parámetro: ..." al mensaje; nos quedamos con la primera línea.
            string limpio = mensaje.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];

            var documento = new XmlDocument();
            XmlNode detalle = documento.CreateNode(XmlNodeType.Element,
                SoapException.DetailElementName.Name, SoapException.DetailElementName.Namespace);
            XmlElement error = documento.CreateElement("ErrorServicio", Constantes.NamespaceServicio);
            XmlElement nodoCodigo = documento.CreateElement("Codigo", Constantes.NamespaceServicio);
            nodoCodigo.InnerText = codigo;
            XmlElement nodoMensaje = documento.CreateElement("Mensaje", Constantes.NamespaceServicio);
            nodoMensaje.InnerText = limpio;
            error.AppendChild(nodoCodigo);
            error.AppendChild(nodoMensaje);
            detalle.AppendChild(error);

            return new SoapException(limpio, SoapException.ClientFaultCode, Context.Request.Url.AbsoluteUri, detalle);
        }
    }
}
