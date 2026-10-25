using System.Collections.Generic;
using System.Linq;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista
{
    /// <summary>
    /// Opciones que muestra el menú: las 16 operaciones del servicio, agrupadas por categoría,
    /// con los nombres y símbolos de las unidades. No calcula nada: cada opción solo dice qué operación llamar.
    /// </summary>
    public static class CatalogoConversiones
    {
        public static readonly IReadOnlyList<Conversion> Todas = new List<Conversion>
        {
            Nueva(Categoria.Temperatura, "celsiusAFahrenheit", "Celsius", "°C", "Fahrenheit", "°F"),
            Nueva(Categoria.Temperatura, "fahrenheitACelsius", "Fahrenheit", "°F", "Celsius", "°C"),
            Nueva(Categoria.Temperatura, "celsiusAKelvin", "Celsius", "°C", "Kelvin", "K"),
            Nueva(Categoria.Temperatura, "kelvinACelsius", "Kelvin", "K", "Celsius", "°C"),
            Nueva(Categoria.Temperatura, "fahrenheitAKelvin", "Fahrenheit", "°F", "Kelvin", "K"),
            Nueva(Categoria.Temperatura, "kelvinAFahrenheit", "Kelvin", "K", "Fahrenheit", "°F"),

            Nueva(Categoria.Longitud, "metrosAPies", "Metros", "m", "Pies", "ft"),
            Nueva(Categoria.Longitud, "piesAMetros", "Pies", "ft", "Metros", "m"),
            Nueva(Categoria.Longitud, "metrosAPulgadas", "Metros", "m", "Pulgadas", "in"),
            Nueva(Categoria.Longitud, "pulgadasAMetros", "Pulgadas", "in", "Metros", "m"),
            Nueva(Categoria.Longitud, "kilometrosAMillas", "Kilómetros", "km", "Millas", "mi"),
            Nueva(Categoria.Longitud, "millasAKilometros", "Millas", "mi", "Kilómetros", "km"),

            Nueva(Categoria.Masa, "kilogramosALibras", "Kilogramos", "kg", "Libras", "lb"),
            Nueva(Categoria.Masa, "librasAKilogramos", "Libras", "lb", "Kilogramos", "kg"),
            Nueva(Categoria.Masa, "gramosAOnzas", "Gramos", "g", "Onzas", "oz"),
            Nueva(Categoria.Masa, "onzasAGramos", "Onzas", "oz", "Gramos", "g"),
        };

        public static IList<Conversion> DeCategoria(Categoria categoria) =>
            Todas.Where(c => c.Categoria == categoria).ToList();

        /// <summary>Texto de la opción: "Celsius (°C) → Fahrenheit (°F)".</summary>
        public static string Descripcion(Conversion c) =>
            $"{c.UnidadOrigen} ({c.SimboloOrigen}) → {c.UnidadDestino} ({c.SimboloDestino})";

        private static Conversion Nueva(Categoria categoria, string operacion,
            string unidadOrigen, string simboloOrigen, string unidadDestino, string simboloDestino) =>
            new Conversion
            {
                Categoria = categoria,
                Operacion = operacion,
                UnidadOrigen = unidadOrigen,
                SimboloOrigen = simboloOrigen,
                UnidadDestino = unidadDestino,
                SimboloDestino = simboloDestino
            };
    }
}
