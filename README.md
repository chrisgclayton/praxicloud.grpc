# PraxiCloud GRPC
PraxiCloud Libraries are a set of common utilities and tools for general software development that simplify common development efforts for software development. The GRPC library includes two portions, one for simplifying client tasks and the other for simplifying the hosting of GRPC services through the use of Kestrel.



# Installing via NuGet

**Server library**

Install-Package PraxiCloud-Grpc-Server

**Client library**

Install-Package PraxiCloud-Grpc-Client

# GRPC Client 

## Key Types and Interfaces

|Class| Description | Notes |
| ------------- | ------------- | ------------- |
|**GrpcClient**|**GetMetadata** Converts a C# dictionary into GRPC Metdata object.<br />**GetCallOptions** Provides the ability to pass cancellation tokens and credentials to calls.<br />**GetChannelOptions** Provides the ability to validate that server certificates and control the maximum concurrent connections to the host.<br />**GetChannel** Creates the GRPC channel to the host, which can be reused.| Direct supported values for Metadata are CLR strings and byte arrays. Other types are converted to a string through the ToString() method. |

## Sample Usage

### Create a Simple Client

```csharp
var callOptions = GrpcClient.GetCallOptions(cancellationToken: ctx);
using var channel = GrpcClient.GetChannel(new Uri("https://localhost:10010"), GrpcClient.GetChannelOptions(100, true, true));
var client = new PraxiCloudPipelineService.PraxiCloudPipelineServiceClient(channel);

var request = new TestRequest { Message = $"Test message {index++}" };
var response = await client.TestAsync(request, callOptions);

Console.WriteLine($"Response was {response.Message}");
```

## Additional Information

For additional information the Visual Studio generated documentation found [here](./documents/praxicloud.grpc/praxicloud.grpc.client.xml), can be viewed using your favorite documentation viewer.

# GRPC Server 

## Key Types and Interfaces

|Class| Description | Notes |
| ------------- | ------------- | ------------- |
|**GrpcConfiguration**| A configuration object that describes the Kestrel server environment. | If certificate files are being used it is recommended to use the CertificateFileGrpcConfiguration object |
|**CertificateFileGrpcConfiguration**|A configuration object that describes the Kestrel server environment using a certificate file with password.|  |
|**GrpcKestrelServer**|An HTTP/2 with TLS v1.2 with Kestrel where all proto files and GRPC service implementations can be surfaced.|  |
|**GrpcKestrelStartup**|The startup object that defines the GRPC mappings.|  |

## Sample Usage

### Create a Custom Startup Type and Register GRPC Services

```csharp
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
```
### Create a Simple TLS v1.2 GRPC Server

```csharp
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
```

## Additional Information

For additional information the Visual Studio generated documentation found [here](./documents/praxicloud.grpc/praxicloud.grpc.server.xml), can be viewed using your favorite documentation viewer.
