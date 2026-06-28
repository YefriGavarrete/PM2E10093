using SQLite;

namespace PM2E10093.Models
{
    public class Sitio
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        public double Latitud { get; set; }

        public double Longitud { get; set; }

        public string ImagenPath { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
