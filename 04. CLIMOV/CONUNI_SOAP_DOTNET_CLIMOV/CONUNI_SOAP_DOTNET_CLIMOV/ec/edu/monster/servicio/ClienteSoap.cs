using System.Globalization;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace ec.edu.monster.servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario (normalmente el mensaje del servidor).</summary>
public class ErrorServicioException : Exception
{
    public ErrorServicioException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Cliente SOAP 1.1 hecho a mano con HttpClient (sin referencias de servicio).
/// Por cada operación arma el sobre SOAP, lo envía por POST con la cabecera SOAPAction
/// y lee &lt;operacionResult&gt; o, si es un SOAP Fault, el &lt;Mensaje&gt; de ErrorServicio.
/// No tiene reglas: el login con sus 3 intentos y los rangos de los valores los valida el servidor.
/// Patrón Singleton: toda la app usa ClienteSoap.Instancia (un solo HttpClient).
/// </summary>
public sealed class ClienteSoap
{
    public const string Namespace = "http://espe.edu.ec/conuni/";
    private const string Contrato = "WSConversion";
    private const string ClavePreferencia = "url_servidor";

    // IP de la PC con IIS en la red WiFi; en el emulador sería http://10.0.2.2:8085/WSConversion.svc.
    public const string UrlPorDefecto = "http://192.168.100.3:8085/WSConversion.svc";

    private static readonly Lazy<ClienteSoap> _instancia = new(() => new ClienteSoap());

    public static ClienteSoap Instancia => _instancia.Value;

    private readonly HttpClient _http;

    // Constructor privado: solo la propia clase crea la instancia.
    private ClienteSoap()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    /// <summary>URL del servicio; se guarda en el teléfono para no escribirla cada vez.</summary>
    public string Url
    {
        get => Preferences.Default.Get(ClavePreferencia, UrlPorDefecto);
        set => Preferences.Default.Set(ClavePreferencia, value.Trim());
    }

    public async Task<bool> LoginAsync(string usuario, string clave)
    {
        string respuesta = await InvocarAsync("login", ("usuario", usuario), ("clave", clave));
        // El servidor responde true o un SOAP Fault con su mensaje. Si respondiera false (un servidor anterior,
        // sin el control de intentos), no se deja entrar.
        if (!bool.Parse(respuesta))
        {
            throw new ErrorServicioException("Usuario o clave incorrectos.");
        }
        return true;
    }

    public async Task<double> ConvertirAsync(string operacion, double valor)
    {
        string respuesta = await InvocarAsync(operacion, ("valor", valor.ToString("R", CultureInfo.InvariantCulture)));
        return double.Parse(respuesta, CultureInfo.InvariantCulture);
    }

    private async Task<string> InvocarAsync(string operacion, params (string Nombre, string Valor)[] parametros)
    {
        string url = Url;
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? destino))
        {
            throw new ErrorServicioException($"La URL del servidor no es válida: {url}");
        }

        var peticion = new HttpRequestMessage(HttpMethod.Post, destino)
        {
            Content = new StringContent(ConstruirSobre(operacion, parametros), Encoding.UTF8, "text/xml")
        };
        peticion.Headers.Add("SOAPAction", $"\"{Namespace}{Contrato}/{operacion}\"");

        try
        {
            using HttpResponseMessage respuesta = await _http.SendAsync(peticion);
            string cuerpo = await respuesta.Content.ReadAsStringAsync();
            return LeerRespuesta(cuerpo, (int)respuesta.StatusCode, operacion);
        }
        catch (ErrorServicioException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new ErrorServicioException("El servidor tardó demasiado en responder. Intente de nuevo.");
        }
        catch (Exception e) when (e is HttpRequestException || e is SocketException || e is IOException)
        {
            throw new ErrorServicioException(
                $"No se pudo conectar con el servidor en {url}. Verifique que esté encendido, la IP y que estén en la misma red WiFi.");
        }
    }

    private static string ConstruirSobre(string operacion, (string Nombre, string Valor)[] parametros)
    {
        var cuerpo = new StringBuilder();
        foreach (var (nombre, valor) in parametros)
        {
            cuerpo.Append($"<{nombre}>{SecurityElement.Escape(valor)}</{nombre}>");
        }
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
            + "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">"
            + $"<s:Body><{operacion} xmlns=\"{Namespace}\">{cuerpo}</{operacion}></s:Body>"
            + "</s:Envelope>";
    }

    /// <summary>Devuelve el texto de &lt;operacionResult&gt;, o lanza el mensaje del Fault.</summary>
    private static string LeerRespuesta(string xml, int codigoHttp, string operacion)
    {
        XDocument documento;
        try
        {
            documento = XDocument.Parse(xml);
        }
        catch (Exception)
        {
            throw new ErrorServicioException($"Respuesta inesperada del servidor (HTTP {codigoHttp}).");
        }

        string? Buscar(string nombre) =>
            documento.Descendants().FirstOrDefault(e => e.Name.LocalName == nombre)?.Value;

        string? resultado = Buscar(operacion + "Result");
        if (resultado != null)
        {
            return resultado.Trim();
        }
        throw new ErrorServicioException(
            Buscar("Mensaje") ?? Buscar("faultstring") ?? $"Respuesta inesperada del servidor (HTTP {codigoHttp}).");
    }
}
