using PM2E10093.Data;
using PM2E10093.Models;

namespace PM2E10093.Controllers
{
    public class SitiosController
    {
        private readonly SitioDatabase sitioDatabase;

        public SitiosController(SitioDatabase sitioDatabase)
        {
            this.sitioDatabase = sitioDatabase;
        }

        public Validaciones ValidarEntradas(string descripcion, string? imagenPath, double? latitud, double? longitud)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                return Validaciones.Fail("Ingrese una descripción del sitio.");
            }

            if (string.IsNullOrWhiteSpace(imagenPath))
            {
                return Validaciones.Fail("Debe tomar una imagen del sitio.");
            }

            if (!latitud.HasValue || !longitud.HasValue)
            {
                return Validaciones.Fail("No se pudo obtener la latitud y longitud del sitio.");
            }

            return Validaciones.Success();
        }

        public Sitio CrearSitio(string descripcion, string imagenPath, double latitud, double longitud)
        {
            return new Sitio
            {
                Descripcion = descripcion.Trim(),
                ImagenPath = imagenPath,
                Latitud = latitud,
                Longitud = longitud,
                FechaCreacion = DateTime.Now
            };
        }

        public Task<int> GuardarAsync(Sitio sitio)
        {
            return sitioDatabase.GuardarAsync(sitio);
        }

        public Task<List<Sitio>> GetAllAsync()
        {
            return sitioDatabase.ObtenerAsync();
        }

        public Task<int> EliminarAsync(Sitio sitio)
        {
            return sitioDatabase.EliminarAsync(sitio);
        }
    }
}
