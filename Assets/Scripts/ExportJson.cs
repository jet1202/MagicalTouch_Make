using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public static class ExportJson
{
    private static NoteSaveData _notesData;
    private static SlideSave[] _slideData;
    
    private static BpmSave _bpmData;
    
    private static Field[] _fieldData;
    
    public static void ExportingSheet(GameObject notes, string name)
    {
        // データの整理, シリアライズしたいデータを順番に_notesDataに格納
        _notesData = new NoteSaveData();
        Dictionary<int, Note> notesDataA = new Dictionary<int, Note>();
        Dictionary<int, SlideMaintain[]> slideDataA = new Dictionary<int, SlideMaintain[]>();

        Note note;
        int num = 0;
        foreach (Transform n in notes.transform)
        {
            if (n.CompareTag("SlideMaintain")) continue;

            if (n.CompareTag("Slide"))
                note = n.GetComponent<SlideData>().note;
            else
                note = n.GetComponent<NotesData>().note;
            notesDataA.Add(num, note);

            if (note.GetKind() == 'S')
            {
                List<SlideMaintain> data = new List<SlideMaintain>(n.GetComponent<SlideData>().slideMaintain.Values);
                data = new List<SlideMaintain>(data.OrderBy(x => x.time));
                slideDataA.Add(num, data.ToArray());
            }

            num++;
        }

        var notesE = notesDataA.OrderBy(x => x.Value.GetTime());
        notesDataA = new Dictionary<int, Note>();
        foreach (var n in notesE)
        {
            notesDataA.Add(n.Key, n.Value);
        }

        // 順番に整理したノーツデータ[notesDataA]を[_notesData]に格納
        int noteNumber = notesDataA.Count;
        int slideNumber = slideDataA.Count;
        _notesData.item = new NoteSave[noteNumber];
        _notesData.slideItem = new SlideSave[slideNumber];
        num = 0;
        int s = 0;
        foreach (var di in notesDataA)
        {
            _notesData.item[num] = new NoteSave();
            _notesData.item[num].number = num;
            _notesData.item[num].time = di.Value.GetTime();
            _notesData.item[num].startLane = di.Value.GetStartLane();
            _notesData.item[num].endLane = di.Value.GetEndLane();
            _notesData.item[num].kind = di.Value.GetKind();
            _notesData.item[num].length = di.Value.GetLength();
            _notesData.item[num].field = di.Value.GetField();

            if (di.Value.GetKind() == 'S')
            {
                var sl = new SlideSave();
                sl.number = num;
                sl.item = slideDataA[di.Key];
                _notesData.slideItem[s] = sl;

                s++;
            }

            num++;
        }

        StreamWriter writer;

        string jsonStr = JsonUtility.ToJson(_notesData, true);

        writer = new StreamWriter(name, false);
        writer.Write(jsonStr);
        writer.Flush();
        writer.Close();
    }

    public static void ExportingBpm(string name, List<Bpm> bpmItems)
    {
        _bpmData = new BpmSave();

        // bpm
        List<BpmItem> bpmA = new List<BpmItem>();
        var bpmE = bpmItems.OrderBy(x => x.GetTime());
        BpmItem item;
        foreach (var b in bpmE)
        {
            item = new BpmItem();
            item.time = b.GetTime();
            item.bpm = b.GetBpm();
            bpmA.Add(item);
        }

        int bpmNumber = bpmA.Count;
        _bpmData.bpmItem = new BpmItem[bpmNumber];
        for (int i = 0; i < bpmNumber; i++)
        {
            _bpmData.bpmItem[i] = bpmA[i];
        }

        StreamWriter writer;

        string jsonStr = JsonUtility.ToJson(_bpmData, true);

        writer = new StreamWriter(name, false);
        writer.Write(jsonStr);
        writer.Flush();
        writer.Close();
    }

    public static void ExportingField(string name, int fields, List<List<Speed>> speeds, List<List<Angle>> angles)
    {
        _fieldData = new Field[fields];

        List<SpeedItem> s = new List<SpeedItem>();
        SpeedItem s_item;

        List<AngleWork> a = new List<AngleWork>();
        AngleWork a_item;
        
        for (int i = 0; i < fields; i++)
        {
            s = new List<SpeedItem>();
            a = new List<AngleWork>();
            foreach (var sp in speeds[i])
            {
                s_item = new SpeedItem();
                s_item.time = sp.GetTime();
                s_item.speed = sp.GetSpeed100();
                s_item.isVariation = sp.GetIsVariation();
                
                s.Add(s_item);
            }
            
            foreach (var ap in angles[i])
            {
                a_item = new AngleWork();
                a_item.time = ap.GetTime();
                a_item.angle = ap.GetDegree();
                a_item.variation = ap.GetVariation();
                
                a.Add(a_item);
            }

            _fieldData[i] = new Field();
            _fieldData[i].field = i;
            _fieldData[i].speedItem = s.ToArray();
            _fieldData[i].angleWork = a.ToArray();
            _fieldData[i].activeTime = Array.Empty<int>();
        }

        var data = new FieldSave();
        data.item = _fieldData;
        
        StreamWriter writer;

        string jsonStr = JsonUtility.ToJson(data, true);

        writer = new StreamWriter(name, false);
        writer.Write(jsonStr);
        writer.Flush();
        writer.Close();
    }

    public static Dictionary<int, Note> ImportingSheet(string name)
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
        _slideData = _notesData.slideItem;

        Dictionary<int, Note> notesDataA = new Dictionary<int, Note>();
        Note note;
        foreach (var n in _notesData.item)
        {
            note = new Note(n.time, n.startLane, n.endLane, n.kind, n.length, n.field);
            notesDataA.Add(n.number, note);
        }

        return notesDataA;
    }

    public static Dictionary<int, SlideMaintain[]> ImportingSlide()
    {
        var a = new Dictionary<int, SlideMaintain[]>();

        if (_slideData == null) return new Dictionary<int, SlideMaintain[]>();
        
        foreach (var s in _slideData)
        {
            a.Add(s.number, s.item);
        }
        
        return a;
    }

    public static BpmSave ImportingBpm(string name)
    {
        if (Path.GetExtension(name) != ".json")
            throw new Exception("ファイル形式が正しくありません");
        
        _bpmData = new BpmSave();

        // デシリアライズ
        string jsonStr = "";
        StreamReader reader;
        reader = new StreamReader(name);
        jsonStr = reader.ReadToEnd();
        reader.Close();

        _bpmData = JsonUtility.FromJson<BpmSave>(jsonStr);

        return _bpmData;
    }

    public static Field[] ImportingField(string name)
    {
        if (Path.GetExtension(name) != ".json")
            throw new Exception("ファイル形式が正しくありません");

        // デシリアライズ
        string jsonStr = "";
        StreamReader reader;
        reader = new StreamReader(name);
        jsonStr = reader.ReadToEnd();
        reader.Close();

        FieldSave _Data = JsonUtility.FromJson<FieldSave>(jsonStr);
        _fieldData = _Data.item;

        foreach (var f in _fieldData)
        {
            foreach (var a in f.angleWork)
            {
                int v = a.variation;
                if (Math.Abs(v) > 0 && Math.Abs(v) < 10)
                    v *= 10;
                a.variation = v;
            }
        }

        return _fieldData;
    }
}
