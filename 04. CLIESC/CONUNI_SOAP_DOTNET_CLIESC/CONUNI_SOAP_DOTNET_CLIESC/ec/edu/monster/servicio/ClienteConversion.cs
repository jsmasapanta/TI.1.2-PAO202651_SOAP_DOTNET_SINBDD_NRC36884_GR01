using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio
{
    /// <summary>Error que la vista puede mostrar tal cual al usuario (normalmente el mensaje del servidor).</summary>
    public class ErrorServicioException : Exception
    {
        public ErrorServicioException(string mensaje) : base(mensaje) { }
    }

    /// <summary>
    /// Llama a las operaciones del servicio con el endpoint de App.config. No tiene reglas: si el servidor
    /// rechaza algo, su SOAP Fault llega aquí y se entrega como ErrorServicioException con su mensaje.
    /// Patrón Singleton: toda la aplicación usa ClienteConversion.Instancia (una sola ChannelFactory).
    /// </summary>
    public sealed class ClienteConversion
    {
        // Nombre del endpoint definido en App.config (system.serviceModel/client).
        private const string NombreEndpoint = "WSConversion";

        private static readonly Lazy<ClienteConversion> _instancia =
            new Lazy<ClienteConversion>(() => new ClienteConversion());

        public static ClienteConversion Instancia => _instancia.Value;

        // Archivo donde se recuerda el último servidor escrito en la ventana de ingreso.
        private static readonly string ArchivoServidor = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CONUNI_CLIESC", "servidor.txt");

        private ChannelFactory<IWSConversion> _fabrica;

        // Constructor privado: solo la propia clase crea la instancia.
        private ClienteConversion()
        {
            _fabrica = CrearFabrica(null);

            // Si ya se usó otro servidor, se arranca con ese; si no, con el de App.config.
            string guardado = File.Exists(ArchivoServidor) ? File.ReadAllText(ArchivoServidor).Trim() : null;
            if (!string.IsNullOrEmpty(guardado))
            {
                try { _fabrica = CrearFabrica(new EndpointAddress(guardado)); }
                catch (UriFormatException) { }
            }
        }

        private static ChannelFactory<IWSConversion> CrearFabrica(EndpointAddress direccion)
        {
            var fabrica = direccion == null
                ? new ChannelFactory<IWSConversion>(NombreEndpoint)
                : new ChannelFactory<IWSConversion>(NombreEndpoint, direccion);
            fabrica.Endpoint.EndpointBehaviors.Add(new CabeceraCliente("Escritorio"));
            return fabrica;
        }

        /// <summary>
        /// Cambia el servidor al que se llama y lo recuerda para la próxima vez. Acepta la URL completa,
        /// "IP:puerto" o solo la IP (completa con el puerto 8085 y /WSConversion.svc).
        /// </summary>
        public void CambiarServidor(string texto)
        {
            string url = NormalizarUrl(texto);
            if (url == UrlServidor) return;

            ChannelFactory<IWSConversion> anterior = _fabrica;
            _fabrica = CrearFabrica(new EndpointAddress(url));
            try { anterior.Close(); } catch { anterior.Abort(); }

            Directory.CreateDirectory(Path.GetDirectoryName(ArchivoServidor));
            File.WriteAllText(ArchivoServidor, url);
        }

        private static string NormalizarUrl(string texto)
        {
            string url = (texto ?? "").Trim();
            if (url.Length == 0) throw new ErrorServicioException("Ingrese la dirección del servidor SOAP.");
            if (!url.Contains("://")) url = "http://" + url;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || uri.Scheme != Uri.UriSchemeHttp)
                throw new ErrorServicioException("Dirección de servidor no válida. Ejemplo: http://192.168.1.10:8085/WSConversion.svc");

            var constructor = new UriBuilder(uri);
            if (uri.IsDefaultPort && !texto.Contains(":80")) constructor.Port = 8085;
            if (constructor.Path == "/") constructor.Path = "/WSConversion.svc";
            return constructor.Uri.ToString();
        }

        public string UrlServidor => _fabrica.Endpoint.Address.Uri.ToString();

        /// <summary>
        /// El servidor responde true o un SOAP Fault con su mensaje. Si respondiera false (un servidor anterior,
        /// sin el control de intentos), no se deja entrar.
        /// </summary>
        public bool Login(string usuario, string clave)
        {
            if (!Invocar(c => c.Login(usuario, clave)))
            {
                throw new ErrorServicioException("Usuario o clave incorrectos.");
            }
            return true;
        }

        /// <summary>Llama a la operación SOAP que se llama igual que <paramref name="operacion"/>.</summary>
        public double Convertir(string operacion, double valor) => Invocar(c => Llamar(c, operacion, valor));

        private static double Llamar(IWSConversion canal, string operacion, double valor)
        {
            switch (operacion)
            {
                case "celsiusAFahrenheit": return canal.CelsiusAFahrenheit(valor);
                case "fahrenheitACelsius": return canal.FahrenheitACelsius(valor);
                case "celsiusAKelvin": return canal.CelsiusAKelvin(valor);
                case "kelvinACelsius": return canal.KelvinACelsius(valor);
                case "fahrenheitAKelvin": return canal.FahrenheitAKelvin(valor);
                case "kelvinAFahrenheit": return canal.KelvinAFahrenheit(valor);
                case "metrosAPies": return canal.MetrosAPies(valor);
                case "piesAMetros": return canal.PiesAMetros(valor);
                case "metrosAPulgadas": return canal.MetrosAPulgadas(valor);
                case "pulgadasAMetros": return canal.PulgadasAMetros(valor);
                case "kilometrosAMillas": return canal.KilometrosAMillas(valor);
                case "millasAKilometros": return canal.MillasAKilometros(valor);
                case "kilogramosALibras": return canal.KilogramosALibras(valor);
                case "librasAKilogramos": return canal.LibrasAKilogramos(valor);
                case "gramosAOnzas": return canal.GramosAOnzas(valor);
                case "onzasAGramos": return canal.OnzasAGramos(valor);
                default: throw new ErrorServicioException($"El servicio no tiene la operación {operacion}.");
            }
        }

        private T Invocar<T>(Func<IWSConversion, T> llamada)
        {
            IWSConversion canal = _fabrica.CreateChannel();
            var comunicacion = (ICommunicationObject)canal;
            try
            {
                T resultado = llamada(canal);
                comunicacion.Close();
                return resultado;
            }
            catch (FaultException<ErrorServicio> ex)
            {
                comunicacion.Abort();
                throw new ErrorServicioException(ex.Detail.Mensaje);
            }
            catch (FaultException ex)
            {
                comunicacion.Abort();
                throw new ErrorServicioException("El servidor rechazó la petición: " + ex.Message);
            }
            catch (EndpointNotFoundException)
            {
                comunicacion.Abort();
                throw new ErrorServicioException(
                    "No se pudo conectar con el servidor en " + UrlServidor + ". Verifique que esté encendido y la dirección del servidor.");
            }
            catch (TimeoutException)
            {
                comunicacion.Abort();
                throw new ErrorServicioException("El servidor tardó demasiado en responder. Intente de nuevo.");
            }
            catch (CommunicationException ex)
            {
                comunicacion.Abort();
                throw new ErrorServicioException("Error de comunicación con el servidor: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Agrega la cabecera HTTP "X-Cliente" a cada llamada para que el panel de monitoreo del servidor
    /// sepa qué cliente consume el servicio.
    /// </summary>
    internal sealed class CabeceraCliente : IEndpointBehavior, IClientMessageInspector
    {
        private readonly string _nombre;

        public CabeceraCliente(string nombre) { _nombre = nombre; }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (!request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out object propiedad))
            {
                propiedad = new HttpRequestMessageProperty();
                request.Properties.Add(HttpRequestMessageProperty.Name, propiedad);
            }
            ((HttpRequestMessageProperty)propiedad).Headers["X-Cliente"] = _nombre;
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) => clientRuntime.ClientMessageInspectors.Add(this);
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
        public void Validate(ServiceEndpoint endpoint) { }
    }
}
