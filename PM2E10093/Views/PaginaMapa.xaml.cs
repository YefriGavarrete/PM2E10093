using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using PM2E10093.Models;

namespace PM2E10093.Views;

public partial class PaginaMapa : ContentPage, IQueryAttributable
{
    private Sitio? sitioSeleccionado;

    public PaginaMapa()
    {
        InitializeComponent();
    }

    public PaginaMapa(Sitio sitio) : this()
    {
        sitioSeleccionado = sitio;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Sitio", out object? value) && value is Sitio sitio)
        {
            sitioSeleccionado = sitio;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await ValidarGpsAsync();
        CargarSitioEnMapa();
    }

    private void CargarSitioEnMapa()
    {
        if (sitioSeleccionado is null)
        {
            lblTituloSitio.Text = "No hay sitio seleccionado";
            MostrarPanelPin(false);
            return;
        }

        if (!CoordenadasValidas(sitioSeleccionado.Latitud, sitioSeleccionado.Longitud))
        {
            lblTituloSitio.Text = "Coordenadas invalidas";
            MostrarPanelPin(false);
            DisplayAlert("Mapa", "El sitio seleccionado no tiene latitud y longitud validas.", "OK");
            return;
        }

        var ubicacion = new Location(sitioSeleccionado.Latitud, sitioSeleccionado.Longitud);
        string descripcionCorta = ObtenerDescripcionCorta(sitioSeleccionado.Descripcion);

        lblTituloSitio.Text = descripcionCorta;
        lblPin.Text = $"Pin: {descripcionCorta}";
        MostrarPanelPin(true);



        MapaSitio.Pins.Clear();
        MapaSitio.Pins.Add(new Pin
        {
            Label = descripcionCorta,
            Address = sitioSeleccionado.Descripcion,
            Type = PinType.Place,
            Location = ubicacion
        });

        MapaSitio.MoveToRegion(MapSpan.FromCenterAndRadius(
            ubicacion,
            Distance.FromKilometers(1)));
    }

    private async Task ValidarGpsAsync()
    {
        try
        {
            PermissionStatus permiso = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permiso != PermissionStatus.Granted)
            {
                permiso = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (permiso != PermissionStatus.Granted)
            {
                MapaSitio.IsShowingUser = false;
                await DisplayAlert("Permiso", "Debe permitir la ubicacion para mostrar su posicion en el mapa.", "OK");
                return;
            }

            MapaSitio.IsShowingUser = true;

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            Location? ubicacionActual = await Geolocation.Default.GetLocationAsync(request);

            if (ubicacionActual is null)
            {
                await DisplayAlert("GPS", "No se pudo obtener la ubicacion actual del usuario.", "OK");
            }
        }
        catch (FeatureNotEnabledException)
        {
            MapaSitio.IsShowingUser = false;
            await DisplayAlert("GPS inactivo", "Active el GPS para mostrar su ubicacion en el mapa.", "OK");
        }
        catch (PermissionException)
        {
            MapaSitio.IsShowingUser = false;
            await DisplayAlert("Permiso", "Debe permitir la ubicacion para usar IsShowingUser.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("GPS", $"No se pudo validar el GPS: {ex.Message}", "OK");
        }
    }

    private async void OnCompartirImagenClicked(object sender, EventArgs e)
    {
        if (sitioSeleccionado is null)
        {
            await DisplayAlert("Compartir", "No hay un sitio seleccionado.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(sitioSeleccionado.ImagenPath) || !File.Exists(sitioSeleccionado.ImagenPath))
        {
            await DisplayAlert("Compartir", "La imagen del sitio no existe o no esta disponible.", "OK");
            return;
        }

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = sitioSeleccionado.Descripcion,
            File = new ShareFile(sitioSeleccionado.ImagenPath, ObtenerMimeType(sitioSeleccionado.ImagenPath))
        });
    }

    private async void OnAtrasClicked(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
            return;
        }

        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private void MostrarPanelPin(bool visible)
    {
        Border? panel = this.FindByName<Border>("PinPanel");
        if (panel is not null)
        {
            panel.IsVisible = visible;
        }
    }

    private static string ObtenerDescripcionCorta(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return "Sitio visitado";
        }

        descripcion = descripcion.Trim();
        return descripcion.Length <= 30 ? descripcion : descripcion[..30];
    }

    private static bool CoordenadasValidas(double latitud, double longitud)
    {
        return latitud >= -90 && latitud <= 90 && longitud >= -180 && longitud <= 180;
    }

    private static string ObtenerMimeType(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }
}

