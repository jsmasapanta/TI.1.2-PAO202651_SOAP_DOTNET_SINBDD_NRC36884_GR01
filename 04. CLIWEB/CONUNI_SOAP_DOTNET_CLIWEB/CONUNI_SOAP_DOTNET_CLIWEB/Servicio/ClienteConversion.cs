using System.ServiceModel;

namespace ConversorUnidades.Web.Servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario.</summary>
public class ErrorConversionException : Exception
{
    public ErrorConversionException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Envuelve el canal WCF. La URL viene de appsettings.json ("ServicioSoap:Url").
/// Se registra como singleton: la ChannelFactory se crea una sola vez y es segura entre hilos.
/// </summary>
public class ClienteConversion : IDisposable
{
    private readonly ChannelFactory<IWSConversion> _fabrica;

    public ClienteConversion(IConfiguration configuracion)
    {
        string url = configuracion["ServicioSoap:Url"]
            ?? throw new InvalidOperationException("Falta 'ServicioSoap:Url' en appsettings.json.");

        var binding = new BasicHttpBinding
        {
            OpenTimeout = TimeSpan.FromSeconds(10),
            SendTimeout = TimeSpan.FromSeconds(15),
            ReceiveTimeout = TimeSpan.FromSeconds(15)
        };
        _fabrica = new ChannelFactory<IWSConversion>(binding, new EndpointAddress(url));
        UrlServidor = url;
    }

    public string UrlServidor { get; }

    public Task<bool> LoginAsync(string usuario, string clave)
    {
        return InvocarAsync(canal => canal.LoginAsync(usuario, clave));
    }

    public Task<double> ConvertirAsync(Func<IWSConversion, double, Task<double>> operacion, double valor)
    {
        return InvocarAsync(canal => operacion(canal, valor));
    }

    private async Task<T> InvocarAsync<T>(Func<IWSConversion, Task<T>> llamada)
    {
        IWSConversion canal = _fabrica.CreateChannel();
        var comunicacion = (ICommunicationObject)canal;
        try
        {
            T resultado = await llamada(canal);
            comunicacion.Close();
            return resultado;
        }
        catch (FaultException<ErrorServicio> ex)
        {
            comunicacion.Abort();
            throw new ErrorConversionException(ex.Detail.Mensaje ?? ex.Message);
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
                $"No se pudo conectar con el servidor en {UrlServidor}. Verifique que esté encendido y la URL en appsettings.json.");
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

    public void Dispose()
    {
        try { _fabrica.Close(); } catch { _fabrica.Abort(); }
    }
}
