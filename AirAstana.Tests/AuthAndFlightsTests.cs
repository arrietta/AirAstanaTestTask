using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AirAstana.Tests;

public class AuthAndFlightsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthAndFlightsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_CreatesNewUser()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "SovaNaSkakalke",
            password = "password67"
        });

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task CreateFlight_ReturnForbidden()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "SovaNaSkakalke",
            password = "password67"
        });

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login?.Token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);

        var response = await client.PostAsJsonAsync("/api/flights", new
        {
            origin = "almaty",
            destination = "astana",
            departure = "2026-07-15T18:51:51+06:00",
            arrival = "2026-07-15T20:51:51+06:00",
            status = 1
        });

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Moderator_CreateFlight()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "moderator",
            password = "password"
        });

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login?.Token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);

        var response = await client.PostAsJsonAsync("/api/flights", new
        {
            origin = "almaty",
            destination = "astana",
            departure = "2026-07-15T18:51:51+06:00",
            arrival = "2026-07-15T20:51:51+06:00",
            status = 1
        });

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Flights_ReturnsFlights()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "read_user",
            password = "password123"
        });

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);

        var response = await client.GetAsync("/api/flights");

        Assert.True(response.IsSuccessStatusCode);
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}