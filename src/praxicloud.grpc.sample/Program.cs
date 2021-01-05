using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using praxicloud.core.containers;
using praxicloud.grpc.server;
using System;
using System.Net;
using System.Threading.Tasks;

namespace praxicloud.grpc.sample
{
    class Program
    {
        private static bool _continue = true;

        static void Main()
        {            

            var task = MainAsync();

            
            

            Console.WriteLine("Press <ENTER> to stop listening");
            Console.ReadLine();

            _continue = false;
            task.GetAwaiter().GetResult();

            Console.WriteLine("Press <ENTER> to terminate");
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var configuration = new CertificateFileGrpcConfiguration
            { 
                AllowSelfSigned = true,
                CheckCertificateRevocation = false,
                ClientCertificateMode = ClientCertificateMode.NoCertificate,
                EnableConnectionLogging = false,
                EnableNonGrpcWarningMessage = true,
                KeepAliveTimeout = TimeSpan.FromSeconds(120),
                MaximumConcurrentConnections = 100,
                MaximumConcurrentUpgradedConnections = 100,
                MaximumStreamsPerConnection = 100,
                NonGrpcWarningMessage = "This requires a GRPC client",
                CertificateFilePassword = Environment.GetEnvironmentVariable("PraxiDemo:CertificatePassword"),
                CertificateFileName = "./certs/my_contoso_local.pfx",
                UseDeveloperExceptionPage = true,
                UseMultipleForConnectionCounts = true
            };

            var server = new GrpcKestrelServer<DemoStartup>(configuration);
            var hostBuilder = server.CreateHostBuilder(IPAddress.Any, 10010);

            using (var host = hostBuilder.Build())
            {
                await host.StartAsync(ContainerLifecycle.CancellationToken).ConfigureAwait(false);

                while (_continue)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }

                await host.StopAsync(ContainerLifecycle.CancellationToken).ConfigureAwait(false);
            }
        }
    }
}
