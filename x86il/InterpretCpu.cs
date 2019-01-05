using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private void ModRm(Func<UInt16, UInt16, UInt16> function, 
            RegisterType r1Type = RegisterType.reg8, 
            RegisterType r2Type = RegisterType.reg8) 
        {
            byte modrm = memory[ip + 1];
            switch (modrm >> 6)
            {
                case 0x00:
                    throw new NotImplementedException();
                    break;
                case 0x01:
                    throw new NotImplementedException();
                    break;
                case 0x02:
                    throw new NotImplementedException();
                    break;
                case 0x03:
                    var r1 = (UInt16)((modrm >> 3) & 7);
                    var r2 = (UInt16)((modrm) & 7);
                    registers.Set(r1, 
                        (byte)function(registers.Get(r1,r1Type), 
                        registers.Get(r2,r2Type)),r1Type);
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
            registers.Set(register, (UInt16)((memory[ip + 2] << 8) | memory[ip + 1]));
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

        public void Xor8()
        {
            ModRm((x, y) => {
                var result = (UInt16)(x ^ y);
                if(result == 0)
                {
                    flags |= Flags.Zero;
                }

                return result;
            });                                     
        }

        public void MovSegRM16()
        {
            ModRm((x, y) => y,RegisterType.segment);
        }

        public void PushValue(UInt16 Value)
        {
            var sp = registers.Get(Reg16.sp);
            memory[(registers.Get(Segments.ss) << 4) + sp] = (Byte)(Value & 0xff);
            memory[(registers.Get(Segments.ss) << 4) + sp + 1] = (Byte)(Value >> 8 & 0xff);
            registers.Set(Reg16.sp, (UInt16)(sp - 2));
        }

        public UInt16 PopValue16()
        {
            var sp = registers.Get(Reg16.sp);
            var ret = (UInt16)((UInt16)(memory[(registers.Get(Segments.ss) << 4) + sp]) << 8 +
                memory[(registers.Get(Segments.ss) << 4) + sp + 1]);
            registers.Set(Reg16.sp, (UInt16)(sp + 2));
            return ret;
        }

        public void Push(Segments seg)
        {
            var value = registers.Get(seg);
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

        public void Execute(int ipStart, int ipEnd)
        {
            ip = ipStart;
            while(ip < ipEnd)
            {
                switch (memory[ip])
                {
                    case 0x0e:
                        Push(Segments.cs);
                        break;
                    case 0x1f:
                        Push(Segments.ds);
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
