# About The Sample
Today the scenario we've enabled is to host ASP.NET Core web application as a stateless service with Service Fabric. We wanted to light up the scenarios that people also can use ASP.NET Core Web API as communication listeners in their stateless services or stateful services, just like what the [OwinCommunicationListener](https://github.com/Azure-Samples/service-fabric-dotnet-getting-started/blob/master/Services/WordCount/WordCount.Common/OwinCommunicationListener.cs) does. With the new hosting APIs having been added to ASP.NET Core 1.0 RC2, this becomes possible.

This sample demonstrates:

1. How ASP.NET Core Web API can be used in a communication listener of stateless/stateful services.
2. How to build an HTTP gateway service to forward requests to multiple services behind it with the reusable and modular components.

Please share your feedback to help us improve the experience in the future releases of SDK and tooling.

# How to Build & Run The Sample

1. Install Service Fabric runtime, SDK and tools - 1.4.87: https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/
2. Launch 'Developer Command Prompt for VS2015' as admin and upgrade DNVM by running: https://github.com/aspnet/home#cmd
3. In the command prompt, run _set DNX_UNSTABLE_FEED=https://www.myget.org/F/aspnetcidev/_.
4. In the command prompt, run _dnvm install 1.0.0-rc2-16444 -a x86 -u_.
5. In the command prompt, run _dnvm install 1.0.0-rc2-16444 -a x64 -u_.
6. Open Visual Studio running as admin, go to Options -> NuGet Package Manager -> Package Sources, and add a new package source: https://www.myget.org/F/aspnetcidev/api/v3/index.json.
7. Clone the repo and open the solution.
8. After all the packages are restored, Ctrl F5 / F5 to run the app.
9. Open Hosting\Hosting.Tests\Hosting.Tests.sln to run the client that will send requests to the services.

# Key Code Snippets

## Create Communication Listener
```csharp
public class MyStatefulService : StatefulService
{
    ...
    
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        // Build an ASP.NET Core web application that serves as the communication listener.
        var webHost = new WebHostBuilder().UseDefaultConfiguration()
                                          .UseStartup<Startup>()
                                          .UseServiceFabricEndpoint(ServiceInitializationParameters, "MyStatefulTypeEndpoint")
                                          .ConfigureServices(services => services.AddSingleton<MyStatefulService>(this))
                                          .Build();

        return new[] { new ServiceReplicaListener(_ => new AspNetCoreCommunicationListener(webHost)) };
    }
}
```

## ASP.NET Core Communication Listener Adapter
```csharp
public class AspNetCoreCommunicationListener : ICommunicationListener
{
    private readonly IWebHost _webHost;

    public AspNetCoreCommunicationListener(IWebHost webHost)
    {
        if (webHost == null)
        {
            throw new ArgumentNullException(nameof(webHost));
        }

        _webHost = webHost;
    }

    public void Abort()
    {
        _webHost.Dispose();
    }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
        _webHost.Dispose();

        return Task.FromResult(true);
    }

    public Task<string> OpenAsync(CancellationToken cancellationToken)
    {
        _webHost.Start();

        var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

        return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses));
    }
}
```

## ServiceManifest.xml
```xml
<?xml version="1.0" encoding="utf-8"?>
<ServiceManifest Name="MyStateful"
                 Version="1.0.0"
                 xmlns="http://schemas.microsoft.com/2011/01/fabric"
                 xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <ServiceTypes>
    <StatefulServiceType ServiceTypeName="MyStatefulType" HasPersistedState="true" />
  </ServiceTypes>
  <CodePackage Name="Code" Version="1.0.0">
    <EntryPoint>
      <ExeHost>
        <Program>approot\runtimes\dnx-clr-win-x64.1.0.0-rc2-16444\bin\dnx.exe</Program>
        <Arguments>--project approot\src\MyStateful MyStateful</Arguments>
        <WorkingFolder>CodePackage</WorkingFolder>
        <ConsoleRedirection FileRetentionCount="5" FileMaxSizeInKb="2048" />
      </ExeHost>
    </EntryPoint>
  </CodePackage>
  <Resources>
    <Endpoints>
      <Endpoint Name="MyStatefulTypeEndpoint" Protocol="http" Type="Input" />
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