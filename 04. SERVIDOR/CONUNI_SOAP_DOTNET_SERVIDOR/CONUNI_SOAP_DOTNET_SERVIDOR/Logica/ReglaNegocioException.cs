using System;

namespace ConversorUnidades.Servidor.Logica
{
    /// <summary>
    /// Regla del servicio que no se cumple (usuario o clave incorrectos, usuario bloqueado).
    /// El servicio la envía como SOAP Fault con código REGLA_NEGOCIO y el cliente muestra el mensaje.
    /// </summary>
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje) { }
    }
}
