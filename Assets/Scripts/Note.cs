using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Note
{
    public float Time { get; set; }
    private int StartLane { get; set; }
    private int EndLane { get; set; }
    private char Kind { get; set; }

    public Note(float time, int startLane, int endLane, char kind)
    {
        this.Time = time;
        this.StartLane = startLane;
        this.EndLane = endLane;
        this.Kind = kind;
    }

    public float GetTime()
    {
        return Time;
    }
    
    public int GetStartLane()
    {
        return StartLane;
    }
    
    public int GetEndLane()
    {
        return EndLane;
    }

    public char GetKind()
    {
        return Kind;
    }

    public void SetTime(float time)
    {
        this.Time = time;
    }

    public void SetLane(int start, int end)
    {
        this.StartLane = start;
        this.EndLane = end;
    }

    public void SetKind(char kind)
    {
        this.Kind = kind;
    }
}
