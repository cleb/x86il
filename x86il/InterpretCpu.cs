using System;
using System.Collections.Generic;

namespace x86il
{

    public class InterpretCpu : ICpu
    {
        public Registers registers;
        public FlagsRegister flagsRegister;
        public Stack stack;
        public Flags CpuFlags => flagsRegister.CpuFlags;
        private ModRMDecoder decoder;
        private ModRmExecutor executor8;
        private ModRmExecutor executor16;
        private ModRmExecutor executorSegment;
        private ModRmExecutor executorSegment32b;
        int ip = 0;
        byte[] memory;
        Dictionary<Byte, Action> IntHandlers;

        public InterpretCpu(byte[] mem)
        {
            memory = mem;
            registers = new Registers();
            IntHandlers = new Dictionary<byte, Action>();
            flagsRegister = new FlagsRegister();
            stack = new Stack(registers, memory);
            decoder = new ModRMDecoder(memory, registers);
            executor8 = new ModRmExecutor8(registers, flagsRegister, memory, decoder);
            executor16 = new ModRmExecutor16(registers, flagsRegister, memory, decoder);
            executorSegment = new ModRmExecutorSegment(registers, flagsRegister, memory, decoder);
            executorSegment32b = new ModRmExecutorSegment32b(registers, flagsRegister, memory, decoder);
        }

        private UInt16 GetUInt16FromMemory(int address)
        {
            return BinaryHelper.Read16Bit(memory, address);
        }

        private void ModRm(Func<UInt16, UInt32, UInt32> function,
            RegisterType r1Type = RegisterType.reg8,
            bool rmFirst = true,
            bool useResult = true)
        {

            var executor = GetExecutor(r1Type);
            decoder.Decode(ip);
            executor.Execute(function, rmFirst, useResult);
            ip += decoder.IpShift;
        }

        private ModRmExecutor GetExecutor(RegisterType r1Type)
        {
            switch (r1Type)
            {
                case RegisterType.reg8:
                    return executor8;
                case RegisterType.segment:
                    return executorSegment;
                case RegisterType.reg16:
                default:
                    return executor16;
            }
        }


        private void ModRmNoReturn(Func<UInt16, UInt32, UInt32> function,
            RegisterType r1Type = RegisterType.reg8)
        {
            ModRm(function,
                r1Type,
                true,
                false);
        }

        public Byte GetRegister(Reg8 register)
        {
            return registers.Get(register);
        }
        public UInt16 GetRegister(Reg16 register)
        {
            return registers.Get(register);
        }
        public UInt16 GetRegister(Segments register)
        {
            return registers.Get(register);
        }

        public void SetRegister(Reg8 register, Byte value)
        {
            registers.Set(register,value);
        }
        public void SetRegister(Reg16 register, UInt16 value)
        {
            registers.Set(register, value);
        }
        public void SetRegister(Segments register, UInt16 value)
        {
            registers.Set(register, value);
        }

        public void Mov8Imm(Reg8 register)
        {
            registers.Set(register, memory[ip+1]);
            ip += 2;
        }
        public void Mov16Imm(Reg16 register)
        {
            registers.Set(register, GetUInt16FromMemory(ip + 1));
            ip += 3;
        }

        public void Interrupt()
        {
            if (IntHandlers.ContainsKey(memory[ip + 1]))
            {
                IntHandlers[memory[ip + 1]]();
                ip += 2;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
            

        public void MovSegRM16()
        {
            ModRm((x, y) => y,RegisterType.segment);
        }

        public void Push(Segments seg)
        {
            stack.Push(seg);
            ip++;
        }

        public void Push(Reg16 reg)
        {
            stack.Push(reg);
            ip++;
        }

        public void Pop(Segments seg)
        {
            stack.Pop(seg);
            ip++;
        }

        public void Pop(Reg16 reg)
        {
            stack.Pop(reg);
            ip++;
        }

        public Byte GetInDs(UInt16 offset)
        {
            return memory[(registers.Get(Segments.ds) << 4) + offset];
        }
        
        public void SetInterruptHandler(Byte number, Action handler)
        {
            IntHandlers[number] = handler;
        }

        public void Add8ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 + r2), RegisterType.reg8, rmFirst);
        }
        public void Add16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r1 + r2), RegisterType.reg16, rmFirst);
        }
        public void AddImm8(Reg8 reg)
        {
            registers.Set(reg, (byte)(registers.Get(reg) + memory[ip + 1]));
            ip += 2;
        }
        public void OrImm8(Reg8 reg)
        {
            registers.Set(reg, (byte)(registers.Get(reg) | memory[ip + 1]));
            ip += 2;
        }
        public void OrImm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) | BinaryHelper.Read16Bit(memory, ip + 1)));
            ip += 2;
        }
        public void Or8ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 | r2), RegisterType.reg8, rmFirst);
        }
        public void Or16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 | r2), RegisterType.reg16, rmFirst);
        }

        public void Add16Imm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) + GetUInt16FromMemory(ip + 1)));
            ip += 3;
        }
        public void Adc8ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 + r2 + (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg8, rmFirst);
        }
        public void Adc16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r1 + r2 + (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg16, rmFirst);
        }
        public void Adc8Imm8(Reg8 reg)
        {
            registers.Set(reg, (byte)(registers.Get(reg) + memory[ip + 1] + (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)));
            ip += 2;
        }
        public void Adc16Imm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) + BinaryHelper.Read16Bit(memory, ip + 1) + (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)));
            ip += 3;
        }
        public void Sbb8ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r2 - r1 - (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg8, rmFirst);
        }
        public void Sbb16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r2 - r1 - (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg16, rmFirst);
        }
        public void Sbb8Imm8(Reg8 reg)
        {
            registers.Set(reg, (byte)(registers.Get(reg) - memory[ip + 1] - (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)));
            ip += 2;
        }
        public void Sbb16Imm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) - BinaryHelper.Read16Bit(memory, ip + 1) - (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)));
            ip += 3;
        }
        public void AndImm8(Reg8 reg)
        {
            registers.Set(reg, (byte)(registers.Get(reg) & memory[ip + 1]));
            ip += 2;
        }
        public void AndImm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) & BinaryHelper.Read16Bit(memory, ip + 1)));
            ip += 3;
        }
        public void And8ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 & r2), RegisterType.reg8, rmFirst);
        }
        public void And16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 & r2), RegisterType.reg16, rmFirst);
        }
        public void Sub8ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r2 - r1), RegisterType.reg8, rmFirst);
        }
        public void Sub16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r2 - r1), RegisterType.reg16, rmFirst);
        }
        public void Sub8Imm8(Reg8 reg)
        {
            registers.Set(reg, (byte)(registers.Get(reg) - memory[ip + 1]));
            ip += 2;
        }
        public void Sub16Imm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) - BinaryHelper.Read16Bit(memory, ip + 1)));
            ip += 3;
        }
        public void XorRm8(bool rmFirst = false)
        {
            ModRm((x, y) => (UInt16)(x ^ y), RegisterType.reg8, rmFirst);
        }

        public void XorRm16(bool rmFirst = false)
        {
            ModRm((x, y) => (UInt32)(x ^ y), RegisterType.reg16,rmFirst);
        }
        public void Xor8Imm8(Reg8 reg)
        {
            registers.Set(reg, (byte)(registers.Get(reg) ^ memory[ip + 1]));
            ip += 2;
        }
        public void Xor16Imm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) ^ BinaryHelper.Read16Bit(memory, ip + 1)));
            ip += 3;
        }
        public void Cmp8ModRm(bool rmFirst = false)
        {
            ModRmNoReturn((r1, r2) => (UInt16)(r2 - r1), RegisterType.reg8);
        }
        public void Cmp16ModRm(bool rmFirst = false)
        {
            ModRmNoReturn((r1, r2) => (UInt16)(r2 - r1), RegisterType.reg16);
        }
        public void Cmp8Imm8(Reg8 reg)
        {
            byte regValue = registers.Get(reg);
            byte immValue = memory[ip + 1];
            flagsRegister.SetFlagsFromInputAndResult(regValue - immValue, regValue, immValue);
            ip += 2;
        }
        public void Cmp16Imm16(Reg16 reg)
        {
            ushort regValue = registers.Get(reg);
            ushort immValue = BinaryHelper.Read16Bit(memory, ip + 1);
            flagsRegister.SetFlagsFromInputAndResult(regValue - immValue, regValue, immValue);
            ip += 3;
        }
        public void Inc8()
        {
            ModRm((r1, r2) => (UInt32)(r2 + 1), RegisterType.reg8, false);
        }
        public void Inc16(Reg16 reg)
        {
            var value = registers.Get(reg);
            value++;
            flagsRegister.SetFlagsFromInputAndResult(value, registers.Get(reg),0,2);
            registers.Set(reg, value);
            ip++;
        }
        public void Dec16(Reg16 reg)
        {
            var value = registers.Get(reg);
            value--;
            flagsRegister.SetFlagsFromInputAndResult(value, registers.Get(reg),0,2);
            registers.Set(reg, value);
            ip++;
        }
        public void Pusha()
        {
            var sp = registers.Get(Reg16.sp);
            stack.Push(Reg16.ax, Reg16.cx, Reg16.dx, Reg16.bx);
            stack.PushValue(sp);
            stack.Push(Reg16.bp, Reg16.si, Reg16.di);
            ip++;
        }

        public void Popa()
        {
            stack.Pop(Reg16.di, Reg16.si, Reg16.bp);
            var sp = stack.PopValue16();
            stack.Pop(Reg16.bx, Reg16.dx, Reg16.cx, Reg16.ax);
            registers.Set(Reg16.sp, sp);
            ip++;
        }

        public void JumpIf(bool condition)
        {
            if (condition)
            {
                ip += (char)memory[ip + 1] + 2;
                return;
            }
            ip += 2;
        }

        public void JumpIf(Flags flag, bool state)
        {
            JumpIf(FlagIsInState(flag, state));
        }

        private bool FlagIsInState(Flags flag, bool state)
        {
            return !(flagsRegister.HasFlag(flag) ^ state);
        }

        public void JumpIfAny(List<Tuple<Flags,bool>> conditions)
        {
            JumpIf(conditions.Exists(x => FlagIsInState(x.Item1,x.Item2)));
        }

        private void Handle0x80()
        {
            Handle0x8X(RegisterType.reg8, 1, "0x80");
        }

        private void Handle0x81()
        {
            Handle0x8X(RegisterType.reg16, 2, "0x81");
        }

        private void Handle0x83()
        {
            Handle0x8X(RegisterType.reg16, 1, "0x83");
        }

        private void RetnImm16()
        {
            var bytes = BinaryHelper.Read16Bit(memory, ip + 1);
            Retn();
            registers.Set(Reg16.sp, (UInt16)(registers.Get(Reg16.sp) + bytes));
        }

        private void Retn()
        {
            ip = stack.PopValue16();            
        }

        private void Les()
        {
            decoder.Decode(ip);
            executorSegment32b.Execute((dv, sv) =>
            {
                var es = sv >> 16;
                registers.Set(Segments.es, (UInt16) es);
                var di = sv & 0xffff;
                return di;
            }, true, true);
            ip += decoder.IpShift;
        }

        private int GetImm(int bytes)
        {
            if(bytes == 1)
            {
                return memory[ip + 2];
            }
            return BinaryHelper.Read16Bit(memory,ip + 2);
        }

        private void Handle0x8X(RegisterType registerType, int immBytes, string instruction)
        {
            var modrm = memory[ip + 1];
            int imm = GetImm(immBytes);
            var opcode = ModRMDecoder.GetRegSegmentFromModRm(modrm);
            switch (opcode)
            {
                case 0x0:
                    ModRm((r1, r2) => (uint)(r2 + imm),registerType, false);
                    break;
                case 0x1:
                    ModRm((r1, r2) => (uint)(r2 | imm), registerType, false);
                    break;
                case 0x2:
                    ModRm((r1, r2) => (uint)(r2 + imm + (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), registerType, false);
                    break;
                case 0x3:
                    ModRm((r1, r2) => (uint)(r2 - imm - (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), registerType, false);
                    break;
                case 0x4:
                    ModRm((r1, r2) => (uint)(r2 & imm), registerType, false);
                    break;
                case 0x5:
                    ModRm((r1, r2) => (uint)(r2 - imm), registerType, false);
                    break;
                case 0x6:
                    ModRm((r1, r2) => (uint)(r2 ^ imm), registerType, false);
                    break;
                case 0x7:
                    ModRmNoReturn((r1, r2) => (uint)(r2 - imm), registerType);
                    break;
                default:
                    throw new NotImplementedException($"{instruction} {opcode} not implemented");
            }
            ip += 1 + immBytes;
        }



        public void Execute(int ipStart, int ipEnd)
        {
            ip = ipStart;
            while(ip < ipEnd)
            {
                switch (memory[ip])
                {
                    case 0x00:
                        Add8ModRm();
                        break;
                    case 0x01:
                        Add16ModRm();
                        break;
                    case 0x02:
                        Add8ModRm(true);
                        break;
                    case 0x03:
                        Add16ModRm(true);
                        break;
                    case 0x04:
                        AddImm8(Reg8.al);
                        break;
                    case 0x05:
                        Add16Imm16(Reg16.ax);
                        break;
                    case 0x06:
                        Push(Segments.es);
                        break;
                    case 0x07:
                        Pop(Segments.es);
                        break;
                    case 0x08:
                        Or8ModRm();
                        break;
                    case 0x09:
                        Or16ModRm();
                        break;
                    case 0x0a:
                        Or8ModRm(true);
                        break;
                    case 0x0b:
                        Or16ModRm(true);
                        break;
                    case 0x0c:
                        OrImm8(Reg8.al);
                        break;
                    case 0x0d:
                        OrImm16(Reg16.ax);
                        break;
                    case 0x0e:
                        Push(Segments.cs);
                        break;
                    case 0x10:
                        Adc8ModRm();
                        break;
                    case 0x11:
                        Adc16ModRm();
                        break;
                    case 0x12:
                        Adc8ModRm(true);
                        break;
                    case 0x13:
                        Adc16ModRm(true);
                        break;
                    case 0x14:
                        Adc8Imm8(Reg8.al);
                        break;
                    case 0x15:
                        Adc16Imm16(Reg16.ax);
                        break;
                    case 0x16:
                        Push(Segments.ss);
                        break;
                    case 0x17:
                        Pop(Segments.ss);
                        break;
                    case 0x18:
                        Sbb8ModRm(false);
                        break;
                    case 0x19:
                        Sbb16ModRm(false);
                        break;
                    case 0x1A:
                        Sbb8ModRm(true);
                        break;
                    case 0x1B:
                        Sbb16ModRm(true);
                        break;
                    case 0x1c:
                        Sbb8Imm8(Reg8.al);
                        break;
                    case 0x1d:
                        Sbb16Imm16(Reg16.ax);
                        break;
                    case 0x1e:
                        Push(Segments.ds);
                        break;
                    case 0x1f:
                        Pop(Segments.ds);
                        break;
                    case 0x20:
                        And8ModRm();
                        break;
                    case 0x21:
                        And16ModRm();
                        break;
                    case 0x22:
                        And8ModRm(true);
                        break;
                    case 0x23:
                        And16ModRm(true);
                        break;
                    case 0x24:
                        AndImm8(Reg8.al);
                        break;
                    case 0x25:
                        AndImm16(Reg16.ax);
                        break;
                    case 0x28:
                        Sub8ModRm(false);
                        break;
                    case 0x29:
                        Sub16ModRm(false);
                        break;
                    case 0x2A:
                        Sub8ModRm(true);
                        break;
                    case 0x2B:
                        Sub16ModRm(true);
                        break;
                    case 0x2c:
                        Sub8Imm8(Reg8.al);
                        break;
                    case 0x2d:
                        Sub16Imm16(Reg16.ax);
                        break;
                    case 0x30:
                        XorRm8();
                        break;
                    case 0x31:
                        XorRm16();
                        break;
                    case 0x32:
                        XorRm8(false);
                        break;
                    case 0x33:
                        XorRm16(false);
                        break;
                    case 0x34:
                        Xor8Imm8(Reg8.al);
                        break;
                    case 0x35:
                        Xor16Imm16(Reg16.ax);
                        break;
                    case 0x38:
                        Cmp8ModRm();
                        break;
                    case 0x39:
                        Cmp16ModRm();
                        break;
                    case 0x3A:
                        Cmp8ModRm(true);
                        break;
                    case 0x3B:
                        Cmp16ModRm(true);
                        break;
                    case 0x3C:
                        Cmp8Imm8(Reg8.al);
                        break;
                    case 0x3D:
                        Cmp16Imm16(Reg16.ax);
                        break;
                    case 0x40:
                        Inc16(Reg16.ax);
                        break;
                    case 0x41:
                        Inc16(Reg16.cx);
                        break;
                    case 0x42:
                        Inc16(Reg16.dx);
                        break;
                    case 0x43:
                        Inc16(Reg16.bx);
                        break;
                    case 0x44:
                        Inc16(Reg16.sp);
                        break;
                    case 0x45:
                        Inc16(Reg16.bp);
                        break;
                    case 0x46:
                        Inc16(Reg16.si);
                        break;
                    case 0x47:
                        Inc16(Reg16.di);
                        break;
                    case 0x48:
                        Dec16(Reg16.ax);
                        break;
                    case 0x49:
                        Dec16(Reg16.cx);
                        break;
                    case 0x4A:
                        Dec16(Reg16.dx);
                        break;
                    case 0x4B:
                        Dec16(Reg16.bx);
                        break;
                    case 0x4C:
                        Dec16(Reg16.sp);
                        break;
                    case 0x4D:
                        Dec16(Reg16.bp);
                        break;
                    case 0x4E:
                        Dec16(Reg16.si);
                        break;
                    case 0x4F:
                        Dec16(Reg16.di);
                        break;
                    case 0x50:
                        Push(Reg16.ax);
                        break;
                    case 0x51:
                        Push(Reg16.cx);
                        break;
                    case 0x52:
                        Push(Reg16.dx);
                        break;
                    case 0x53:
                        Push(Reg16.bx);
                        break;
                    case 0x54:
                        Push(Reg16.sp);
                        break;
                    case 0x55:
                        Push(Reg16.bp);
                        break;
                    case 0x56:
                        Push(Reg16.si);
                        break;
                    case 0x57:
                        Push(Reg16.di);
                        break;
                    case 0x58:
                        Pop(Reg16.ax);
                        break;
                    case 0x59:
                        Pop(Reg16.cx);
                        break;
                    case 0x5A:
                        Pop(Reg16.dx);
                        break;
                    case 0x5B:
                        Pop(Reg16.bx);
                        break;
                    case 0x5C:
                        Pop(Reg16.sp);
                        break;
                    case 0x5D:
                        Pop(Reg16.bp);
                        break;
                    case 0x5E:
                        Pop(Reg16.si);
                        break;
                    case 0x5F:
                        Pop(Reg16.di);
                        break;
                    case 0x60:
                        Pusha();
                        break;
                    case 0x61:
                        Popa();
                        break;
                    case 0x70:
                        JumpIf(Flags.Overflow, true);
                        break;
                    case 0x71:
                        JumpIf(Flags.Overflow, false);
                        break;
                    case 0x72:
                        JumpIf(Flags.Carry, true);
                        break;
                    case 0x73:
                        JumpIf(Flags.Carry, false);
                        break;
                    case 0x74:
                        JumpIf(Flags.Zero, true);
                        break;
                    case 0x75:
                        JumpIf(Flags.Zero, false);
                        break;
                    case 0x76:
                        JumpIfAny(new List<Tuple<Flags, bool>> {
                            new Tuple<Flags,bool>(Flags.Zero, true),
                            new Tuple<Flags,bool>(Flags.Carry, true)
                        });
                        break;
                    case 0x77:
                        JumpIfAny(new List<Tuple<Flags,bool>> {
                            new Tuple<Flags,bool>(Flags.Zero, false),
                            new Tuple<Flags,bool>(Flags.Carry, false)
                        });
                        break;
                    case 0x78:
                        JumpIf(Flags.Sign, true);
                        break;
                    case 0x79:
                        JumpIf(Flags.Sign, false);
                        break;
                    case 0x7A:
                        JumpIf(Flags.Parity, true);
                        break;
                    case 0x7B:
                        JumpIf(Flags.Parity, false);
                        break;
                    case 0x7C:
                        JumpIf(Flags.Sign, !flagsRegister.HasFlag(Flags.Overflow));
                        break;
                    case 0x7D:
                        JumpIf(Flags.Sign, flagsRegister.HasFlag(Flags.Overflow));
                        break;
                    case 0x7E:
                        JumpIf(FlagIsInState(Flags.Zero, true) || flagsRegister.HasFlag(Flags.Sign) != flagsRegister.HasFlag(Flags.Overflow));
                        break;
                    case 0x7F:
                        JumpIf(FlagIsInState(Flags.Zero, false) && flagsRegister.HasFlag(Flags.Sign) == flagsRegister.HasFlag(Flags.Overflow));
                        break;
                    case 0x80:
                        Handle0x80();
                        break;
                    case 0x81:
                        Handle0x81();
                        break;
                    case 0x82:
                        Handle0x80();
                        break;
                    case 0x83:
                        Handle0x83();
                        break;
                    case 0x8e:
                        MovSegRM16();
                        break;
                    case 0xb0:
                        Mov8Imm(Reg8.al);
                        break;
                    case 0xb1:
                        Mov8Imm(Reg8.cl);
                        break;
                    case 0xb2:
                        Mov8Imm(Reg8.dl);
                        break;
                    case 0xb3:
                        Mov8Imm(Reg8.bl);
                        break;
                    case 0xb4:
                        Mov8Imm(Reg8.ah);
                        break;
                    case 0xb5:
                        Mov8Imm(Reg8.ch);
                        break;
                    case 0xb6:
                        Mov8Imm(Reg8.dh);
                        break;
                    case 0xb7:
                        Mov8Imm(Reg8.bh);
                        break;
                    case 0xb8:
                        Mov16Imm(Reg16.ax);
                        break;
                    case 0xb9:
                        Mov16Imm(Reg16.cx);
                        break;
                    case 0xba:
                        Mov16Imm(Reg16.dx);
                        break;
                    case 0xbb:
                        Mov16Imm(Reg16.bx);
                        break;
                    case 0xbc:
                        Mov16Imm(Reg16.sp);
                        break;
                    case 0xbd:
                        Mov16Imm(Reg16.bp);
                        break;
                    case 0xbe:
                        Mov16Imm(Reg16.si);
                        break;
                    case 0xbf:
                        Mov16Imm(Reg16.di);
                        break;
                    case 0xc2:
                        RetnImm16();
                        break;
                    case 0xc3:
                        Retn();
                        break;
                    case 0xc4:
                        Les();
                        break;
                    case 0xcd:
                        Interrupt();
                        break;
                    case 0xfe:
                        Inc8();
                        break;
                    default:
                        throw new NotImplementedException($"Instruction not implemented: {memory[ip]}");
                }
            }
        }
    }
}
