using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer {
    public float StartTime;
    public float Duration;
    bool Idle;
    public Timer(float d, bool i = true)
    {
        Idle = i;
        Duration = d;
        StartTime = Time.time;
    }
    public bool isReady()
    {
        return Idle || Time.time > StartTime + Duration;
    }
    //Start timer or reset Start Time
    public void Reset()
    {
        StartTime = Time.time;
        Idle = false;
    }
    public bool ResetIfReady()
    {
        if (Idle || isReady())
        {
            Reset();
            Idle = false;
            return true;
        } else
            return false;
    }
    public void ResetIfIdle()
    {
        if (Idle) Reset();
    }
    public void SetIdle()
    {
        Idle = true;
    }
}