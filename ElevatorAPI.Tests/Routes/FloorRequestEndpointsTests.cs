using System.Net;
using System.Net.Http.Json;
using ElevatorAPI.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ElevatorAPI.Tests.Routes;

public class FloorRequestEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;
  private const int TestMinFloor = 1;
  private const int TestMaxFloor = 100;

  public FloorRequestEndpointsTests(WebApplicationFactory<Program> factory)
  {
    _factory = factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureAppConfiguration((context, config) =>
      {
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["Elevator:MinFloor"] = TestMinFloor.ToString(),
          ["Elevator:MaxFloor"] = TestMaxFloor.ToString()
        });
      });
    });
    _client = _factory.CreateClient();
  }

  [Fact]
  public async Task GetAllRequests_EmptyList_ReturnsOkWithEmptyArray()
  {
    // Arrange
    await ClearAllRequests();

    // Act
    var response = await _client.GetAsync("/floorrequests");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var requests = await response.Content.ReadFromJsonAsync<FloorRequest[]>();
    Assert.NotNull(requests);
    Assert.Empty(requests);
  }

  [Fact]
  public async Task GetAllRequests_WithRequests_ReturnsAllRequests()
  {
    // Arrange
    await ClearAllRequests();
    var request1 = new FloorRequest(5, Direction.Up, Origin.External);
    var request2 = new FloorRequest(10, Direction.Down, Origin.Internal);
    await _client.PostAsJsonAsync("/floorrequests", request1);
    await _client.PostAsJsonAsync("/floorrequests", request2);

    // Act
    var response = await _client.GetAsync("/floorrequests");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var requests = await response.Content.ReadFromJsonAsync<FloorRequest[]>();
    Assert.NotNull(requests);
    Assert.Equal(2, requests.Length);
  }

  [Fact]
  public async Task GetInternalRequests_FiltersInternalOnly()
  {
    // Arrange
    await ClearAllRequests();
    await _client.PostAsJsonAsync("/floorrequests", new FloorRequest(5, Direction.Both, Origin.Internal));
    await _client.PostAsJsonAsync("/floorrequests", new FloorRequest(7, Direction.Up, Origin.External));
    await _client.PostAsJsonAsync("/floorrequests", new FloorRequest(10, Direction.Both, Origin.Internal));

    // Act
    var response = await _client.GetAsync("/floorrequests/internal");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var requests = await response.Content.ReadFromJsonAsync<FloorRequest[]>();
    Assert.NotNull(requests);
    Assert.Equal(2, requests.Length);
    Assert.All(requests, r => Assert.Equal(Origin.Internal, r.Origin));
  }

  [Fact]
  public async Task GetNextStop_NoRequests_ReturnsNotFound()
  {
    // Arrange
    await ClearAllRequests();

    // Act
    var response = await _client.GetAsync("/floorrequests/next");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetNextStop_WithRequests_ReturnsNextFloor()
  {
    // Arrange
    await ClearAllRequests();
    await _client.PostAsJsonAsync("/floorrequests", new FloorRequest(5, Direction.Up, Origin.External));

    // Act
    var response = await _client.GetAsync("/floorrequests/next");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var nextStop = await response.Content.ReadFromJsonAsync<FloorRequest>();
    Assert.NotNull(nextStop);
    Assert.Equal(5, nextStop.Floor);
  }

  [Fact]
  public async Task PostFloorRequest_ValidRequest_ReturnsCreated()
  {
    // Arrange
    await ClearAllRequests();
    var request = new FloorRequest(8, Direction.Up, Origin.External);

    // Act
    var response = await _client.PostAsJsonAsync("/floorrequests", request);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var createdRequest = await response.Content.ReadFromJsonAsync<FloorRequest>();
    Assert.NotNull(createdRequest);
    Assert.Equal(8, createdRequest.Floor);
    Assert.Equal(Direction.Up, createdRequest.Direction);
    Assert.Equal(Origin.External, createdRequest.Origin);
  }

  [Fact]
  public async Task PostFloorRequest_FloorBelowMinimum_ReturnsValidationError()
  {
    // Arrange
    var request = new FloorRequest(TestMinFloor - 1, Direction.Up, Origin.External);

    // Act
    var response = await _client.PostAsJsonAsync("/floorrequests", request);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task PostFloorRequest_FloorAboveMaximum_ReturnsValidationError()
  {
    // Arrange
    var request = new FloorRequest(TestMaxFloor + 1, Direction.Up, Origin.External);

    // Act
    var response = await _client.PostAsJsonAsync("/floorrequests", request);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task PostFloorRequest_ValidFloorInRange_ReturnsCreated()
  {
    // Arrange
    await ClearAllRequests();
    var request = new FloorRequest((TestMinFloor + TestMaxFloor) / 2, Direction.Down, Origin.Internal);

    // Act
    var response = await _client.PostAsJsonAsync("/floorrequests", request);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }

  [Fact]
  public async Task PostFloorRequest_MaxRange_ReturnsCreated()
  {
    // Arrange
    await ClearAllRequests();
    var request = new FloorRequest(TestMaxFloor, Direction.Down, Origin.Internal);

    // Act
    var response = await _client.PostAsJsonAsync("/floorrequests", request);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }

  [Fact]
  public async Task PostFloorRequest_MinRange_ReturnsCreated()
  {
    // Arrange
    await ClearAllRequests();
    var request = new FloorRequest(TestMinFloor, Direction.Up, Origin.External);

    // Act
    var response = await _client.PostAsJsonAsync("/floorrequests", request);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }

  [Fact]
  public async Task DeleteFloorRequest_ExistingFloor_ReturnsNoContent()
  {
    // Arrange
    await ClearAllRequests();
    await _client.PostAsJsonAsync("/floorrequests", new FloorRequest(7, Direction.Up, Origin.External));

    // Act
    var response = await _client.DeleteAsync("/floorrequests/7");

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task DeleteFloorRequest_NonExistingFloor_ReturnsNotFound()
  {
    // Arrange
    await ClearAllRequests();

    // Act
    var response = await _client.DeleteAsync("/floorrequests/99");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task ClearAllRequests_RemovesAllRequests()
  {
    // Arrange
    await _client.PostAsJsonAsync("/floorrequests", new FloorRequest(5, Direction.Up, Origin.External));
    await _client.PostAsJsonAsync("/floorrequests", new FloorRequest(10, Direction.Down, Origin.Internal));

    // Act
    var response = await _client.PostAsync("/floorrequests/clear", null);

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    // Verify all cleared
    var getResponse = await _client.GetAsync("/floorrequests");
    var requests = await getResponse.Content.ReadFromJsonAsync<FloorRequest[]>();
    Assert.NotNull(requests);
    Assert.Empty(requests);
  }

  [Fact]
  public async Task PostFloorRequest_CreatedLocationHeader_ContainsFloorNumber()
  {
    // Arrange
    await ClearAllRequests();
    var request = new FloorRequest(15, Direction.Both, Origin.Internal);

    // Act
    var response = await _client.PostAsJsonAsync("/floorrequests", request);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);
    Assert.Contains("15", response.Headers.Location.ToString());
  }

  [Fact]
  public async Task ServiceCurrentFloor_RemovesRequestForCurrentFloor()
  {
    // Arrange
    await ClearAllRequests();
    await _client.PostAsJsonAsync("/floorrequests", new FloorRequest(3, Direction.Up, Origin.External));
    await _client.GetAsync("/floorrequests/next"); // Set next stop to floor 3

    // Act
    var response = await _client.PostAsync("/floorrequests/servicecurrentfloor", null);

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    // Verify the request for floor 3 is removed
    var getResponse = await _client.GetAsync("/floorrequests");
    var requests = await getResponse.Content.ReadFromJsonAsync<FloorRequest[]>();
    Assert.NotNull(requests);
    Assert.DoesNotContain(requests, r => r.Floor == 3);
  }

  private async Task ClearAllRequests()
  {
    await _client.PostAsync("/floorrequests/clear", null);
  }
}
