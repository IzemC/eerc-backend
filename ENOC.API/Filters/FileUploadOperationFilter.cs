using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ENOC.API.Filters;

/// <summary>
/// Request filter that skips form file parameters to avoid Swagger generation errors
/// </summary>
public class FileUploadRequestFilter : IRequestBodyFilter
{
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        // This filter is needed but the actual work is done in the operation filter
    }
}

/// <summary>
/// Operation filter that properly configures multipart/form-data for file uploads
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parameters = context.ApiDescription.ParameterDescriptions;

        // Check if this operation has any file upload parameters
        var hasFileParameter = parameters.Any(p =>
            p.ModelMetadata != null &&
            (p.ModelMetadata.ModelType == typeof(IFormFile) ||
             p.ModelMetadata.ModelType == typeof(IFormFile[])));

        if (!hasFileParameter)
            return;

        // Collect all form parameters from the method
        var formParameters = new Dictionary<string, OpenApiSchema>();
        var requiredFields = new HashSet<string>();

        foreach (var parameter in context.MethodInfo.GetParameters())
        {
            // Skip route/path parameters (like inspectionId in the route)
            if (context.ApiDescription.ParameterDescriptions
                .Any(p => p.Name == parameter.Name && p.Source.Id == "Path"))
                continue;

            var paramName = parameter.Name ?? "field";
            var isRequired = !IsNullableType(parameter.ParameterType) && !parameter.HasDefaultValue;

            if (parameter.ParameterType == typeof(IFormFile))
            {
                formParameters[paramName] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = $"Upload {paramName} file"
                };
                if (isRequired) requiredFields.Add(paramName);
            }
            else if (parameter.ParameterType == typeof(IFormFile[]))
            {
                formParameters[paramName] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
                if (isRequired) requiredFields.Add(paramName);
            }
            else if (parameter.GetCustomAttribute<FromFormAttribute>() != null)
            {
                formParameters[paramName] = new OpenApiSchema
                {
                    Type = "string",
                    Description = paramName
                };
                if (isRequired) requiredFields.Add(paramName);
            }
        }

        if (formParameters.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = requiredFields.Any(),
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = formParameters,
                            Required = requiredFields
                        }
                    }
                }
            };

            // Remove form parameters from query string
            var parametersToRemove = operation.Parameters
                .Where(p => formParameters.ContainsKey(p.Name))
                .ToList();

            foreach (var param in parametersToRemove)
            {
                operation.Parameters.Remove(param);
            }
        }
    }

    private static bool IsNullableType(Type type)
    {
        if (!type.IsValueType)
            return true; // Reference types are nullable by default in this context

        return Nullable.GetUnderlyingType(type) != null;
    }
}
