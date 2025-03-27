using ui.tools;

namespace ui.tools
{
    public enum IconType
    {
        Gold,
        Diamond,
        Health,
        Exp,
        Level
    }
}

public static class IconExtensions
{
    public static string ToIcon(this IconType input)
    {
        return  $"<sprite name={input.ToString()}>";
    }
}