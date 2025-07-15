using API.Infrastructure;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UnitTests.API.Infrastructure;

public class HealthCheckDocumentFilterTests
{
    [Fact]
    public void Apply_ShouldAddHealthCheckPath()
    {
        // Arrange
        var openApiDoc = new OpenApiDocument
        {
            Paths = new OpenApiPaths()
        };
        var context = new DocumentFilterContext(null, null, null);
        var filter = new HealthCheckDocumentFilter();

        // Act
        filter.Apply(openApiDoc, context);

        // Assert
        Assert.True(openApiDoc.Paths.ContainsKey("/health"));
        var pathItem = openApiDoc.Paths["/health"];
        var operation = pathItem.Operations[OperationType.Get];

        Assert.NotNull(operation);
        Assert.Equal("Get health status", operation.Summary);
        Assert.Equal("Returns the health status of the API services", operation.Description);
        Assert.Equal("HealthCheck", operation.OperationId);
        Assert.Contains(operation.Tags, t => t.Name == "Health");
    }
}