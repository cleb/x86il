using System;

namespace x86il
{
    public class BinaryHelper
    {
        public static UInt16 Read16Bit(byte[] array, int offset)
        {
            return (UInt16)(((UInt16)array[offset+1] << 8) + array[offset]);
        }
        public static void Write16Bit(byte[] array, int offset, UInt16 value)
        {
            array[offset] = (Byte)(value & 0xff);
            array[offset+1] = (Byte)((value >> 8) & 0xff);
        }
        public static UInt32 Read32Bit(byte[] array, int offset)
        {
            return (UInt32)(((UInt32)array[offset + 3] << 24) 
                + ((UInt32)array[offset + 2] << 16) 
                + ((UInt32)array[offset + 1] << 8) 
                + array[offset]);
        }
    }
}
