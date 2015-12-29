# How to Build & Run The Sample

1. Install Service Fabric runtime, SDK and tools - 1.4.87: https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/
2. Launch 'Developer Command Prompt for VS2015' as admin and upgrade DNVM by running: https://github.com/aspnet/home#cmd
3. In the command prompt, run _dnvm install 1.0.0-rc2-16351 -a x86 -u_.
4. In the command prompt, run _dnvm install 1.0.0-rc2-16351 -a x64 -u_.
5. Clone the repo and open the solution in Visual Studio running as admin.
6. In Visual Studio, go to Options -> NuGet Package Manager -> Package Sources, and add a new package source: https://www.myget.org/F/aspnetvnext/api/v3/index.json.
7. After all the packages are restored, F5 to run the app.

# Key Code Snippets

## Create Communication Listener
```csharp
public class MyStatefulService : StatefulService
{
    ...
    
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        // Build an ASP.NET 5 web application that serves as the communication listener.
        var webApp = new WebApplicationBuilder().UseConfiguration(WebApplicationConfiguration.GetDefault())
                                                .UseStartup<Startup>()
                                                .ConfigureServices(services => services.AddSingleton<ICounterService>(this))
                                                .Build();

        // Replace the address with the one dynamically allocated by Service Fabric.
        var endpoint = ServiceInitializationParameters.CodePackageActivationContext.GetEndpoint("WebTypeEndpoint");
        webApp.GetAddresses().Clear();
        webApp.GetAddresses().Add($"{endpoint.Protocol}://+:{endpoint.Port}");

        return new[] { new ServiceReplicaListener(_ => new AspNetCommunicationListener(webApp)) };
    }
}
```

## ASP.NET 5 Communication Listener Adapter
```csharp
public class AspNetCommunicationListener : ICommunicationListener
{
    private IWebApplication _webApp;
    private IDisposable _token;

    public AspNetCommunicationListener(IWebApplication webApp)
    {
        _webApp = webApp;
    }

    public void Abort()
    {
        _token?.Dispose();
    }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
        _token?.Dispose();

        return Task.FromResult(true);
    }

    public Task<string> OpenAsync(CancellationToken cancellationToken)
    {
        _token = _webApp.Start();

        return Task.FromResult(_webApp.GetAddresses().First());
    }
}
```

## ServiceManifest.xml
```xml
<?xml version="1.0" encoding="utf-8"?>
<ServiceManifest Name="Web"
                 Version="1.0.0"
                 xmlns="http://schemas.microsoft.com/2011/01/fabric"
                 xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <ServiceTypes>
    <StatefulServiceType ServiceTypeName="WebType" HasPersistedState="true" />
  </ServiceTypes>
  <CodePackage Name="C" Version="1.0.0">
    <EntryPoint>
      <ExeHost>
        <Program>approot\runtimes\dnx-clr-win-x64.1.0.0-rc2-16351\bin\dnx.exe</Program>
        <Arguments>--project approot\src\Web Web</Arguments>
        <WorkingFolder>CodePackage</WorkingFolder>
        <ConsoleRedirection FileRetentionCount="5" FileMaxSizeInKb="2048" />
      </ExeHost>
    </EntryPoint>
  </CodePackage>
  <Resources>
    <Endpoints>
      <Endpoint Name="WebTypeEndpoint" Protocol="http" Type="Input" />
    </Endpoints>
  </Resources>
</ServiceManifest>
```