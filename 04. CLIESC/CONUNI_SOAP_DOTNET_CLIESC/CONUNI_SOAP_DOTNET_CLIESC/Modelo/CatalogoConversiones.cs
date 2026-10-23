using System.Collections.Generic;
using System.Linq;

namespace ConversorUnidades.Escritorio.Modelo
{
    /// <summary>Unidad de medida que se muestra en las listas desplegables.</summary>
    public class Unidad
    {
        public string Nombre { get; }
        public string Simbolo { get; }

        public Unidad(string nombre, string simbolo)
        {
            Nombre = nombre;
            Simbolo = simbolo;
        }

        public override string ToString() => $"{Nombre} ({Simbolo})";
    }

    /// <summary>
    /// Las 16 conversiones que ofrece el servidor, agrupadas por categoría.
    /// </summary>
    public static class CatalogoConversiones
    {
        public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
        {
            new Conversion(Categoria.Temperatura, "Celsius", "°C", "Fahrenheit", "°F", (s, v) => s.CelsiusAFahrenheit(v)),
            new Conversion(Categoria.Temperatura, "Fahrenheit", "°F", "Celsius", "°C", (s, v) => s.FahrenheitACelsius(v)),
            new Conversion(Categoria.Temperatura, "Celsius", "°C", "Kelvin", "K", (s, v) => s.CelsiusAKelvin(v)),
            new Conversion(Categoria.Temperatura, "Kelvin", "K", "Celsius", "°C", (s, v) => s.KelvinACelsius(v)),
            new Conversion(Categoria.Temperatura, "Fahrenheit", "°F", "Kelvin", "K", (s, v) => s.FahrenheitAKelvin(v)),
            new Conversion(Categoria.Temperatura, "Kelvin", "K", "Fahrenheit", "°F", (s, v) => s.KelvinAFahrenheit(v)),

            new Conversion(Categoria.Longitud, "Metros", "m", "Pies", "ft", (s, v) => s.MetrosAPies(v)),
            new Conversion(Categoria.Longitud, "Pies", "ft", "Metros", "m", (s, v) => s.PiesAMetros(v)),
            new Conversion(Categoria.Longitud, "Metros", "m", "Pulgadas", "in", (s, v) => s.MetrosAPulgadas(v)),
            new Conversion(Categoria.Longitud, "Pulgadas", "in", "Metros", "m", (s, v) => s.PulgadasAMetros(v)),
            new Conversion(Categoria.Longitud, "Kilómetros", "km", "Millas", "mi", (s, v) => s.KilometrosAMillas(v)),
            new Conversion(Categoria.Longitud, "Millas", "mi", "Kilómetros", "km", (s, v) => s.MillasAKilometros(v)),

            new Conversion(Categoria.Masa, "Kilogramos", "kg", "Libras", "lb", (s, v) => s.KilogramosALibras(v)),
            new Conversion(Categoria.Masa, "Libras", "lb", "Kilogramos", "kg", (s, v) => s.LibrasAKilogramos(v)),
            new Conversion(Categoria.Masa, "Gramos", "g", "Onzas", "oz", (s, v) => s.GramosAOnzas(v)),
            new Conversion(Categoria.Masa, "Onzas", "oz", "Gramos", "g", (s, v) => s.OnzasAGramos(v)),
        };

        public static IList<Conversion> DeCategoria(Categoria categoria)
        {
            return Todas.Where(c => c.Categoria == categoria).ToList();
        }

        /// <summary>Unidades que se pueden usar como origen en la categoría.</summary>
        public static IList<Unidad> UnidadesOrigen(Categoria categoria)
        {
            return DeCategoria(categoria)
                .GroupBy(c => c.SimboloOrigen)
                .Select(g => new Unidad(g.First().UnidadOrigen, g.Key))
                .ToList();
        }

        /// <summary>Unidades a las que el servidor sabe convertir desde la unidad de origen.</summary>
        public static IList<Unidad> UnidadesDestino(Categoria categoria, string simboloOrigen)
        {
            return DeCategoria(categoria)
                .Where(c => c.SimboloOrigen == simboloOrigen)
                .Select(c => new Unidad(c.UnidadDestino, c.SimboloDestino))
                .ToList();
        }

        public static Conversion Buscar(string simboloOrigen, string simboloDestino)
        {
            return Todas.FirstOrDefault(c => c.SimboloOrigen == simboloOrigen && c.SimboloDestino == simboloDestino);
        }
    }
}
