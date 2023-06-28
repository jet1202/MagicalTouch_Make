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
    private int Lane { get; set; }

    public Note(int time, int startLane, int endLane, char kind, int length, int lane)
    {
        this.Time100 = time;
        this.StartLane = startLane;
        this.EndLane = endLane;
        this.Kind = kind;
        this.Length100 = length;
        this.Lane = lane;
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

    public int GetSub()
    {
        return Lane;
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

    public void SetSub(int lane)
    {
        this.Lane = lane;
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
    public SlideSave[] slideItem;
}

[Serializable]
public class NoteSave
{
    public int number;
    public int time100;
    public int startLane;
    public int endLane;
    public char kind;
    public int length100;
}

[Serializable]
public class SlideSave
{
    public int number;
    public SlideMaintain[] item;
}

[Serializable]
public class SlideMaintain
{
    public int time100;
    public int startLine;
    public int endLine;
    public bool isJudge;
    public bool isVariation;
}

[Serializable]
public class SubLaneSave
{
    public int[] number;
    public SpeedItem[] speedItem;
    public CameraWork[] cameraWork;
    public int[] activeTime100;
}

[Serializable]
public class CameraWork
{
    public int time100;
    public int angle;
}
