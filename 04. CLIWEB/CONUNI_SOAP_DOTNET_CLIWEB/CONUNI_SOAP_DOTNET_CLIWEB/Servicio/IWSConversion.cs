using System.Runtime.Serialization;
using System.ServiceModel;

namespace ConversorUnidades.Web.Servicio;

/// <summary>
/// Copia del contrato del servidor (mismo Name y Namespace), en versión asíncrona:
/// en ASP.NET Core las llamadas de red no deben bloquear el hilo de la petición.
/// El "Name" de cada operación coincide con el del servidor, así que el SOAPAction es el mismo.
/// </summary>
[ServiceContract(Name = "WSConversion", Namespace = ConstantesServicio.Namespace)]
public interface IWSConversion
{
    [OperationContract(Name = "login")]
    [FaultContract(typeof(ErrorServicio))]
    Task<bool> LoginAsync(string usuario, string clave);

    [OperationContract(Name = "celsiusAFahrenheit")] [FaultContract(typeof(ErrorServicio))] Task<double> CelsiusAFahrenheitAsync(double valor);
    [OperationContract(Name = "fahrenheitACelsius")] [FaultContract(typeof(ErrorServicio))] Task<double> FahrenheitACelsiusAsync(double valor);
    [OperationContract(Name = "celsiusAKelvin")] [FaultContract(typeof(ErrorServicio))] Task<double> CelsiusAKelvinAsync(double valor);
    [OperationContract(Name = "kelvinACelsius")] [FaultContract(typeof(ErrorServicio))] Task<double> KelvinACelsiusAsync(double valor);
    [OperationContract(Name = "fahrenheitAKelvin")] [FaultContract(typeof(ErrorServicio))] Task<double> FahrenheitAKelvinAsync(double valor);
    [OperationContract(Name = "kelvinAFahrenheit")] [FaultContract(typeof(ErrorServicio))] Task<double> KelvinAFahrenheitAsync(double valor);

    [OperationContract(Name = "metrosAPies")] [FaultContract(typeof(ErrorServicio))] Task<double> MetrosAPiesAsync(double valor);
    [OperationContract(Name = "piesAMetros")] [FaultContract(typeof(ErrorServicio))] Task<double> PiesAMetrosAsync(double valor);
    [OperationContract(Name = "metrosAPulgadas")] [FaultContract(typeof(ErrorServicio))] Task<double> MetrosAPulgadasAsync(double valor);
    [OperationContract(Name = "pulgadasAMetros")] [FaultContract(typeof(ErrorServicio))] Task<double> PulgadasAMetrosAsync(double valor);
    [OperationContract(Name = "kilometrosAMillas")] [FaultContract(typeof(ErrorServicio))] Task<double> KilometrosAMillasAsync(double valor);
    [OperationContract(Name = "millasAKilometros")] [FaultContract(typeof(ErrorServicio))] Task<double> MillasAKilometrosAsync(double valor);

    [OperationContract(Name = "kilogramosALibras")] [FaultContract(typeof(ErrorServicio))] Task<double> KilogramosALibrasAsync(double valor);
    [OperationContract(Name = "librasAKilogramos")] [FaultContract(typeof(ErrorServicio))] Task<double> LibrasAKilogramosAsync(double valor);
    [OperationContract(Name = "gramosAOnzas")] [FaultContract(typeof(ErrorServicio))] Task<double> GramosAOnzasAsync(double valor);
    [OperationContract(Name = "onzasAGramos")] [FaultContract(typeof(ErrorServicio))] Task<double> OnzasAGramosAsync(double valor);
}

[DataContract(Namespace = ConstantesServicio.Namespace)]
public class ErrorServicio
{
    [DataMember(Order = 1)] public string? Codigo { get; set; }
    [DataMember(Order = 2)] public string? Mensaje { get; set; }
}

public static class ConstantesServicio
{
    public const string Namespace = "http://espe.edu.ec/conuni/";
}
