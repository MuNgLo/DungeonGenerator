using Godot;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Text.Json;

namespace Munglo.SettingsModule;

public static class Settings
{
    /// <summary>
    /// When a config is reset or saved. Carries the updated version.
    /// </summary>
    static public event EventHandler<object> OnSettingsChange;
    static readonly string folder = "Settings";
    static private Dictionary<string, object> _settings = null;

    private static void RaiseOnSettingsChangedEvent<T>(T obj)
    {
        EventHandler<object> raiseEvent = OnSettingsChange;
        if (raiseEvent != null)
        {
            raiseEvent(typeof(Settings), obj);
        }
    }
    public static object GetPropertyValue(string key, string propertyName)
    {
        if (!_settings.ContainsKey(key))
        {
            GD.Print($"The Key ({key}) does not exist in the dictionary!");
        }

        PropertyInfo field = _settings[key].GetType().GetProperty(propertyName);
        return field.GetValue(_settings[key]);
    }
    public static object GetFieldValue(string key, string fieldName)
    {
        if (!_settings.ContainsKey(key))
        {
            GD.Print($"The Key ({key}) does not exist in the dictionary!");
        }

        FieldInfo field = _settings[key].GetType().GetField(fieldName);
        return field.GetValue(_settings[key]);
    }
    public static void SetFieldEnumValue(string key, string fieldName, int value, string subFodler)
    {
        FieldInfo field = _settings[key].GetType().GetField(fieldName);
        SetFieldValueAndSave(key, fieldName, value, subFodler);
    }
    public static void SetFieldKeyBindValue(string key, string fieldName, PlayerKeyBind value, string subFodler)
    {
        FieldInfo field = _settings[key].GetType().GetField(fieldName);
        SetFieldValueAndSave(key, fieldName, value, subFodler);
    }
    /// <summary>
    /// Sets and saves
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fieldName"></param>
    /// <param name="value"></param>
    /// <param name="subFodler"></param>
    /// <returns></returns>
    public static float SetFieldValue(string key, string fieldName, float value, string subFodler)
    {
        // Clamp FLOATS
        FieldInfo field = _settings[key].GetType().GetField(fieldName);
        value = Mathf.Clamp(value, field.GetCustomAttribute<RangeAttribute>().Min, field.GetCustomAttribute<RangeAttribute>().Max);
        SetFieldValueAndSave(key, fieldName, value, subFodler);
        return value;
    }
    public static int SetFieldValue(string key, string fieldName, int value, string subFodler)
    {
        // Clamp INTS
        FieldInfo field = _settings[key].GetType().GetField(fieldName);
        float min = field.GetCustomAttribute<RangeAttribute>().Min;
        float max = field.GetCustomAttribute<RangeAttribute>().Max;
        int value2 = Mathf.Clamp(value, (int)min, (int)max);
        SetFieldValueAndSave(key, fieldName, value2, subFodler);
        return value2;
    }
    public static char SetFieldValue(string key, string fieldName, char value, string subFodler)
    {
        // Clamp INTS
        FieldInfo field = _settings[key].GetType().GetField(fieldName);
        SetFieldValueAndSave(key, fieldName, value, subFodler);
        return value;
    }
    /// <summary>
    /// Sets the value and saves the config
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fieldName"></param>
    /// <param name="value"></param>
    /// <param name="subFodler"></param>
    public static void SetFieldValue(string key, string fieldName, object value, string subFodler)
    {
        SetFieldValueAndSave(key, fieldName, value, subFodler);
    }
    private static void SetFieldValueAndSave(string key, string fieldName, object value, string subFodler)
    {
        if (_settings[key].GetType().GetField(fieldName) is null)
        {
            SetPropertyValueAndSave(key, fieldName, value, subFodler);
            return;
        }
        _settings[key].GetType().GetField(fieldName).SetValue(_settings[key], value);
        //Debug.Log($"Looking for it! {key}->{fieldName} in {subFodler}");
        SaveSettings(_settings[key], subFodler);
    }
    private static void SetPropertyValueAndSave(string key, string propertyName, object value, string subFodler)
    {
        _settings[key].GetType().GetProperty(propertyName).SetValue(_settings[key], value);
        SaveSettings(_settings[key], subFodler);
    }
    private static string FilePath(string subFolder)
    {
        string filePath = folder + "/";
        if (subFolder.Length > 0)
        {
            filePath += subFolder + "/";
        }
        return filePath;
    }
    /// <summary>
    /// Before accessing anything through this method the Type trying to be accessed should already been accessed through the generic
    /// Getsettings<T>.
    /// If there is no hit in the cache, NULL will be returned.
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Object GetCachedSettings(string typeName)
    {
        if (_settings == null)
        {
            _settings = new Dictionary<string, object>();
        }
        if (_settings.ContainsKey(typeName))
        {
            return _settings[typeName];
        }
        return null;
    }
    /// <summary>
    /// Will try to get the config from file or create a new default file
    /// Then return content of the file after caching
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subFolder"></param>
    /// <returns></returns>
    public static T GetSettings<T>(string subFolder)
    {
        if (_settings == null)
        {
            _settings = new Dictionary<string, object>();
        }
        string filePath = FilePath(subFolder);
        VerifyFolder(filePath);
        filePath += typeof(T).Name + ".json";

        if (_settings.ContainsKey(typeof(T).Name))
        {
            if (!File.Exists(filePath))
            {
                GD.Print($"Memory config ({typeof(T).Name}) does not exist as a file!");
            }
            return (T)_settings[typeof(T).Name];
        }

        // Check for file. Create a new if needed
        if (!File.Exists(filePath))
        {
            ResetSettings<T>(typeof(T).Name, subFolder);
            //ResetSettingsALT(typeof(T), subFolder);
        }
        if (!_settings.ContainsKey(typeof(T).Name))
        {
            //Debug.Log($"Adding config file to memory ({typeof(T).Name})");
            //T asd = JsonUtility.FromJson<SkateboardConfig>(File.ReadAllText(filePath));

            //Variant asd = Json.ParseString(File.ReadAllText(filePath));

            T asd = JsonSerializer.Deserialize<T>(File.ReadAllText(filePath));

            _settings[typeof(T).Name] = asd;
        }
        return JsonSerializer.Deserialize<T>(File.ReadAllText(filePath));
    }

    private static void WriteConfigFile<T>(string filePath, T obj)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(filePath, JsonSerializer.Serialize(obj, options));
        RaiseOnSettingsChangedEvent<T>(obj);
    }

    /// <summary>
    /// tHIS IS USED TO CREATE A NEW FILE OR REPLACE OLD WITH A NEW THAT HAS ALL DEFAULT VALUES
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="settingsName"></param>
    /// <param name="subFolder"></param>
    /// <returns></returns>
    public static object ResetSettings<T>(string settingsName, string subFolder)
    {
        //if (!_settings.ContainsKey(settingsName))
        //{
        _settings[settingsName] = (T)System.Activator.CreateInstance(typeof(T));
        //}
        T obj = (T)_settings[settingsName];
        string filePath = FilePath(subFolder);
        VerifyFolder(filePath);
        filePath += settingsName + ".json";
        GD.Print($"WriteConfigFile<{typeof(T)}>({settingsName}) path {filePath}) resetOBJ({obj}) RESET!!");
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(filePath, JsonSerializer.Serialize(obj, options));
        RaiseOnSettingsChangedEvent<T>(obj);
        return _settings[settingsName];
    }

     public static void ResetField(string settingsName, string field, string subFolder)
    {
        object ogConfig = System.Activator.CreateInstance(Type.GetType(settingsName));
        FieldInfo fieldInfo = Type.GetType(settingsName).GetField(field);

        if (fieldInfo is null)
        {
            PropertyInfo pInfo = ogConfig.GetType().GetProperty(field);
            if (pInfo is null)
            {
                GD.PushError($"Settings::ResetField() fieldInfo[{field}] returned as NULL! Neither Field or Property found");
                return;
            }

            object pvalue = pInfo.GetValue(ogConfig);
            SetPropertyValueAndSave(settingsName, field, pvalue, subFolder);
            return;
        }

        object value = fieldInfo.GetValue(ogConfig);
        SetFieldValueAndSave(settingsName, field, value, subFolder);
    }

    public static void SaveSettings<T>(T config, string subFolder)
    {
        string filePath = FilePath(subFolder);
        VerifyFolder(filePath);
        filePath += config.GetType().Name + ".json";
        WriteConfigFile<T>(filePath, config);
    }

    private static void VerifyFolder(string path)
    {
        Directory.CreateDirectory(path);
    }
}// EOF CLASS
