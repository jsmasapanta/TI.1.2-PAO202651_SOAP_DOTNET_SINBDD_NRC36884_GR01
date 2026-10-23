using System;
using System.ServiceModel;

namespace ConversorUnidades.Consola.Servicio
{
    /// <summary>
    /// Error que la vista puede mostrar tal cual al usuario.
    /// </summary>
    public class ErrorConversionException : Exception
    {
        public ErrorConversionException(string mensaje) : base(mensaje) { }
    }

    /// <summary>
    /// Envuelve el canal WCF: abre la conexión con el endpoint de App.config
    /// y traduce cualquier falla técnica a un mensaje entendible.
    /// Patrón Singleton: toda la aplicación usa la misma instancia (ClienteConversion.Instancia),
    /// así la ChannelFactory se crea una sola vez.
    /// </summary>
    public sealed class ClienteConversion
    {
        // Nombre del endpoint definido en App.config (system.serviceModel/client).
        private const string NombreEndpoint = "WSConversion";

        private static readonly Lazy<ClienteConversion> _instancia =
            new Lazy<ClienteConversion>(() => new ClienteConversion());

        public static ClienteConversion Instancia => _instancia.Value;

        private readonly ChannelFactory<IWSConversion> _fabrica;

        // Constructor privado: solo la propia clase crea la instancia.
        private ClienteConversion()
        {
            _fabrica = new ChannelFactory<IWSConversion>(NombreEndpoint);
        }

        public string UrlServidor => _fabrica.Endpoint.Address.Uri.ToString();

        public bool Login(string usuario, string clave)
        {
            return Invocar(canal => canal.Login(usuario, clave));
        }

        public double Convertir(Func<IWSConversion, double, double> operacion, double valor)
        {
            return Invocar(canal => operacion(canal, valor));
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
                throw new ErrorConversionException(ex.Detail.Mensaje);
            }
            catch (FaultException ex)
            {
                comunicacion.Abort();
                throw new ErrorConversionException("El servidor rechazó la petición: " + ex.Message);
            }
            catch (EndpointNotFoundException)
            {
                comunicacion.Abort();
                throw new ErrorConversionException(
                    "No se pudo conectar con el servidor en " + UrlServidor + ". Verifique que esté encendido y la URL en App.config.");
            }
            catch (TimeoutException)
            {
                comunicacion.Abort();
                throw new ErrorConversionException("El servidor tardó demasiado en responder. Intente de nuevo.");
            }
            catch (CommunicationException ex)
            {
                comunicacion.Abort();
                throw new ErrorConversionException("Error de comunicación con el servidor: " + ex.Message);
            }
        }
    }
}
