using System.Globalization;
using I2.Loc;
using UnityEngine;

public static class StringExtensions
{
    public const string ClanColor = "00AAFF";
        
    public static string SetClanColor(this string text)
    {
        return $"<color=#{ClanColor}>{text}</color>";
    }
    
    public static string SetColor(this string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
    }

    public static string SetSize(this string text, int size)
    {
        return $"<size={size}>{text}</size>";
    }
    
    public static string Bold(this string text)
    {
        return $"<b>{text}</b>";
    }
        
    public static string Loc(this string text)
    {
        return LocalizationManager.GetTranslation(text);
    }
    
    public static string WithDots(this int value)
    {
        return value.ToString("N0", CultureInfo.GetCultureInfo("de-DE"));
    }
}