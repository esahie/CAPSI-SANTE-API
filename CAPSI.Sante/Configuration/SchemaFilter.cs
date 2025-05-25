using CAPSI.Sante.Common;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CAPSI.Sante.API.Configuration
{
    public class SchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsGenericType &&
                context.Type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                // Personnaliser le schéma pour ApiResponse<T>
                schema.Title = context.Type.Name;
                // Autres personnalisations si nécessaire
            }
        }
    }
}
