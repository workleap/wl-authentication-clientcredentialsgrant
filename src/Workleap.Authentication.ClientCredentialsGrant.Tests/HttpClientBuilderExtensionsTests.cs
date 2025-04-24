using Workleap.Extensions.Http.Authentication.ClientCredentialsGrant;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;

namespace Workleap.Authentication.ClientCredentialsGrant.Tests;

public class HttpClientBuilderExtensionsTests
{
    [Fact]
    public async Task Calling_AddClientCredentialsHandler_With_Same_Client_Name_Multiple_Times_Does_Not_Create_Duplicate_Http_Message_Handlers()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();

        // Call our extension method multiple times with different ways
        services.AddHttpClient("MyTestApi")
            .AddClientCredentialsHandler(ConfigureFakeOptions)
            .AddClientCredentialsHandler(ConfigureFakeOptions)
            .AddClientCredentialsHandler();

        // Keep a reference on the http message handlers that will be created when instantiating the http clients
        var myTestApiHttpMessageHandlerInspector = EnableInternalHttpMessageHandlerInspection(services, "MyTestApi");
        var backchannelHttpMessageHandlerInspector = EnableInternalHttpMessageHandlerInspection(services, ClientCredentialsConstants.BackchannelHttpClientName);

        await using var serviceProvider = services.BuildServiceProvider();

        // Instantiating http clients also instantiate the underlying http message handlers
        _ = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("MyTestApi");
        _ = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(ClientCredentialsConstants.BackchannelHttpClientName);

        // The inspected handlers should be in this order: ClientCredentialsTokenHttpMessageHandler, LoggingHttpMessageHandler then HttpClientHandler
        var myTestApiHttpMessageHandlers = myTestApiHttpMessageHandlerInspector.HttpMessageHandlers.ToArray();

        Assert.Equal(3, myTestApiHttpMessageHandlers.Length);
        Assert.IsType<ClientCredentialsTokenHttpMessageHandler>(myTestApiHttpMessageHandlers[0]);
        Assert.IsType<LoggingHttpMessageHandler>(myTestApiHttpMessageHandlers[1]);
        Assert.IsType<SocketsHttpHandler>(myTestApiHttpMessageHandlers[2]);

        // The inspected handlers should be in this order: BackchannelRetryHttpMessageHandler, LoggingHttpMessageHandler then HttpClientHandler
        var backchannelHttpMessageHandlers = backchannelHttpMessageHandlerInspector.HttpMessageHandlers.ToArray();

        Assert.Equal(3, backchannelHttpMessageHandlers.Length);
        Assert.IsType<BackchannelRetryHttpMessageHandler>(backchannelHttpMessageHandlers[0]);
        Assert.IsType<LoggingHttpMessageHandler>(backchannelHttpMessageHandlers[1]);
        Assert.IsType<SocketsHttpHandler>(backchannelHttpMessageHandlers[2]);
    }

    [Fact]
    public void Calling_AddClientCredentialsHandler_With_Many_Client_Names_Multiple_Times_Only_Adds_One_Configuration_Per_Client_Name()
    {
        var services = new ServiceCollection();

        // Call our extension method multiple times with different client names
        services.AddHttpClient("MyTestApi1").AddClientCredentialsHandler().AddClientCredentialsHandler();
        services.AddHttpClient("MyTestApi2").AddClientCredentialsHandler().AddClientCredentialsHandler();

        Assert.Single(services, x => x.ImplementationType == typeof(ClientCredentialsHttpClientBuilderExtensions.AddBackchannelRetryHandlerConfigureOptions));
        Assert.Single(services, x => x.ImplementationInstance is ClientCredentialsHttpClientBuilderExtensions.AddClientCredentialsTokenHandlerConfigureOptions { Name: "MyTestApi1" });
        Assert.Single(services, x => x.ImplementationInstance is ClientCredentialsHttpClientBuilderExtensions.AddClientCredentialsTokenHandlerConfigureOptions { Name: "MyTestApi2" });
    }

    private static void ConfigureFakeOptions(ClientCredentialsOptions options)
    {
        options.Authority = "https://whatever";
        options.ClientId = "whatever";
        options.ClientSecret = "whatever";
    }

    private static HttpClientHandlerInspector EnableInternalHttpMessageHandlerInspection(IServiceCollection services, string clientName)
    {
        var inspector = new HttpClientHandlerInspector();

        services.Configure<HttpClientFactoryOptions>(clientName, options =>
        {
            options.HttpMessageHandlerBuilderActions.Insert(0, builder =>
            {
                builder.AdditionalHandlers.Insert(0, inspector);
            });
        });

        return inspector;
    }

    private sealed class HttpClientHandlerInspector : DelegatingHandler
    {
        public IEnumerable<HttpMessageHandler> HttpMessageHandlers => this.CollectInternalHttpMessageHandlers();

        private IEnumerable<HttpMessageHandler> CollectInternalHttpMessageHandlers()
        {
            var innerHandler = this.InnerHandler;

            while (innerHandler != null)
            {
                yield return innerHandler;
                innerHandler = (innerHandler as DelegatingHandler)?.InnerHandler;
            }
        }
    }
}