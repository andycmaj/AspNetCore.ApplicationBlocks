using AspNetCore.ApplicationBlocks.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AspNetCore.ApplicationBlocks
{
    public static class JsonConfiguration
    {
        public static readonly JsonSerializerSettings DefaultSerializerSettings;

        static JsonConfiguration()
        {
            // Default settings taken from https://github.com/aspnet/Mvc/blob/6.0.0-rc1/src/Microsoft.AspNet.Mvc.Formatters.Json/Internal/SerializerSettingsProvider.cs
            // in order to match ASP.NET MVC's default JSON settings.
            DefaultSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Populate,

                // Limit the object graph we'll consume to a fixed depth. This prevents stackoverflow exceptions
                // from deserialization errors that might occur from deeply nested objects.
                MaxDepth = 32,

                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
                // TypeNameHandling = TypeNameHandling.None
            };

            DefaultSerializerSettings.ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver();
            DefaultSerializerSettings.Converters.Add(new StringEnumConverter());
        }
    }
}
