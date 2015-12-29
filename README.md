# How to Build & Run The Sample

1. Install Service Fabric runtime, SDK and tools - 1.4.87: https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/
2. Launch 'Developer Command Prompt for VS2015' as admin and upgrade DNVM by running: https://github.com/aspnet/home#cmd
3. In the command prompt, run 'dnvm install 1.0.0-rc2-16351 -a x86 -u'.
4. In the command prompt, run 'dnvm install 1.0.0-rc2-16351 -a x64 -u'.
5. Clone the repo and open the solution in Visual Studio running as admin.
6. In Visual Studio, go to Options -> NuGet Package Manager -> Package Sources, and add a new package source: https://www.myget.org/F/aspnetvnext/api/v3/index.json.
7. After all the packages are restored, F5 to run the app.

# Key Code Snippets

## Create Communication Listener
```csharp
public class CounterService : StatefulService, ICounterService
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
