namespace PM2E10093.Models
{
    public class Validaciones
    {
        public bool Valido { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public static Validaciones Success() => new() { Valido = true };

        public static Validaciones Fail(string mensaje) => new()
        {
            Valido = false,
            Mensaje = mensaje
        };
    }
}
