using System.ServiceModel;

namespace ConversorUnidades.Servidor.Contratos
{
    /// <summary>
    /// Contrato SOAP del servicio de conversión de unidades.
    /// Todas las conversiones reciben un único parámetro llamado "valor",
    /// así los clientes (consola, escritorio, web y móvil) arman la petición igual.
    /// </summary>
    [ServiceContract(Name = "WSConversion", Namespace = Constantes.NamespaceServicio)]
    public interface IWSConversion
    {
        // ---------- Autenticación ----------

        /// <summary>
        /// Devuelve true si las credenciales son correctas. Si no, responde un Fault REGLA_NEGOCIO con los
        /// intentos restantes; al tercer intento fallido el usuario queda bloqueado 2 minutos.
        /// </summary>
        [OperationContract(Name = "login")]
        [FaultContract(typeof(ErrorServicio))]
        bool Login(string usuario, string clave);

        // ---------- Temperatura ----------

        [OperationContract(Name = "celsiusAFahrenheit")]
        [FaultContract(typeof(ErrorServicio))]
        double CelsiusAFahrenheit(double valor);

        [OperationContract(Name = "fahrenheitACelsius")]
        [FaultContract(typeof(ErrorServicio))]
        double FahrenheitACelsius(double valor);

        [OperationContract(Name = "celsiusAKelvin")]
        [FaultContract(typeof(ErrorServicio))]
        double CelsiusAKelvin(double valor);

        [OperationContract(Name = "kelvinACelsius")]
        [FaultContract(typeof(ErrorServicio))]
        double KelvinACelsius(double valor);

        [OperationContract(Name = "fahrenheitAKelvin")]
        [FaultContract(typeof(ErrorServicio))]
        double FahrenheitAKelvin(double valor);

        [OperationContract(Name = "kelvinAFahrenheit")]
        [FaultContract(typeof(ErrorServicio))]
        double KelvinAFahrenheit(double valor);

        // ---------- Longitud ----------

        [OperationContract(Name = "metrosAPies")]
        [FaultContract(typeof(ErrorServicio))]
        double MetrosAPies(double valor);

        [OperationContract(Name = "piesAMetros")]
        [FaultContract(typeof(ErrorServicio))]
        double PiesAMetros(double valor);

        [OperationContract(Name = "metrosAPulgadas")]
        [FaultContract(typeof(ErrorServicio))]
        double MetrosAPulgadas(double valor);

        [OperationContract(Name = "pulgadasAMetros")]
        [FaultContract(typeof(ErrorServicio))]
        double PulgadasAMetros(double valor);

        [OperationContract(Name = "kilometrosAMillas")]
        [FaultContract(typeof(ErrorServicio))]
        double KilometrosAMillas(double valor);

        [OperationContract(Name = "millasAKilometros")]
        [FaultContract(typeof(ErrorServicio))]
        double MillasAKilometros(double valor);

        // ---------- Masa ----------

        [OperationContract(Name = "kilogramosALibras")]
        [FaultContract(typeof(ErrorServicio))]
        double KilogramosALibras(double valor);

        [OperationContract(Name = "librasAKilogramos")]
        [FaultContract(typeof(ErrorServicio))]
        double LibrasAKilogramos(double valor);

        [OperationContract(Name = "gramosAOnzas")]
        [FaultContract(typeof(ErrorServicio))]
        double GramosAOnzas(double valor);

        [OperationContract(Name = "onzasAGramos")]
        [FaultContract(typeof(ErrorServicio))]
        double OnzasAGramos(double valor);
    }
}
