
namespace praxicloud.grpc.sample
{
    #region Using Clauses
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using praxicloud.grpc.sample.services;
    using praxicloud.grpc.server;
    #endregion

    /// <summary>
    /// GRPC service startup
    /// </summary>
    public sealed class DemoStartup : GrpcKestrelStartup
    {
        #region Methods
        /// <inheritdoc />
        protected override void MapGrpcServices(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapGrpcService<TestMessageService>();
        }
        #endregion
    }
}
