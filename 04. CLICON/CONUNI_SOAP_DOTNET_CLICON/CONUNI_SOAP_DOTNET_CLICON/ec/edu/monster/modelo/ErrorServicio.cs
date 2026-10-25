using System.Runtime.Serialization;

namespace ec.edu.monster.modelo
{
    /// <summary>Detalle del SOAP Fault que envía el servidor. Solo datos (get/set).</summary>
    [DataContract(Namespace = "http://espe.edu.ec/conuni/")]
    public class ErrorServicio
    {
        [DataMember(Order = 1)] public string Codigo { get; set; }
        [DataMember(Order = 2)] public string Mensaje { get; set; }
    }
}
