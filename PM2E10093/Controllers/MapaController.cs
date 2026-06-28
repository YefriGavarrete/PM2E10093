using PM2E10093.Models;

namespace PM2E10093.Controllers
{
    public class MapaController
    {
        public async Task ShareImageAsync(Sitio sitio)
        {
            if (string.IsNullOrWhiteSpace(sitio.ImagenPath) || !File.Exists(sitio.ImagenPath))
            {
                return;
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = sitio.Descripcion,
                File = new ShareFile(sitio.ImagenPath)
            });
        }

        public Task ShowSiteOnMapAsync(Sitio sitio)
        {
            var location = new Location(sitio.Latitud, sitio.Longitud);
            var options = new MapLaunchOptions
            {
                Name = sitio.Descripcion,
                NavigationMode = NavigationMode.None
            };

            return Map.Default.OpenAsync(location, options);
        }
    }
}
