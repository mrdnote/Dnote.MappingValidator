using System;

namespace Dnote.MappingValidator.Library
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ValidatePropertyMappingAttribute : Attribute
    {
        public string[] ExcludedProperties { get; } = Array.Empty<string>();

        public bool SkipChildObjects { get; set; }

        public ValidatePropertyMappingAttribute()
        {
        }

        public ValidatePropertyMappingAttribute(bool skipChildObjects = false, params string[] excludedProperties) 
            : this()
        {
            SkipChildObjects = skipChildObjects;
            ExcludedProperties = excludedProperties;
        }
    }
}
