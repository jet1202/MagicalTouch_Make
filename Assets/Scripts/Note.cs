using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Note
{
    private int Time100 { get; set; }
    private int StartLane { get; set; }
    private int EndLane { get; set; }
    private char Kind { get; set; }
    private int Length100 { get; set; }

    public Note(int time, int startLane, int endLane, char kind, int length)
    {
        this.Time100 = time;
        this.StartLane = startLane;
        this.EndLane = endLane;
        this.Kind = kind;
        this.Length100 = length;
    }

    public int GetTime100()
    {
        return Time100;
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

    public int GetLength100()
    {
        return Length100;
    }

    public void SetTime100(int time)
    {
        this.Time100 = time;
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

    public void SetLength100(int length)
    {
        this.Length100 = length;
    }
}

[Serializable]
public class Bpms
{
    private int Time100 { get; set; }
    private int Bpm { get; set; }

    public Bpms(int time, int bpm)
    {
        this.Time100 = time;
        this.Bpm = bpm;
    }

    public int GetTime100()
    {
        return Time100;
    }

    public int GetBpm()
    {
        return Bpm;
    }

    public void SetTime100(int time100)
    {
        this.Time100 = time100;
    }

    public void SetBpm(int bpm)
    {
        this.Bpm = bpm;
    }
}

[Serializable]
public class NoteAddition
{
    public SpeedItem[] speedItem;
    public BpmItem[] bpmItem;
}

[Serializable]
public class SpeedItem
{
    public int time100;
    public int speed100;
    private bool isVariation;
}

[Serializable]
public class BpmItem
{
    public int time100;
    public int bpm;
}

[Serializable]
public class NoteSaveData
{
    public NoteSave[] item;
}

[Serializable]
public class NoteSave
{
    public int time100;
    public int startLane;
    public int endLane;
    public char kind;
    public int length100;
}
