using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// TODO: if the target of a mapped class gets a new property, validation will still succeed. Find out a way to check this change too.

namespace Dnote.MappingValidator.Library
{
    public class Validator
    {
        public static bool ValidateExpression(Expression expression, List<string>? report, bool skipChildObjects = false, params string[] excludedProperties)
        {
            report ??= new List<string>();

            var lambdaExpression = expression as LambdaExpression ?? throw new InvalidOperationException();
            var func = lambdaExpression.Compile();
            var input = Activator.CreateInstance(lambdaExpression.Parameters[0].Type) ?? throw new InvalidOperationException();
            fillWithSampleValues(input, excludedProperties, null, false);
            var parameters = new List<object?> { input };
            addDefaultLambdaExpressionParameters(parameters, lambdaExpression);
            var output = func.DynamicInvoke(parameters.ToArray()) ?? throw new InvalidOperationException();

            var input2 = Activator.CreateInstance(lambdaExpression.Parameters[0].Type) ?? throw new InvalidOperationException();
            fillWithSampleValues(input2, excludedProperties, null, true);
            var parameters2 = new List<object?> { input2 };
            addDefaultLambdaExpressionParameters(parameters2, lambdaExpression);
            var output2 = func.DynamicInvoke(parameters2.ToArray()) ?? throw new InvalidOperationException();

            checkIfAllPropertiesAreChanged(output, output2, skipChildObjects, excludedProperties, report, null);

            return !report.Any();
        }

        private static void addDefaultLambdaExpressionParameters(List<object?> parameters, LambdaExpression lambdaExpression)
        {
            foreach (var parameter in lambdaExpression.Parameters.Skip(1))
            {
                if (parameter.Type.IsValueType)
                {
                    parameters.Add(Activator.CreateInstance(parameter.Type));
                }
                else
                {
                    parameters.Add(null);
                }
            }
        }

        public static bool ValidateProcedure(Delegate method, List<string>? report, bool skipChildObjects, params string[] excludedProperties)
        {
            var methodInfo = method.GetMethodInfo() ?? throw new InvalidOperationException();
            return validateProcedure(methodInfo, report, skipChildObjects, excludedProperties);
        }

        private static bool validateProcedure(MethodInfo method, List<string>? report, bool skipChildObjects, params string[] excludedProperties)
        {
            report ??= new List<string>();

            var parameters = method.GetParameters();

            if (parameters.Length < 2)
            {
                throw new InvalidOperationException($"Validated procedures must have 2 or more parameters! Method: {method.DeclaringType.FullName}.{method.Name}");
            }

            var sourceType = parameters[0].ParameterType;
            var destType = parameters[1].ParameterType;

            var source1 = Activator.CreateInstance(sourceType) ?? throw new InvalidOperationException();
            fillWithSampleValues(source1, excludedProperties, null, false);
            
            var dest1 = Activator.CreateInstance(destType) ?? throw new InvalidOperationException();

            var paramList1 = new List<object?> { source1, dest1 };
            for (int i = 2; i < parameters.Count(); i++)
            {
                paramList1.Add(null);
            }
            method.Invoke(null, paramList1.ToArray());

            var source2 = Activator.CreateInstance(sourceType) ?? throw new InvalidOperationException();
            fillWithSampleValues(source2, excludedProperties, null, true);

            var dest2 = Activator.CreateInstance(destType) ?? throw new InvalidOperationException();

            var paramList2 = new List<object?> { source2, dest2 };
            for (int i = 2; i < parameters.Count(); i++)
            {
                paramList2.Add(null);
            }
            method.Invoke(null, paramList2.ToArray());

            checkIfAllPropertiesAreChanged(dest1, dest2, skipChildObjects, excludedProperties, report, null);

            return !report.Any();
        }        
        
        public static bool ValidateFunction(Delegate method, List<string>? report, bool skipChildObjects, params string[] excludedProperties)
        {
            var methodInfo = method.GetMethodInfo() ?? throw new InvalidOperationException();
            return validateFunction(methodInfo, report, skipChildObjects, excludedProperties);
        }

        private static bool validateFunction(MethodInfo method, List<string>? report, bool skipChildObjects, params string[] excludedProperties)
        {
            report ??= new List<string>();

            var parameters = method.GetParameters();
            var sourceType = parameters[0].ParameterType;

            var source1 = Activator.CreateInstance(sourceType) ?? throw new InvalidOperationException();
            fillWithSampleValues(source1, excludedProperties, null, false);

            var paramList1 = new List<object?> { source1 };
            for (int i = 1; i < parameters.Count(); i++)
            {
                paramList1.Add(null);
            }
            var dest1 = method.Invoke(null, paramList1.ToArray()) ?? throw new InvalidOperationException();

            var source2 = Activator.CreateInstance(sourceType) ?? throw new InvalidOperationException();
            fillWithSampleValues(source2, excludedProperties, null, true);

            var paramList2 = new List<object?> { source2 };
            for (int i = 1; i < parameters.Count(); i++)
            {
                paramList2.Add(null);
            }
            var dest2 = method.Invoke(null, paramList2.ToArray()) ?? throw new InvalidOperationException();

            checkIfAllPropertiesAreChanged(dest1, dest2, skipChildObjects, excludedProperties, report, null);

            return !report.Any();
        }

        public static bool ValidateAssembly(Assembly assembly, List<string>? invalidExpressionReport = null)
        {
            var result = true;

            var types = assembly.GetTypes().Where(t => t.GetCustomAttributes().OfType<ValidateMappingAttribute>().Any()).ToList();

            foreach (var type in types)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(t => t.GetCustomAttributes().OfType<ValidatePropertyMappingAttribute>().Any());
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttributes().OfType<ValidatePropertyMappingAttribute>().First();
                    var expression = property.GetValue(null) as Expression ?? throw new InvalidOperationException();
                    var report = new List<string>();
                    try
                    {
                        var validateResult = ValidateExpression(expression, report, attribute.SkipChildObjects, attribute.ExcludedProperties);
                        if (validateResult == false)
                        {
                            result = false;
                            invalidExpressionReport?.Add($"{type.FullName}.{property.Name}");
                            invalidExpressionReport?.AddRange(report);
                        }
                    }
                    catch(Exception ex)
                    {
                        result = false;
                        invalidExpressionReport?.Add($"Validation failed for {type.FullName}.{property.Name}: {ex.Message}");
                    }
                }

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(t => t.GetCustomAttributes().OfType<ValidateProcedureMappingAttribute>().Any());
                foreach (var method in methods)
                {
                    var report = new List<string>();
                    var attribute = method.GetCustomAttributes().OfType<ValidateProcedureMappingAttribute>().First();
                    var validateResult = validateProcedure(method, report, attribute.SkipChildObjects, attribute.ExcludedProperties);
                    if (validateResult == false)
                    {
                        result = false;
                        invalidExpressionReport?.Add($"{type.FullName}.{method.Name}");
                        invalidExpressionReport?.AddRange(report);
                    }
                }

                var functions = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(t => t.GetCustomAttributes().OfType<ValidateFunctionMappingAttribute>().Any());
                foreach (var function in functions)
                {
                    var report = new List<string>();
                    var attribute = function.GetCustomAttributes().OfType<ValidateFunctionMappingAttribute>().First();
                    try
                    {
                        var validateResult = validateFunction(function, report, attribute.SkipChildObjects, attribute.ExcludedProperties);
                        if (validateResult == false)
                        {
                            result = false;
                            invalidExpressionReport?.Add($"{type.FullName}.{function.Name}");
                            invalidExpressionReport?.AddRange(report);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        invalidExpressionReport?.Add($"Validation failed for {type.FullName}.{function.Name}: {ex.Message}");
                    }
                }
            }

            return result;
        }

        private static void checkIfAllPropertiesAreChanged(object output, object output2, bool skipChildObjects, 
            IEnumerable<string>? excludedProperties, List<string> unmappedProperties, string? unmappedPrefix)
        {
            Debug.Assert(output.GetType() == output2.GetType());

            var properties = output.GetType().GetProperties();

            if (excludedProperties != null)
            {
                properties = properties.Where(p => !excludedProperties.Any(pp => pp == concat(unmappedPrefix, p.Name))).ToArray();
            }

            if (skipChildObjects)
            {
                properties = properties.Where(p => isSimple(p.PropertyType)).ToArray();
            }

            foreach (var property in properties)
            {
                if (property.CanRead && property.CanWrite)
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
                        unmappedProperties.Add($"- {concat(unmappedPrefix, property.Name)}");
                    }

                    // If we already established the props are not the same, not need to traverse children
                    if (propertyResult) 
                    {
                        if (isList(property.PropertyType))
                        {
                            var list1 = value1 as IEnumerable ?? throw new InvalidOperationException();
                            var list2 = value2 as IEnumerable ?? throw new InvalidOperationException();
                            var enumerator = list1.GetEnumerator();
                            if (enumerator.MoveNext())
                            {
                                var item1 = enumerator.Current;
                                var enumerator2 = list2.GetEnumerator();
                                if (enumerator2.MoveNext())
                                {
                                    var item2 = enumerator2.Current;

                                    if (item1 == null || item2 == null)
                                    {
                                        unmappedProperties.Add($"- {concat(unmappedPrefix, property.Name)}");
                                    }
                                    else
                                    {
                                        checkIfAllPropertiesAreChanged(item1, item2, skipChildObjects, excludedProperties, unmappedProperties,
                                            concat(unmappedPrefix, property.Name));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string concat(string? s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
            {
                return s2;
            }

            return $"{s1}.{s2}";
        }

        /// <summary>
        /// Handy utility function to fill an object with sample values.
        /// </summary>
        public static void FillWithSampleValues(object inputObject, IEnumerable<string>? excludedProperties)
        {
            fillWithSampleValues(inputObject, excludedProperties, null, false, null, 0);
        }

        private static void fillWithSampleValues(object inputObject, IEnumerable<string>? excludedProperties, string? unmappedPrefix, bool variant,
            IDictionary<(Type, int), object>? instantiatedObjects = null, int level = 0)
        {
            _ = inputObject ?? throw new ArgumentNullException(nameof(inputObject));

            // Another safety: only instantiate/fill each class type once
            instantiatedObjects ??= new Dictionary<(Type, int), object>();

            var properties = inputObject.GetType().GetProperties();

            if (excludedProperties != null)
            {
                properties = properties.Where(p => !excludedProperties.Any(pp => pp == concat(unmappedPrefix, p.Name))).ToArray();
            }

            foreach (var property in properties)
            {
                var canWrite = property.CanWrite;
                if (isSimple(property.PropertyType))
                {
                    if (canWrite)
                    {
                        fillProperty(property, inputObject, variant);
                    }
                }
                else if (isList(property.PropertyType))
                {
                    var isGenericList = property.PropertyType.GenericTypeArguments.Length == 1;
                    if (isGenericList && canWrite)
                    {
                        var listType = typeof(List<>);
                        var itemType = property.PropertyType.GenericTypeArguments[0];
                        var constructedListType = listType.MakeGenericType(itemType);
                        var list = (Activator.CreateInstance(constructedListType) as IList) ?? throw new InvalidOperationException();
                        property.SetValue(inputObject, list);

                        if (isSimple(itemType))
                        {
                            var listElement = getSimpleSampleValue(itemType, variant);
                            list.Add(listElement);
                        }
                        else
                        {
                            var listElement = getOrCreateAndFillInstance(itemType, excludedProperties, concat(unmappedPrefix, property.Name), variant,
                                instantiatedObjects, level);
                            if (listElement != null)
                            {
                                list.Add(listElement);
                            }
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
                            childObject = getOrCreateAndFillInstance(property.PropertyType, excludedProperties, concat(unmappedPrefix, property.Name), 
                                variant, instantiatedObjects, level);
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

        private static object? getOrCreateAndFillInstance(Type propertyType, IEnumerable<string>? excludedProperties, string? unmappedPrefix, 
            bool variant, IDictionary<(Type, int), object> instantiatedObjects, int level)
        {
            if (level > 10)
            {
                return null;
            }

            if (instantiatedObjects.ContainsKey((propertyType, level)))
            {
                return instantiatedObjects[(propertyType, level)];
            }

            if (!propertyType.IsAbstract)
            {
                var instance = Activator.CreateInstance(propertyType) ?? throw new InvalidOperationException();
                instantiatedObjects[(propertyType, level)] = instance;
                fillWithSampleValues(instance, excludedProperties, unmappedPrefix, variant, instantiatedObjects, level + 1);
                return instance;
            }

            return null;
        }

        private static void fillProperty(PropertyInfo property, object instance, bool variant)
        {
            var value = getSimpleSampleValue(property.PropertyType, variant);

            property.SetValue(instance, value);
        }

        private static object? getSimpleSampleValue(Type type, bool variant)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            if (type == typeof(string))
            {
                return variant ? "/dummy" : "/alternative-dummy";
            }
            else if (type == typeof(bool))
            {
                return variant;
            }
            else if (type == typeof(char))
            {
                return variant ? 'a' : 'b';
            }
            else if (type == typeof(sbyte))
            {
                return variant ? 12 : 45;
            }
            else if (type == typeof(byte))
            {
                return variant ? 123 : 456;
            }
            else if (type == typeof(short))
            {
                return variant ? 1 : -4;
            }
            else if (type == typeof(ushort))
            {
                return variant ? 1 : 4;
            }
            else if (type == typeof(int))
            {
                return variant ? 1234 : -4567;
            }
            else if (type == typeof(uint))
            {
                return variant ? 1234 : 4567;
            }
            else if (type == typeof(long))
            {
                return variant ? 12345L : -45678L;
            }
            else if (type == typeof(ulong))
            {
                return variant ? 12345L : 45678L;
            }
            else if (type == typeof(float))
            {
                return variant ? 23.45F : 678.999F;
            }
            else if (type == typeof(double))
            {
                return variant ? 23.456 : 678.9999;
            }
            else if (type == typeof(DateTime))
            {
                return variant ? new DateTime(2022, 3, 31) : new DateTime(1971, 2, 10);
            }
            else if (type == typeof(DateTimeOffset))
            {
                return variant ? new DateTimeOffset(new DateTime(2022, 3, 30)) : new DateTimeOffset(new DateTime(1971, 2, 9));
            }
            else if (type == typeof(Guid))
            {
                return variant ? new Guid("666507cd-08e4-4a9a-a65f-cba49a2162a7") : new Guid("983fe766-c921-45db-b0f1-55a3c2336d4b");
            }
            else if (type.IsEnum)
            {
                return variant ? Enum.GetValues(type).GetValue(0) : Enum.GetValues(type).Length > 1 ? Enum.GetValues(type).GetValue(1) : Enum.GetValues(type).GetValue(0);
            }
            else if (type == typeof(Uri))
            {
                return variant ? new Uri("http://abc.com") : new Uri("http://xyz.com");
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static bool isSimple(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return isSimple(typeInfo.GetGenericArguments()[0]);
            }
            return typeInfo.IsPrimitive
              || typeInfo.IsEnum
              || type.Equals(typeof(DateTime))
              || type.Equals(typeof(DateTimeOffset))
              || type.Equals(typeof(Guid))
              || type.Equals(typeof(Uri))
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }

        private static bool isList(Type type)
        {
            return type != typeof(string) && type.GetInterfaces().Contains(typeof(IEnumerable));
        }

    }
}
