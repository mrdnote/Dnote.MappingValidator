using System;

namespace Dnote.MappingValidator.Library
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateFunctionMappingAttribute : Attribute
    {
        public string[] ExcludedProperties { get; } = Array.Empty<string>();

        public bool SkipChildObjects { get; set; }

        public ValidateFunctionMappingAttribute()
        {
        }

        public ValidateFunctionMappingAttribute(bool skipChildObjects = false, params string[] excludedProperties) 
            : this()
        {
            SkipChildObjects = skipChildObjects;
            ExcludedProperties = excludedProperties;
        }
    }
}
