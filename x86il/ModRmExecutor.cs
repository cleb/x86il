using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{
    public abstract class ModRmExecutor
    {
        protected Registers registers;
        protected FlagsRegister flagsRegister;
        protected byte[] memory;
        protected ModRMDecoder decoder;

        protected abstract RegisterType RegisterType { get; }
        protected abstract RegisterType ComplementRegisterType { get; }

        public void RmResult(Func<UInt16, UInt16, UInt32> function, bool rmFirst, bool useResult)
        {
            ushort r1Value = registers.Get(decoder.R1, RegisterType);
            var memValue = ReadMemory();
            var res = function(r1Value, memValue);
            flagsRegister.SetFlagsFromInputAndResult((Int32)res, r1Value, memValue, RegisterType.ToNumBytes());
            if (useResult)
            {
                RmFunc(res, rmFirst);
            }
        }

        public void RmFunc(uint res, bool rmFirst)
        {
            if (rmFirst)
            {
                registers.Set(decoder.R1, (UInt16)res, RegisterType);
            }
            else
            {
                WriteMemoryResult((UInt16)res);
            }
        }

        public abstract void WriteMemoryResult(UInt16 res);

        public abstract ushort ReadMemory();

        public void RegFunc(uint res, bool rmFirst)
        {
            registers.Set(rmFirst ? decoder.R1 : decoder.R2, (UInt16)res, RegisterType);
        }


        public ModRmExecutor(Registers regs, FlagsRegister flags, byte[] mem, ModRMDecoder dec)
        {
            registers = regs;
            flagsRegister = flags;
            memory = mem;
            decoder = dec;
        }

        public void RegResult(Func<UInt16, UInt16, UInt32> function, bool rmFirst, bool useResult)
        {
            var r1Value = registers.Get(decoder.R1, RegisterType);
            var r2Value = registers.Get(decoder.R2, ComplementRegisterType);
            var result = function(registers.Get(decoder.R1, RegisterType), registers.Get(decoder.R2, ComplementRegisterType));
            flagsRegister.SetFlagsFromInputAndResult((Int32)result, r1Value, r2Value, RegisterType.ToNumBytes());
            if (useResult)
            {
                RegFunc((UInt16)result, rmFirst);
            }
        }

        public void Execute(Func<UInt16, UInt16, UInt32> function, bool rmFirst, bool useResult)
        {
            if (decoder.Type == ModRMType.RM)
            {
                RmResult(function, rmFirst, useResult);
            }
            else
            {
                RegResult(function, rmFirst, useResult);
            }
        }
    }
}
