using ElevatorAPI.Services;

namespace ElevatorAPI.Routes;

public static class FloorRequestEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async (FloorRequestService service) =>
        {
            // Get all floor requests
            var requests = service.GetAllRequests();
            return Results.Ok(requests);
        });

        group.MapGet("/internal", async context =>
        {
            // Get all internal floor requests
            await context.Response.WriteAsJsonAsync(new { Message = "All internal floor requests" });
        });

        group.MapGet("/next", async context =>
        {
            // Get the next floor to service
            // Note: This should also remove the current floor being serviced from the request list
            await context.Response.WriteAsJsonAsync(new { Message = "Next floor to service" });
        });

        group.MapPost("/", async context =>
        {
            // Create a new floor request
            await context.Response.WriteAsJsonAsync(new { Message = "New floor request created" });
        });

        group.MapDelete("/{id}", async context =>
        {
            // Delete a floor request by ID / Floor Number
            var id = context.Request.RouteValues["id"];
            await context.Response.WriteAsJsonAsync(new { Message = $"Floor request {id} deleted" });
        });

        group.MapPost("/clear", async context =>
        {
            // Clear all floor requests
            await context.Response.WriteAsJsonAsync(new { Message = "All floor requests cleared" });
        });
    }
}
