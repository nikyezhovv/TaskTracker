using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;


namespace TaskTracker.Api.Implementations.HostConfiguration;

public sealed class BearerSecuritySchemeTransformer
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes.Add(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization"
            });

        foreach (var operation in document.Paths.Values
                     .SelectMany(path => path.Operations!))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(
                        "Bearer", document)] = []
                });
        }

        return Task.CompletedTask;
    }
}
