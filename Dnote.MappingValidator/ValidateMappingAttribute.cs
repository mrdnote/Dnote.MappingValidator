﻿using System;

namespace Dnote.MappingValidator.Library
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ValidateMappingAttribute : Attribute
    {
        public string[] ExcludedProperties { get; } = Array.Empty<string>();

        public bool SkipChildObjects { get; set; }

        public ValidateMappingAttribute()
        {
        }

        public ValidateMappingAttribute(bool skipChildObjects = false, params string[] excludedProperties) 
            : this()
        {
            SkipChildObjects = skipChildObjects;
            ExcludedProperties = excludedProperties;
        }
    }
}
