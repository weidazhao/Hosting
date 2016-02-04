# About The Sample
Today the scenario we've enabled is to host ASP.NET Core web application as a stateless service with Service Fabric. We wanted to light up the scenarios that people also can use ASP.NET Core as communication listeners in their stateless services and stateful services, similar to what the [OwinCommunicationListener](https://github.com/Azure-Samples/service-fabric-dotnet-getting-started/blob/master/Services/WordCount/WordCount.Common/OwinCommunicationListener.cs) does. With the new hosting APIs having been added to ASP.NET Core 1.0 RC2, this becomes possible.

This sample demonstrates:

1. How ASP.NET Core can be used in a communication listener of stateless/stateful services.
2. How to build an HTTP gateway service to forward requests to multiple services behind it with the reusable and modular components.

Please share your feedback to help us improve the experience in the future releases of SDK and tooling.

# How to Build & Run The Sample

1. Install Service Fabric runtime, SDK and tools - 1.4.87: https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/
2. Launch 'Developer Command Prompt for VS2015' as admin and upgrade DNVM by running: https://github.com/aspnet/home#cmd
3. In the command prompt, run _set DNX_UNSTABLE_FEED=https://www.myget.org/F/aspnetcidev/_.
4. In the command prompt, run _dnvm install 1.0.0-rc2-16453 -a x86 -u_.
5. In the command prompt, run _dnvm install 1.0.0-rc2-16453 -a x64 -u_.
6. Open Visual Studio running as admin, go to Options -> NuGet Package Manager -> Package Sources, and add a new package source: https://www.myget.org/F/aspnetcidev/api/v3/index.json.
7. Clone the repo and open the solution.
8. After all the packages are restored, Ctrl F5 / F5 to run the app.
9. Open Hosting\Hosting.Tests\Hosting.Tests.sln to run the client that will send requests to the services.

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