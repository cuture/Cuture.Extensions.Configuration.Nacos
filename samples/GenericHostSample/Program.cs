using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericHostSample;

internal class Program
{
    #region Public 方法

    public static async Task Main(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
                              .ConfigureAppConfiguration(builder =>
                              {
                                  AddNacosWithConfiguration(builder);
                                  //AddNacosWithExtensions(builder);
                              })
                              .ConfigureServices((context, services) =>
                              {
                                  services.AddOptions<SampleOptions>().Bind(context.Configuration.GetSection("Sample"));

                                  services.AddHostedService<SampleHostedService>();
                              });

        var host = hostBuilder.Build();
        await host.RunAsync();
    }

    #endregion Public 方法

    #region Private 方法

    private static void AddNacosWithConfiguration(IConfigurationBuilder builder)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.nacos.configuration.json")
                                                      .AddUserSecrets<Program>()
                                                      .Build();

        //builder.AddNacos(configuration)  //仅使用Http
        builder.AddNacosWithGrpcClientAllowed(configuration, options => options.UseLoggerFactory(loggerFactory))    //允许使用Grpc
               .AddJsonFile("appsettings.lastvalue.json");
    }

    private static void AddNacosWithExtensions(IConfigurationBuilder builder)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        builder.AddNacos(options =>
        {
            options.AddServerAddress("http://127.0.0.1:18848").AddServerAddress("http://127.0.0.1:8848")
                   .UseGrpcClient()
                   //.UseHttpClient()
                   .WithUser("username", "password")
                   .UseLoggerFactory(loggerFactory)
                   .SubscribeConfiguration("test1", "Cfg1")
                   .SubscribeConfiguration("test1", "Cfg2");
        })
        .AddJsonFile("appsettings.lastvalue.json");
    }

    #endregion Private 方法

    // sample test1 - Cfg1
    //{
    //  "Sample": {
    //    "BoolArrayProperty": [
    //      true,
    //      true,
    //      false,
    //      true
    //    ],
    //    "BoolProperty": true,
    //    "IntArrayProperty": [
    //      0,
    //      1,
    //      2,
    //      3,
    //      4
    //    ],
    //    "IntProperty": "123",
    //    "ObjectArrayProperty": [
    //      { "Value": "PV0" },
    //      { "Value": "PV1" },
    //      { "Value": "PV2" },
    //      { "Value": "PV3" }
    //    ],
    //    "ObjectProperty": {
    //    "Value": "PV"
    //    },
    //    "StringArrayProperty": [
    //      "0",
    //      "1",
    //      "2",
    //      "3",
    //      "4"
    //    ],
    //    "EAStringProperty": null,
    //    "StringProperty": "stringdasdsadasdsa"
    //  }
    //}
}
