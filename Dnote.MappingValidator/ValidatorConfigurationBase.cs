using System.Collections.Generic;

namespace Dnote.MappingValidator
{
    public class ValidatorConfigurationBase
    {
        internal readonly HashSet<string> IgnoredPropertiesValue = new HashSet<string>();

        public HashSet<string> IgnoredProperties() => IgnoredPropertiesValue;

        internal bool IgnoreChildObjectsValue;

        public ValidatorConfigurationBase IgnoreChildObjects()
        {
            IgnoreChildObjectsValue = true;
            return this;
        }
    }
}