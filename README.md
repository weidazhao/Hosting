# About The Sample

This sample demonstrates:

1. How ASP.NET Core can be used in a communication listener of stateless/stateful services. Today the scenario we've enabled is to host ASP.NET Core web application as a stateless service with Service Fabric. We wanted to light up the scenarios that people also can use ASP.NET Core as communication listeners in their stateless services and stateful services, similar to what the [OwinCommunicationListener](https://github.com/Azure-Samples/service-fabric-dotnet-getting-started/blob/master/Services/WordCount/WordCount.Common/OwinCommunicationListener.cs) does. With the new hosting APIs having been added to ASP.NET Core 1.0 RC2, this becomes possible.

2. How to build an API gateway service to forward requests to multiple micro services behind it with the reusable and modular component. Service Fabric is a great platform for building micro services. The gateway middleware (Microsoft.ServiceFabric.AspNetCore.Gateway) is an attempt to provide a building block for people to easily implement the API gateway pattern of micro services on Service Fabric. There are a couple good articles elaborating the API gateway pattern, such as http://microservices.io/patterns/apigateway.html, http://www.infoq.com/articles/microservices-intro, etc.

Please share your feedback to help us improve the experience in the future releases of SDK and tooling.

# How to Build & Run The Sample

1. Install Service Fabric runtime, SDK and tools - 1.4.87: https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/
2. Install DotNet CLI: https://github.com/dotnet/cli. If you install it via binaries (not MSI), add path-to-dotnet-cli\bin to the environment variable PATH. Current version: 1.0.0.001483 SHA: f81ba05a7c6cc8c2c2e87d273944a1f9663d8b96
3. Clone the repo.
4. Go to Hosting\Hosting, and run dotnet-publish.cmd.
5. Open 'Windows PowerShell' command prompt as administrator, navigate to Hosting\Hosting\, and run _Connect-ServiceFabricCluster | .\Scripts\Deploy-FabricApplication.ps1 -PublishProfileFile .\PublishProfiles\Local.xml -ApplicationPackagePath .\pkg\Debug\ -OverwriteBehavior Always_ 

# Key Code Snippets

## Entry Point
```csharp
public static class Program
{
    public static void Main(string[] args)
    {
        var context = CreateAspNetCoreCommunicationContext(args);

        using (var fabricRuntime = FabricRuntime.Create())
        {
            fabricRuntime.RegisterStatefulServiceFactory("CounterType", () => new CounterService(context));

            context.WebHost.Run();
        }
    }

    private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext(string[] args)
    {
        var serviceDescription = new ServiceDescription()
        {
            ServiceType = typeof(CounterService),
            InterfaceTypes = ImmutableArray.Create(typeof(ICounterService))
        };

        var options = new ServiceFabricOptions()
        {
            EndpointName = "CounterTypeEndpoint",
            ServiceDescriptions = ImmutableArray.Create(serviceDescription)
        };

        var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                          .UseStartup<Startup>()
                                          .UseServiceFabric(options)
                                          .Build();

        return new AspNetCoreCommunicationContext(webHost, addUrlPrefix: true);
    }
}
```

## Create Communication Listener
```csharp
public class CounterService : StatefulService, ICounterService
{
    ...
    
    private readonly AspNetCoreCommunicationContext _context;        

    public CounterService(AspNetCoreCommunicationContext context)
    {
        _context = context;
    }
    
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return new[] { new ServiceReplicaListener(_ => _context.CreateCommunicationListener(this)) };
    }
}
```

## ServiceManifest.xml
```xml
<?xml version="1.0" encoding="utf-8"?>
<ServiceManifest xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                 xmlns="http://schemas.microsoft.com/2011/01/fabric"
                 Name="Counter"
                 Version="1.0.0">
  <ServiceTypes>
    <StatefulServiceType ServiceTypeName="CounterType" HasPersistedState="true" />
  </ServiceTypes>
  <CodePackage Name="Code" Version="1.0.0">
    <EntryPoint>
      <ExeHost>
        <Program>Counter.exe</Program>
        <WorkingFolder>CodePackage</WorkingFolder>
        <ConsoleRedirection FileRetentionCount="5" FileMaxSizeInKb="2048" />
      </ExeHost>
    </EntryPoint>
  </CodePackage>
  <Resources>
    <Endpoints>
      <Endpoint Name="CounterTypeEndpoint" Protocol="http" Type="Input" />
    </Endpoints>
  </Resources>
</ServiceManifest>
```

## Configure HTTP Gateway
```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        //
        // Scenarios:
        // 1. Multiple services.
        // 2. Various versions or kinds of clients side by side.
        //

        //
        // SMS
        //
        app.Map("/sms",
            subApp =>
            {
                subApp.RunGateway(new GatewayOptions() { ServiceDescription = new SmsServiceDescription() });
            }
        );

        //
        // Counter
        //
        app.Map("/counter",
            subApp =>
            {
                subApp.RunGateway(new GatewayOptions() { ServiceDescription = new CounterServiceDescription() });
            }
        );

        app.Map("/Hosting/CounterService",
            subApp =>
            {
                subApp.RunGateway(new GatewayOptions() { ServiceDescription = new CounterServiceDescription() });
            }
        );

        app.MapWhen(
            context =>
            {
                StringValues serviceNames;

                return context.Request.Headers.TryGetValue("SF-ServiceName", out serviceNames) &&
                       serviceNames.Count == 1 &&
                       serviceNames[0] == "fabric:/Hosting/CounterService";
            },
            subApp =>
            {
                subApp.RunGateway(new GatewayOptions() { ServiceDescription = new CounterServiceDescription() });
            }
        );
    }
}
```