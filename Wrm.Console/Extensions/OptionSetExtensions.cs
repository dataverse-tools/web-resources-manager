using System;
using Microsoft.Xrm.Sdk;


namespace Xrm.Plugins.Common.Extensions
{
    public static class OptionSetExtensions
    {
        public static TEnum? ToEnum<TEnum>(this OptionSetValue optionSetValue) where TEnum : struct
        {
            return optionSetValue == null ? null : (TEnum?)Enum.ToObject(typeof(TEnum), optionSetValue.Value);
        }

        public static OptionSetValue ToOptionSetValue<TEnum>(this TEnum enumValue) where TEnum : struct
        {
            return new OptionSetValue(enumValue.GetHashCode());
        }
    }
}
