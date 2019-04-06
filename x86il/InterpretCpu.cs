﻿using System;
using System.Collections.Generic;

namespace x86il
{

    public class InterpretCpu : ICpu
    {
        public Registers registers;
        public Flags flags;
        public Flags CpuFlags {
            get => flags;
        }
        int ip = 0;
        byte[] memory;
        Dictionary<Byte, Action> IntHandlers;

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
            byte modrm = memory[ip + 1];
            var r1 = (UInt16)((modrm >> 3) & 7);
            switch (modrm >> 6)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    var address = getEffectiveAddressFromModRm(modrm);
                    if (r1Type == RegisterType.reg8)
                    {
                        var res8 = function(registers.Get(r1, r1Type), memory[address]);
                        if (rmFirst)
                        {
                            registers.Set(r1,(byte)res8, r1Type);
                        } else
                        {
                            memory[address] = (byte)res8;
                        }
                    }
                    else
                    {
                        var res16 = function(registers.Get(r1, r1Type), GetUInt16FromMemory(address));
                        if (rmFirst)
                        {
                            registers.Set(r1, (UInt16) res16, r1Type);
                        }
                        else
                        {
                            var low = res16 & 0xff;
                            var high = res16 >> 8;
                            memory[address] = (byte)low;
                            memory[address + 1] = (byte)high;
                        }
                    }
                    break;
                case 0x03:
                    var r2 = (UInt16)((modrm) & 7);

                    var result = function(registers.Get(r1, r1Type),
                        registers.Get(r2, r2Type));

                    SetFlagsFromResult((Int32)result, r1Type == RegisterType.reg8 ? 1 : 2);

                    registers.Set(rmFirst ? r1 : r2, (UInt16)result,  rmFirst ? r1Type : r2Type);
                    ip += 2;
                    break;
            }
            
        }

        public InterpretCpu(byte[] mem)
        {
            memory = mem;
            registers = new Registers();
            IntHandlers = new Dictionary<byte, Action>();
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

        public void SetFlagsFromResult(Int32 result, int bits = 1)
        {
            if (result == 0)
            {
                flags |= Flags.Zero;
            }
            if (result >= (bits << 8) || result < 0)
            {
                flags |= Flags.Carry;
            }
        }
            

        public void Xor8()
        {
            ModRm((x, y) =>  (UInt16)(x ^ y));                                     
        }

        public void MovSegRM16()
        {
            ModRm((x, y) => y,RegisterType.segment);
        }

        public void PushValue(UInt16 Value)
        {
            var sp = registers.Get(Reg16.sp);
            BinaryHelper.Write16Bit(memory, (registers.Get(Segments.ss) << 4) + sp - 1, Value);
            registers.Set(Reg16.sp, (UInt16)(sp - 2));
        }

        public UInt16 PopValue16()
        {
            var sp = registers.Get(Reg16.sp);
            var ret = BinaryHelper.Read16Bit(memory, (registers.Get(Segments.ss) << 4) + sp + 1);
            registers.Set(Reg16.sp, (UInt16)(sp + 2));
            return ret;
        }

        public void Push(Segments seg)
        {
            var value = registers.Get(seg);
            PushValue(value);
            ip++;
        }

        public void Pop(Segments seg)
        {
            var segment = PopValue16();
            registers.Set(seg, segment);
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
            ModRm((r1, r2) => (UInt16)(r1 + r2 + (flags.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg8, RegisterType.reg8, rmFirst);
        }
        public void Adc16ModRm(bool rmFirst = false)
        {
            ModRm((r1, r2) => (UInt32)(r1 + r2 + (flags.HasFlag(Flags.Carry) ? 1 : 0)), RegisterType.reg16, RegisterType.reg16, rmFirst);
        }
        public void Adc8Imm8(Reg8 reg)
        {
            registers.Set(reg, (byte)(registers.Get(reg) + memory[ip + 1] + (flags.HasFlag(Flags.Carry) ? 1 : 0)));
            ip += 2;
        }
        public void Adc16Imm16(Reg16 reg)
        {
            registers.Set(reg, (UInt16)(registers.Get(reg) + BinaryHelper.Read16Bit(memory, ip + 1) + (flags.HasFlag(Flags.Carry) ? 1 : 0)));
            ip += 3;
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
                        Pop(Segments.ss);
                        break;
                    case 0x1f:
                        Pop(Segments.ds);
                        break;
                    case 0x30:
                        Xor8();
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
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
