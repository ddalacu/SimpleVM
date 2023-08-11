using System;

namespace SimpleVM;

public class VMRuntimeException : Exception
{
    public VMRuntimeException(string message) : base(message)
    {
    }
}