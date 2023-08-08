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
    
    public static void ExportingSheet(GameObject notes, string name, string subName)
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

    public static void ExportingField(string name, int fields, List<List<Speed>> speeds)
    {
        _fieldData = new Field[fields];

        List<SpeedItem> s;
        SpeedItem s_item;

        List<AngleWork> a;
        
        for (int i = 0; i < fields; i++)
        {
            s = new List<SpeedItem>();
            foreach (var sp in speeds[i])
            {
                s_item = new SpeedItem();
                s_item.time = sp.GetTime();
                s_item.speed = sp.GetSpeed100();
                s_item.isVariation = sp.GetIsVariation();
                
                s.Add(s_item);
            }

            _fieldData[i].speedItem = s.ToArray();
        }

        // _fieldData = new FieldSave()
        // {
        //     item = new Field[]
        //     {
        //         new Field()
        //         {
        //             field = 1,
        //             speedItem = new SpeedItem[]
        //             {
        //                 new SpeedItem()
        //                 {
        //                     time = 0,
        //                     speed = 100
        //                 }
        //             },
        //             angleWork = new AngleWork[]
        //             {
        //                 new AngleWork()
        //                 {
        //                     time = 0,
        //                     angle = 0,
        //                     variation = 0
        //                 }
        //             },
        //             activeTime = Array.Empty<int>()
        //         }
        //     }
        // };
        
        StreamWriter writer;

        string jsonStr = JsonUtility.ToJson(_fieldData, true);

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

        return _fieldData;
    }
}
