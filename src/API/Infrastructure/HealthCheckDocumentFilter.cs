using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Infrastructure;

public class HealthCheckDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
    {
        var pathItem = new OpenApiPathItem();
        
        var operation = new OpenApiOperation
        {
            Tags = new List<OpenApiTag> { new() { Name = "Health" } },
            Summary = "Get health status",
            Description = "Returns the health status of the API services",
            OperationId = "HealthCheck"
         };

        pathItem.AddOperation(OperationType.Get, operation);
        openApiDoc.Paths.Add("/health", pathItem);
    }
}