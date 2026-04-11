using infrastructure.services.map;

public class BoolIntConverter
{
    public static void UnpackBoolsAndNumber(int packed, out BlockFaces faces)
    {
        // Извлечение числа (биты 6 и 7)
        int number = (packed >> 6) & 0b11;
        
        // Извлечение булевых значений
        faces = (BlockFaces)(packed & 0b0011_1111);
    }
}