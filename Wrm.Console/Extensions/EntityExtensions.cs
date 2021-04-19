using Microsoft.Xrm.Sdk;


namespace Xrm.Plugins.Common.Extensions
{
    public static class EntityExtensions
    {
        public static TEnum? GetOptionSetValueAsEnum<TEnum>(this Entity target, string attributeName) where TEnum : struct
        {
            return target?.GetAttributeValue<OptionSetValue>(attributeName).ToEnum<TEnum>();
        }
    }
}
