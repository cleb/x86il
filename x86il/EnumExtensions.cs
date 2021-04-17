namespace x86il
{
    internal static class EnumExtensions
    {
        public static int ToNumBytes(this RegisterType type)
        {
            return type == RegisterType.reg8 ? 1 : 2;
        }
    }
}