using NotaFiscal.Views;
using NotaFiscal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NotaFiscal.Controllers;
using NotaFiscal.Services;

namespace NotaFiscal
{
    internal static class Program
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configurar serviços
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            ApplicationConfiguration.Initialize();
            
            Application.Run(ServiceProvider.GetRequiredService<FormImportador>());
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Configuração
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Controllers
            services.AddTransient<PlanilhaController>();

            // Services
            services.AddSingleton<LogService>();
            services.AddSingleton<ValidacaoService>();

            // Forms
            services.AddTransient<FormImportador>(provider => 
                new FormImportador(
                    provider.GetRequiredService<PlanilhaController>(),
                    provider.GetRequiredService<AppDbContext>()
                ));
        }
    }
}