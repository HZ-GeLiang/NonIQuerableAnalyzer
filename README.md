# NonIQuerableAnalyzer
Prevents IQueryable from using parent class extension methods

NuGet: https://www.nuget.org/packages/NonIQuerableAnalyzer

# How to use it

## Declare a Attribute

- The analyzer checks the attribute only by name - any namespace,

```
using System;

[AttributeUsage(AttributeTargets.Struct)]
internal class NonCopyableAttribute : Attribute { }
```

## case1 : apply a Method

```

public static class EnumerableExtensions
{
    [NonIQueryableAttribute]
    public static IEnumerable<TSource> OrderByExpression<TSource>(this IEnumerable<TSource> source, int a)
    {
        // balabala.....
        return source;
    }
}
```

## case2 :apply all Method

```

[NonIQueryableAttribute]
public static class EnumerableExtensions
{
    public static IEnumerable<TSource> OrderByExpression<TSource>(this IEnumerable<TSource> source, int a)
    {
        return source;
    }
}
```



## misuse

```
class Program
{
    static void Main()
    {
        IQueryable<int> queryableNumbers = null;
        var queryableResult = queryableNumbers.OrderByExpression(1);  // error
    }
}
```

