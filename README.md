# NonIQuerableAnalyzer
Prevents IQueryable from using parent class extension methods

NuGet: https://www.nuget.org/packages/NonIQuerableAnalyzer

In EF Core 3.1, if the IQuerable object uses its own IEnumerable extension method, it will prompt untranslatable statements at runtime. To solve this situation, it is necessary to detect it in advance during the compilation phase

# How to use it

## Declare a Attribute

- The analyzer checks the attribute only by name - any namespace,

```
using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class NonIQueryableAttribute : Attribute { }
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



## Misuse

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

![image](https://github.com/HZ-GeLiang/NonIQuerableAnalyzer/assets/16562680/294bd349-8935-47af-98b5-2e7e8c46b744)


![image](https://github.com/user-attachments/assets/b5ba045c-a268-466a-813d-2c5e53301f4c)

