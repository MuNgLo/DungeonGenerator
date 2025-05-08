using System;
using System.Reflection;
using Godot;
using Munglo.Commons;
namespace Munglo.SettingsModule;
/// <summary>
/// Drop this in to control child UI Nodes.
/// Set FieldTarget to the exact field name in the class definition you want to control.
/// Remember that the label for the setting's name need to be named "FieldName" for this script to pick it up
/// Supports: LineEdit, Slider, CheckBox, OptionButton
/// </summary>
[GlobalClass]
public partial class UISettingsEntry : Control
{
    [Export(hintString: "This should be the parent of all the nodes we look for")]
    private Control elementParent;
    [Export] private string settingsName;
    [Export] private string fieldTarget = "UNSET";
    #region  The references to first layer children by type. The fieldTarget Type determense which of them get used;
    private LineEdit lineEdit;
    private Slider slider;
    private CheckButton toggleButton;
    private CheckBox toggleBox;
    private OptionButton dropdown;
    private Label labelFieldName;
    private Label labelValue;
    private ColorPickerButton btnColourPicker;

    private TextureButton btnReset;
    private Button btnKeyBind;
    private Button btnKeyBindAlt;
    #endregion
    //private string fullSettingsName;
    /*
    [Export] private string _keyBindText;
    [Export] private string _keyBindAltText;
    [Export] private InputAction _rebindAction;
    */
    private Object Config => Settings.GetCachedSettings(settingsName);

    public override void _Ready()
    {
        // GrabChildren
        foreach (Control child in elementParent.GetChildren())
        {
            if (lineEdit is null && child is LineEdit) { lineEdit = child as LineEdit; }
            if (slider is null && child is Slider) { slider = child as Slider; }
            if (toggleButton is null && child is CheckButton) { toggleButton = child as CheckButton; }
            if (toggleBox is null && child is CheckBox) { toggleBox = child as CheckBox; }
            if (dropdown is null && child is OptionButton) { dropdown = child as OptionButton; }
            if (child is Label && child.Name == "FieldName") { labelFieldName = child as Label; }
            if (child is Label && child.Name == "Value") { labelValue = child as Label; }
            if (child is ColorPickerButton) { btnColourPicker = child as ColorPickerButton; }

            if (child is Button && child.Name == "KeyBind") { btnKeyBind = child as Button; }
            if (child is Button && child.Name == "KeyBindAlt") { btnKeyBindAlt = child as Button; }

            if (child is TextureButton) { btnReset = child as TextureButton; }
        }
        // Listen to changes in UI elements
        //if (slider is not null) { slider.ValueChanged += WhenSliderValueChanged; } // This might be bugged so disconnect this when changing slider
        if (slider is not null) { slider.DragEnded += WhenSliderDragEnded; }
        //if (lineEdit is not null) { lineEdit.TextChanged += WhenLineEditChanged; }
        if (lineEdit is not null) { lineEdit.TextSubmitted += WhenLineEditChanged; }
        if (lineEdit is not null) { lineEdit.FocusExited += () => { WhenLineEditChanged(lineEdit.Text); }; }
        if (toggleButton is not null) { toggleButton.Toggled += WhenToggleToggled; }
        if (toggleBox is not null) { toggleBox.Toggled += WhenToggleToggled; }
        if (btnColourPicker is not null) { btnColourPicker.PopupClosed += WhenBtnColPickPopUpClosed; }
        if (btnReset is not null) { btnReset.Pressed += WhenbtnResetPressed; }
        //dropdown.ItemSelected += WhenItemSelected;
        // Get Field, verify it is valid and set it up
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);

        if (fieldInfo is null)
        {
            PropertyInfo pInfo = Config.GetType().GetProperty(fieldTarget);
            if (pInfo is null)
            {
                GD.PushError($"UISettingsEntry::_Ready() fieldInfo[{fieldTarget}] returned as NULL! Neitehr Field or Property found");
                return;
            }
            SetupProperty(settingsName, pInfo);
            return;
        }
        UpdateAllElements(settingsName, fieldInfo, true);
    }



    private void WhenBtnColPickPopUpClosed()
    {
        Color c = btnColourPicker.Color;
        Settings.SetFieldValue(settingsName, fieldTarget, c, "");
    }


    private void UpdateAllElements(string settingsName, FieldInfo field, bool firstTime = false)
    {
        if (field.FieldType == typeof(float)) { FillFloat(settingsName, field); }
        if (field.FieldType == typeof(int)) { FillInt(settingsName, field); }
        if (field.FieldType == typeof(bool)) { FillBool(settingsName, field); }
        if (field.FieldType == typeof(string)) { FillString(settingsName, field); }
        if (field.FieldType == typeof(char)) { FillString(settingsName, field); }
        if (field.FieldType == typeof(PlayerKeyBind)) { FillKeyBind(settingsName, field, firstTime); }

        //if (field.FieldType == typeof(Color)) { FillColor(settingsName, field); }
        //if (field.FieldType.IsEnum) { FillEnum(_settingsName, field); }
    }
    internal void SetupProperty(string settingsName, PropertyInfo field)
    {

        if (field.PropertyType == typeof(Vector4)) { FillColor(settingsName, field); }
        if (field.PropertyType == typeof(Color)) { FillColor(settingsName, field); }

    }


    private void FillKeyBind(string settingsTypeName, FieldInfo field, bool firstTime = false)
    {
        PlayerKeyBind value = (PlayerKeyBind)Settings.GetFieldValue(settingsTypeName, field.Name);

        if (labelFieldName is not null)
        {
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            else
            {
                labelFieldName.Text = field.Name;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (field.GetCustomAttribute<IsKeyBind>() is not null)
        {
            UIKeybindPopup.OnKeyBindUpdated += WhenNewKeyBindIsMade;
            if (btnKeyBind is not null)
            {
                btnKeyBind.Text = value.Key == Key.None ? "M" + value.MouseButton.ToString() : value.Key.ToString();
                if (btnKeyBind.Text == "MNone") { btnKeyBind.Text = "-"; }
                if (firstTime) { btnKeyBind.Pressed += () => { UIKeybindPopup.StartKeyBind(settingsName, fieldTarget); }; }
            }
            if (btnKeyBindAlt is not null)
            {
                btnKeyBindAlt.Text = value.KeyAlt == Key.None ? "M" + value.MouseButtonAlt.ToString() : value.KeyAlt.ToString();
                if (btnKeyBindAlt.Text == "MNone") { btnKeyBindAlt.Text = "-"; }
                if (firstTime) { btnKeyBindAlt.Pressed += () => { UIKeybindPopup.StartKeyBind(settingsName, fieldTarget, true); }; }
            }
        }
        if (lineEdit is not null) { lineEdit.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
        if (slider is not null) { slider.Hide(); }
    }

    private void WhenNewKeyBindIsMade(object sender, string[] e)
    {
        if (e[0] == settingsName && e[1] == fieldTarget)
        {
            object Config = Settings.GetCachedSettings(settingsName);
            FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
            PlayerKeyBind value = (PlayerKeyBind)Settings.GetFieldValue(settingsName, fieldTarget);

            if (fieldInfo.GetCustomAttribute<IsKeyBind>() is not null)
            {
                if (btnKeyBind is not null)
                {
                    btnKeyBind.Text = value.Key == Key.None ? "M" + value.MouseButton.ToString() : value.Key.ToString();
                }
                if (btnKeyBindAlt is not null)
                {
                    btnKeyBindAlt.Text = value.KeyAlt == Key.None ? "M" + value.MouseButtonAlt.ToString() : value.KeyAlt.ToString();
                }
            }

        }
    }

    private void FillColor(string settingsTypeName, PropertyInfo field)
    {
        Object obj = Settings.GetPropertyValue(settingsTypeName, field.Name);
        Color value = Colors.White;
        if (obj is Vector4)
        {
            Vector4 valueTest = (Vector4)obj;
            value = new Color(valueTest.X, valueTest.Y, valueTest.Z, valueTest.W);
        }
        if (obj is Color)
        {
            value = (Color)obj;
        }
        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (btnColourPicker is not null)
        {
            btnColourPicker.Color = value;
            btnColourPicker.Show();
        }
        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (lineEdit is not null) { lineEdit.Hide(); }
        if (slider is not null) { slider.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
    }


    /// <summary>
    /// Works as of Nov 26 '24
    /// </summary>
    /// <param name="settingsTypeName"></param>
    /// <param name="field"></param>
    private void FillFloat(string settingsTypeName, FieldInfo field)
    {
        float value = (float)Settings.GetFieldValue(settingsTypeName, field.Name);
        if (slider is not null)
        {
            slider.MinValue = field.GetCustomAttribute<RangeAttribute>().Min;
            slider.MaxValue = field.GetCustomAttribute<RangeAttribute>().Max;
            slider.SetValueNoSignal(value);
        }

        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }

        if (field.GetCustomAttribute<EditableValue>() is null || !field.GetCustomAttribute<EditableValue>().isEditable)
        {
            if (labelValue is not null) { labelValue.Text = value.ToString("0.00"); }
            if (lineEdit is not null) { lineEdit.Hide(); }
        }
        else
        {
            if (lineEdit is not null)
            {
                lineEdit.Size = labelValue.Size;
                lineEdit.Position = labelValue.Position;
                lineEdit.Text = value.ToString("0.00");
            }
            if (labelValue is not null) { labelValue.Hide(); }
        }



        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
    }
    /// <summary>
    /// Works as of Nov 26 '24
    /// </summary>
    /// <param name="settingsTypeName"></param>
    /// <param name="field"></param>
    private void FillInt(string settingsTypeName, FieldInfo field)
    {
        int value = (int)Settings.GetFieldValue(settingsTypeName, field.Name);
        if (slider is not null)
        {
            slider.MaxValue = field.GetCustomAttribute<RangeAttribute>().Max;
            slider.MinValue = field.GetCustomAttribute<RangeAttribute>().Min;
            slider.SetValueNoSignal(value);
        }
        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }

        if (field.GetCustomAttribute<EditableValue>() is null || !field.GetCustomAttribute<EditableValue>().isEditable)
        {
            if (labelValue is not null) { labelValue.Text = value.ToString(); }
            if (lineEdit is not null) { lineEdit.Hide(); }
        }
        else
        {
            if (lineEdit is not null) { lineEdit.Text = value.ToString(); }
            if (labelValue is not null) { labelValue.Hide(); }
        }


        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
    }
    /// <summary>
    /// Works as of Nov 26 '24
    /// </summary>
    /// <param name="settingsTypeName"></param>
    /// <param name="field"></param>
    private void FillBool(string settingsTypeName, FieldInfo field)
    {
        bool value = (bool)Settings.GetFieldValue(settingsTypeName, field.Name);
        if (toggleBox is not null && field.GetCustomAttribute<IsCheckBox>() is not null)
        { toggleBox.SetPressedNoSignal(value); }
        else { toggleBox.Hide(); }
        if (toggleButton is not null && field.GetCustomAttribute<IsCheckBox>() is null)
        { toggleButton.SetPressedNoSignal(value); }
        else { toggleButton.Hide(); }

        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (slider is not null) { slider.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (lineEdit is not null) { lineEdit.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
    }
    private void FillString(string settingsTypeName, FieldInfo field)
    {
        string value = string.Empty;

        if (field.FieldType == typeof(char))
        {
            value = value + (char)Settings.GetFieldValue(settingsTypeName, field.Name);
        }
        else
        {
            value = (string)Settings.GetFieldValue(settingsTypeName, field.Name);
        }

        float width = Size.X * 0.5f;
        float height = Size.Y - 2;
        Vector2 size = new Vector2(width, height);

        if (slider is not null) { slider.Hide(); }
        if (lineEdit is not null)
        {
            lineEdit.Size = size;
            lineEdit.Position = Vector2.Right * width;
            if (field.GetCustomAttribute<Encrypt>() is not null)
            {
                lineEdit.Text = Cipher.Decrypt(value.ToString());
            }
            else
            {
                lineEdit.Text = value.ToString();
            }

            if (field.GetCustomAttribute<SecretAttribute>() is not null)
            {
                lineEdit.Secret = true;
            }
            if (field.GetCustomAttribute<Tooltip>() is not null) { lineEdit.TooltipText = field.GetCustomAttribute<Tooltip>().Text; }
        }
        if (labelFieldName is not null)
        {
            labelFieldName.Text = field.Name;
            if (field.GetCustomAttribute<MenuLabel>() is not null)
            {
                labelFieldName.Text = field.GetCustomAttribute<MenuLabel>().Text;
            }
            labelFieldName.Size = size - Vector2.Right * width * 0.2f;
            if (field.GetCustomAttribute<Tooltip>() is not null)
            {
                labelFieldName.TooltipText = field.GetCustomAttribute<Tooltip>().Text;
                if (labelFieldName.MouseFilter == MouseFilterEnum.Ignore) { labelFieldName.MouseFilter = MouseFilterEnum.Pass; }
            }
        }
        if (btnKeyBind is not null) { btnKeyBind.Hide(); }
        if (btnKeyBindAlt is not null) { btnKeyBindAlt.Hide(); }
        if (labelValue is not null) { labelValue.Hide(); }
        if (toggleBox is not null) { toggleBox.Hide(); }
        if (toggleButton is not null) { toggleButton.Hide(); }
        if (btnColourPicker is not null) { btnColourPicker.Hide(); }
    }
    private void FillEnum(string settingsTypeName, FieldInfo field)
    {
        Enum value = (Enum)Settings.GetFieldValue(settingsTypeName, field.Name);

        dropdown.Clear();
        //List<string> options = new List<string>();
        // Add current picked first
        //options.Add(value.ToString());
        foreach (int enumValue in Enum.GetValues(field.FieldType))
        {
            // Exclude current pick
            if (value.ToString() == Enum.GetName(field.FieldType, enumValue)) { continue; }
            dropdown.AddItem(Enum.GetName(field.FieldType, enumValue));
        }
    }




    #region Listeners
    private void WhenbtnResetPressed()
    {
        Settings.ResetField(settingsName, fieldTarget, "");
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        if (fieldInfo is null)
        {
            PropertyInfo pInfo = Config.GetType().GetProperty(fieldTarget);
            if (pInfo is null)
            {
                GD.PushError($"UISettingsEntry::WhenbtnResetPressed() fieldInfo[{fieldTarget}] returned as NULL! Neitehr Field or Property found");
                return;
            }
            SetupProperty(settingsName, pInfo);
            return;
        }
        UpdateAllElements(settingsName, fieldInfo);
    }
    private void WhenLineEditChanged(string newText)
    {
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        if (fieldInfo.FieldType == typeof(float))
        {
            if (float.TryParse(newText, out float value))
            {
                Settings.SetFieldValue(settingsName, fieldTarget, (float)value, "");
                FillFloat(settingsName, fieldInfo);
            }
        }
        if (fieldInfo.FieldType == typeof(int))
        {
            if (int.TryParse(newText, out int value))
            {
                Settings.SetFieldValue(settingsName, fieldTarget, (int)value, "");
                FillInt(settingsName, fieldInfo);
            }
        }
        if (fieldInfo.FieldType == typeof(string))
        {
            if (fieldInfo.GetCustomAttribute<Encrypt>() is not null)
            {
                Settings.SetFieldValue(settingsName, fieldTarget, Cipher.Encrypt(newText), "");
            }
            else
            {
                Settings.SetFieldValue(settingsName, fieldTarget, newText, "");
            }
            FillString(settingsName, fieldInfo);
        }
        if (fieldInfo.FieldType == typeof(char))
        {
            if (newText.Length > 0)
            {
                Settings.SetFieldValue(settingsName, fieldTarget, newText[0], "");
                FillString(settingsName, fieldInfo);
            }
        }
    }

    private void WhenSliderDragEnded(bool valueChanged)
    {
        if (valueChanged)
        {
            WhenSliderValueChanged(slider.Value);
        }
    }
    private void WhenSliderValueChanged(double value)
    {
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        if (fieldInfo.FieldType == typeof(float))
        {
            Settings.SetFieldValue(settingsName, fieldTarget, (float)value, "");
            FillFloat(settingsName, fieldInfo);
        }
        if (fieldInfo.FieldType == typeof(int))
        {
            Settings.SetFieldValue(settingsName, fieldTarget, (int)value, "");
            FillInt(settingsName, fieldInfo);
        }
    }
    private void WhenToggleToggled(bool toggleState)
    {
        Settings.SetFieldValue(settingsName, fieldTarget, toggleState, "");
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        if (fieldInfo.FieldType == typeof(bool)) { FillBool(settingsName, fieldInfo); }
    }
    #endregion
}// EOF CLASS