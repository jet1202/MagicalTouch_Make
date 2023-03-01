using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Note
{
    public float Time { get; set; }
    public int StartLane { get; set; }
    public int EndLane { get; set; }
    public char Kind { get; set; }
    public float Length { get; set; }

    public Note(float time, int startLane, int endLane, char kind, float length)
    {
        this.Time = time;
        this.StartLane = startLane;
        this.EndLane = endLane;
        this.Kind = kind;
        this.Length = length;
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

    public float GetLength()
    {
        return Length;
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

    public void SetLength(float length)
    {
        this.Length = length;
    }
}

[Serializable]
public class Base
{
    public string filePath;
    public int bpm;
    public float offset;
}

[Serializable]
public class NoteSaveData
{
    public NoteSave[] item;
}

[Serializable]
public class NoteSave
{
    public float time;
    public int startLane;
    public int endLane;
    public char kind;
    public float length;
}
