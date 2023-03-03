using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ExportJson
{
    private static NoteSaveData _notesData;
    private static Base _baseData;
    
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

        var notesE = notesDataA.OrderBy(x => x.Time);
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
            _notesData.item[i].time = notesDataA[i].GetTime();
            _notesData.item[i].startLane = notesDataA[i].GetStartLane();
            _notesData.item[i].endLane = notesDataA[i].GetEndLane();
            _notesData.item[i].kind = notesDataA[i].GetKind();
            _notesData.item[i].length = notesDataA[i].GetLength();
        }

        StreamWriter writer;

        string jsonStr = JsonUtility.ToJson(_notesData);

        writer = new StreamWriter(name, false);
        writer.Write(jsonStr);
        writer.Flush();
        writer.Close();
    }

    public static void ExportingBase(int bpm, float offset, string filename, string name)
    {
        // baseDataはListや配列で直接管理できないため、クラスとして使用する。
        _baseData = new Base();
        _baseData.filePath = filename;
        _baseData.bpm = bpm;
        _baseData.offset = offset;
        
        StreamWriter writer;

        string jsonStr = JsonUtility.ToJson(_baseData);

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
            note = new Note(n.time, n.startLane, n.endLane, n.kind, n.length);
            notesDataA.Add(note);
        }

        return notesDataA;
    }

    public static KeyValuePair<string, KeyValuePair<int, float>> ImportingBase(string name)
    {
        if (Path.GetExtension(name) != ".json")
            throw new Exception("ファイル形式が正しくありません");
        
        _baseData = new Base();

        // デシリアライズ
        string jsonStr = "";
        StreamReader reader;
        reader = new StreamReader(name);
        jsonStr = reader.ReadToEnd();
        reader.Close();

        _baseData = JsonUtility.FromJson<Base>(jsonStr);

        KeyValuePair<string, KeyValuePair<int, float>> baseDataA =
            new KeyValuePair<string, KeyValuePair<int, float>>(_baseData.filePath,
                new KeyValuePair<int, float>(_baseData.bpm, _baseData.offset));

        return baseDataA;
    }
}
