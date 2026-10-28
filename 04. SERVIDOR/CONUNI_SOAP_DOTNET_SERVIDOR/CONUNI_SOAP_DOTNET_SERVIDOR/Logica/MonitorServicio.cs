using System;
using System.Collections.Generic;
using System.Linq;

namespace ConversorUnidades.Servidor.Logica
{
    /// <summary>
    /// Registra en memoria cada operación que consumen los clientes (svc y asmx):
    /// hora, IP del cliente, operación, valor de entrada, resultado o error y duración.
    /// Patrón Singleton: una sola instancia junta las llamadas de todas las peticiones;
    /// la página Monitor.ashx la lee para mostrar el consumo en vivo.
    /// Sin base de datos: al reiniciar el sitio el registro empieza de cero.
    /// </summary>
    public sealed class MonitorServicio
    {
        private const int MaximoRegistros = 300;

        private static readonly Lazy<MonitorServicio> _instancia =
            new Lazy<MonitorServicio>(() => new MonitorServicio());

        public static MonitorServicio Instancia => _instancia.Value;

        private readonly LinkedList<RegistroLlamada> _registros = new LinkedList<RegistroLlamada>();
        private readonly Dictionary<string, int> _porOperacion = new Dictionary<string, int>();
        private readonly Dictionary<string, DateTime> _clientes = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, ConsumoCliente> _porTipo = new Dictionary<string, ConsumoCliente>();
        private int _total;
        private int _errores;

        public DateTime Inicio { get; } = DateTime.Now;

        private MonitorServicio() { }

        public void Registrar(RegistroLlamada registro)
        {
            lock (_registros)
            {
                _registros.AddFirst(registro);
                if (_registros.Count > MaximoRegistros) _registros.RemoveLast();

                _total++;
                if (!registro.Exito) _errores++;
                _porOperacion.TryGetValue(registro.Operacion, out int veces);
                _porOperacion[registro.Operacion] = veces + 1;
                _clientes[$"{registro.TipoCliente} · {registro.Cliente}"] = registro.Hora;
                if (!_porTipo.TryGetValue(registro.TipoCliente, out ConsumoCliente consumo))
                {
                    consumo = new ConsumoCliente();
                    _porTipo[registro.TipoCliente] = consumo;
                }
                consumo.Llamadas++;
                if (!registro.Exito) consumo.Errores++;
                consumo.Ultima = registro.Hora;
            }
        }

        /// <summary>Copia de los datos para mostrarlos sin bloquear a los que siguen llamando.</summary>
        public ResumenMonitor Resumen()
        {
            lock (_registros)
            {
                return new ResumenMonitor
                {
                    Inicio = Inicio,
                    Total = _total,
                    Errores = _errores,
                    Registros = _registros.ToList(),
                    PorOperacion = _porOperacion.OrderByDescending(p => p.Value).ToList(),
                    Clientes = _clientes.OrderByDescending(c => c.Value).ToList(),
                    PorTipo = _porTipo.ToDictionary(p => p.Key, p => new ConsumoCliente
                    {
                        Llamadas = p.Value.Llamadas, Errores = p.Value.Errores, Ultima = p.Value.Ultima,
                    }),
                };
            }
        }

        public void Limpiar()
        {
            lock (_registros)
            {
                _registros.Clear();
                _porOperacion.Clear();
                _clientes.Clear();
                _porTipo.Clear();
                _total = 0;
                _errores = 0;
            }
        }
    }

    public sealed class RegistroLlamada
    {
        public DateTime Hora { get; set; }
        public string Cliente { get; set; }
        public string TipoCliente { get; set; }
        public string Tecnologia { get; set; }
        public string Operacion { get; set; }
        public string Entrada { get; set; }
        public string Resultado { get; set; }
        public bool Exito { get; set; }
        public double Milisegundos { get; set; }
    }

    public sealed class ResumenMonitor
    {
        public DateTime Inicio { get; set; }
        public int Total { get; set; }
        public int Errores { get; set; }
        public List<RegistroLlamada> Registros { get; set; }
        public List<KeyValuePair<string, int>> PorOperacion { get; set; }
        public List<KeyValuePair<string, DateTime>> Clientes { get; set; }
        public Dictionary<string, ConsumoCliente> PorTipo { get; set; }
    }

    public sealed class ConsumoCliente
    {
        public int Llamadas { get; set; }
        public int Errores { get; set; }
        public DateTime Ultima { get; set; }
    }
}
