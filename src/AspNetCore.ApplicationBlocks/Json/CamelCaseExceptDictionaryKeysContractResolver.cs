using System;
using Newtonsoft.Json.Serialization;

namespace AspNetCore.ApplicationBlocks.Json
{
    /// <summary>
    /// Resolves member mappings for a type, camel casing property names but NOT dictionary keys, as the default resolver does.
    /// </summary>
    public class CamelCaseExceptDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
        }
    }
}
