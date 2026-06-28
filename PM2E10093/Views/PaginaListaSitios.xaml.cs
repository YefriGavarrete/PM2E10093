using PM2E10093.Controllers;
using PM2E10093.Data;
using PM2E10093.Models;

namespace PM2E10093.Views;

public partial class PaginaListaSitios : ContentPage
{
    private readonly SitiosController sitiosController;
    private readonly MapaController mapaController = new();

    public PaginaListaSitios()
    {
        InitializeComponent();
        sitiosController = new SitiosController(new SqliteSitios());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarSitiosAsync();
    }

    private async Task CargarSitiosAsync()
    {
        SitiosCollection.ItemsSource = await sitiosController.GetAllAsync();
    }

    private async void OnMapaClicked(object sender, EventArgs e)
    {
        if (ObtenerSitio(sender) is not Sitio sitio)
        {
            return;
        }

        try
        {
            await mapaController.ShowSiteOnMapAsync(sitio);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Mapa", $"No se pudo abrir el mapa: {ex.Message}", "OK");
        }
    }

    private async void OnCompartirClicked(object sender, EventArgs e)
    {
        if (ObtenerSitio(sender) is not Sitio sitio)
        {
            return;
        }

        try
        {
            await mapaController.ShareImageAsync(sitio);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Compartir", $"No se pudo compartir la imagen: {ex.Message}", "OK");
        }
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (ObtenerSitio(sender) is not Sitio sitio)
        {
            return;
        }

        bool confirmar = await DisplayAlert("Eliminar", "Desea eliminar este sitio?", "Si", "No");
        if (!confirmar)
        {
            return;
        }

        await sitiosController.EliminarAsync(sitio);

        if (!string.IsNullOrWhiteSpace(sitio.ImagenPath) && File.Exists(sitio.ImagenPath))
        {
            File.Delete(sitio.ImagenPath);
        }

        await CargarSitiosAsync();
    }

    private static Sitio? ObtenerSitio(object sender)
    {
        return sender is Button button ? button.BindingContext as Sitio : null;
    }
}
