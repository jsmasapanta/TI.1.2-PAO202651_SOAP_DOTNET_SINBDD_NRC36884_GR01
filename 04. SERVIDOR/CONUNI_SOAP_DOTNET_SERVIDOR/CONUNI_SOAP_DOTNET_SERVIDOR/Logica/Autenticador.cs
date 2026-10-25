using System;
using System.Collections.Generic;
using System.Configuration;

namespace ConversorUnidades.Servidor.Logica
{
    /// <summary>
    /// Valida credenciales contra los valores de Web.config (appSettings) y controla los intentos:
    /// al tercer intento fallido el usuario queda bloqueado 2 minutos. Los clientes no cuentan
    /// intentos: solo muestran el mensaje que llega en el SOAP Fault.
    /// Patrón Singleton: una sola instancia lee Web.config una vez y guarda los intentos de todas
    /// las peticiones (Lazy garantiza que la creación sea segura entre hilos).
    /// </summary>
    public sealed class Autenticador
    {
        private const int IntentosMaximos = 3;
        private static readonly TimeSpan TiempoBloqueo = TimeSpan.FromMinutes(2);

        private static readonly Lazy<Autenticador> _instancia =
            new Lazy<Autenticador>(() => new Autenticador());

        public static Autenticador Instancia => _instancia.Value;

        private readonly string _usuarioValido;
        private readonly string _claveValida;

        // Intentos fallidos por usuario (en minúsculas).
        private readonly Dictionary<string, ControlIntentos> _intentos = new Dictionary<string, ControlIntentos>();

        private sealed class ControlIntentos
        {
            public int Fallidos { get; set; }
            public DateTime? BloqueadoHasta { get; set; }
        }

        // Constructor privado: nadie fuera de la clase puede hacer "new Autenticador()".
        private Autenticador()
        {
            _usuarioValido = ConfigurationManager.AppSettings["UsuarioServicio"];
            _claveValida = ConfigurationManager.AppSettings["ClaveServicio"];
        }

        /// <summary>Devuelve true si las credenciales son correctas; si no, lanza el motivo.</summary>
        public bool Validar(string usuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
            {
                throw new ArgumentException("Ingrese usuario y clave.");
            }

            if (string.IsNullOrEmpty(_usuarioValido) || string.IsNullOrEmpty(_claveValida))
            {
                throw new InvalidOperationException("El servidor no tiene credenciales configuradas en Web.config.");
            }

            string llave = usuario.Trim().ToLowerInvariant();
            lock (_intentos)
            {
                if (_intentos.TryGetValue(llave, out ControlIntentos control)
                    && control.BloqueadoHasta.HasValue && control.BloqueadoHasta.Value > DateTime.Now)
                {
                    int segundos = (int)Math.Ceiling((control.BloqueadoHasta.Value - DateTime.Now).TotalSeconds);
                    throw new ReglaNegocioException(
                        $"Usuario bloqueado por superar {IntentosMaximos} intentos. Intente de nuevo en {segundos} segundos.");
                }
            }

            // El usuario no distingue mayúsculas; la clave sí.
            bool correcto = string.Equals(usuario.Trim(), _usuarioValido, StringComparison.OrdinalIgnoreCase)
                && string.Equals(clave, _claveValida, StringComparison.Ordinal);

            lock (_intentos)
            {
                if (correcto)
                {
                    _intentos.Remove(llave);
                    return true;
                }

                if (!_intentos.TryGetValue(llave, out ControlIntentos control) || control.BloqueadoHasta.HasValue)
                {
                    // Primer fallo, o el bloqueo anterior ya terminó: se empieza a contar de nuevo.
                    control = new ControlIntentos();
                    _intentos[llave] = control;
                }
                control.Fallidos++;

                if (control.Fallidos >= IntentosMaximos)
                {
                    control.BloqueadoHasta = DateTime.Now.Add(TiempoBloqueo);
                    throw new ReglaNegocioException(
                        $"Se superaron los {IntentosMaximos} intentos. El usuario queda bloqueado {TiempoBloqueo.TotalMinutes:0} minutos.");
                }
                throw new ReglaNegocioException(
                    $"Usuario o clave incorrectos. Intentos restantes: {IntentosMaximos - control.Fallidos}.");
            }
        }
    }
}
