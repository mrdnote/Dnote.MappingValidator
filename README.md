# Dnote.MappingValidator
## Preface
A common task in modern programming is writing mapping code between objects, e.g. between an entity object and a model 
or dto object. Many programmers don't like to write this boring mapping code and resort to libraries that do this mapping 
automatically for them like the popular [AutoMapper](https://automapper.org/).

Now I'm gonna be upfront. I don't like AutoMapper, and like many I consider it to have major drawbacks.
Just Google "[don't use automapper](https://www.google.com/search?q=don%27t+use+automapper)", and there are plenty of arguments against it's usage.
My main argument is that it doesn't fit well with the premise of [Extreme Programming](https://en.wikipedia.org/wiki/Extreme_programming). Extreme 
Programming heavily relies on tools making it easy to refactor code, and one part of that is being able to lookup referenced class members. However
class properties that are mapped by AutoMapper are not being referenced at all. 

Example given: You think a property isn't used anymore, while in fact it still 
is, but in runtime. Then you go ahead and remove the property, do hours of refactoring, hit run: and BAM, AutoMapper throws you an error (well not 
throws, it probably logs it somewhere in an obscure place) telling you the property it wants to map no long exists.

## Alternative

So that's why I like to write out mapping code, simple, neat and performant:
```C#
PersonViewModel MapPersonViewModel(PersonDto person) 
{
    return new PersonViewModel
    {
        FirstName = person.FirstName,
        LastName = person.LastName,
        FullName = person.FirstName + " " + person.LastName
    }
}
```

BTW, I like to use a Visual Studio extension which takes most of this coding out of my hands: 
[MappingGenerator](https://github.com/cezarypiatek/MappingGenerator) by Cezary Piątek.

## An AutoMapper advantage

What I **do** like about AutoMapper is that it has a way (not the most elegent way tho) to tell us when a class is changed in such a way that it causes 
some properties not being mapped anymore.

## Here's MappingValidator

Now lets say a property is added to `PersonViewModel` and `PersonDto` named `Age`. If we don't adjust the mapping code, the Age property in the 
source object of class `PersonDto` will never be set in the target object of class `PersonViewModel`.

MappingValidator to the rescue. It will warn you, or exit your application if you want, when this situation arises. It will even tell you which 
property is missing in the mapping.

## MappingValidator supports 3 mapping methods

MappingValidator can validate 3 types of mapping methods: Expression mapping, Procedural mapping and Functional mapping.

### Expression mapping

Expressions encapsulate code that can be easily be reused inside other pieces of code or other expressions. 
Linq relies heavily on expressions and e.g. EntityFramework uses it to convert your C# code into SQL statements.

Example of the mapping we saw before, but in the form of an expression:
```C#
public class PersonMappers
{
    public static Expression<Func<PersonDto, PersonViewModel>> MapPersonViewModel
    {
        get 
        {
            return person => new PersonViewModel
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                FullName = person.FirstName + " " + person.LastName
            };
        }
    }
}
```

### LinqKit

LinqKit is a .NET library that allows you, amongst many other things, to nest expressions. I'm glad to state that 
MappingValidator validated nested expressions without any issue.

### Procedural mapping

Expressions cannot be used to map data to an existing object. We can do this type of mapping with procedural mapping,
which simply means that the objects are mapped using a static method:
```C#
public class PersonMappers
{
    public static void MapPersonViewModel(PersonDto source, PersonViewModel destination)
    {
        destination.FirstName = source.FirstName;
        destination.LastName = source.LastName;
        destination.FullName = source.FirstName + " " + person.LastName;
    }
}
```

Note that the first 2 parameters of your method must be the source and destination object, in that order.
You are free to add extra parameters (which must be nullable) if needed and also have the method return values 
instead of void.

### Functional mapping

Mapping can also be done by creating the target object and returning it from a method:
```C#
public class PersonMappers
{
    public static PersonViewModel MapPersonViewModel(PersonDto source)
    {
        return new PersonViewModel 
        {
            FirstName = source.FirstName,
            LastName = source.LastName,
            FullName = source.FirstName + " " + person.LastName
        };
    }
}
```

Note that the first 1 parameters of your method must be the source object.
You are free to add extra parameters (which must be nullable) if needed.

## Usage

You can use MappingValidator in two ways. Explicitly or declaratively.

### Explicit usage

To validate an expression, just pass it to the static `Validator.Validate` method:
```C#
var isValid = Validator.ValidateExpression(PersonMappers.MapPersonViewModel, null);
```

You can pass a string list as the second parameter, which will be filled with information about the properties missing in the mapping:
```C#
var report = new List<string>();
var isValid = Validator.ValidateExpression(PersonMappers.MapPersonViewModel, report);

Assert.AreEqual("- Age", report[0]);
```

Furthermore, if you don't want the Age property to be included in the check, you can explicitly exclude it by specifying it as a parameter in the 
`Validate` call:
```C#
var isValid = Validator.ValidateExpression(PersonMappers.MapPersonViewModel, null, 
    nameof(MapPersonViewModel.Age));

Assert.IsTrue(isValid);
```

Nested properties can also be specified by using "dot" notation:
```C#
var isValid = Validator.ValidateExpression(PersonMappers.MapPersonViewModel, null, 
    $"{nameof(MapPersonViewModel.Pets)}.{nameof(MapPetViewModel.Age)}");

Assert.IsTrue(isValid);
```

To validate a procedure mapping, pass it to the static `Validator.ValidateProcedure` method:
```C#
var isValid = Validator.ValidateProcedure(PersonMappers.MapPersonViewModel, null);
```

To validate a function mapping, pass it to the static `Validator.ValidateFunction` method:
```C#
var isValid = Validator.ValidateFunction(PersonMappers.MapPersonViewModel, null);
```

### Declarative usage

You can decorate your mapping expression by a `ValidateMapping` attribute, and then, on application start, validate the entire assembly or assemblies.
You also need to decorate the class the expression is part of! Like this:
```C#
[ValidateMapping]
public class PersonMappers
{
    [ValidatePropertyMapping]
    public static Expression<Func<PersonViewModel, PersonDto>> MapPersonViewModel
    // ...
```

Then you can automatically validate the mappings of an entire assembly using:
```C#
var isValid = Library.Validator.ValidateAssembly(assembly, report);
```

A nice way to scan all assemblies of your application, is by scanning them upon load:
```C#
public static void Main(string[] args)
{
#if DEBUG
    AppDomain.CurrentDomain.AssemblyLoad += (object? sender, AssemblyLoadEventArgs args) => 
    {
        if (args.LoadedAssembly.FullName != null 
            && !args.LoadedAssembly.FullName.StartsWith("System.") 
            && !args.LoadedAssembly.FullName.StartsWith("Microsoft."))
        {
            var report = new List<string>();
            if (!Validator.ValidateAssembly(args.LoadedAssembly, report))
            {
                Debug.WriteLine("");
                Debug.WriteLine($"--- Mapping validation errors detected! ----------------------------------------------------------------");
                report.ForEach(x => Debug.WriteLine(x));
                Debug.WriteLine($"--------------------------------------------------------------------------------------------------------");
                Debug.WriteLine("Exiting process...");
                Environment.Exit(-1);
            }
        }
    };
#endif
```

When validating mappings in this declarative way, it's also possible to skip certain properties, just like we did before. We do this by specifying 
them as parameters of the `ValidateMapping` attribute:
```C#
[ValidatePropertyMapping(nameof(MapPersonViewModel.Age))]
public static Expression<Func<PersonViewModel, PersonDto>> MapPersonViewModel
{
    // ...
```

To declaratively validate a procedural mapping do this:
```C#
[ValidateMapping]
public class PersonMappers
{
    [ValidateProcedureMapping]
    public static void MapPersonViewModel(PersonDto source, PersonViewModel destination)
    // ...
```

To declaratively validate a function mapping do this:
```C#
[ValidateMapping]
public class PersonMappers
{
    [ValidateFunctionMapping]
    public static PersonViewModel MapPersonViewModel(PersonDto source)
    // ...
```

Add the following test to you unit test project(s) in order to validate an entire assembly for correct mappings
(provided declarative mapping validation is used).

```C#
[Test]
public void Validators_work_for_assembly()
{
    var assembly = Assembly.GetAssembly(typeof(global::MyProject.MyAssembly.SomeClass));
    Debug.Assert(assembly != null);
    var report = new List<string>();
    var isValid = Validator.ValidateAssembly(assembly, report);
    Assert.IsTrue(isValid);
}
```