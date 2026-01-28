using ElevatorAPI.Filters;
using ElevatorAPI.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ElevatorAPI.Tests.Filters;

public class EnumSchemaFilterTests
{
  [Fact]
  public void Apply_EnumType_SetsTypeToString()
  {
    // Arrange
    var schema = new OpenApiSchema { Type = "integer" };
    var context = new SchemaFilterContext(
      typeof(Direction),
      null!,
      null!,
      null!
    );
    var filter = new EnumSchemaFilter();

    // Act
    filter.Apply(schema, context);

    // Assert
    Assert.Equal("string", schema.Type);
  }

  [Fact]
  public void Apply_DirectionEnum_AddsAllEnumNamesAsStrings()
  {
    // Arrange
    var schema = new OpenApiSchema
    {
      Type = "integer",
      Enum = new List<IOpenApiAny> { new OpenApiInteger(0), new OpenApiInteger(1) }
    };
    var context = new SchemaFilterContext(
      typeof(Direction),
      null!,
      null!,
      null!
    );
    var filter = new EnumSchemaFilter();

    // Act
    filter.Apply(schema, context);

    // Assert
    Assert.Equal(3, schema.Enum.Count);
    Assert.Contains(schema.Enum, e => (e as OpenApiString)?.Value == "Up");
    Assert.Contains(schema.Enum, e => (e as OpenApiString)?.Value == "Down");
    Assert.Contains(schema.Enum, e => (e as OpenApiString)?.Value == "Both");
  }

  [Fact]
  public void Apply_OriginEnum_AddsAllEnumNamesAsStrings()
  {
    // Arrange
    var schema = new OpenApiSchema
    {
      Type = "integer",
      Enum = new List<IOpenApiAny> { new OpenApiInteger(0) }
    };
    var context = new SchemaFilterContext(
      typeof(Origin),
      null!,
      null!,
      null!
    );
    var filter = new EnumSchemaFilter();

    // Act
    filter.Apply(schema, context);

    // Assert
    Assert.Equal(2, schema.Enum.Count);
    Assert.Contains(schema.Enum, e => (e as OpenApiString)?.Value == "Internal");
    Assert.Contains(schema.Enum, e => (e as OpenApiString)?.Value == "External");
  }

  [Fact]
  public void Apply_EnumType_ClearsExistingEnumValues()
  {
    // Arrange
    var schema = new OpenApiSchema
    {
      Type = "integer",
      Enum = new List<IOpenApiAny>
      {
        new OpenApiInteger(0),
        new OpenApiInteger(1),
        new OpenApiInteger(2)
      }
    };
    var context = new SchemaFilterContext(
      typeof(Direction),
      null!,
      null!,
      null!
    );
    var filter = new EnumSchemaFilter();

    // Act
    filter.Apply(schema, context);

    // Assert
    Assert.DoesNotContain(schema.Enum, e => e is OpenApiInteger);
    Assert.All(schema.Enum, e => Assert.IsType<OpenApiString>(e));
  }

  [Fact]
  public void Apply_NonEnumType_DoesNotModifySchema()
  {
    // Arrange
    var schema = new OpenApiSchema
    {
      Type = "object",
      Properties = new Dictionary<string, OpenApiSchema>
      {
        ["Floor"] = new OpenApiSchema { Type = "integer" }
      }
    };
    var originalType = schema.Type;
    var originalPropertiesCount = schema.Properties.Count;

    var context = new SchemaFilterContext(
      typeof(FloorRequest),
      null!,
      null!,
      null!
    );
    var filter = new EnumSchemaFilter();

    // Act
    filter.Apply(schema, context);

    // Assert
    Assert.Equal(originalType, schema.Type);
    Assert.Equal(originalPropertiesCount, schema.Properties.Count);
  }

  [Fact]
  public void Apply_StringType_DoesNotModifySchema()
  {
    // Arrange
    var schema = new OpenApiSchema { Type = "string" };
    var context = new SchemaFilterContext(
      typeof(string),
      null!,
      null!,
      null!
    );
    var filter = new EnumSchemaFilter();

    // Act
    filter.Apply(schema, context);

    // Assert
    Assert.Equal("string", schema.Type);
    Assert.Empty(schema.Enum);
  }
}
