using System;

namespace x86il
{
    public abstract class ModRmExecutor
    {
        protected ModRMDecoder decoder;
        protected FlagsRegister flagsRegister;
        protected byte[] memory;
        protected Registers registers;


        public ModRmExecutor(Registers regs, FlagsRegister flags, byte[] mem, ModRMDecoder dec)
        {
            registers = regs;
            flagsRegister = flags;
            memory = mem;
            decoder = dec;
        }

        protected abstract RegisterType RegisterType { get; }
        protected abstract RegisterType ComplementRegisterType { get; }

        public void RmResult(Func<ushort, uint, uint> function, bool rmFirst, bool useResult, bool setFlags)
        {
            var r1Value = registers.Get(decoder.R1, RegisterType);
            var memValue = ReadMemory();
            var res = function(r1Value, memValue);
            if (setFlags)
                flagsRegister.SetFlagsFromInputAndResult((int) res, r1Value, memValue, RegisterType.ToNumBytes());
            if (useResult) RmFunc(res, rmFirst);
        }

        public void RmFunc(uint res, bool rmFirst)
        {
            if (rmFirst)
                registers.Set(decoder.R1, (ushort) res, RegisterType);
            else
                WriteMemoryResult((ushort) res);
        }

        public abstract void WriteMemoryResult(ushort res);

        public abstract uint ReadMemory();

        public void RegFunc(uint res, bool rmFirst)
        {
            registers.Set(rmFirst ? decoder.R1 : decoder.R2, (ushort) res, RegisterType);
        }

        public void RegResult(Func<ushort, uint, uint> function, bool rmFirst, bool useResult, bool setFlags)
        {
            var r1Value = registers.Get(decoder.R1, RegisterType);
            var r2Value = registers.Get(decoder.R2, ComplementRegisterType);
            var result = function(registers.Get(decoder.R1, RegisterType),
                registers.Get(decoder.R2, ComplementRegisterType));
            if (setFlags)
                flagsRegister.SetFlagsFromInputAndResult((int) result, r1Value, r2Value, RegisterType.ToNumBytes());
            if (useResult) RegFunc((ushort) result, rmFirst);
        }

        public void Execute(Func<ushort, uint, uint> function, bool rmFirst, bool useResult, bool setFlags)
        {
            if (decoder.Type == ModRMType.RM)
                RmResult(function, rmFirst, useResult, setFlags);
            else
                RegResult(function, rmFirst, useResult, setFlags);
        }
    }
}