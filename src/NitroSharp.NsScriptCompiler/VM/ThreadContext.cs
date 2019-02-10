﻿using NitroSharp.NsScriptNew.Utilities;

namespace NitroSharp.NsScriptNew.VM
{
    public sealed class ThreadContext
    {
        internal ValueStack<CallFrame> CallFrameStack;
        internal ValueStack<ConstantValue> EvalStack;

        internal ThreadContext(string name, ref CallFrame frame)
        {
            Name = name;
            CallFrameStack = new ValueStack<CallFrame>(4);
            CallFrameStack.Push(ref frame);
            EvalStack = new ValueStack<ConstantValue>(8);
        }

        public string Name { get; }
        public bool DoneExecuting => CallFrameStack.Count == 0;
        internal long? SuspensionTime;
        internal long? SleepTimeout;

        public bool IsActive => SuspensionTime == null;

        internal ref CallFrame CurrentFrame => ref CallFrameStack.Peek();
    }

    internal struct CallFrame
    {
        public readonly NsxModule Module;
        public readonly ushort SubroutineIndex;
        public readonly ushort ArgStart;
        public readonly ushort ArgCount;

        public int ProgramCounter;

        public CallFrame(NsxModule module, ushort subroutineIndex, ushort argStart, ushort argCount)
        {
            Module = module;
            SubroutineIndex = subroutineIndex;
            ProgramCounter = 0;
            ArgStart = argStart;
            ArgCount = argCount;
        }
    }
}
