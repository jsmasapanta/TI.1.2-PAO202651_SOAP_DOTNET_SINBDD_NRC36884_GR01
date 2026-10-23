namespace ConversorUnidades.Movil.Modelo;

public enum Categoria
{
    Temperatura,
    Longitud,
    Masa
}

public record Unidad(string Nombre, string Simbolo)
{
    public override string ToString() => $"{Nombre} ({Simbolo})";
}

/// <summary>
/// Una conversión disponible. <see cref="Operacion"/> es el nombre EXACTO de la operación SOAP del servidor.
/// </summary>
public record Conversion(Categoria Categoria, Unidad Origen, Unidad Destino, string Operacion);

/// <summary>Las 16 conversiones del servidor, agrupadas por categoría.</summary>
public static class Catalogo
{
    private static readonly Unidad Celsius = new("Celsius", "°C");
    private static readonly Unidad Fahrenheit = new("Fahrenheit", "°F");
    private static readonly Unidad Kelvin = new("Kelvin", "K");
    private static readonly Unidad Metros = new("Metros", "m");
    private static readonly Unidad Pies = new("Pies", "ft");
    private static readonly Unidad Pulgadas = new("Pulgadas", "in");
    private static readonly Unidad Kilometros = new("Kilómetros", "km");
    private static readonly Unidad Millas = new("Millas", "mi");
    private static readonly Unidad Kilogramos = new("Kilogramos", "kg");
    private static readonly Unidad Libras = new("Libras", "lb");
    private static readonly Unidad Gramos = new("Gramos", "g");
    private static readonly Unidad Onzas = new("Onzas", "oz");

    public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
    {
        new(Categoria.Temperatura, Celsius, Fahrenheit, "celsiusAFahrenheit"),
        new(Categoria.Temperatura, Fahrenheit, Celsius, "fahrenheitACelsius"),
        new(Categoria.Temperatura, Celsius, Kelvin, "celsiusAKelvin"),
        new(Categoria.Temperatura, Kelvin, Celsius, "kelvinACelsius"),
        new(Categoria.Temperatura, Fahrenheit, Kelvin, "fahrenheitAKelvin"),
        new(Categoria.Temperatura, Kelvin, Fahrenheit, "kelvinAFahrenheit"),

        new(Categoria.Longitud, Metros, Pies, "metrosAPies"),
        new(Categoria.Longitud, Pies, Metros, "piesAMetros"),
        new(Categoria.Longitud, Metros, Pulgadas, "metrosAPulgadas"),
        new(Categoria.Longitud, Pulgadas, Metros, "pulgadasAMetros"),
        new(Categoria.Longitud, Kilometros, Millas, "kilometrosAMillas"),
        new(Categoria.Longitud, Millas, Kilometros, "millasAKilometros"),

        new(Categoria.Masa, Kilogramos, Libras, "kilogramosALibras"),
        new(Categoria.Masa, Libras, Kilogramos, "librasAKilogramos"),
        new(Categoria.Masa, Gramos, Onzas, "gramosAOnzas"),
        new(Categoria.Masa, Onzas, Gramos, "onzasAGramos")
    };

    public static List<Unidad> UnidadesOrigen(Categoria categoria) =>
        Todas.Where(c => c.Categoria == categoria).Select(c => c.Origen).Distinct().ToList();

    public static List<Unidad> UnidadesDestino(Categoria categoria, Unidad origen) =>
        Todas.Where(c => c.Categoria == categoria && c.Origen == origen).Select(c => c.Destino).ToList();

    public static Conversion? Buscar(Unidad origen, Unidad destino) =>
        Todas.FirstOrDefault(c => c.Origen == origen && c.Destino == destino);

    public static string Simbolo(Categoria categoria) => categoria switch
    {
        Categoria.Temperatura => "°C",
        Categoria.Longitud => "m",
        _ => "kg"
    };
}
