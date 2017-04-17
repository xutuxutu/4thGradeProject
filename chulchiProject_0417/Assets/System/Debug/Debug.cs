using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// 디버그 상황
public enum DebugType
{
    ETC                 = 1 << 0,
    PLAYER_ACTION_STATE = 1 << 1,
    OBJECT_STATE        = 1 << 2,
    
}

public class Debug {

    static DebugType printDebugTypes;
    public Debug()
    {
        DebugTypeSatting(DebugType.ETC, DebugType.PLAYER_ACTION_STATE, DebugType.OBJECT_STATE);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(DebugType type, object message)
    {
        
        if ((printDebugTypes & type) == type)
            UnityEngine.Debug.Log(message);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(object message)
    { 
        if ((printDebugTypes & DebugType.ETC) == DebugType.ETC)
            UnityEngine.Debug.Log(message);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawLine(Vector3 start, Vector3 end)
    {
        UnityEngine.Debug.DrawLine(start, end);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        UnityEngine.Debug.DrawLine(start, end, color);
    }

    private void DebugTypeSatting(params DebugType[] debugTypes)
    {
        foreach (DebugType type in debugTypes)
            printDebugTypes |= type;
    }
}
