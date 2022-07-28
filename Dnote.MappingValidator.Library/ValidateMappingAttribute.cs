namespace Dnote.MappingValidator.Library
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class ValidateMappingAttribute : Attribute
    {
        public string[] ExcludedProperties { get; } = Array.Empty<string>();

        public ValidateMappingAttribute()
        {
        }

        public ValidateMappingAttribute(params string[] excludedProperties) 
            : this()
        {
            ExcludedProperties = excludedProperties;
        }
    }
}
