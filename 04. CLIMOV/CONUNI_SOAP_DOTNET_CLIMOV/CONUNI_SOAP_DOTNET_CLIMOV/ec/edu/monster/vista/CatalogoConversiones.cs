using ec.edu.monster.modelo;

namespace ec.edu.monster.vista;

/// <summary>
/// Opciones que muestran las listas: las 16 operaciones del servicio, agrupadas por categoría.
/// No calcula nada: cada opción solo dice qué operación llamar.
/// </summary>
public static class CatalogoConversiones
{
    private static readonly Unidad Celsius = U("Celsius", "°C");
    private static readonly Unidad Fahrenheit = U("Fahrenheit", "°F");
    private static readonly Unidad Kelvin = U("Kelvin", "K");
    private static readonly Unidad Metros = U("Metros", "m");
    private static readonly Unidad Pies = U("Pies", "ft");
    private static readonly Unidad Pulgadas = U("Pulgadas", "in");
    private static readonly Unidad Kilometros = U("Kilómetros", "km");
    private static readonly Unidad Millas = U("Millas", "mi");
    private static readonly Unidad Kilogramos = U("Kilogramos", "kg");
    private static readonly Unidad Libras = U("Libras", "lb");
    private static readonly Unidad Gramos = U("Gramos", "g");
    private static readonly Unidad Onzas = U("Onzas", "oz");

    public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
    {
        C(Categoria.Temperatura, Celsius, Fahrenheit, "celsiusAFahrenheit"),
        C(Categoria.Temperatura, Fahrenheit, Celsius, "fahrenheitACelsius"),
        C(Categoria.Temperatura, Celsius, Kelvin, "celsiusAKelvin"),
        C(Categoria.Temperatura, Kelvin, Celsius, "kelvinACelsius"),
        C(Categoria.Temperatura, Fahrenheit, Kelvin, "fahrenheitAKelvin"),
        C(Categoria.Temperatura, Kelvin, Fahrenheit, "kelvinAFahrenheit"),

        C(Categoria.Longitud, Metros, Pies, "metrosAPies"),
        C(Categoria.Longitud, Pies, Metros, "piesAMetros"),
        C(Categoria.Longitud, Metros, Pulgadas, "metrosAPulgadas"),
        C(Categoria.Longitud, Pulgadas, Metros, "pulgadasAMetros"),
        C(Categoria.Longitud, Kilometros, Millas, "kilometrosAMillas"),
        C(Categoria.Longitud, Millas, Kilometros, "millasAKilometros"),

        C(Categoria.Masa, Kilogramos, Libras, "kilogramosALibras"),
        C(Categoria.Masa, Libras, Kilogramos, "librasAKilogramos"),
        C(Categoria.Masa, Gramos, Onzas, "gramosAOnzas"),
        C(Categoria.Masa, Onzas, Gramos, "onzasAGramos")
    };

    public static List<Unidad> UnidadesOrigen(Categoria categoria) =>
        Todas.Where(c => c.Categoria == categoria).Select(c => c.Origen).Distinct().ToList();

    public static List<Unidad> UnidadesDestino(Categoria categoria, Unidad origen) =>
        Todas.Where(c => c.Categoria == categoria && c.Origen == origen).Select(c => c.Destino).ToList();

    public static Conversion? Buscar(Unidad origen, Unidad destino) =>
        Todas.FirstOrDefault(c => c.Origen == origen && c.Destino == destino);

    private static Unidad U(string nombre, string simbolo) =>
        new() { Nombre = nombre, Simbolo = simbolo, Texto = $"{nombre} ({simbolo})" };

    private static Conversion C(Categoria categoria, Unidad origen, Unidad destino, string operacion) =>
        new() { Categoria = categoria, Origen = origen, Destino = destino, Operacion = operacion };
}
