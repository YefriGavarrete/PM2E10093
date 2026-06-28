using PM2E10093.Models;

namespace PM2E10093.Data
{
    public interface SitioDatabase
    {
        Task InitializeAsync();

        Task<int> GuardarAsync(Sitio sitio);

        Task<List<Sitio>> ObtenerAsync();

        Task<int> EliminarAsync(Sitio sitio);
    }
}
