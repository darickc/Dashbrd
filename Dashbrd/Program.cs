using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dashbrd
{
    public class Program
    {
        private const string DockerSecretPath = "/run/secrets";
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("secrets.json", true);
                    if (Directory.Exists(DockerSecretPath))
                    {
                        foreach (var file in Directory.GetFiles(DockerSecretPath))
                        {
                            builder.AddJsonFile(file, true);
                        }
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
