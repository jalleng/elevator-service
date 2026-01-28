using System.Text.Json.Serialization;
using ElevatorAPI.Filters;
using ElevatorAPI.Models;
using ElevatorAPI.Routes;
using ElevatorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.Configure<ElevatorOptions>(builder.Configuration.GetSection("Elevator"));
builder.Services.AddSingleton<FloorRequestService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new()
  {
    Title = "Elevator Service API",
    Version = "v1",
    Description = "A local testing API for elevator floor request management"
  });
  options.SchemaFilter<EnumSchemaFilter>();
});

// JSON configuration - serialize enums as strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// CORS - allow all for local development
builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// HTTP request logging
builder.Services.AddHttpLogging(logging =>
{
  logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
                          Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
                          Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode;
});

var app = builder.Build();

// Middleware pipeline
app.UseDeveloperExceptionPage();
app.UseHttpLogging();
app.UseCors();

// Swagger at root
app.UseSwagger();
app.UseSwaggerUI(options =>
{
  options.SwaggerEndpoint("/swagger/v1/swagger.json", "Elevator Service API v1");
  options.RoutePrefix = string.Empty;
});

// Endpoints
var floorRequestGroup = app.MapGroup("/floorrequests");
FloorRequestEndpoints.Map(floorRequestGroup);

app.Run();
