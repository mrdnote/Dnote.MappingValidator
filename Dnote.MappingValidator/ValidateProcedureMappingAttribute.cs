using System;

namespace Dnote.MappingValidator.Library
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateProcedureMappingAttribute : Attribute
    {
        public string[] ExcludedProperties { get; } = Array.Empty<string>();

        public bool SkipChildObjects { get; set; }

        public ValidateProcedureMappingAttribute()
        {
        }

        public ValidateProcedureMappingAttribute(bool skipChildObjects = false, params string[] excludedProperties) 
            : this()
        {
            SkipChildObjects = skipChildObjects;
            ExcludedProperties = excludedProperties;
        }
    }
}
