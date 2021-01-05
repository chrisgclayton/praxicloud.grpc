// Copyright (c) Christopher Clayton. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace praxicloud.grpc.server
{
    #region Using Clauses
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using praxicloud.core.security;
    #endregion

    /// <summary>
    /// A Kestrel based Grpc server that uses HTTP/2 with TLS 1.2
    /// </summary>
    /// <typeparam name="T">The startup type of the Kestrel server</typeparam>
    public class GrpcKestrelServer<T> where T : GrpcKestrelStartup
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the type
        /// </summary>
        /// <param name="configuration">The configuration for the Kestrel Server</param>
        public GrpcKestrelServer(IGrpcConfiguration configuration)
        {
            Guard.NotNull(nameof(configuration), configuration);

            Configuration = configuration;
        }
        #endregion
        #region Properties
        /// <summary>
        /// The configuration that defines the Kestrel Servers behavior
        /// </summary>
        protected IGrpcConfiguration Configuration { get; }        
        #endregion
        #region Methods
        /// <summary>
        /// A method that creates creates the Kestrel Host Builder to listen on the specified IP Address and Port
        /// </summary>
        /// <param name="ipAddress">The IP Address to listen on</param>
        /// <param name="port">The port to listen on</param>
        /// <returns>A Kestrel host builder that has been configured to listen for GRPC requests</returns>
        public IHostBuilder CreateHostBuilder(IPAddress ipAddress, ushort port)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IGrpcConfiguration>(Configuration);
                    services.AddSingleton<IGrpcStartupConfiguration>(Configuration);

                    ConfigureServices(context, services);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<T>();
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Limits.MaxConcurrentConnections = (Configuration.UseMultipleForConnectionCounts ?? false) ? Configuration.MaximumConcurrentConnections * Environment.ProcessorCount : Configuration.MaximumConcurrentConnections;
                        serverOptions.Limits.MaxConcurrentUpgradedConnections = (Configuration.UseMultipleForConnectionCounts ?? false) ? Configuration.MaximumConcurrentUpgradedConnections * Environment.ProcessorCount : Configuration.MaximumConcurrentUpgradedConnections; ;
                        serverOptions.Limits.KeepAliveTimeout = Configuration.KeepAliveTimeout == null || Configuration.KeepAliveTimeout < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(120) : Configuration.KeepAliveTimeout.Value;
                        serverOptions.Limits.RequestHeadersTimeout = Configuration.RequestHeadersTimeout == null || Configuration.RequestHeadersTimeout < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(60) : Configuration.RequestHeadersTimeout.Value;
                        serverOptions.Limits.Http2.MaxStreamsPerConnection = Configuration.MaximumStreamsPerConnection == null || Configuration.MaximumStreamsPerConnection < 1 ? 100 : Configuration.MaximumStreamsPerConnection.Value;

                        if(!string.IsNullOrWhiteSpace(Configuration.UnixSocketPath))
                        {
                            serverOptions.ListenUnixSocket(Configuration.UnixSocketPath, listenOptions =>
                            {
                                if (Configuration.EnableConnectionLogging ?? false) listenOptions.UseConnectionLogging();
                                listenOptions.Protocols = HttpProtocols.Http2;

                                if (Configuration.ServerCertificate != null)
                                {
                                    listenOptions.UseHttps(Configuration.ServerCertificate, httpsOptions =>
                                    {
                                        httpsOptions.SslProtocols = SslProtocols.Tls12;
                                        httpsOptions.ClientCertificateValidation = ValidateClientCertificate;
                                        httpsOptions.CheckCertificateRevocation = Configuration.CheckCertificateRevocation ?? true;
                                        httpsOptions.ClientCertificateMode = Configuration.ClientCertificateMode ?? ClientCertificateMode.NoCertificate;
                                    });
                                }
                            });
                        }
                        else
                        {
                            serverOptions.Listen(ipAddress, port, listenOptions =>
                            {
                                if (Configuration.EnableConnectionLogging ?? false) listenOptions.UseConnectionLogging();
                                listenOptions.Protocols = HttpProtocols.Http2;

                                if (Configuration.ServerCertificate != null)
                                {
                                    listenOptions.UseHttps(Configuration.ServerCertificate, httpsOptions =>
                                    {
                                        httpsOptions.SslProtocols = SslProtocols.Tls12;
                                        httpsOptions.ClientCertificateValidation = ValidateClientCertificate;
                                        httpsOptions.CheckCertificateRevocation = Configuration.CheckCertificateRevocation ?? true;
                                        httpsOptions.ClientCertificateMode = Configuration.ClientCertificateMode ?? ClientCertificateMode.NoCertificate;
                                    });
                                }

                                listenOptions.Protocols = HttpProtocols.Http2;
                            });
                        }
                    });
                });
        }

        /// <summary>
        /// A method that can be overridden to add dependencies to the container
        /// </summary>
        /// <param name="context">The host builder context</param>
        /// <param name="services">The service collection to add the dependencies to</param>
        protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

        }

        /// <summary>
        /// Performs client certificate validation
        /// </summary>
        /// <param name="certificate">The client provided certificate to validate</param>
        /// <param name="chain">The certificate chain that is associated with the client certificate</param>
        /// <param name="policyErrors">A list of policy errors</param>
        /// <returns>True if the certificate should be accepted</returns>
        protected virtual bool ValidateClientCertificate(X509Certificate2 certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return (policyErrors == SslPolicyErrors.None) || ((Configuration.AllowSelfSigned ?? false) && chain.ChainElements.Count < 2);
        }
        #endregion
    }
}
