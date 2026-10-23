using System;
using ConversorUnidades.Consola.Servicio;

namespace ConversorUnidades.Consola.Modelo
{
    public enum Categoria
    {
        Temperatura,
        Longitud,
        Masa
    }

    /// <summary>
    /// Una conversión disponible: de qué unidad a cuál, y qué operación del servicio la resuelve.
    /// </summary>
    public class Conversion
    {
        public Categoria Categoria { get; }
        public string UnidadOrigen { get; }
        public string SimboloOrigen { get; }
        public string UnidadDestino { get; }
        public string SimboloDestino { get; }
        public Func<IWSConversion, double, double> Operacion { get; }

        public Conversion(Categoria categoria,
                          string unidadOrigen, string simboloOrigen,
                          string unidadDestino, string simboloDestino,
                          Func<IWSConversion, double, double> operacion)
        {
            Categoria = categoria;
            UnidadOrigen = unidadOrigen;
            SimboloOrigen = simboloOrigen;
            UnidadDestino = unidadDestino;
            SimboloDestino = simboloDestino;
            Operacion = operacion;
        }

        public string Descripcion => $"{UnidadOrigen} ({SimboloOrigen}) → {UnidadDestino} ({SimboloDestino})";
    }
}
