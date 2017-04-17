using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TimeState
{
    public int hour;
    public int minute;
    public int second;
    public float millisecond;
}
public class CTimer
{
    public TimeState timeState;

    public void fStart()
    {
        timeState.hour = 0;
        timeState.minute = 0;
        timeState.millisecond = 0;
    }

    public void fUpdate ()
    {
        timeState.millisecond += Time.deltaTime;
        
        if(timeState.millisecond > 1f)
        {
            timeState.millisecond = 0;
            timeState.second += 1;
            if (timeState.second > 59)
            {
                timeState.second = 0;
                timeState.minute += 1;

                if (timeState.minute > 59)
                {
                    timeState.minute = 0;
                    timeState.hour += 1;
                }
            }
        }
        
        Debug.Log(DebugType.ETC, timeState.hour + "시간 " + timeState.minute + "분 " + timeState.second + "초");
	}

    
}
