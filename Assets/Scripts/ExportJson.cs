using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class ExportJson
{
    private static NoteSaveData _notesData;
    private static NoteAddition _additionData;
    
    public static void ExportingSheet(GameObject notes, string name)
    {
        // データの整理, シリアライズしたいデータを順番に_notesDataに格納
        _notesData = new NoteSaveData();
        List<Note> notesDataA = new List<Note>();
        Note note;
        foreach (Transform n in notes.transform)
        {
            note = n.GetComponent<NotesData>().note;
            notesDataA.Add(note);
        }

        var notesE = notesDataA.OrderBy(x => x.GetTime100());
        notesDataA = new List<Note>();
        foreach (Note n in notesE)
        {
            notesDataA.Add(n);
        }

        // 順番に整理したノーツデータ[notesDataA]を[_notesData]に格納
        int NoteNumber = notesDataA.Count;
        _notesData.item = new NoteSave[NoteNumber];
        for (int i = 0; i < NoteNumber; i++)
        {
            _notesData.item[i] = new NoteSave();
            _notesData.item[i].number = i;
            _notesData.item[i].time100 = notesDataA[i].GetTime100();
            _notesData.item[i].startLane = notesDataA[i].GetStartLane();
            _notesData.item[i].endLane = notesDataA[i].GetEndLane();
            _notesData.item[i].kind = notesDataA[i].GetKind();
            _notesData.item[i].length100 = notesDataA[i].GetLength100();
        }

        StreamWriter writer;

        string jsonStr = JsonUtility.ToJson(_notesData, true);

        writer = new StreamWriter(name, false);
        writer.Write(jsonStr);
        writer.Flush();
        writer.Close();
    }

    public static void ExportingAddition(string name, List<SpeedItem> speedItems, List<Bpms> bpmItems)
    {
        // _additionDataはListや配列で直接管理できないため、クラスとして使用する。
        _additionData = new NoteAddition();
        
        // speed
        List<SpeedItem> speedA = new List<SpeedItem>();
        var speedE = speedItems.OrderBy(x => x.time100);
        foreach (var s in speedE)
        {
            speedA.Add(s);
        }

        int speedNumber = speedA.Count;
        _additionData.speedItem = new SpeedItem[speedNumber];
        for (int i = 0; i < speedNumber; i++)
        {
            _additionData.speedItem[i] = speedA[i];
        }

        // bpm
        List<BpmItem> bpmA = new List<BpmItem>();
        var bpmE = bpmItems.OrderBy(x => x.GetTime100());
        BpmItem item;
        foreach (var b in bpmE)
        {
            item = new BpmItem();
            item.time100 = b.GetTime100();
            item.bpm = b.GetBpm();
            bpmA.Add(item);
        }

        int bpmNumber = bpmA.Count;
        _additionData.bpmItem = new BpmItem[bpmNumber];
        for (int i = 0; i < bpmNumber; i++)
        {
            _additionData.bpmItem[i] = bpmA[i];
        }

        StreamWriter writer;

        string jsonStr = JsonUtility.ToJson(_additionData, true);

        writer = new StreamWriter(name, false);
        writer.Write(jsonStr);
        writer.Flush();
        writer.Close();
    }

    public static List<Note> ImportingSheet(string name)
    {
        if (Path.GetExtension(name) != ".json")
            throw new Exception("ファイル形式が正しくありません");
        
        // データを読み込む
        _notesData = new NoteSaveData();
        
        string jsonStr = "";
        StreamReader reader;
        reader = new StreamReader(name);
        jsonStr = reader.ReadToEnd();
        reader.Close();

        _notesData = JsonUtility.FromJson<NoteSaveData>(jsonStr);

        List<Note> notesDataA = new List<Note>();
        Note note;
        foreach (var n in _notesData.item)
        {
            note = new Note(n.time100, n.startLane, n.endLane, n.kind, n.length100);
            notesDataA.Add(note);
        }

        return notesDataA;
    }

    public static NoteAddition ImportingAddition(string name)
    {
        if (Path.GetExtension(name) != ".json")
            throw new Exception("ファイル形式が正しくありません");
        
        _additionData = new NoteAddition();

        // デシリアライズ
        string jsonStr = "";
        StreamReader reader;
        reader = new StreamReader(name);
        jsonStr = reader.ReadToEnd();
        reader.Close();

        _additionData = JsonUtility.FromJson<NoteAddition>(jsonStr);

        return _additionData;
    }
}
