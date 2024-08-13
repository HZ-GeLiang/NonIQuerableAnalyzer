namespace NonIQuerableAnalyzerTest
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal class NonIQueryableAttribute : Attribute { }
}