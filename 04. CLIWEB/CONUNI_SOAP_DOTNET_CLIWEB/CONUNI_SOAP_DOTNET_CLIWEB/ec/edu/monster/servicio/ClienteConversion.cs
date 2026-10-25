using System.ServiceModel;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario (normalmente el mensaje del servidor).</summary>
public class ErrorServicioException : Exception
{
    public ErrorServicioException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Llama a las operaciones del servicio. La URL viene de appsettings.json ("ServicioSoap:Url").
/// No tiene reglas: si el servidor rechaza algo, su SOAP Fault llega como ErrorServicioException con su mensaje.
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

    public Task<bool> LoginAsync(string usuario, string clave) =>
        InvocarAsync(canal => canal.LoginAsync(usuario, clave));

    /// <summary>Llama a la operación SOAP que se llama igual que <paramref name="operacion"/>.</summary>
    public Task<double> ConvertirAsync(string operacion, double valor) =>
        InvocarAsync(canal => Llamar(canal, operacion, valor));

    private static Task<double> Llamar(IWSConversion canal, string operacion, double valor) => operacion switch
    {
        "celsiusAFahrenheit" => canal.CelsiusAFahrenheitAsync(valor),
        "fahrenheitACelsius" => canal.FahrenheitACelsiusAsync(valor),
        "celsiusAKelvin" => canal.CelsiusAKelvinAsync(valor),
        "kelvinACelsius" => canal.KelvinACelsiusAsync(valor),
        "fahrenheitAKelvin" => canal.FahrenheitAKelvinAsync(valor),
        "kelvinAFahrenheit" => canal.KelvinAFahrenheitAsync(valor),
        "metrosAPies" => canal.MetrosAPiesAsync(valor),
        "piesAMetros" => canal.PiesAMetrosAsync(valor),
        "metrosAPulgadas" => canal.MetrosAPulgadasAsync(valor),
        "pulgadasAMetros" => canal.PulgadasAMetrosAsync(valor),
        "kilometrosAMillas" => canal.KilometrosAMillasAsync(valor),
        "millasAKilometros" => canal.MillasAKilometrosAsync(valor),
        "kilogramosALibras" => canal.KilogramosALibrasAsync(valor),
        "librasAKilogramos" => canal.LibrasAKilogramosAsync(valor),
        "gramosAOnzas" => canal.GramosAOnzasAsync(valor),
        "onzasAGramos" => canal.OnzasAGramosAsync(valor),
        _ => throw new ErrorServicioException($"El servicio no tiene la operación {operacion}.")
    };

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
            throw new ErrorServicioException(ex.Detail.Mensaje ?? ex.Message);
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
                $"No se pudo conectar con el servidor en {UrlServidor}. Verifique que esté encendido y la URL en appsettings.json.");
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

    public void Dispose()
    {
        try { _fabrica.Close(); } catch { _fabrica.Abort(); }
    }
}
