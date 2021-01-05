using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using praxicloud.core.containers;
using praxicloud.grpc.client;
using praxicloud.grpc.samples.messages;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace praxicloud.grpc.sample.client
{
    class Program
    {
        private static bool _continue = true;

        static void Main()
        {
            Console.WriteLine("Press <ENTER> to start");
            Console.ReadLine();
            var task = MainAsync();

            Console.WriteLine("Press <ENTER> to stop");
            Console.ReadLine();
            _continue = false;

            task.GetAwaiter().GetResult();
            Console.WriteLine("Press <ENTER> to terminate");
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var index = 1;
            var callOptions = GrpcClient.GetCallOptions(cancellationToken: ContainerLifecycle.CancellationToken);

            using var channel = GrpcClient.GetChannel(new Uri("https://localhost:10010"), GrpcClient.GetChannelOptions(100, true, true));

            var client = new PraxiCloudPipelineService.PraxiCloudPipelineServiceClient(channel);


            while (_continue)
            {
                await Task.Delay(10).ConfigureAwait(false);
                var request = new TestRequest { Message = $"Test message {index++}" };
                var response = await client.TestAsync(request, callOptions);

                if (index % 100 == 0)
                {
                    Console.WriteLine($"Response was {response.Message}");
                }
            }
        }

        public static bool ValidateClientCertificate(HttpRequestMessage message, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            var isValid = policyErrors == SslPolicyErrors.None;

            if (!isValid)
            {
                // If the only errors are chain specific errors review the chain
                if ((policyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0 && (policyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == 0 && (policyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == 0)
                {
                    if (chain == null)
                    {
                        // At this point the only errors in the certificate chain are untrusted root errors for self-signed certificates
                    }
                    else
                    {
                        var isChainErrorFound = false;

                        foreach (var status in chain.ChainStatus)
                        {
                            // If there is an error that has the subject of the certificate being the same as the issuer and it is untrusted root error, with no others it is valid, otherwise an addition chain error was found
                            if (!((certificate.Subject == certificate.Issuer) && (status.Status == X509ChainStatusFlags.UntrustedRoot)) && (status.Status != X509ChainStatusFlags.NoError))
                            {
                                isChainErrorFound = true;
                            }
                        }

                        isValid = !isChainErrorFound;
                    }
                }
            }

            return isValid;
        }
    }
}
