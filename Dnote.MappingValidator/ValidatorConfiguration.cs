using System;
using System.Linq.Expressions;

namespace Dnote.MappingValidator
{
    public class ValidatorConfiguration<T> : ValidatorConfigurationBase
    {
        /// <summary>
        /// Adds a property to the ignore list.
        /// </summary>
        public ValidatorConfiguration<T> Ignore<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            var propertyName = getPropertyName(propertySelector);
            IgnoredPropertiesValue.Add(propertyName);
            return this;
        }

        /// <summary>
        /// Helper method to extract the property name from a lambda expression.
        /// </summary>
        private static string getPropertyName<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            if (propertySelector == null)
            {
                throw new ArgumentNullException(nameof(propertySelector));
            }

            if (propertySelector.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            throw new ArgumentException("Invalid property selector expression.");
        }
    }
}