using System.Runtime.Serialization;

namespace ConversorUnidades.Servidor.Contratos
{
    /// <summary>
    /// Detalle que viaja dentro del SOAP Fault cuando una petición no es válida.
    /// Los clientes muestran "Mensaje" al usuario.
    /// </summary>
    [DataContract(Namespace = Constantes.NamespaceServicio)]
    public class ErrorServicio
    {
        [DataMember(Order = 1)]
        public string Codigo { get; set; }

        [DataMember(Order = 2)]
        public string Mensaje { get; set; }

        public ErrorServicio(string codigo, string mensaje)
        {
            Codigo = codigo;
            Mensaje = mensaje;
        }
    }
}
