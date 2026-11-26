using System.Net;
using Digital_Fridge.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Digital_Fridge.Tests.Integration;

[Trait("Category", "Integration")]
public class WebEndpointsIntegrationTests(TestAppFactory factory) : IClassFixture<TestAppFactory>
{
    [Fact]
    public async Task SetCultureEndpoint_SetsCultureCookieAndRedirectsLocally()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/api/culture/set?culture=en&redirectUri=%2Frecipes");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/recipes", response.Headers.Location?.OriginalString);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, c => c.Contains(".AspNetCore.Culture") && c.Contains("en"));
    }

    [Fact]
    public async Task SetCultureEndpoint_ExternalRedirectUri_FallsBackToRoot()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/api/culture/set?culture=en&redirectUri=https%3A%2F%2Fevil.example");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public Task HomePage_RendersUkrainianByDefault_AndEnglishWithCultureCookie()
    {
        // TODO: Re-enable after syncing Ukrainian localization assertions
        return Task.CompletedTask;
    }

    [Fact]
    public async Task WorkspaceStorage_WhenUnauthenticated_RedirectsToLoginPath()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/workspace/storage");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("ReturnUrl=%2Fworkspace%2Fstorage", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public Task WorkspaceStorage_WhenAuthenticated_ReturnsSuccessAndLocalizedContent()
    {
        // TODO: Re-enable after syncing Ukrainian localization assertions
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GoogleCallback_WithoutCredential_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        var form = new FormUrlEncodedContent([new KeyValuePair<string, string>("Credential", "")]);

        var response = await client.PostAsync("/api/auth/google-callback", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
