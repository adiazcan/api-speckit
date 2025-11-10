using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RobotApi.Models.Dtos;
using RobotApi.Services;

namespace RobotApi.Endpoints;

/// <summary>
/// Endpoints for subscription management.
/// </summary>
public static class SubscriptionEndpoints
{
    public static RouteGroupBuilder MapSubscriptionEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateSubscription)
            .WithName("CreateSubscription")
            .WithSummary("Create a new event subscription")
            .RequireAuthorization("OperatorPolicy");

        group.MapGet("/", ListSubscriptions)
            .WithName("ListSubscriptions")
            .WithSummary("List all subscriptions for the current operator")
            .RequireAuthorization("OperatorPolicy");

        group.MapGet("/{id}", GetSubscription)
            .WithName("GetSubscription")
            .WithSummary("Get subscription by ID")
            .RequireAuthorization("OperatorPolicy");

        group.MapPut("/{id}", UpdateSubscription)
            .WithName("UpdateSubscription")
            .WithSummary("Update an existing subscription")
            .RequireAuthorization("OperatorPolicy");

        group.MapDelete("/{id}", DeleteSubscription)
            .WithName("DeleteSubscription")
            .WithSummary("Delete a subscription")
            .RequireAuthorization("OperatorPolicy");

        return group;
    }

    private static async Task<IResult> CreateSubscription(
        [FromBody] SubscriptionRequest request,
        ISubscriptionService subscriptionService,
        IValidator<SubscriptionRequest> validator,
        HttpContext context)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // Get operator ID from claims
        var operatorId = context.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(operatorId))
        {
            return Results.Unauthorized();
        }

        var subscription = await subscriptionService.CreateSubscriptionAsync(operatorId, request);
        var response = SubscriptionResponse.FromSubscription(subscription);

        return Results.Created($"/v1/subscriptions/{subscription.Id}", response);
    }

    private static async Task<IResult> ListSubscriptions(
        ISubscriptionService subscriptionService,
        HttpContext context)
    {
        // Get operator ID from claims
        var operatorId = context.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(operatorId))
        {
            return Results.Unauthorized();
        }

        var subscriptions = await subscriptionService.ListSubscriptionsAsync(operatorId);
        var responses = subscriptions.Select(SubscriptionResponse.FromSubscription);

        return Results.Ok(responses);
    }

    private static async Task<IResult> GetSubscription(
        string id,
        ISubscriptionService subscriptionService,
        HttpContext context)
    {
        // Get operator ID from claims
        var operatorId = context.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(operatorId))
        {
            return Results.Unauthorized();
        }

        var subscription = await subscriptionService.GetSubscriptionAsync(id);

        if (subscription == null)
        {
            return Results.NotFound(new { message = $"Subscription with ID '{id}' not found" });
        }

        // Verify ownership
        if (subscription.OperatorId != operatorId)
        {
            return Results.NotFound(new { message = $"Subscription with ID '{id}' not found" });
        }

        var response = SubscriptionResponse.FromSubscription(subscription);
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateSubscription(
        string id,
        [FromBody] SubscriptionRequest request,
        ISubscriptionService subscriptionService,
        IValidator<SubscriptionRequest> validator,
        HttpContext context)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // Get operator ID from claims
        var operatorId = context.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(operatorId))
        {
            return Results.Unauthorized();
        }

        var subscription = await subscriptionService.UpdateSubscriptionAsync(id, operatorId, request);

        if (subscription == null)
        {
            return Results.NotFound(new { message = $"Subscription with ID '{id}' not found" });
        }

        var response = SubscriptionResponse.FromSubscription(subscription);
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteSubscription(
        string id,
        ISubscriptionService subscriptionService,
        HttpContext context)
    {
        // Get operator ID from claims
        var operatorId = context.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(operatorId))
        {
            return Results.Unauthorized();
        }

        var deleted = await subscriptionService.DeleteSubscriptionAsync(id, operatorId);

        if (!deleted)
        {
            return Results.NotFound(new { message = $"Subscription with ID '{id}' not found" });
        }

        return Results.NoContent();
    }
}
