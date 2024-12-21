using Microsoft.Extensions.DependencyInjection;
using TestApp.ViewModels;

namespace TestApp;

public static class Services
{
    public static ServiceProvider Provider { get; }

    static Services()
    {
        var services = new ServiceCollection();

        services.AddSingleton(static _ => new Scryfall.ScryfallClient("MBBI.Application"));
        services.AddSingleton<ApplicationViewModel>();

        Provider = services.BuildServiceProvider();
    }
}