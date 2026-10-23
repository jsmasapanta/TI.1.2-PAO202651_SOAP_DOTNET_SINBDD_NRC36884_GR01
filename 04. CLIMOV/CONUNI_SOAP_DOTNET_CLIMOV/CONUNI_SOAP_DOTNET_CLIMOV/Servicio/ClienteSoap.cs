using System.Globalization;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace ConversorUnidades.Movil.Servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario.</summary>
public class ErrorConversionException : Exception
{
    public ErrorConversionException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Cliente SOAP 1.1 hecho a mano con HttpClient (sin referencias de servicio).
/// Por cada operación arma el sobre SOAP, lo envía por POST con la cabecera SOAPAction
/// y lee &lt;operacionResult&gt; o, si es un SOAP Fault, el &lt;Mensaje&gt; de ErrorServicio.
/// Patrón Singleton: toda la app usa ClienteSoap.Instancia (un solo HttpClient).
/// </summary>
public sealed class ClienteSoap
{
    public const string Namespace = "http://espe.edu.ec/conuni/";
    private const string Contrato = "WSConversion";
    private const string ClavePreferencia = "url_servidor";

    // 10.0.2.2 es la PC vista desde el emulador; en un celular real se cambia por la IP de la PC.
    public const string UrlPorDefecto = "http://10.0.2.2:8085/WSConversion.svc";

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
        return bool.Parse(respuesta);
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
            throw new ErrorConversionException($"La URL del servidor no es válida: {url}");
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
        catch (ErrorConversionException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new ErrorConversionException("El servidor tardó demasiado en responder. Intente de nuevo.");
        }
        catch (Exception e) when (e is HttpRequestException || e is SocketException || e is IOException)
        {
            throw new ErrorConversionException(
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
            throw new ErrorConversionException($"Respuesta inesperada del servidor (HTTP {codigoHttp}).");
        }

        string? Buscar(string nombre) =>
            documento.Descendants().FirstOrDefault(e => e.Name.LocalName == nombre)?.Value;

        string? resultado = Buscar(operacion + "Result");
        if (resultado != null)
        {
            return resultado.Trim();
        }
        throw new ErrorConversionException(
            Buscar("Mensaje") ?? Buscar("faultstring") ?? $"Respuesta inesperada del servidor (HTTP {codigoHttp}).");
    }
}
