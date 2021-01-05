// Copyright (c) Christopher Clayton. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace praxicloud.grpc.server
{
    #region Using Clauses
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Client;
    using praxicloud.core.security;
    using System.Threading.Tasks;
    #endregion

    /// <summary>
    /// A Kestrel Startup type that handles GRPC requests
    /// </summary>
    public abstract class GrpcKestrelStartup
    {
        #region Constants
        /// <summary>
        /// The default GRPC client requirement message content
        /// </summary>
        public const string DefaultNonGrpcWarningMessage = "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909";
        #endregion
        #region Properties
        /// <summary>
        /// The configuration for the startup object
        /// </summary>
        protected IGrpcStartupConfiguration StartupConfiguration { get; private set; }
        #endregion
        #region Methods
        /// <summary>
        /// Provides a method for derived types to register their GRPC types that are exposed by the service by calling the endpoints.MapGrpcService{T} method.
        /// </summary>
        /// <param name="routeBuilder">The builder that exposes the endpoints configuration details</param>
        protected abstract void MapGrpcServices(IEndpointRouteBuilder routeBuilder);

        /// <summary>
        /// Provid-es a way to configure additional HTTP/2 TLS 1.2 endpoints
        /// </summary>
        /// <param name="routeBuilder">The route builder to setup the configuration on</param>
        protected virtual void ConfigureEndpoints(IEndpointRouteBuilder routeBuilder)
        {

        }

        /// <summary>
        /// Configures the startup method beyond the standard GRPC configurations
        /// </summary>
        /// <param name="app">The app builder to configure</param>
        /// <param name="env">The web hosting environment being configured</param>
        protected virtual void ConfigureAdditional(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        /// <summary>
        /// Maps additional services in use by the GRPC startup
        /// </summary>
        /// <param name="services">The services collection to add the services to</param>
        protected virtual void ConfigureAdditionalServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// Adds specific services for the current startup
        /// </summary>
        /// <param name="services">The service collection to add the GRPC service to</param>
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddGrpc();
            ConfigureAdditionalServices(services);
        }

        

        /// <summary>
        /// Configures the startup method adding developer exception pages, routing etc.
        /// </summary>
        /// <param name="app">The app builder to configure</param>
        /// <param name="env">The web hosting environment being configured</param>
        /// <param name="startupConfiguration">The startup configuration</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IGrpcStartupConfiguration startupConfiguration)
        {
            StartupConfiguration = startupConfiguration;

            if (((StartupConfiguration.UseDeveloperExceptionPageNonProduction ?? true) && env.IsDevelopment()) || (StartupConfiguration.UseDeveloperExceptionPage ?? false))
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            ConfigureAdditional(app, env);

            app.UseEndpoints(endpoints =>
            {                
                MapGrpcServices(endpoints);
                ConfigureEndpoints(endpoints);
                if (StartupConfiguration.EnableNonGrpcWarningMessage ?? true) endpoints.MapGet("/", WriteGrpcErrorMessageAsync);
            });
        }

        /// <summary>
        /// A method that can be overridden to write a different content type and error message for only GRPC supported requests
        /// </summary>
        /// <param name="context">The HTTP Context to write the warning to</param>
        public virtual async Task WriteGrpcErrorMessageAsync(HttpContext context)
        {
            var message = string.IsNullOrWhiteSpace(StartupConfiguration.NonGrpcWarningMessage) ? DefaultNonGrpcWarningMessage : StartupConfiguration.NonGrpcWarningMessage;

            await context.Response.WriteAsync(message).ConfigureAwait(false);
        }
        #endregion
    }
}
