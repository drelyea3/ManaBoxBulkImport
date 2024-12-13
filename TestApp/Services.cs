using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.ViewModels;

namespace TestApp
{
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
}
