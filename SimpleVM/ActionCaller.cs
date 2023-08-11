using System;
using System.Runtime.InteropServices;

namespace SimpleVM;

public interface ICall
{
    void Call(ref Stack stack);
}

public class ActionCaller<T> : ICall
{
    private readonly Action<T> _action;

    public ActionCaller(Action<T> action)
    {
        _action = action;
    }

    public void Call(ref Stack stack)
    {
        var a = stack.SafePop<T>();
        _action(a);
    }
}

public class FunctionCaller<T, T1, TReturn> : ICall
{
    private readonly Func<T, T1, TReturn> _action;

    public FunctionCaller(Func<T, T1, TReturn> action)
    {
        _action = action;
    }

    public void Call(ref Stack stack)
    {
        var b = stack.SafePop<T1>();
        var a = stack.SafePop<T>();
        var ret = _action(a, b);
        stack.SafePush(ret);
    }
}