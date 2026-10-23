using System.Globalization;
using ConversorUnidades.Web.Servicio;

namespace ConversorUnidades.Web.Modelo;

public enum Categoria
{
    Temperatura,
    Longitud,
    Masa
}

/// <summary>Una conversión disponible y la operación del servicio que la resuelve.</summary>
public record Conversion(
    Categoria Categoria,
    string UnidadOrigen, string SimboloOrigen,
    string UnidadDestino, string SimboloDestino,
    Func<IWSConversion, double, Task<double>> Operacion);

public record Unidad(string Nombre, string Simbolo)
{
    public string Texto => $"{Nombre} ({Simbolo})";
}

/// <summary>Las 16 conversiones del servidor, agrupadas por categoría.</summary>
public static class Catalogo
{
    public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
    {
        new(Categoria.Temperatura, "Celsius", "°C", "Fahrenheit", "°F", (s, v) => s.CelsiusAFahrenheitAsync(v)),
        new(Categoria.Temperatura, "Fahrenheit", "°F", "Celsius", "°C", (s, v) => s.FahrenheitACelsiusAsync(v)),
        new(Categoria.Temperatura, "Celsius", "°C", "Kelvin", "K", (s, v) => s.CelsiusAKelvinAsync(v)),
        new(Categoria.Temperatura, "Kelvin", "K", "Celsius", "°C", (s, v) => s.KelvinACelsiusAsync(v)),
        new(Categoria.Temperatura, "Fahrenheit", "°F", "Kelvin", "K", (s, v) => s.FahrenheitAKelvinAsync(v)),
        new(Categoria.Temperatura, "Kelvin", "K", "Fahrenheit", "°F", (s, v) => s.KelvinAFahrenheitAsync(v)),

        new(Categoria.Longitud, "Metros", "m", "Pies", "ft", (s, v) => s.MetrosAPiesAsync(v)),
        new(Categoria.Longitud, "Pies", "ft", "Metros", "m", (s, v) => s.PiesAMetrosAsync(v)),
        new(Categoria.Longitud, "Metros", "m", "Pulgadas", "in", (s, v) => s.MetrosAPulgadasAsync(v)),
        new(Categoria.Longitud, "Pulgadas", "in", "Metros", "m", (s, v) => s.PulgadasAMetrosAsync(v)),
        new(Categoria.Longitud, "Kilómetros", "km", "Millas", "mi", (s, v) => s.KilometrosAMillasAsync(v)),
        new(Categoria.Longitud, "Millas", "mi", "Kilómetros", "km", (s, v) => s.MillasAKilometrosAsync(v)),

        new(Categoria.Masa, "Kilogramos", "kg", "Libras", "lb", (s, v) => s.KilogramosALibrasAsync(v)),
        new(Categoria.Masa, "Libras", "lb", "Kilogramos", "kg", (s, v) => s.LibrasAKilogramosAsync(v)),
        new(Categoria.Masa, "Gramos", "g", "Onzas", "oz", (s, v) => s.GramosAOnzasAsync(v)),
        new(Categoria.Masa, "Onzas", "oz", "Gramos", "g", (s, v) => s.OnzasAGramosAsync(v)),
    };

    public static IList<Conversion> DeCategoria(Categoria categoria) =>
        Todas.Where(c => c.Categoria == categoria).ToList();

    public static IList<Unidad> UnidadesOrigen(Categoria categoria) =>
        DeCategoria(categoria)
            .GroupBy(c => c.SimboloOrigen)
            .Select(g => new Unidad(g.First().UnidadOrigen, g.Key))
            .ToList();

    public static IList<Unidad> UnidadesDestino(Categoria categoria, string simboloOrigen) =>
        DeCategoria(categoria)
            .Where(c => c.SimboloOrigen == simboloOrigen)
            .Select(c => new Unidad(c.UnidadDestino, c.SimboloDestino))
            .ToList();

    public static Conversion? Buscar(Categoria categoria, string? origen, string? destino) =>
        Todas.FirstOrDefault(c => c.Categoria == categoria && c.SimboloOrigen == origen && c.SimboloDestino == destino);

    /// <summary>Acepta punto o coma decimal.</summary>
    public static bool IntentarLeerNumero(string? texto, out double valor)
    {
        string normalizado = (texto ?? string.Empty).Trim().Replace(',', '.');
        return double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out valor)
            && double.IsFinite(valor);
    }

    /// <summary>Hasta 6 decimales, sin ceros sobrantes.</summary>
    public static string Formatear(double numero) =>
        Math.Round(numero, 6).ToString("0.######", CultureInfo.InvariantCulture);
}
