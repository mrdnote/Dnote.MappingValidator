using System.Collections.Generic;

namespace Dnote.MappingValidator
{
    public class ValidatorConfigurationBase
    {
        public readonly HashSet<string> IgnoredProperties = new HashSet<string>();

        internal bool IgnoreChildObjectsValue;

        public ValidatorConfigurationBase IgnoreChildObjects()
        {
            IgnoreChildObjectsValue = true;
            return this;
        }
    }
}