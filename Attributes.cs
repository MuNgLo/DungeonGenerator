using System;

namespace Munglo.SettingsModule;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class NormalizedVolumeAttribute : Attribute
{
    public float Max;
    public float Min;

    public NormalizedVolumeAttribute(float max = 0.0f, float min = -30.0f)
    {
        Max = max; Min = min;
    }
}
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class IsCheckBox : Attribute{ }
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class IsKeyBind : Attribute{ }
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class GodotAction : Attribute{
    public string actionName;
    public GodotAction(string name){actionName = name;}
 }
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class RangeAttribute : Attribute
{
    public float Max;
    public float Min;
    /// <summary>
    /// Define the min and max value for the field
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public RangeAttribute(float min = 0.0f, float max = 1000.0f)
    {
        Max = max; Min = min;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SecretAttribute : Attribute
{
    public SecretAttribute()
    {
    }
}
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class Encrypt : Attribute
{
    public Encrypt()
    {
    }
}
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class Tooltip : Attribute
{
    public string Text;
    public Tooltip(string tip)
    {
        Text=tip;
    }
}
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class MenuLabel : Attribute
{
    public string Text;
    public MenuLabel(string text)
    {
        Text=text;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class EditableValue : Attribute
{
    public bool isEditable;
    public EditableValue(bool v)
    {
        isEditable=v;
    }
}