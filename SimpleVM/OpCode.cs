using System.Runtime.InteropServices;
using SimpleVM.Collections;

namespace SimpleVM
{
    public enum OpCode : byte
    {
        OP_CONSTANT,
        OP_ADD,
        OP_RETURN,
        OP_CALL,
        OP_POP,
        OP_SET_LOCAL,
        OP_GET_LOCAL,
        OP_EXTERNAL_CALL
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct OPData
    {
        [FieldOffset(0)]
        public OpCode Code;

        [FieldOffset(1)]
        public ConstantData ConstantData;
        
        [FieldOffset(1)]
        public AddData AddData;
        
        [FieldOffset(1)]
        public ExternalCallData ExternalCallData;
        
        [FieldOffset(1)]
        public CallData CallData;
        
        [FieldOffset(1)]
        public PopData PopData;
        
        [FieldOffset(1)]
        public SetLocalData SetLocalData;
        
        [FieldOffset(1)]
        public GetLocalData GetLocalData;
    }


    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct ConstantData
    {
        [FieldOffset(0)]
        public BuiltInType Type;

        [FieldOffset(1)]
        public ConstantPosition Position;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct AddData
    {
        [FieldOffset(0)]
        public BuiltInType Type;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct ExternalCallData
    {
        [FieldOffset(0)]
        private byte _unused;

        [FieldOffset(1)]
        public ConstantPosition Position;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct CallData
    {
        [FieldOffset(0)]
        private byte _unused;

        [FieldOffset(1)]
        public ConstantPosition Position;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct PopData
    {
        [FieldOffset(0)]
        public BuiltInType Type;
    } 
    
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct SetLocalData
    {
        [FieldOffset(0)]
        public BuiltInType Type;
        
        [FieldOffset(1)]
        public byte Offset;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct GetLocalData
    {
        [FieldOffset(0)]
        public BuiltInType Type;
        
        [FieldOffset(1)]
        public byte Offset;
    }
}