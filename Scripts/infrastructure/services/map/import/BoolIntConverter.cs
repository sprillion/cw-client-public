using System;

public class BoolIntConverter
{
    public static Tuple<bool, bool, bool, bool, bool, bool, int> UnpackBoolsAndNumber(int packed)
    {
        // Извлечение числа (биты 6 и 7)
        int number = (packed >> 6) & 0b11;
        
        // Извлечение булевых значений
        bool flag6 = (packed & (1 << 5)) != 0;
        bool flag5 = (packed & (1 << 4)) != 0;
        bool flag4 = (packed & (1 << 3)) != 0;
        bool flag3 = (packed & (1 << 2)) != 0;
        bool flag2 = (packed & (1 << 1)) != 0;
        bool flag1 = (packed & (1 << 0)) != 0;
        
        return Tuple.Create(flag1, flag2, flag3, flag4, flag5, flag6, number);
    }
}