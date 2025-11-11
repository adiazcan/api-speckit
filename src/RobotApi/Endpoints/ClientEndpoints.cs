using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RobotApi.Models.Dtos;
using RobotApi.Services;

namespace RobotApi.Endpoints;

/// <summary>
/// Endpoints for client management.
/// </summary>
public static class ClientEndpoints
{
    public static RouteGroupBuilder MapClientEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListClients)
            .WithName("ListClients")
            .WithSummary("List all clients")
            .RequireAuthorization("ViewerPolicy");

        group.MapPost("/", CreateClient)
            .WithName("CreateClient")
            .WithSummary("Create a new client")
            .RequireAuthorization("AdminPolicy");

        group.MapGet("/{clientId}", GetClient)
            .WithName("GetClient")
            .WithSummary("Get client by ID")
            .RequireAuthorization("ViewerPolicy");

        group.MapPut("/{clientId}", UpdateClient)
            .WithName("UpdateClient")
            .WithSummary("Update an existing client")
            .RequireAuthorization("AdminPolicy");

        group.MapDelete("/{clientId}", DeleteClient)
            .WithName("DeleteClient")
            .WithSummary("Delete a client")
            .RequireAuthorization("AdminPolicy");

        return group;
    }

    private static async Task<IResult> ListClients(IClientService clientService)
    {
        var clients = await clientService.ListClientsAsync();
        var responses = clients.Select(ClientResponse.FromClient);
        return Results.Ok(responses);
    }

    private static async Task<IResult> CreateClient(
        [FromBody] ClientRequest request,
        IClientService clientService,
        IValidator<ClientRequest> validator)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var client = await clientService.CreateClientAsync(request);
            var response = ClientResponse.FromClient(client);
            return Results.Created($"/v1/clients/{client.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetClient(
        string clientId,
        IClientService clientService)
    {
        var client = await clientService.GetClientAsync(clientId);

        if (client == null)
        {
            return Results.NotFound(new { message = $"Client with ID '{clientId}' not found" });
        }

        var response = ClientResponse.FromClient(client);
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateClient(
        string clientId,
        [FromBody] ClientRequest request,
        IClientService clientService,
        IValidator<ClientRequest> validator)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var client = await clientService.UpdateClientAsync(clientId, request);

            if (client == null)
            {
                return Results.NotFound(new { message = $"Client with ID '{clientId}' not found" });
            }

            var response = ClientResponse.FromClient(client);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteClient(
        string clientId,
        IClientService clientService)
    {
        var deleted = await clientService.DeleteClientAsync(clientId);

        if (!deleted)
        {
            return Results.NotFound(new { message = $"Client with ID '{clientId}' not found" });
        }

        return Results.NoContent();
    }
}
