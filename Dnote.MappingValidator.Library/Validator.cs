using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

// TODO: if the target of a mapped class gets a new property, validation will still succeed. Find out a way to check this change too.

namespace Dnote.MappingValidator.Library
{
    public class Validator
    {
        public static bool Validate(Expression expression, List<string>? report, bool skipChildObjects = false, params string[] excludedProperties)
        {
            report ??= new List<string>();

            var lambdaExpression = expression as LambdaExpression;
            Debug.Assert(lambdaExpression != null);

            var func = lambdaExpression.Compile();

            var input = Activator.CreateInstance(lambdaExpression.Parameters[0].Type);
            Debug.Assert(input != null);
            FillWithSampleValues(input, false);
            var output = func.DynamicInvoke(input);
            Debug.Assert(output != null);

            var input2 = Activator.CreateInstance(lambdaExpression.Parameters[0].Type);
            Debug.Assert(input2 != null);
            FillWithSampleValues(input2, true);
            var output2 = func.DynamicInvoke(input2);
            Debug.Assert(output2 != null);

            CheckIfAllPropertiesAreChanged(output, output2, skipChildObjects, excludedProperties, report, null);

            return !report.Any();
        }

        public static bool ValidateMethod(Delegate method, List<string>? report, bool skipChildObjects, params string[] excludedProperties)
        {
            return Validate(method.GetMethodInfo(), report, skipChildObjects, excludedProperties);
        }

        public static bool Validate(MethodInfo method, List<string>? report, bool skipChildObjects, params string[] excludedProperties)
        {
            report ??= new List<string>();

            var sourceType = method.GetParameters()[0].ParameterType;
            var destType = method.GetParameters()[1].ParameterType;

            var source1 = Activator.CreateInstance(sourceType);
            Debug.Assert(source1 != null);
            FillWithSampleValues(source1, false);
            
            var dest1 = Activator.CreateInstance(destType);
            Debug.Assert(dest1 != null);
            
            method.Invoke(null, new object[] { source1, dest1 });

            var source2 = Activator.CreateInstance(sourceType);
            Debug.Assert(source2 != null);
            FillWithSampleValues(source2, true);

            var dest2 = Activator.CreateInstance(destType);
            Debug.Assert(dest2 != null);

            method.Invoke(null, new object[] { source2, dest2 });

            CheckIfAllPropertiesAreChanged(dest1, dest2, skipChildObjects, excludedProperties, report, null);

            return !report.Any();
        }

        public static bool ValidateAssembly(Assembly assembly, List<string>? invalidExpressionReport = null)
        {
            var result = true;

            var types = assembly.GetTypes().Where(t => t.GetCustomAttributes().OfType<ValidateMappingAttribute>().Any());

            foreach (var type in types)
            {
                var properties = type.GetProperties().Where(t => t.GetCustomAttributes().OfType<ValidateMappingAttribute>().Any());
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttributes().OfType<ValidateMappingAttribute>().First();
                    var expression = property.GetValue(null) as Expression;
                    Debug.Assert(expression != null);
                    var report = new List<string>();
                    var validateResult = Validate(expression, report, attribute.SkipChildObjects, attribute.ExcludedProperties);
                    if (validateResult == false)
                    {
                        result = false;
                        invalidExpressionReport?.Add($"{type.FullName}.{property.Name}");
                        invalidExpressionReport?.AddRange(report);
                    }
                }

                var methods = type.GetMethods().Where(t => t.GetCustomAttributes().OfType<ValidateMappingAttribute>().Any());
                foreach (var method in methods)
                {
                    var report = new List<string>();
                    var attribute = method.GetCustomAttributes().OfType<ValidateMappingAttribute>().First();
                    var validateResult = Validate(method, report, attribute.SkipChildObjects, attribute.ExcludedProperties);
                    if (validateResult == false)
                    {
                        result = false;
                        invalidExpressionReport?.Add($"{type.FullName}.{method.Name}");
                        invalidExpressionReport?.AddRange(report);
                    }
                }
            }

            return result;
        }

        private static void CheckIfAllPropertiesAreChanged(object output, object output2, bool skipChildObjects, 
            IEnumerable<string>? excludedProperties, List<string> unmappedProperties, string? unmappedPrefix)
        {
            Debug.Assert(output.GetType() == output2.GetType());

            var properties = output.GetType().GetProperties();

            if (excludedProperties != null)
            {
                properties = properties.Where(p => !excludedProperties.Any(pp => pp == Concat(unmappedPrefix, p.Name))).ToArray();
            }

            if (skipChildObjects)
            {
                properties = properties.Where(p => !IsSimple(p.PropertyType)).ToArray();
            }

            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    var propertyResult = true;

                    var value1 = property.GetValue(output, null);
                    var value2 = property.GetValue(output2, null);

                    if (value1 == null && value2 == null)
                    {
                        propertyResult = false;
                    }
                    else if (value1 == null || value2 == null)
                    {
                        propertyResult = false;
                    }
                    else if (value1.Equals(value2))
                    {
                        propertyResult = false;
                    }

                    if (!propertyResult)
                    {
                        unmappedProperties.Add($"- {Concat(unmappedPrefix, property.Name)}");
                    }

                    // If we already established the props are not the same, not need to traverse children
                    if (propertyResult) 
                    {
                        if (IsList(property.PropertyType))
                        {
                            var list1 = value1 as IEnumerable ?? throw new InvalidOperationException();
                            var list2 = value2 as IEnumerable ?? throw new InvalidOperationException();
                            var enumerator = list1.GetEnumerator();
                            enumerator.MoveNext();
                            var item1 = enumerator.Current;
                            var enumerator2 = list2.GetEnumerator();
                            enumerator2.MoveNext();
                            var item2 = enumerator2.Current;

                            if (item1 == null || item2 == null)
                            {
                                unmappedProperties.Add($"- {Concat(unmappedPrefix, property.Name)}");
                            }
                            else
                            {
                                CheckIfAllPropertiesAreChanged(item1, item2, skipChildObjects, excludedProperties, unmappedProperties, 
                                    Concat(unmappedPrefix, property.Name));
                            }
                        }
                    }
                }
            }
        }

        private static string Concat(string? s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
            {
                return s2;
            }

            return $"{s1}.{s2}";
        }

        private static void FillWithSampleValues(object inputObject, bool variant, IDictionary<Type, object>? instantiatedObjects = null, int level = 0)
        {
            Debug.Assert(inputObject != null);

            // Another safety: only instantiate/fill each class type once
            instantiatedObjects ??= new Dictionary<Type, object>();

            var properties = inputObject.GetType().GetProperties();

            foreach (var property in properties)
            {
                var canWrite = property.CanWrite;
                if (IsSimple(property.PropertyType))
                {
                    if (canWrite)
                    {
                        FillProperty(property, inputObject, variant);
                    }
                }
                else if (IsList(property.PropertyType))
                {
                    var isGenericList = property.PropertyType.GenericTypeArguments.Length == 1;
                    if (isGenericList && canWrite)
                    {
                        var listType = typeof(List<>);
                        var itemType = property.PropertyType.GenericTypeArguments[0];
                        var constructedListType = listType.MakeGenericType(itemType);
                        var list = (Activator.CreateInstance(constructedListType) as IList) ?? throw new InvalidOperationException();
                        property.SetValue(inputObject, list);

                        var listElement = GetOrCreateAndFillInstance(itemType, variant, instantiatedObjects, level);

                        if (listElement != null)
                        {
                            list.Add(listElement);
                        }
                    }
                }
                else
                {
                    var childObject = property.GetValue(inputObject, null);

                    if (childObject != null || canWrite)
                    {
                        if (childObject == null)
                        {
                            childObject = GetOrCreateAndFillInstance(property.PropertyType, variant, instantiatedObjects, level);
                            property.SetValue(inputObject, childObject);
                        }
                        else
                        {
                           // FillWithSampleValues(childObject, variant, instantiatedObjects, level + 1);
                        }
                    }
                }
            }
        }

        private static object? GetOrCreateAndFillInstance(Type propertyType, bool variant, IDictionary<Type, object> instantiatedObjects, int level)
        {
            if (level > 50)
            {
                return null;
            }

            if (instantiatedObjects.ContainsKey(propertyType))
            {
                return instantiatedObjects[propertyType];
            }

            var instance = Activator.CreateInstance(propertyType);
            Debug.Assert(instance != null);
            instantiatedObjects[propertyType] = instance;
            FillWithSampleValues(instance, variant, instantiatedObjects, level + 1);
            return instance;
        }

        private static void FillProperty(PropertyInfo property, object instance, bool variant)
        {
            object? value;
            var propertyType = property.PropertyType;

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            if (propertyType == typeof(string))
            {
                value = variant ? "lkjdfklj" : "mnbcxvnhfn";
            }
            else if (propertyType == typeof(bool))
            {
                value = variant;
            }
            else if (propertyType == typeof(int))
            {
                value = variant ? 123 : 456;
            }
            else if (propertyType == typeof(double))
            {
                value = variant ? 23.45 : 678.999;
            }
            else if (propertyType == typeof(DateTime))
            {
                value = variant ? new DateTime(2022, 3, 31) : new DateTime(1971, 2, 10);
            }
            else if (propertyType == typeof(DateTimeOffset))
            {
                value = variant ? new DateTimeOffset(new DateTime(2022, 3, 30)) : new DateTimeOffset(new DateTime(1971, 2, 9));
            }
            else if (propertyType == typeof(Guid))
            {
                value = variant ? new Guid("666507cd-08e4-4a9a-a65f-cba49a2162a7") : new Guid("983fe766-c921-45db-b0f1-55a3c2336d4b");
            }
            else if (propertyType.IsEnum)
            {
                value = variant ? Enum.GetValues(propertyType).GetValue(0) : Enum.GetValues(propertyType).GetValue(1);
            }
            else
            {
                throw new InvalidOperationException();
            }
            property.SetValue(instance, value);
        }

        private static bool IsSimple(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(typeInfo.GetGenericArguments()[0]);
            }
            return typeInfo.IsPrimitive
              || typeInfo.IsEnum
              || type.Equals(typeof(DateTime))
              || type.Equals(typeof(DateTimeOffset))
              || type.Equals(typeof(Guid))
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }

        private static bool IsList(Type type)
        {
            return type != typeof(string) && type.GetInterfaces().Contains(typeof(IEnumerable));
        }

    }
}
