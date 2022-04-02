namespace Dnote.MappingValidator.Library
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class ValidateMappingAttribute : Attribute
    {
        public string[] ExcludedProperties { get; } = new string[0];

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
