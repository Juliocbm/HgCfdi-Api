using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ryder.Api.Client.Configuration;
using Ryder.Api.Client.Services;

namespace Ryder.Api.Client.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra el cliente de Ryder con valores por defecto embebidos.
        /// </summary>
        public static IServiceCollection AddRyderApiClient(this IServiceCollection services)
        {
            // 1) Registrar opciones con valores por defecto:

            //production
            //services.Configure<RyderApiOptions>(opts =>
            //{
            //    opts.BaseUrl = "https://api.ryder.com";
            //    opts.AccessKey = "3dacad261a906fa0148a5956a4052c9f33d6b9b6773a285f00a72e090d8c3558";
            //    opts.Email = "sistemas@hgtransportaciones.com";
            //    opts.SubscriptionKey = "2630c2bd8a4141a9a5f65e07f903c344";
            //});

            //develop
            services.Configure<RyderApiOptions>(opts =>
            {
                opts.BaseUrl = "https://apiqa.ryder.com";
                opts.AccessKey = "4815c5a413acee24b4e049b449786a3fcab2bf8a613ee6ca587d31783d10858c";
                opts.Email = "jsanchez@hgtransportaciones.com";
                opts.SubscriptionKey = "2630c2bd8a4141a9a5f65e07f903c344";
            });

            // 2) Registrar HttpClient tipado usando esos valores:
            services.AddHttpClient<IRyderApiClient, RyderApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<RyderApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);

                // Header de suscripción (Ocp-Apim-Subscription-Key)
                if (!string.IsNullOrWhiteSpace(options.SubscriptionKey))
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", options.SubscriptionKey);
                }
            });

            return services;
        }
    }
}
