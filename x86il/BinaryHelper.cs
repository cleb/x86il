namespace x86il
{
    public class BinaryHelper
    {
        public static ushort Read16Bit(byte[] array, int offset)
        {
            return (ushort) ((array[offset + 1] << 8) + array[offset]);
        }

        public static void Write16Bit(byte[] array, int offset, ushort value)
        {
            array[offset] = (byte) (value & 0xff);
            array[offset + 1] = (byte) ((value >> 8) & 0xff);
        }

        public static uint Read32Bit(byte[] array, int offset)
        {
            return ((uint) array[offset + 3] << 24)
                   + ((uint) array[offset + 2] << 16)
                   + ((uint) array[offset + 1] << 8)
                   + array[offset];
        }
    }
}