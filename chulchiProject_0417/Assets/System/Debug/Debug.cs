#region MyDebug DLL 코드
/*
using UnityEngine;
using System;


namespace MyDebug
{
    [Flags]
    public enum DummyDebugType
    {
        DUMMY_01 = 1 << 0,
        DUMMY_02 = 1 << 1,
        DUMMY_03 = 1 << 2,
        DUMMY_04 = 1 << 3,
        DUMMY_05 = 1 << 4,
        DUMMY_06 = 1 << 5,
        DUMMY_07 = 1 << 6,
        DUMMY_08 = 1 << 7,
        DUMMY_09 = 1 << 8,
        DUMMY_10 = 1 << 9,
        DUMMY_11 = 1 << 10,
        DUMMY_12 = 1 << 11,
    }


    public class CDebug
    {

        static DummyDebugType printDebugTypes;
        public CDebug(params object[] debugTypes)
        {
            foreach (DummyDebugType type in debugTypes)
                printDebugTypes |= type;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Log(object type, object message)
        {
            if ((printDebugTypes & (DummyDebugType)type) == (DummyDebugType)type)
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

    }
}
*/
#endregion

#region 이전 디버그 ( 콜 스택 때문에 호출 위치로 이동 못함 )
/*
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
*/
#endregion