using PM2E10093.Models;
using SQLite;

namespace PM2E10093.Data;

public class SqliteSitios : SitioDatabase
{
    private SQLiteAsyncConnection? _database;

    private async Task InicializarAsync()
    {
        if (_database is not null)
            return;

        string ruta = Path.Combine(FileSystem.AppDataDirectory, "sitios.db3");
        _database = new SQLiteAsyncConnection(ruta);
        await _database.CreateTableAsync<Sitio>();
    }

    public async Task InitializeAsync()
    {
        await InicializarAsync();
    }

    public async Task<List<Sitio>> ObtenerSitiosAsync()
    {
        await InicializarAsync();
        return await _database!.Table<Sitio>()
            .OrderByDescending(sitio => sitio.FechaCreacion)
            .ToListAsync();
    }

    public async Task<int> GuardarSitioAsync(Sitio sitio)
    {
        await InicializarAsync();
        return sitio.Id != 0
            ? await _database!.UpdateAsync(sitio)
            : await _database!.InsertAsync(sitio);
    }

    public async Task<int> EliminarSitioAsync(Sitio sitio)
    {
        await InicializarAsync();
        return await _database!.DeleteAsync(sitio);
    }

    public Task<List<Sitio>> ObtenerAsync()
    {
        return ObtenerSitiosAsync();
    }

    public Task<int> GuardarAsync(Sitio sitio)
    {
        return GuardarSitioAsync(sitio);
    }

    public Task<int> EliminarAsync(Sitio sitio)
    {
        return EliminarSitioAsync(sitio);
    }
}
