using System;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using ConversorUnidades.Servidor.Contratos;
using ConversorUnidades.Servidor.Logica;

namespace ConversorUnidades.Servidor.Servicios
{
    /// <summary>
    /// Implementación del servicio SOAP. Solo delega en la capa Logica y
    /// traduce los errores de validación a un SOAP Fault con mensaje claro.
    /// </summary>
    // Mismo namespace que el contrato: así el WSDL sale en un solo documento (más fácil para los clientes).
    [ServiceBehavior(Namespace = Constantes.NamespaceServicio)]
    public class WSConversion : IWSConversion
    {
        public bool Login(string usuario, string clave)
        {
            return Ejecutar(() => Autenticador.Instancia.Validar(usuario, clave), usuario);
        }

        // ---------- Temperatura ----------
        public double CelsiusAFahrenheit(double valor) => Ejecutar(() => ConvertidorTemperatura.CelsiusAFahrenheit(valor), valor);
        public double FahrenheitACelsius(double valor) => Ejecutar(() => ConvertidorTemperatura.FahrenheitACelsius(valor), valor);
        public double CelsiusAKelvin(double valor) => Ejecutar(() => ConvertidorTemperatura.CelsiusAKelvin(valor), valor);
        public double KelvinACelsius(double valor) => Ejecutar(() => ConvertidorTemperatura.KelvinACelsius(valor), valor);
        public double FahrenheitAKelvin(double valor) => Ejecutar(() => ConvertidorTemperatura.FahrenheitAKelvin(valor), valor);
        public double KelvinAFahrenheit(double valor) => Ejecutar(() => ConvertidorTemperatura.KelvinAFahrenheit(valor), valor);

        // ---------- Longitud ----------
        public double MetrosAPies(double valor) => Ejecutar(() => ConvertidorLongitud.MetrosAPies(valor), valor);
        public double PiesAMetros(double valor) => Ejecutar(() => ConvertidorLongitud.PiesAMetros(valor), valor);
        public double MetrosAPulgadas(double valor) => Ejecutar(() => ConvertidorLongitud.MetrosAPulgadas(valor), valor);
        public double PulgadasAMetros(double valor) => Ejecutar(() => ConvertidorLongitud.PulgadasAMetros(valor), valor);
        public double KilometrosAMillas(double valor) => Ejecutar(() => ConvertidorLongitud.KilometrosAMillas(valor), valor);
        public double MillasAKilometros(double valor) => Ejecutar(() => ConvertidorLongitud.MillasAKilometros(valor), valor);

        // ---------- Masa ----------
        public double KilogramosALibras(double valor) => Ejecutar(() => ConvertidorMasa.KilogramosALibras(valor), valor);
        public double LibrasAKilogramos(double valor) => Ejecutar(() => ConvertidorMasa.LibrasAKilogramos(valor), valor);
        public double GramosAOnzas(double valor) => Ejecutar(() => ConvertidorMasa.GramosAOnzas(valor), valor);
        public double OnzasAGramos(double valor) => Ejecutar(() => ConvertidorMasa.OnzasAGramos(valor), valor);

        private static T Ejecutar<T>(Func<T> operacion, object entrada, [CallerMemberName] string nombre = "")
        {
            try
            {
                return Consumo.Medir("SVC", nombre, entrada, operacion);
            }
            catch (ArgumentException ex)
            {
                // ArgumentOutOfRangeException agrega "Nombre del parámetro: ..." al mensaje; lo quitamos.
                string mensaje = ex.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                throw new FaultException<ErrorServicio>(
                    new ErrorServicio("VALOR_INVALIDO", mensaje), new FaultReason(mensaje));
            }
            catch (ReglaNegocioException ex)
            {
                // Usuario o clave incorrectos (con intentos restantes) o usuario bloqueado.
                throw new FaultException<ErrorServicio>(
                    new ErrorServicio("REGLA_NEGOCIO", ex.Message), new FaultReason(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                throw new FaultException<ErrorServicio>(
                    new ErrorServicio("ERROR_SERVIDOR", ex.Message), new FaultReason(ex.Message));
            }
        }
    }
}
