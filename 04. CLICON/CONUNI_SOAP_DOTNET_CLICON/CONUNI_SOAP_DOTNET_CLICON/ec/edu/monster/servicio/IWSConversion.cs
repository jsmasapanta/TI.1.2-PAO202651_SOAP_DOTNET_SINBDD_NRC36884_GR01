using System.ServiceModel;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio
{
    /// <summary>
    /// Operaciones que publica el servidor (mismo Name y Namespace que su contrato).
    /// Con esta interfaz el cliente llama al servicio mediante ChannelFactory.
    /// </summary>
    [ServiceContract(Name = "WSConversion", Namespace = "http://espe.edu.ec/conuni/")]
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
}
