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
    private static NoteAddition _additionData;
    
    private static SlideSave[] _slideData;
    
    public static void ExportingSheet(GameObject notes, string name, string subName)
    {
        // データの整理, シリアライズしたいデータを順番に_notesDataに格納
        _notesData = new NoteSaveData();
        Dictionary<int, Note> notesDataA = new Dictionary<int, Note>();
        Dictionary<int, SlideMaintain[]> slideDataA = new Dictionary<int, SlideMaintain[]>();
        List<int> subNumber = new List<int>();

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
                data = new List<SlideMaintain>(data.OrderBy(x => x.time100));
                slideDataA.Add(num, data.ToArray());
            }

            num++;
        }

        var notesE = notesDataA.OrderBy(x => x.Value.GetTime100());
        notesDataA = new Dictionary<int, Note>();
        foreach (var n in notesE)
        {
            notesDataA.Add(n.Key, n.Value);
        }

        // 順番に整理したノーツデータ[notesDataA]を[_notesData]に格納
        int NoteNumber = notesDataA.Count;
        int slideNumber = slideDataA.Count;
        _notesData.item = new NoteSave[NoteNumber];
        _notesData.slideItem = new SlideSave[slideNumber];
        num = 0;
        int s = 0;
        foreach (var di in notesDataA)
        {
            _notesData.item[num] = new NoteSave();
            _notesData.item[num].number = num;
            _notesData.item[num].time100 = di.Value.GetTime100();
            _notesData.item[num].startLane = di.Value.GetStartLane();
            _notesData.item[num].endLane = di.Value.GetEndLane();
            _notesData.item[num].kind = di.Value.GetKind();
            _notesData.item[num].length100 = di.Value.GetLength100();
            
            if (di.Value.GetSub() == 1)
                subNumber.Add(num);

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

        SubLaneSave sub = new SubLaneSave();
        sub.number = subNumber.ToArray();

        StreamWriter subWriter;
        string subStr = JsonUtility.ToJson(sub, true);
        subWriter = new StreamWriter(subName, false);
        subWriter.Write(subStr);
        subWriter.Flush();
        subWriter.Close();
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

    public static Dictionary<int, Note> ImportingSheet(string name, string subName)
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

        int[] subNumber = Array.Empty<int>();
        if (subName != "")
        {
            if (Path.GetExtension(subName) != ".json")
                throw new Exception("ファイル形式が正しくありません");

            string subStr = "";
            StreamReader subReader;
            subReader = new StreamReader(subName);
            subStr = subReader.ReadToEnd();
            subReader.Close();

            var data = JsonUtility.FromJson<SubLaneSave>(subStr);
            subNumber = data.number;
        }

        _notesData = JsonUtility.FromJson<NoteSaveData>(jsonStr);
        _slideData = _notesData.slideItem;

        Dictionary<int, Note> notesDataA = new Dictionary<int, Note>();
        Note note;
        foreach (var n in _notesData.item)
        {
            var nu = subNumber.Contains(n.number) ? 1 : 0;
            note = new Note(n.time100, n.startLane, n.endLane, n.kind, n.length100, nu);
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
