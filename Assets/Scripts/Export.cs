using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Export
{
    static List<Note> _notesData;
    private static KeyValuePair<string, KeyValuePair<int, float>> _baseData;

    public static void ExportingSheet(GameObject notes, string name)
    {
        // データの整理, シリアライズしたいデータを順番に_notesDataに格納
        _notesData = new List<Note>();
        List<Note> notesDataA = new List<Note>();
        Note note;
        foreach (Transform n in notes.transform)
        {
            note = n.GetComponent<NotesData>().note;
            notesDataA.Add(note);
        }

        var notesE = notesDataA.OrderBy(x => x.Time);
        foreach (Note n in notesE)
        {
            _notesData.Add(n);
        }

        // シリアライズ
        var formatter = new BinaryFormatter();

        // byte[] data;
        // // streamでシリアライズしてdata変数にデータをコピー
        // using (var stream = new MemoryStream())
        // {
        //     // クラスをstreamタイプに変換
        //     formatter.Serialize(stream, _notesData);
        //     // 変数のサイズを割り当てする
        //     data = new byte[stream.Length];
        //     // streamのデータをbyteタイプに変換
        //     data = stream.GetBuffer();
        // }
        // // シリアライズしたデータをファイルに格納する
        // using (var stream = new FileStream(name, FileMode.Create, FileAccess.Write))
        // {
        //     stream.Write(data, 0, data.Length);
        // }
        
        FileStream fs = new FileStream(name, FileMode.Create);
        formatter.Serialize(fs, _notesData);
        fs.Close();
    }

    public static void ExportingBase(int bpm, float offset, string filename, string name)
    {
        _baseData = new KeyValuePair<string, KeyValuePair<int, float>>(filename, new KeyValuePair<int, float>(bpm, offset));
        
        // シリアライズ
        var formatter = new BinaryFormatter();
        FileStream fs = new FileStream(name, FileMode.Create);
        formatter.Serialize(fs, _baseData);
        fs.Close();
    }

    public static List<Note> ImportingSheet(string name)
    {
        // データを読み込む
        _notesData = new List<Note>();
        var formatter = new BinaryFormatter();
        
        // var info = new FileInfo(name);
        // // データを再割り当て
        // byte[] data = new byte[info.Length];
        //
        // // ファイルからデータを読み込む
        // using (var stream = new MemoryStream())
        // {
        //     // byteをstreamに読み込む
        //     stream.Write(data, 0, data.Length);
        //     // Stream seekを最初に移動する
        //     stream.Seek(0, SeekOrigin.Begin);
        //     // クラスをデシリアライズしてNoteクラスにキャストする
        //     _notesData = (List<Note>)(formatter.Deserialize(stream));
        // }
        
        FileStream fs = new FileStream(name, FileMode.Open);
        _notesData = (List<Note>)formatter.Deserialize(fs);
        fs.Close();

        return _notesData;
    }

    public static KeyValuePair<string, KeyValuePair<int, float>> ImportingBase(string name)
    {
        _baseData = new KeyValuePair<string, KeyValuePair<int, float>>();
        
        // デシリアライズ
        var formatter = new BinaryFormatter();
        FileStream fs = new FileStream(name, FileMode.Open);
        _baseData = (KeyValuePair<string, KeyValuePair<int, float>>)formatter.Deserialize(fs);
        fs.Close();

        return _baseData;
    }
}
