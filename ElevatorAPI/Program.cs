
using ElevatorAPI.Routes;
using ElevatorAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<FloorRequestService>();
var app = builder.Build();

var floorRequestGroup = app.MapGroup("/floorrequests");
FloorRequestEndpoints.Map(floorRequestGroup);

app.Run();
