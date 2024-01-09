using Api.OtherMicroservicesClients;
using CSharpFunctionalExtensions;
using MicroservicesCommon;
using MicroservicesCommon.Api;
using Polly;
using ShoppingCartLogic.ShoppingCarts;
using ShoppingCartLogic.Utils;
using System.Net.Http.Headers;

namespace Api.Utils
{
    public class Startup
    {
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string shoppingCartConnectionName = "ShoppingCartConnection";
            Maybe<string> connectionStringOrNull = Configuration.GetConnectionString(shoppingCartConnectionName).AsMaybe();
            if (connectionStringOrNull.HasNoValue)
                throw new KeyNotFoundException($"Не удалось найти строку подключения с ConnectionName = {shoppingCartConnectionName}!");
            services.AddSingleton(new SessionFactory(connectionStringOrNull.Value));
            services.AddScoped<UnitOfWork>();
            services.AddTransient<ShoppingCartStore>();
            services.AddTransient<ShoppingCartItemStore>();
            services.AddTransient<ShoppingCartLogic.Events.EventStore>();
            services.AddSingleton<ProductCatalogCache>();
            DomainEvents.Init();
            string productCatalogBaseUrlKey = "ProductCatalogClientBaseAddress";
            Maybe<string> productCatalogBaseUrlOrDefault = Configuration.GetValue<string>(productCatalogBaseUrlKey).AsMaybe();
            if (productCatalogBaseUrlOrDefault.HasNoValue)
                throw new KeyNotFoundException($"Не удалось найти базовый адрес ProductCatalog по ключу \"{productCatalogBaseUrlKey}\"!");
            services.AddHttpClient<ProductCatalogClient>((HttpClient _Client) =>
            {
                _Client.BaseAddress = new Uri(productCatalogBaseUrlOrDefault.Value);
                _Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
                .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt))));
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => await context.Response.WriteAsync("Server is launch!"));
                endpoints.MapControllers();
            });
        }
    }
}
