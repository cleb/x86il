﻿using System;
using System.Collections.Generic;

namespace x86il
{

    public class InterpretCpu : ICpu
    {
        public Registers registers;
        public FlagsRegister flagsRegister;
        public Stack stack;
        public Flags CpuFlags => flagsRegister.CpuFlags;
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
        }

        private UInt16 getEffectiveAddress(UInt16 modrm)
        {
            switch (modrm & 7)
            {
                case 0x0:
                    return (UInt16)(registers.Get(Reg16.bx) + registers.Get(Reg16.si));
                case 0x1:
                    return (UInt16)(registers.Get(Reg16.bx) + registers.Get(Reg16.di));
                case 0x2:
                    return (UInt16)(registers.Get(Reg16.bp) + registers.Get(Reg16.si));
                case 0x3:
                    return (UInt16)(registers.Get(Reg16.bp) + registers.Get(Reg16.di));
                case 0x4:
                    return registers.Get(Reg16.si);
                case 0x5:
                    return registers.Get(Reg16.di);
                case 0x6:
                    return registers.Get(Reg16.bp);
                case 0x7:
                    return registers.Get(Reg16.bx);
                default:
                    throw new NotImplementedException();
            }
        }

        private UInt16 getEffectiveAddressFromModRm(UInt16 modrm)
        {
            switch (modrm >> 6)
            {
                case 0x00:
                    if((modrm & 7) == 0x6)
                    {
                        var imm16 = BinaryHelper.Read16Bit(memory, ip + 2);
                        ip += 4;
                        return imm16;
                    } else
                    {
                        ip += 2;
                        return getEffectiveAddress(modrm);
                    }
                case 0x01:
                    byte displacement = memory[ip + 2];
                    ip += 3;
                    return (UInt16)(getEffectiveAddress(modrm) + displacement);
                case 0x02:
                    var disp16 = BinaryHelper.Read16Bit(memory, ip + 2); ;    
                    ip += 4;
                    return (UInt16)(getEffectiveAddress(modrm) + disp16);
                default:
                    throw new InvalidOperationException();
            }
        }

        private UInt16 GetUInt16FromMemory(int address)
        {
            return BinaryHelper.Read16Bit(memory, address);
        }

        private void ModRm(Func<UInt16, UInt16, UInt32> function,
            RegisterType r1Type = RegisterType.reg8,
            RegisterType r2Type = RegisterType.reg8,
            bool rmFirst = true)
        {
            Action<UInt16,UInt16,byte> rm8func = (UInt16 r1, UInt16 address, byte res8) 
                => registers.Set(r1, (byte)res8, r1Type);
            Action<UInt16, UInt16, byte> rm8funcReverse = (UInt16 r1, UInt16 address, byte res8) 
                => memory[address] = (byte)res8;
            Action<UInt16, UInt16, UInt16> rm16func = (UInt16 r1, UInt16 address, UInt16 res16) 
                => registers.Set(r1, (UInt16)res16, r1Type);
            Action<UInt16, UInt16, UInt16> rm16funcReverse = (UInt16 r1, UInt16 address, UInt16 res16) 
                => BinaryHelper.Write16Bit(memory, address, (UInt16)res16);
            Action<UInt16, UInt16,  UInt16> r16func = (UInt16 r1, UInt16 r2, UInt16 result) 
                => registers.Set(rmFirst ? r1 : r2, (UInt16)result, rmFirst ? r1Type : r2Type);

            ModRm(function, r1Type, r2Type,
                rmFirst ? rm8func : rm8funcReverse,
                rmFirst ? rm16func : rm16funcReverse,
                r16func);            
        }

        private void ModRm(Func<UInt16, UInt16, UInt32> function,
            RegisterType r1Type,
            RegisterType r2Type,
            Action<UInt16, UInt16, byte> Rm8ResultFunc,
            Action<UInt16, UInt16, UInt16> Rm16ResultFunc,
            Action<UInt16, UInt16, UInt16> R16ResultFunc)
        {
            Action<UInt16, UInt16> rm8Result = (UInt16 r1, UInt16 address) =>
            {
                ushort r1Value = registers.Get(r1, r1Type);
                byte memValue = memory[address];
                var res8 = function(r1Value, memValue);
                SetFlagsFromInputAndResult((Int32)res8, r1Value, memValue, 1);
                Rm8ResultFunc(r1, address, (byte)res8);
            };
            Action<UInt16, UInt16> rm16Result = (UInt16 r1, UInt16 address) =>
            {
                ushort r1Value = registers.Get(r1, r1Type);
                ushort memValue = GetUInt16FromMemory(address);
                var res16 = function(r1Value, memValue);
                SetFlagsFromInputAndResult((Int32)res16, r1Value, memValue, 2);
                Rm16ResultFunc(r1, address, (UInt16)res16);
            };
            Action<UInt16, UInt16, UInt32> R16Result = (UInt16 r1, UInt16 r2, UInt32 result) =>
            {
                var r1Value = registers.Get(r1, r1Type);
                var r2Value = registers.Get(r2, r2Type);
                SetFlagsFromInputAndResult((Int32)result, r1Value, r2Value, r1Type == RegisterType.reg8 ? 1 : 2);
                R16ResultFunc(r1, r2, (UInt16)result);
            };

            ModRm(function, r1Type, r2Type, r1Type == RegisterType.reg8 ? rm8Result : rm16Result, R16Result);
        }

        private void ModRm(Func<UInt16, UInt16, UInt32> function, 
            RegisterType r1Type, 
            RegisterType r2Type,
            Action<UInt16,UInt16> RmResult,
            Action<UInt16,UInt16,UInt32> RegResult) 
        {
            byte modrm = memory[ip + 1];
            var r1 = (UInt16)((modrm >> 3) & 7);
            switch (modrm >> 6)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    var address = getEffectiveAddressFromModRm(modrm);
                    RmResult(r1, address);
                    break;
                case 0x03:
                    var r2 = (UInt16)((modrm) & 7);
                    var result = function(registers.Get(r1, r1Type),
                        registers.Get(r2, r2Type));

                    RegResult(r1, r2, result);
                    ip += 2;
                    break;
            }
            
        }

        private void ModRmNoReturn(Func<UInt16, UInt16, UInt32> function,
            RegisterType r1Type = RegisterType.reg8,
            RegisterType r2Type = RegisterType.reg8)
        {
            ModRm(function,
                r1Type,
                r2Type,
                (UInt16 x, UInt16 y, byte z) => { },
                (UInt16 x, UInt16 y, UInt16 z) => { },
                (UInt16 x, UInt16 y, UInt16 z) => { });
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

        public void SetFlagsFromInputAndResult(Int32 result, UInt16 input1, UInt16 input2, int bytes = 1)
        {
            flagsRegister.CheckZero(result, input1, input2);
            flagsRegister.CheckCarry(result, input1, input2,bytes);
            flagsRegister.CheckOverflow(result, input1, input2, bytes);
            flagsRegister.CheckSign((UInt32)result, bytes);
            flagsRegister.CheckParity(result);
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
            ModRm((r1, r2) => (UInt16)(r1 + r2), RegisterType.reg8, RegisterType.reg8, rmFirst);
        }
        public void Add16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r1 + r2), RegisterType.reg16, RegisterType.reg16, rmFirst);
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
            ModRm((r1, r2) => (UInt16)(r1 | r2), RegisterType.reg8, RegisterType.reg8, rmFirst);
        }
        public void Or16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 | r2), RegisterType.reg16, RegisterType.reg16, rmFirst);
        }

        public void Add16Imm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) + GetUInt16FromMemory(ip + 1)));
            ip += 3;
        }
        public void Adc8ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 + r2 + (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg8, RegisterType.reg8, rmFirst);
        }
        public void Adc16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r1 + r2 + (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg16, RegisterType.reg16, rmFirst);
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
            ModRm((r1, r2) => (UInt16)(r2 - r1 - (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg8, RegisterType.reg8, rmFirst);
        }
        public void Sbb16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r2 - r1 - (flagsRegister.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg16, RegisterType.reg16, rmFirst);
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
            ModRm((r1, r2) => (UInt16)(r1 & r2), RegisterType.reg8, RegisterType.reg8, rmFirst);
        }
        public void And16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r1 & r2), RegisterType.reg16, RegisterType.reg16, rmFirst);
        }
        public void Sub8ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt16)(r2 - r1), RegisterType.reg8, RegisterType.reg8, rmFirst);
        }
        public void Sub16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r2 - r1), RegisterType.reg16, RegisterType.reg16, rmFirst);
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
            ModRm((x, y) => (UInt16)(x ^ y), RegisterType.reg8, RegisterType.reg8, rmFirst);
        }

        public void XorRm16(bool rmFirst = false)
        {
            ModRm((x, y) => (UInt32)(x ^ y), RegisterType.reg16, RegisterType.reg16,rmFirst);
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
            ModRmNoReturn((r1, r2) => (UInt16)(r2 - r1), RegisterType.reg8, RegisterType.reg8);
        }
        public void Cmp16ModRm(bool rmFirst = false)
        {
            ModRmNoReturn((r1, r2) => (UInt16)(r2 - r1), RegisterType.reg16, RegisterType.reg16);
        }
        public void Cmp8Imm8(Reg8 reg)
        {
            byte regValue = registers.Get(reg);
            byte immValue = memory[ip + 1];
            SetFlagsFromInputAndResult(regValue - immValue, regValue, immValue);
            ip += 2;
        }
        public void Cmp16Imm16(Reg16 reg)
        {
            ushort regValue = registers.Get(reg);
            ushort immValue = BinaryHelper.Read16Bit(memory, ip + 1);
            SetFlagsFromInputAndResult(regValue - immValue, regValue, immValue);
            ip += 3;
        }
        public void Inc8()
        {
            ModRm((r1, r2) => (UInt32)(r2 + 1), RegisterType.reg8, RegisterType.reg8,false);
        }
        public void Inc16(Reg16 reg)
        {
            var value = registers.Get(reg);
            value++;
            SetFlagsFromInputAndResult(value, registers.Get(reg),0,2);
            registers.Set(reg, value);
            ip++;
        }
        public void Dec16(Reg16 reg)
        {
            var value = registers.Get(reg);
            value--;
            SetFlagsFromInputAndResult(value, registers.Get(reg),0,2);
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
                    case 0xcd:
                        Interrupt();
                        break;
                    case 0xfe:
                        Inc8();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
