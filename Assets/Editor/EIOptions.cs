using UnityEditor;
using UnityEngine;
using System.Collections;

public class EIOptions : EditorWindow
{
    [MenuItem("Window/Extended Inspector/Options")]
    static void Init()
    {
        GetWindow(typeof(EIOptions), false, "EI Options");
    }

    void OnGUI()
    {
        EISettings.DisplayMode = (EISettings.DisplayModes)EditorGUILayout.Popup("Display Mode", (int)EISettings.DisplayMode, new[] { "Drop Down", "Title Bars" }, EditorStyles.popup);
        EISettings.ShowField = (EISettings.ShowFields)EditorGUILayout.Popup("Show Fields", (int)EISettings.ShowField, new[] { "Switch", "Always", "Never" }, EditorStyles.popup);
        if(GUILayout.Button("Save"))
        {
            EditorPrefs.SetInt("DisplayMode", (int)EISettings.DisplayMode);
            EditorPrefs.SetInt("ShowField", (int)EISettings.ShowField);
        }
    }
}

public static class EISettings
{
    public enum DisplayModes
    {
        DropDown,
        TitleBars
    }

    public enum ShowFields
    {
        Switch,
        Always,
        Never
    }

    public static DisplayModes DisplayMode;
    public static ShowFields ShowField;

    static EISettings()
    {
        DisplayMode = (DisplayModes)EditorPrefs.GetInt("DisplayMode", 0);
        ShowField = (ShowFields)EditorPrefs.GetInt("ShowField", 0);
    }
}