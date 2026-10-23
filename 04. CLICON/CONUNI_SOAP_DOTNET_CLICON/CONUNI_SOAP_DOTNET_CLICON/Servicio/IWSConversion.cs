using System.Runtime.Serialization;
using System.ServiceModel;

namespace ConversorUnidades.Consola.Servicio
{
    /// <summary>
    /// Copia del contrato del servidor (mismo Name y Namespace).
    /// Con esto el cliente se conecta con ChannelFactory, sin generar proxies.
    /// </summary>
    [ServiceContract(Name = "WSConversion", Namespace = ConstantesServicio.Namespace)]
    public interface IWSConversion
    {
        [OperationContract(Name = "login")]
        [FaultContract(typeof(ErrorServicio))]
        bool Login(string usuario, string clave);

        [OperationContract(Name = "celsiusAFahrenheit")] [FaultContract(typeof(ErrorServicio))] double CelsiusAFahrenheit(double valor);
        [OperationContract(Name = "fahrenheitACelsius")] [FaultContract(typeof(ErrorServicio))] double FahrenheitACelsius(double valor);
        [OperationContract(Name = "celsiusAKelvin")] [FaultContract(typeof(ErrorServicio))] double CelsiusAKelvin(double valor);
        [OperationContract(Name = "kelvinACelsius")] [FaultContract(typeof(ErrorServicio))] double KelvinACelsius(double valor);
        [OperationContract(Name = "fahrenheitAKelvin")] [FaultContract(typeof(ErrorServicio))] double FahrenheitAKelvin(double valor);
        [OperationContract(Name = "kelvinAFahrenheit")] [FaultContract(typeof(ErrorServicio))] double KelvinAFahrenheit(double valor);

        [OperationContract(Name = "metrosAPies")] [FaultContract(typeof(ErrorServicio))] double MetrosAPies(double valor);
        [OperationContract(Name = "piesAMetros")] [FaultContract(typeof(ErrorServicio))] double PiesAMetros(double valor);
        [OperationContract(Name = "metrosAPulgadas")] [FaultContract(typeof(ErrorServicio))] double MetrosAPulgadas(double valor);
        [OperationContract(Name = "pulgadasAMetros")] [FaultContract(typeof(ErrorServicio))] double PulgadasAMetros(double valor);
        [OperationContract(Name = "kilometrosAMillas")] [FaultContract(typeof(ErrorServicio))] double KilometrosAMillas(double valor);
        [OperationContract(Name = "millasAKilometros")] [FaultContract(typeof(ErrorServicio))] double MillasAKilometros(double valor);

        [OperationContract(Name = "kilogramosALibras")] [FaultContract(typeof(ErrorServicio))] double KilogramosALibras(double valor);
        [OperationContract(Name = "librasAKilogramos")] [FaultContract(typeof(ErrorServicio))] double LibrasAKilogramos(double valor);
        [OperationContract(Name = "gramosAOnzas")] [FaultContract(typeof(ErrorServicio))] double GramosAOnzas(double valor);
        [OperationContract(Name = "onzasAGramos")] [FaultContract(typeof(ErrorServicio))] double OnzasAGramos(double valor);
    }

    [DataContract(Namespace = ConstantesServicio.Namespace)]
    public class ErrorServicio
    {
        [DataMember(Order = 1)] public string Codigo { get; set; }
        [DataMember(Order = 2)] public string Mensaje { get; set; }
    }

    public static class ConstantesServicio
    {
        public const string Namespace = "http://espe.edu.ec/conuni/";
    }
}
