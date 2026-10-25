namespace ec.edu.monster.modelo;

public enum Categoria
{
    Temperatura,
    Longitud,
    Masa
}

/// <summary>Unidad de medida. Solo datos (get/set).</summary>
public class Unidad
{
    public string Nombre { get; set; } = "";
    public string Simbolo { get; set; } = "";
    /// <summary>Texto que muestra la lista: "Celsius (°C)".</summary>
    public string Texto { get; set; } = "";
}

/// <summary>
/// Una conversión que ofrece el servicio. Solo datos (get/set): las fórmulas y las validaciones
/// están en el servidor. <see cref="Operacion"/> es el nombre EXACTO de la operación SOAP.
/// </summary>
public class Conversion
{
    public Categoria Categoria { get; set; }
    public Unidad Origen { get; set; } = new();
    public Unidad Destino { get; set; } = new();
    public string Operacion { get; set; } = "";
}
