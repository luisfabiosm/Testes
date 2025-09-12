using Domain.Core.Common.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Configurations
{
    public static class SerializeConfiguration
    {
        public static IServiceCollection ConfigureSerializeJsonOptions(this IServiceCollection services)
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                ConfigureJsonOptions(options.SerializerOptions);
            });

            return services;
        }

        private static void ConfigureJsonOptions(JsonSerializerOptions options)
        {

            // Performance configurations
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = false;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;

            //options.TypeInfoResolver = ApiJsonContext.Default;

            try
            {
                if (ApiJsonContext.Default != null)
                {
                    options.TypeInfoResolver = JsonTypeInfoResolver.Combine(
                        ApiJsonContext.Default,
                        new DefaultJsonTypeInfoResolver()
                    );
                }
                else
                {
                    options.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                }
            }
            catch
            {
                options.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
            }

            // Performance optimizations
            options.DefaultBufferSize = 16 * 1024;
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        }
    }
}