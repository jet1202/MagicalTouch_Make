using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Note
{
    private int Time { get; set; }
    private int StartLane { get; set; }
    private int EndLane { get; set; }
    private char Kind { get; set; }
    private int Length { get; set; }
    private int Field { get; set; }

    public Note(int time, int startLane, int endLane, char kind, int length, int field)
    {
        this.Time = time;
        this.StartLane = startLane;
        this.EndLane = endLane;
        this.Kind = kind;
        this.Length = length;
        this.Field = field;
    }

    public int GetTime()
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

    public int GetLength()
    {
        return Length;
    }

    public int GetField()
    {
        return Field;
    }

    public void SetTime(int time)
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

    public void SetLength(int length)
    {
        this.Length = length;
    }

    public void SetField(int field)
    {
        this.Field = field;
    }
}

[Serializable]
public class Bpm
{
    private int Time { get; set; }
    private int Num { get; set; }

    public Bpm(int time, int bpm)
    {
        this.Time = time;
        this.Num = bpm;
    }

    public int GetTime()
    {
        return Time;
    }

    public int GetBpm()
    {
        return Num;
    }

    public void SetTime(int time)
    {
        this.Time = time;
    }

    public void SetBpm(int bpm)
    {
        this.Num = bpm;
    }
}

[Serializable]
public class BpmSave // save
{
    public BpmItem[] bpmItem;
}

[Serializable]
public class BpmItem
{
    public int time;
    public int bpm;
}

[Serializable]
public class NoteSaveData // save
{
    public NoteSave[] item;
    public SlideSave[] slideItem;
}

[Serializable]
public class NoteSave
{
    public int number;
    public int time;
    public int startLane;
    public int endLane;
    public char kind;
    public int length;
    public int field;
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
    public int time;
    public int startLine;
    public int endLine;
    public bool isJudge;
    public bool isVariation;
}

[Serializable]
public class FieldSave
{
    public Field[] item;
}

[Serializable]
public class Field
{
    public int field;
    public SpeedItem[] speedItem;
    public AngleWork[] angleWork;
    public int[] activeTime;
}


[Serializable]
public class SpeedItem
{
    public int time;
    public int speed;
    private bool isVariation;
}

[Serializable]
public class AngleWork
{
    public int time;
    public int angle;
    public int variation;
}
