using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace SimpleVMTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseGenericAttribute : TestCaseAttribute, ITestBuilder
{
    public Type[] Types { get; private set; }

    public TestCaseGenericAttribute(Type[] types, params object[] arguments)
        : base(arguments)
    {
        Types = types;
    }

    IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite)
    {
        if (!method.IsGenericMethodDefinition)
            return base.BuildFrom(method, suite);

        if (Types == null || Types.Length != method.GetGenericArguments().Length)
        {
            var parms = new TestCaseParameters
            {
                RunState = RunState.NotRunnable
            };
            parms.Properties.Set(PropertyNames.SkipReason,
                $"{nameof(Types)} should have {method.GetGenericArguments().Length} elements");

            return new[]
            {
                new NUnitTestCaseBuilder().BuildTestMethod(method, suite, parms)
            };
        }

        var genMethod = method.MakeGenericMethod(Types);
        return base.BuildFrom(genMethod, suite);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute<T> : TestCaseGenericAttribute
{
    public TestCaseAttribute(params object[] arguments) : base(new[] {typeof(T)}, arguments)
    {
        
    }
}