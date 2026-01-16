namespace ElevatorAPI.Routes;

public static class FloorRequestEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async context =>
        {
            // Get all floor requests
            await context.Response.WriteAsJsonAsync(new { Message = "All floor requests" });
        });

        group.MapGet("/internal", async context =>
        {
            // Get all internal floor requests
            await context.Response.WriteAsJsonAsync(new { Message = "All internal floor requests" });
        });

        group.MapGet("/nextfloor", async context =>
        {
            // Get the next floor to service
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

        group.MapDelete("/clear", async context =>
        {
            // Clear all floor requests
            await context.Response.WriteAsJsonAsync(new { Message = "All floor requests cleared" });
        });
    }
}
