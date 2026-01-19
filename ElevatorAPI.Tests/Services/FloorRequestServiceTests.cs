using ElevatorAPI.Models;
using ElevatorAPI.Services;
using Xunit;

namespace ElevatorAPI.Tests.Services;

public class FloorRequestServiceTests
{
    [Fact]
    public void GetAllRequests_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        var result = service.GetAllRequests();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void AddRequest_FirstRequest_AddsSuccessfully()
    {
        // Arrange
        var service = new FloorRequestService();
        var request = new FloorRequest (5, Direction.Up, Origin.External);
        // Act
        service.AddRequest(request);

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Single(requests);
        Assert.Equal(5, requests[0].Floor);
    }

    [Fact]
    public void AddRequest_MultipleRequests_MaintainsSortedOrder()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (8, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (3, Direction.Up, Origin.External));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Equal(4, requests.Count);
        Assert.Equal(2, requests[0].Floor);
        Assert.Equal(3, requests[1].Floor);
        Assert.Equal(5, requests[2].Floor);
        Assert.Equal(8, requests[3].Floor);
    }

    [Fact]
    public void AddRequest_DuplicateFloorDifferentDirections_MergesToBoth()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (5, Direction.Down, Origin.External));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Single(requests);
        Assert.Equal(Direction.Both, requests[0].Direction);
    }

    [Fact]
    public void AddRequest_DuplicateFloorSameDirection_DoesNotDuplicate()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Single(requests);
        Assert.Equal(Direction.Up, requests[0].Direction);
    }

    [Fact]
    public void AddRequest_ExternalThenInternal_UpgradesToInternal()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.Internal));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Single(requests);
        Assert.Equal(Origin.Internal, requests[0].Origin);
    }

    [Fact]
    public void GetInternalRequests_MixedRequests_ReturnsOnlyInternal()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (2, Direction.Up, Origin.Internal));
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (7, Direction.Down, Origin.Internal));

        // Act
        var result = service.GetInternalRequests().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(Origin.Internal, r.Origin));
    }

    [Fact]
    public void GetNextStop_EmptyList_ReturnsNull()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        var result = service.GetNextStop();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNextStop_FirstCall_ReturnsFirstFloorAndSetsDirectionUp()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));

        // Act
        var result = service.GetNextStop();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Floor);
        Assert.Equal(Direction.Up, service.CurrentDirection);
    }

    [Fact]
    public void GetNextStop_GoingUp_ReturnsNextFloorUp()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (2, Direction.Up, Origin.Internal));
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.Internal));
        service.AddRequest(new FloorRequest (7, Direction.Up, Origin.Internal));

        // Act
        var first = service.GetNextStop();   // Floor 2
        var second = service.GetNextStop();  // Floor 5
        var third = service.GetNextStop();   // Floor 7

        // Assert
        Assert.Equal(2, first?.Floor);
        Assert.Equal(5, second?.Floor);
        Assert.Equal(7, third?.Floor);
        Assert.Equal(Direction.Up, service.CurrentDirection);
    }

    [Fact]
    public void GetNextStop_ReachesTop_ReversesDirectionAndGoesDown()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (2, Direction.Both, Origin.Internal));
        service.AddRequest(new FloorRequest (5, Direction.Both, Origin.Internal));
        service.AddRequest(new FloorRequest (7, Direction.Both, Origin.Internal));

        // Act
        service.GetNextStop();  // Floor 2 (going up)
        service.GetNextStop();  // Floor 5 (going up)
        service.GetNextStop();  // Floor 7 (going up, at top)
        var afterReverse = service.GetNextStop();  // Should reverse to down

        // Assert
        Assert.Equal(5, afterReverse?.Floor);
        Assert.Equal(Direction.Down, service.CurrentDirection);
    }

    [Fact]
    public void GetNextStop_SkipsWrongDirectionExternalRequests()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (5, Direction.Down, Origin.External)); // Should skip going up
        service.AddRequest(new FloorRequest (7, Direction.Up, Origin.External));

        // Act
        var first = service.GetNextStop();   // Floor 2 (up)
        var second = service.GetNextStop();  // Floor 7 (skips 5, wrong direction)

        // Assert
        Assert.Equal(2, first?.Floor);
        Assert.Equal(7, second?.Floor);
    }

    [Fact]
    public void GetNextStop_InternalRequest_AlwaysStops()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (5, Direction.Down, Origin.Internal)); // Internal - must stop
        service.AddRequest(new FloorRequest (7, Direction.Up, Origin.External));

        // Act
        var first = service.GetNextStop();   // Floor 2
        var second = service.GetNextStop();  // Floor 5 (internal, stops even though going up)

        // Assert
        Assert.Equal(2, first?.Floor);
        Assert.Equal(5, second?.Floor);
    }

    [Fact]
    public void RemoveRequest_ExistingFloor_RemovesAndReturnsTrue()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));

        // Act
        var result = service.RemoveRequest(5);

        // Assert
        Assert.True(result);
        Assert.Empty(service.GetAllRequests());
    }

    [Fact]
    public void RemoveRequest_NonExistingFloor_ReturnsFalse()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));

        // Act
        var result = service.RemoveRequest(10);

        // Assert
        Assert.False(result);
        Assert.Single(service.GetAllRequests());
    }

    [Fact]
    public void ClearAllRequests_RemovesAllRequestsAndResetsState()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.External));
        service.GetNextStop(); // Move to floor 2

        // Act
        service.ClearAllRequests();

        // Assert
        Assert.Empty(service.GetAllRequests());
        Assert.Equal(Direction.Up, service.CurrentDirection);
    }

    [Fact]
    public void ServiceCurrentFloor_RemovesCurrentFloorAndAdvances()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest (2, Direction.Up, Origin.Internal));
        service.AddRequest(new FloorRequest (5, Direction.Up, Origin.Internal));
        service.GetNextStop(); // Move to floor 2

        // Act
        service.ServiceCurrentFloor();

        // Assert
        var remaining = service.GetAllRequests().ToList();
        Assert.Single(remaining);
        Assert.Equal(5, remaining[0].Floor);
    }
}
