using ElevatorAPI.Models;
using ElevatorAPI.Services;

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
        var request = new FloorRequest(5, Direction.Up, Origin.External);

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
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(8, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(3, Direction.Up, Origin.External));

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
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Down, Origin.External));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Single(requests);
        Assert.Equal(Direction.Both, requests[0].Direction);
    }

    [Fact]
    public void AddRequest_DuplicateFloorDifferentDirections_MergesToBoth_WithMultipleRequests()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Down, Origin.External));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Equal(2, requests.Count);
        Assert.Equal(Direction.Both, requests[1].Direction);
    }

    [Fact]
    public void AddRequest_DuplicateFloorSameDirection_DoesNotDuplicate()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Single(requests);
        Assert.Equal(Direction.Up, requests[0].Direction);
    }

    [Fact]
    public void AddRequest_DuplicateFloorSameDirection_DoesNotDuplicate_WithMultipleRequests()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Equal(2, requests.Count);
        Assert.Equal(Direction.Up, requests[1].Direction);
    }

    [Fact]
    public void AddRequest_ExternalThenInternal_UpdatesToExternalBoth_WithMultipleRequests()
    {
        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.Internal));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Equal(2, requests.Count);
        Assert.Equal(Origin.External, requests[1].Origin);
        Assert.Equal(Direction.Both, requests[1].Direction);
    }

    [Fact]
    public void AddRequest_ExternalThenInternal_ServicesBothDirectionsWhenNecessary()
    {
        //This test ensures that the car services both directions when there are mixed requests internal and external.

        // Arrange
        var service = new FloorRequestService();

        // Act
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Down, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.Internal));
        service.AddRequest(new FloorRequest(7, Direction.Up, Origin.External));

        // Assert
        var requests = service.GetAllRequests().ToList();
        Assert.Equal(3, requests.Count);
        Assert.Equal(Origin.External, requests[1].Origin);
        Assert.Equal(Direction.Both, requests[1].Direction);

        service.GetNextStop(); // Floor 2
        service.ServiceCurrentFloor();

        var secondStop = service.GetNextStop(); // Floor 5 on way up
        service.ServiceCurrentFloor();

        var thirdStop = service.GetNextStop(); // Floor 7
        service.ServiceCurrentFloor();

        var fourthStop = service.GetNextStop(); // Floor 5 on way down

        Assert.Equal(5, secondStop?.Floor);
        Assert.Equal(Direction.Both, secondStop?.Direction);
        Assert.Equal(7, thirdStop?.Floor);
        Assert.Equal(5, fourthStop?.Floor);
        Assert.Equal(Direction.Down, fourthStop?.Direction);
    }

    [Fact]
    public void GetInternalRequests_MixedRequests_ReturnsOnlyInternal()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.Internal));
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(7, Direction.Down, Origin.Internal));

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
    public void GetNextStop_FirstCall_ReturnsFirstFloor()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));

        // Act
        var result = service.GetNextStop();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Floor);
    }

    [Fact]
    public void GetNextStop_GoingUp_ReturnsNextFloorUp()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(6, Direction.Down, Origin.External));
        service.AddRequest(new FloorRequest(7, Direction.Up, Origin.External));

        // Act
        var first = service.GetNextStop();
        service.ServiceCurrentFloor(); // Service floor 2

        var second = service.GetNextStop();
        service.ServiceCurrentFloor(); // Service floor 5

        var third = service.GetNextStop();

        // Assert
        Assert.Equal(2, first?.Floor);
        Assert.Equal(5, second?.Floor);
        Assert.Equal(7, third?.Floor);
    }

    [Fact]
    public void GetNextStop_ReachesTop_ReversesDirectionAndGoesDown()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest(2, Direction.Both, Origin.Internal));
        service.AddRequest(new FloorRequest(5, Direction.Both, Origin.External));
        service.AddRequest(new FloorRequest(6, Direction.Down, Origin.External));
        service.AddRequest(new FloorRequest(7, Direction.Both, Origin.Internal));

        // Act
        service.GetNextStop();
        service.ServiceCurrentFloor(); // Service floor 2

        service.GetNextStop();
        service.ServiceCurrentFloor(); // Service floor 5

        service.GetNextStop();
        service.ServiceCurrentFloor(); // Service floor 7

        var afterReverse = service.GetNextStop();

        // Assert
        Assert.Equal(6, afterReverse?.Floor);
    }

    [Fact]
    public void GetNextStop_InternalRequest_AlwaysStops()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Down, Origin.Internal));
        service.AddRequest(new FloorRequest(7, Direction.Up, Origin.External));

        // Act
        var first = service.GetNextStop();
        service.ServiceCurrentFloor(); // Service floor 2

        var second = service.GetNextStop();

        // Assert
        Assert.Equal(2, first?.Floor);
        Assert.Equal(5, second?.Floor);
    }

    [Fact]
    public void RemoveRequest_ExistingFloor_RemovesAndReturnsTrue()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));

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
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));

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
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.External));
        service.GetNextStop();

        // Act
        service.ClearAllRequests();

        // Assert
        Assert.Empty(service.GetAllRequests());
    }

    [Fact]
    public void ServiceCurrentFloor_RemovesCurrentFloorAndAdvances()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest(2, Direction.Up, Origin.Internal));
        service.AddRequest(new FloorRequest(5, Direction.Up, Origin.Internal));
        service.GetNextStop(); // Move to floor 2

        // Act
        service.ServiceCurrentFloor();
        var next = service.GetNextStop();

        // Assert
        var remaining = service.GetAllRequests().ToList();
        Assert.Single(remaining);
        Assert.Equal(5, remaining[0].Floor);
        Assert.Equal(5, next?.Floor);
    }

    [Fact]
    public void ServiceCurrentFloor_UpdatesDirectionOnRequestIfRequestIsExternalAndBoth()
    {
        // Arrange
        var service = new FloorRequestService();
        service.AddRequest(new FloorRequest(1, Direction.Up, Origin.External));
        service.AddRequest(new FloorRequest(5, Direction.Both, Origin.External));
        service.AddRequest(new FloorRequest(7, Direction.Up, Origin.External));

        service.GetNextStop(); // Move to floor 1
        service.ServiceCurrentFloor(); // Service floor 1

        service.GetNextStop(); // Move to floor 5
        service.ServiceCurrentFloor(); // Service floor 5

        service.GetNextStop(); // Move to floor 7
        service.ServiceCurrentFloor(); // Service floor 7

        var nextStop = service.GetNextStop(); // Move to floor 5 again

        // Assert
        var remaining = service.GetAllRequests().ToList();
        Assert.Single(remaining);
        Assert.Equal(5, remaining[0].Floor);
        Assert.Equal(Direction.Down, remaining[0].Direction); // Direction should be updated to Down
        Assert.Equal(5, nextStop?.Floor);
    }

    [Fact]
    public void GetNextStop_AllWrongDirection_ReversesAndReturnsLastFloor_LargeNumberOfFloors()
    {
        // Arrange
        var service = new FloorRequestService();

        service.AddRequest(new FloorRequest(1, Direction.Up, Origin.External));

        for (int i = 1; i <= 1000; i++)
        {
            service.AddRequest(new FloorRequest(i, Direction.Down, Origin.External));
        }

        // Act - Going up initially, should reverse and find floor 1000 going down
        var firstStop = service.GetNextStop();
        service.ServiceCurrentFloor();

        var secondStop = service.GetNextStop();
        service.ServiceCurrentFloor();

        var thirdStop = service.GetNextStop();

        // Assert

        Assert.NotNull(firstStop);
        Assert.Equal(1, firstStop.Floor); // Should reverse and stop at top floor going down
        Assert.NotNull(secondStop);
        Assert.Equal(1000, secondStop.Floor); // Next floor down
        Assert.NotNull(thirdStop);
        Assert.Equal(999, thirdStop.Floor); // Next floor down
    }

    [Fact]
    public void GetNextStop_MixedDirections_SkipsWrongDirection_LargeNumberOfFloors()
    {
        // Arrange
        var service = new FloorRequestService();

        service.AddRequest(new FloorRequest(1, Direction.Up, Origin.External));

        for (int i = 2; i <= 100; i++)
        {
            service.AddRequest(new FloorRequest(i, Direction.Down, Origin.External));
        }
        service.AddRequest(new FloorRequest(101, Direction.Up, Origin.External));

        // Act
        var firstStop = service.GetNextStop(); // Should go to floor 1 first and set direction Up
        service.ServiceCurrentFloor();

        var secondStop = service.GetNextStop(); // Should skip 2-100 and go to 101

        // Assert
        Assert.NotNull(firstStop);
        Assert.Equal(1, firstStop.Floor);
        Assert.NotNull(secondStop);
        Assert.Equal(101, secondStop.Floor);
    }
}
