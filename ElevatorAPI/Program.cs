
using ElevatorAPI.Routes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var floorRequestGroup = app.MapGroup("/floorrequests");
FloorRequestEndpoints.Map(floorRequestGroup);

app.Run();
