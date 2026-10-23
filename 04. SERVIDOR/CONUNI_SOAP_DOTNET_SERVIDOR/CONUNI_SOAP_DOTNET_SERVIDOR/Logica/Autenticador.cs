using System;
using System.Configuration;

namespace ConversorUnidades.Servidor.Logica
{
    /// <summary>
    /// Valida credenciales contra los valores de Web.config (appSettings).
    /// Patrón Singleton: una sola instancia lee Web.config una vez y la comparten
    /// todas las peticiones (Lazy garantiza que la creación sea segura entre hilos).
    /// </summary>
    public sealed class Autenticador
    {
        private static readonly Lazy<Autenticador> _instancia =
            new Lazy<Autenticador>(() => new Autenticador());

        public static Autenticador Instancia => _instancia.Value;

        private readonly string _usuarioValido;
        private readonly string _claveValida;

        // Constructor privado: nadie fuera de la clase puede hacer "new Autenticador()".
        private Autenticador()
        {
            _usuarioValido = ConfigurationManager.AppSettings["UsuarioServicio"];
            _claveValida = ConfigurationManager.AppSettings["ClaveServicio"];
        }

        public bool Validar(string usuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
            {
                return false;
            }

            if (string.IsNullOrEmpty(_usuarioValido) || string.IsNullOrEmpty(_claveValida))
            {
                throw new InvalidOperationException("El servidor no tiene credenciales configuradas en Web.config.");
            }

            // El usuario no distingue mayúsculas; la clave sí.
            return string.Equals(usuario.Trim(), _usuarioValido, StringComparison.OrdinalIgnoreCase)
                && string.Equals(clave, _claveValida, StringComparison.Ordinal);
        }
    }
}
