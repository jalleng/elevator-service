using ElevatorAPI.Models;
using ElevatorAPI.Services;
using Microsoft.Extensions.Options;

namespace ElevatorAPI.Routes;

public static class FloorRequestEndpoints
{
  public static void Map(RouteGroupBuilder group)
  {
    group.MapGet("/", IResult (FloorRequestService service) =>
    {
      // Get all floor requests
      var requests = service.GetAllRequests();
      return TypedResults.Ok(requests);
    });

    group.MapGet("/internal", IResult (FloorRequestService service) =>
    {
      // Get all internal floor requests
      var requests = service.GetInternalRequests();
      return TypedResults.Ok(requests);
    });

    group.MapGet("/next", IResult (FloorRequestService service) =>
    {
      // Get the next floor to service
      var nextStop = service.GetNextStop();
      return nextStop is not null
        ? TypedResults.Ok(nextStop)
        : TypedResults.NotFound();
    });

    group.MapPost("/", IResult (FloorRequestService service, FloorRequest request, IOptions<ElevatorOptions> options) =>
    {
      var config = options.Value;

      // Validate floor range from configuration, allows for custom floor range.
      if (request.Floor < config.MinFloor || request.Floor > config.MaxFloor)
      {
        var errors = new Dictionary<string, string[]>
        {
          ["Floor"] = [$"Floor must be between {config.MinFloor} and {config.MaxFloor}"]
        };
        return TypedResults.ValidationProblem(errors);
      }
      // Future validations can be added here

      // Create a new floor request
      service.AddRequest(request);
      return TypedResults.Created($"/floorrequests/{request.Floor}", request);
    });

    group.MapDelete("/{floor}", IResult (FloorRequestService service, int floor) =>
    {
      // Delete a floor request Floor Number
      var success = service.RemoveRequest(floor);
      return success
        ? TypedResults.NoContent()
        : TypedResults.NotFound();
    });

    group.MapPost("/clear", IResult (FloorRequestService service) =>
    {
      // Clear all floor requests
      service.ClearAllRequests();
      return TypedResults.NoContent();
    });

    group.MapPost("/servicecurrentfloor", IResult (FloorRequestService service) =>
    {
      // Service the current floor. Should be called when the elevator arrives at a floor.
      service.ServiceCurrentFloor();
      return TypedResults.NoContent();
    });
  }
}
