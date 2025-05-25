using Microsoft.OpenApi.Models;
using System;

namespace CAPSI.Sante.API.Configuration
{
    public static class SwaggerConfig
    {
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CAPSI.Sante API",
                    Version = "v1",
                    Description = "API RESTful pour la gestion des rendez-vous médicaux",
                    Contact = new OpenApiContact
                    {
                        Name = "Support Team",
                        Email = "support@capsisante.com"
                    }
                });

                // Configuration pour les types génériques
                c.CustomSchemaIds(type =>
                {
                    if (type.IsGenericType)
                    {
                        string genericTypeName = type.Name;
                        if (genericTypeName.IndexOf('`') > 0)
                        {
                            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                        }

                        string genericArgs = string.Join("_", type.GetGenericArguments().Select(t => GetSchemaIdForType(t)));
                        return $"{genericTypeName}Of{genericArgs}";
                    }

                    return GetSchemaIdForType(type);
                });

                // Ajout de la sécurité JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        private static string GetSchemaIdForType(Type type)
        {
            if (type.IsGenericType)
            {
                string genericTypeName = type.Name;
                if (genericTypeName.IndexOf('`') > 0)
                {
                    genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                }

                string genericArgs = string.Join("_", type.GetGenericArguments().Select(GetSchemaIdForType));
                return $"{genericTypeName}Of{genericArgs}";
            }

            string typeName = type.Name;
            if (type.FullName?.Contains("Firestore") == true)
                return "Firestore" + typeName;

            return typeName;
        }
    }
}