using System;

namespace tools
{
    public static class EnumExtensions
    {
        public static byte ToByte<TEnum>(this TEnum e) where TEnum : Enum
        {
            return (byte)(object)e;
        }
    }
}