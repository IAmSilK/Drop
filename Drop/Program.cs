using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Resources;

namespace Drop
{
    public class Program
    {
        private static void ExportResource(string resource)
        {
            var resourcePath = Path.Combine(Environment.CurrentDirectory, resource);

            if (File.Exists(resourcePath)) return;

            var assembly = typeof(Program).Assembly;

            using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{resource}");
            using var reader = new StreamReader(stream ?? throw new MissingManifestResourceException("Missing embedded resource"));

            var contents = reader.ReadToEnd();

            File.WriteAllText(resourcePath, contents);
        }

        public static void Main(string[] args)
        {
#if DEBUG
            var configResource = "appsettings.Development.json";
#else
            var configResource = "appsettings.json";
#endif

            ExportResource(configResource);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
