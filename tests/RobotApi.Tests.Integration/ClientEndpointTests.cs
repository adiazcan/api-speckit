using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RobotApi.Models.Dtos;
using RobotApi.Tests.Integration.Helpers;

namespace RobotApi.Tests.Integration;

/// <summary>
/// Integration tests for Client endpoints
/// </summary>
public class ClientEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ClientEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListClients_ReturnsOk_WithClients()
    {
        // Arrange
        var token = AuthHelper.GetViewerToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/v1/clients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clients = await response.Content.ReadFromJsonAsync<List<ClientResponse>>();
        Assert.NotNull(clients);
        Assert.NotEmpty(clients);
    }

    [Fact]
    public async Task CreateClient_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var token = AuthHelper.GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new ClientRequest
        {
            Name = "Test Client",
            Email = $"testclient-{Guid.NewGuid()}@example.com",
            Phone = "+1-555-1234",
            Company = "Test Company"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/clients", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var client = await response.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.NotNull(client);
        Assert.Equal(request.Name, client.Name);
        Assert.Equal(request.Email, client.Email);
        Assert.Equal(request.Phone, client.Phone);
        Assert.Equal(request.Company, client.Company);
        Assert.NotNull(client.Id);
    }

    [Fact]
    public async Task CreateClient_ReturnsBadRequest_WithInvalidEmail()
    {
        // Arrange
        var token = AuthHelper.GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new ClientRequest
        {
            Name = "Test Client",
            Email = "invalid-email",
            Phone = "+1-555-1234",
            Company = "Test Company"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/clients", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateClient_ReturnsConflict_WithDuplicateEmail()
    {
        // Arrange
        var token = AuthHelper.GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var uniqueEmail = $"duplicate-{Guid.NewGuid()}@example.com";

        var request1 = new ClientRequest
        {
            Name = "Test Client 1",
            Email = uniqueEmail,
            Phone = "+1-555-1234",
            Company = "Test Company"
        };

        var request2 = new ClientRequest
        {
            Name = "Test Client 2",
            Email = uniqueEmail,
            Phone = "+1-555-5678",
            Company = "Test Company 2"
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/v1/clients", request1);
        var response2 = await _client.PostAsJsonAsync("/v1/clients", request2);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task GetClient_ReturnsOk_WithExistingClient()
    {
        // Arrange
        var token = AuthHelper.GetViewerToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/v1/clients/client-1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var client = await response.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.NotNull(client);
        Assert.Equal("client-1", client.Id);
    }

    [Fact]
    public async Task GetClient_ReturnsNotFound_WithNonExistentClient()
    {
        // Arrange
        var token = AuthHelper.GetViewerToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/v1/clients/non-existent-client");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var token = AuthHelper.GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First, create a client
        var createRequest = new ClientRequest
        {
            Name = "Original Name",
            Email = $"update-test-{Guid.NewGuid()}@example.com",
            Phone = "+1-555-1234",
            Company = "Original Company"
        };

        var createResponse = await _client.PostAsJsonAsync("/v1/clients", createRequest);
        var createdClient = await createResponse.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.NotNull(createdClient);

        // Update the client
        var updateRequest = new ClientRequest
        {
            Name = "Updated Name",
            Email = createdClient.Email,
            Phone = "+1-555-9999",
            Company = "Updated Company"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/v1/clients/{createdClient.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedClient = await response.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.NotNull(updatedClient);
        Assert.Equal(updateRequest.Name, updatedClient.Name);
        Assert.Equal(updateRequest.Phone, updatedClient.Phone);
        Assert.Equal(updateRequest.Company, updatedClient.Company);
    }

    [Fact]
    public async Task UpdateClient_ReturnsNotFound_WithNonExistentClient()
    {
        // Arrange
        var token = AuthHelper.GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new ClientRequest
        {
            Name = "Updated Name",
            Email = "updated@example.com",
            Phone = "+1-555-9999",
            Company = "Updated Company"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/v1/clients/non-existent-client", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_ReturnsNoContent_WithExistingClient()
    {
        // Arrange
        var token = AuthHelper.GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First, create a client
        var createRequest = new ClientRequest
        {
            Name = "To Be Deleted",
            Email = $"delete-test-{Guid.NewGuid()}@example.com",
            Phone = "+1-555-1234",
            Company = "Delete Test Company"
        };

        var createResponse = await _client.PostAsJsonAsync("/v1/clients", createRequest);
        var createdClient = await createResponse.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.NotNull(createdClient);

        // Act
        var response = await _client.DeleteAsync($"/v1/clients/{createdClient.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify client is deleted
        var getResponse = await _client.GetAsync($"/v1/clients/{createdClient.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_ReturnsNotFound_WithNonExistentClient()
    {
        // Arrange
        var token = AuthHelper.GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync("/v1/clients/non-existent-client");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateClient_ReturnsUnauthorized_WithoutToken()
    {
        // Arrange
        var request = new ClientRequest
        {
            Name = "Test Client",
            Email = "test@example.com",
            Phone = "+1-555-1234",
            Company = "Test Company"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/clients", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateClient_ReturnsForbidden_WithViewerToken()
    {
        // Arrange
        var token = AuthHelper.GetViewerToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new ClientRequest
        {
            Name = "Test Client",
            Email = "test@example.com",
            Phone = "+1-555-1234",
            Company = "Test Company"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/clients", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
