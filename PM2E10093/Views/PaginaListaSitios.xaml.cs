using System.Globalization;
using PM2E10093.Controllers;
using PM2E10093.Data;
using PM2E10093.Models;

namespace PM2E10093.Views;

public partial class PaginaListaSitios : ContentPage
{
    private readonly SitiosController sitiosController;
    private readonly MapaController mapaController = new();
    private Sitio? sitioSeleccionado;

    public PaginaListaSitios()
    {
        InitializeComponent();
        sitiosController = new SitiosController(new SqliteSitios());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await CargarSitiosAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lista de sitios", $"No se pudieron cargar los sitios: {ex.Message}", "OK");
        }
    }

    private async Task CargarSitiosAsync()
    {
        ListasMapas.ItemsSource = await sitiosController.GetAllAsync();
    }

    private void ListasMapas_Selected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Sitio sitio)
        {
            sitioSeleccionado = sitio;
        }
    }

    private async void ListasMapas_Tapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is not Sitio sitio)
            return;

        sitioSeleccionado = sitio;

        bool respuesta = await DisplayAlert(
            "Accion",
            "Desea ir a la ubicacion indicada?",
            "Si",
            "No");

        if (respuesta)
        {
            await Navigation.PushAsync(new PaginaMapa(sitio));
        }
    }

    private async void OnAtrasClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnMapaClicked(object sender, EventArgs e)
    {
        if (sitioSeleccionado is null)
        {
            await DisplayAlert("Mapa", "Seleccione un sitio de la lista", "OK");
            return;
        }

        try
        {
            await mapaController.ShowSiteOnMapAsync(sitioSeleccionado);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Mapa", $"No se pudo abrir el mapa: {ex.Message}", "OK");
        }
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (sitioSeleccionado is null)
        {
            await DisplayAlert("Eliminar", "Seleccione primero un sitio de la lista.", "OK");
            return;
        }

        bool confirmar = await DisplayAlert(
            "Eliminar",
            $"Desea eliminar este sitio?\n\n{sitioSeleccionado.Descripcion}",
            "Si",
            "No");

        if (!confirmar)
        {
            return;
        }

        await sitiosController.EliminarAsync(sitioSeleccionado);

        if (!string.IsNullOrWhiteSpace(sitioSeleccionado.ImagenPath) && File.Exists(sitioSeleccionado.ImagenPath))
        {
            File.Delete(sitioSeleccionado.ImagenPath);
        }

        sitioSeleccionado = null;
        ListasMapas.SelectedItem = null;
        await CargarSitiosAsync();
    }
}
