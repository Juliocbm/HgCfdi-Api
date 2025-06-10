namespace HG.CFDI.CORE.Models;

public class ParametrosGenerales
{
    private string _ordenarPor = "FechaCreacion";
    private bool _descending = false;
    private int _noPagina = 1;
    private int _tamanoPagina = 5;
    private bool _activos = true;
    private string _rangoFechas;

    public string OrdenarPor
    {
        get => _ordenarPor;
        set => _ordenarPor = value;
    }

    public bool Descending
    {
        get => _descending;
        set => _descending = value;
    }

    public int NoPagina
    {
        get => _noPagina;
        set => _noPagina = value;
    }

    public int TamanoPagina
    {
        get => _tamanoPagina;
        set => _tamanoPagina = value;
    }

    public bool Activos
    {
        get => _activos;
        set => _activos = value;
    }

    public int IdCompania { get; set; }
    public Dictionary<string, string>? filtrosIniciales { get; set; }
    public Dictionary<string, string>? filtrosPorColumna { get; set; }
    public string? multiIds { get; set; }
    public string? actionMulti { get; set; }
    public string? rangoFechas
    {
        get => _rangoFechas;
        set => _rangoFechas = value;
    }

    public ParametrosGenerales()
    {
        // Fecha actual
        DateTime fechaActual = DateTime.Today;

        // Fecha una semana atrás
        DateTime fechaUnaSemanaAtras = fechaActual.AddDays(-7);

        // Formatear las fechas al formato dd/MM/yyyy
        string fechaInicio = fechaUnaSemanaAtras.ToString("dd/MM/yyyy");
        string fechaFin = fechaActual.ToString("dd/MM/yyyy");

        // Asignar el rango de fechas
        _rangoFechas = $"{fechaInicio}-{fechaFin}";
    }

}

