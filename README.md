# About The Sample

This sample demonstrates:

1. How ASP.NET Core can be used in a communication listener of stateless/stateful services. Today the scenario we've enabled is to host ASP.NET Core web application as a stateless service with Service Fabric. We wanted to light up the scenarios that people also can use ASP.NET Core as communication listeners in stateless services and stateful services, similar to what the [OwinCommunicationListener](https://github.com/Azure-Samples/service-fabric-dotnet-getting-started/blob/master/Services/WordCount/WordCount.Common/OwinCommunicationListener.cs) does. With the new hosting APIs having been added to ASP.NET Core 1.0 RC2, this becomes possible.

2. How to build an API gateway service to forward requests to multiple microservices behind it with the reusable and modular component. Service Fabric is a great platform for building microservices. The gateway middleware (Microsoft.ServiceFabric.AspNetCore.Gateway) is an attempt to provide a building block for people to easily implement the API gateway pattern of microservices on Service Fabric. There are a couple good articles elaborating the API gateway pattern, such as http://microservices.io/patterns/apigateway.html, http://www.infoq.com/articles/microservices-intro, etc. For more information about microservices, check out https://azure.microsoft.com/en-us/blog/microservices-an-application-revolution-powered-by-the-cloud/, http://martinfowler.com/articles/microservices.html.

Please share your feedback to help us improve the experience in the future releases of Service Fabric SDK and tooling.

# How to Build & Run The Sample

1. Install Service Fabric runtime, SDK and tools - 2.0.135: https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/
2. Install DotNet CLI: https://dotnetcli.blob.core.windows.net/dotnet/beta/Installers/Latest/dotnet-dev-win-x64.latest.exe. Latest compatible version: 2be5e84f874a2d15ee3f091130a3e22e479eb700 1.0.0-rc2-002339
3. Clone the repo.
4. Go to Hosting\Hosting, and run dotnet-publish.cmd. **Note:** Visual Studio 2015 doesn't support DotNet CLI yet, so you won't be able to publish the app from within VS at the moment.
5. Open 'Windows PowerShell' command prompt as administrator, navigate to Hosting\Hosting\, and run _Connect-ServiceFabricCluster  localhost:19000 | .\Scripts\Deploy-FabricApplication.ps1 -PublishProfileFile .\PublishProfiles\Local.xml -ApplicationPackagePath .\pkg\Debug\ -OverwriteBehavior Always_

# Key Code Snippets

## Entry Point
```csharp
public static class Program
{
    public static void Main(string[] args)
    {
        var communicationContext = CreateAspNetCoreCommunicationContext(args);

        ServiceRuntime.RegisterServiceAsync("CounterType", serviceContext => new CounterService(serviceContext, communicationContext)).GetAwaiter().GetResult();

        communicationContext.WebHost.Run();
    }

    private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext(string[] args)
    {
        var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                          .UseStartup<Startup>()
                                          .UseKestrel()
                                          .UseServiceFabricEndpoint("CounterTypeEndpoint")
                                          .Build();

        return new AspNetCoreCommunicationContext(webHost);
    }
}
```

## Create Communication Listener
```csharp
public class CounterService : StatefulService, ICounterService
{
    ...
    
    private readonly AspNetCoreCommunicationContext _communicationContext;

    public CounterService(StatefulServiceContext serviceContext, AspNetCoreCommunicationContext communicationContext)
        : base(serviceContext)
    {
        _communicationContext = communicationContext;
    }
    
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return new[] { new ServiceReplicaListener(_ => _communicationContext.CreateCommunicationListener(this)) };
    }
}
```

## Service Dependency Injection
```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Add CounterService.
        services.AddServiceFabricService<ICounterService, CounterService>();

        ...
    }

    ...
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

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        //
        // Adds HttpRequestDispatcherProvider that is required by GatewayMiddleware.
        //
        services.AddDefaultHttpRequestDispatcherProvider();
    }

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
        var smsOptions = new GatewayOptions()
        {
            ServiceUri = new Uri("fabric:/Hosting/SmsService", UriKind.Absolute),

            OperationRetrySettings = new OperationRetrySettings(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2), 30),

            GetServicePartitionKey = context =>
            {
                var pathSegments = context.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                string user = pathSegments[pathSegments.Length - 1];

                return new ServicePartitionKey(Fnv1aHashCode.Get64bitHashCode(user));

            }
        };

        app.Map("/sms",
            subApp =>
            {
                subApp.RunGateway(smsOptions);
            }
        );

        //
        // Counter
        //
        var counterOptions = new GatewayOptions()
        {
            ServiceUri = new Uri("fabric:/Hosting/CounterService", UriKind.Absolute),

            OperationRetrySettings = new OperationRetrySettings(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2), 30)
        };

        app.Map("/counter",
            subApp =>
            {
                subApp.RunGateway(counterOptions);
            }
        );

        app.Map("/Hosting/CounterService",
            subApp =>
            {
                subApp.RunGateway(counterOptions);
            }
        );

        app.MapWhen(
            context =>
            {
                StringValues serviceUri;

                return context.Request.Headers.TryGetValue("SF-ServiceUri", out serviceUri) &&
                       serviceUri.Count == 1 &&
                       serviceUri[0] == "fabric:/Hosting/CounterService";
            },
            subApp =>
            {
                subApp.RunGateway(counterOptions);
            }
        );
    }
}
```
