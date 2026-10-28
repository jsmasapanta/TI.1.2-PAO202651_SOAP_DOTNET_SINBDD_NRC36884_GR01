using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
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
        _fabrica.Endpoint.EndpointBehaviors.Add(new CabeceraCliente("Web"));
        UrlServidor = url;
    }

    public string UrlServidor { get; }

    /// <summary>
    /// El servidor responde true o un SOAP Fault con su mensaje. Si respondiera false (un servidor anterior,
    /// sin el control de intentos), no se deja entrar.
    /// </summary>
    public async Task<bool> LoginAsync(string usuario, string clave)
    {
        if (!await InvocarAsync(canal => canal.LoginAsync(usuario, clave)))
        {
            throw new ErrorServicioException("Usuario o clave incorrectos.");
        }
        return true;
    }

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

/// <summary>
/// Agrega la cabecera HTTP "X-Cliente" a cada llamada para que el panel de monitoreo del servidor
/// sepa qué cliente consume el servicio.
/// </summary>
internal sealed class CabeceraCliente : IEndpointBehavior, IClientMessageInspector
{
    private readonly string _nombre;

    public CabeceraCliente(string nombre) { _nombre = nombre; }

    public object? BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        if (!request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out object? propiedad))
        {
            propiedad = new HttpRequestMessageProperty();
            request.Properties.Add(HttpRequestMessageProperty.Name, propiedad);
        }
        ((HttpRequestMessageProperty)propiedad).Headers["X-Cliente"] = _nombre;
        return null;
    }

    public void AfterReceiveReply(ref Message reply, object? correlationState) { }
    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) => clientRuntime.ClientMessageInspectors.Add(this);
    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }
}
