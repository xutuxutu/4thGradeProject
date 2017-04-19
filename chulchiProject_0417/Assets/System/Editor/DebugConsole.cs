using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Runtime.InteropServices;
public class DebugConsole : EditorWindow
{

    [MenuItem("Window/My DebugConsole")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(DebugConsole));
    }

    Vector2 scrollPos;
    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        if(GUILayout.Button("클리어", EditorStyles.toolbarButton))
        {

        }
        if(GUILayout.Button(" 겹침 ", EditorStyles.toolbarButton))
        {

        }
        GUILayout.EndHorizontal();


        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        GUILayout.BeginHorizontal(); EditorGUILayout.HelpBox("This is my text ", MessageType.Info); GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(); EditorGUILayout.HelpBox("This is my text ", MessageType.Info); GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(); EditorGUILayout.HelpBox("This is my text ", MessageType.Info); GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(); EditorGUILayout.HelpBox("This is my text ", MessageType.Info); GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(); EditorGUILayout.HelpBox("This is my text ", MessageType.Info); GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        
    }
}
