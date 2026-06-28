using System.Globalization;
using PM2E10093.Controllers;
using PM2E10093.Data;

namespace PM2E10093.Views;

public partial class PaginaInicio : ContentPage
{
    private readonly SitiosController sitiosController;
    private string? imagenPath;
    private double? latitud;
    private double? longitud;
    private bool ubicacionSolicitada;

    public PaginaInicio()
    {
        InitializeComponent();
        sitiosController = new SitiosController(new SqliteSitios());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!ubicacionSolicitada)
        {
            ubicacionSolicitada = true;
            await ObtenerUbicacionAsync();
        }
    }

    private async void OnTomarImagenClicked(object sender, EventArgs e)
    {
        try
        {
            FileResult? foto = await MediaPicker.Default.CapturePhotoAsync();

            if (foto is null)
            {
                return;
            }

            imagenPath = await GuardarImagenAsync(foto);
            imgSitio.Source = ImageSource.FromFile(imagenPath);
            lblImagenPendiente.IsVisible = false;
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Camara", "La cámara no esta disponible en este dispositivo.", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permiso", "Debe permitir el uso de la cámara para tomar la imagen.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo tomar la imagen: {ex.Message}", "OK");
        }
    }

    private async void OnAgregarClicked(object sender, EventArgs e)
    {
        var validacion = sitiosController.ValidarEntradas(txtDescripcion.Text, imagenPath, latitud, longitud);

        if (!validacion.Valido)
        {
            await DisplayAlert("Validacion", validacion.Mensaje, "OK");
            return;
        }

        try
        {
            var sitio = sitiosController.CreateSiteDraft(
                txtDescripcion.Text,
                imagenPath!,
                latitud!.Value,
                longitud!.Value);

            await sitiosController.GuardarAsync(sitio);
            await DisplayAlert("Sitio", "Registro agregado correctamente.", "OK");

            LimpiarFormulario();
            await ObtenerUbicacionAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar el sitio: {ex.Message}", "OK");
        }
    }

    private async void OnListarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PaginaListaSitios));
    }

    private void OnSalirClicked(object sender, EventArgs e)
    {
        Application.Current?.Quit();
    }

    private async Task ObtenerUbicacionAsync()
    {
        try
        {
            lblEstadoGps.Text = "Obteniendo ubicacion actual...";

            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            Location? location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                latitud = null;
                longitud = null;
                txtLatitud.Text = string.Empty;
                txtLongitud.Text = string.Empty;
                lblEstadoGps.Text = "No se pudo obtener la ubicacion.";
                await DisplayAlert("GPS", "No se pudo obtener la latitud y longitud.", "OK");
                return;
            }

            latitud = location.Latitude;
            longitud = location.Longitude;
            txtLatitud.Text = latitud.Value.ToString("F6", CultureInfo.InvariantCulture);
            txtLongitud.Text = longitud.Value.ToString("F6", CultureInfo.InvariantCulture);
            lblEstadoGps.Text = "Ubicacion obtenida automaticamente.";
        }
        catch (FeatureNotEnabledException)
        {
            latitud = null;
            longitud = null;
            txtLatitud.Text = string.Empty;
            txtLongitud.Text = string.Empty;
            lblEstadoGps.Text = "GPS inactivo.";
            await DisplayAlert("GPS inactivo", "Active el GPS para registrar el sitio.", "OK");
        }
        catch (PermissionException)
        {
            lblEstadoGps.Text = "Permiso de ubicacion denegado.";
            await DisplayAlert("Permiso", "Debe permitir la ubicacion para registrar el sitio.", "OK");
        }
        catch (Exception ex)
        {
            lblEstadoGps.Text = "Error al obtener ubicacion.";
            await DisplayAlert("GPS", $"No se pudo obtener la ubicacion: {ex.Message}", "OK");
        }
    }

    private static async Task<string> GuardarImagenAsync(FileResult foto)
    {
        string folder = Path.Combine(FileSystem.AppDataDirectory, "imagenes_sitios");
        Directory.CreateDirectory(folder);

        string extension = Path.GetExtension(foto.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        string destino = Path.Combine(folder, $"sitio_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}");

        await using Stream origen = await foto.OpenReadAsync();
        await using FileStream salida = File.OpenWrite(destino);
        await origen.CopyToAsync(salida);

        return destino;
    }

    private void LimpiarFormulario()
    {
        txtDescripcion.Text = string.Empty;
        imagenPath = null;
        imgSitio.Source = null;
        lblImagenPendiente.IsVisible = true;
        ubicacionSolicitada = false;
    }
}
