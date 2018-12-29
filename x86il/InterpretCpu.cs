﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86il
{

    public class InterpretCpu : ICpu
    {
        private Registers registers;
        private Flags flags;
        public Flags CpuFlags{
            get => flags;
        }
        int ip = 0;
        byte[] memory;

        private void ModRm(Func<UInt16, UInt16, UInt16> function)
        {
            byte modrm = memory[ip + 1];
            switch(modrm >> 6)
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
                    Reg8 r1 = (Reg8) ((modrm >> 3) & 7);
                    Reg8 r2 = (Reg8)((modrm) & 7);
                    registers.Set(r1, (byte) function(registers.Get(r1), registers.Get(r2)));
                    ip += 2;
                    break;
            }
        }

        public InterpretCpu(byte[] mem)
        {
            memory = mem;
            registers = new Registers();
        }

        public Byte GetRegister(Reg8 register)
        {
            return registers.Get(register);
        }
        public UInt16 GetRegister(Reg16 register)
        {
            return registers.Get(register);
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

        public void Execute(int ipStart, int ipEnd)
        {
            ip = ipStart;
            while(ip < ipEnd)
            {
                switch (memory[ip])
                {
                    case 0x30:
                        Xor8();
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
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
