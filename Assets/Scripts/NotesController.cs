using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class NotesController : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private GameObject measureLines;
    [SerializeField] private GameObject subLines;
    [SerializeField] private GameObject notes;
    [SerializeField] private AudioSource audio;
    [SerializeField] private GameObject measureLinePrefab;
    [SerializeField] private GameObject subLinePrefab;
    private float time;
    private int leng;

    public List<float> bpmMeasureLines = new List<float>();
        
    private void Start()
    {
        gameEvent
            .ObserveEveryValueChanged(x => x.focusBeat)
            .Subscribe(x => BeatLineSet(x, gameEvent.split));
    }

    void Update()
    {
        transform.position = new Vector3(gameEvent.time * -gameEvent.speed, 0, 0);
    }

    public void MeasureLineSet(Dictionary<GameObject, Bpms> bpmData)
    {
        if (audio.clip == null) return;
        
        // 既存のラインを削除
        foreach (Transform g in measureLines.transform)
        {
            Destroy(g.gameObject);
        }

        List<Bpms> bpms = new List<Bpms>(new List<Bpms>(bpmData.Values).OrderBy(x => x.GetTime100()));
        
        // ラインを生成
        time = audio.clip.length;
        leng = bpms.Count;
        Bpms bpm, nextBpm;
        bpmMeasureLines = new List<float>();
        for (int i = 0; i < leng; i++)
        {
            bpm = bpms[i];
            if (i == leng - 1)
                nextBpm = new Bpms((int)(time * 100), 0);
            else
                nextBpm = bpms[i + 1];

            for (float j = bpm.GetTime100() / 100f; j < nextBpm.GetTime100() / 100f; j += 60f / bpm.GetBpm() * 4)
            {
                bpmMeasureLines.Add(j);
            }
        }

        foreach (var t in bpmMeasureLines)
        {
            GameObject obj = Instantiate(measureLinePrefab, measureLines.transform);
            obj.transform.localPosition = new Vector3(t * gameEvent.speed, 0.5f, 0);
            obj.SetActive(true);
        }

        // ノーツの位置調整
        foreach (Transform g in notes.gameObject.transform)
        {
            g.GetComponent<NotesData>().ChangeTimeBySpeed();
        }
        
        // Bpmの位置調整
        foreach (var g in bpmData)
        {
            g.Key.GetComponent<BpmData>().ChangeTime(g.Value.GetTime100() / 100f);
        }
    }

    public void BeatLineSet(int beat, int split)
    {
        if (gameEvent.isFileSet)
        {
            // サブラインの数の調整
            int subNum = subLines.transform.childCount;
            if (subNum > split - 1)
            {
                for (int i = 0; i < subNum - split + 1; i++)
                    Destroy(subLines.transform.GetChild(subNum - i - 1).gameObject);
            }
            else if (subLines.transform.childCount < split - 1)
            {
                for (int i = 0; i < split - subNum - 1; i++)
                    Instantiate(subLinePrefab, subLines.transform);
            }

            // サブラインの調整
            float beatTime = 0f;
            float measureTime = 0f;
            if (beat / split >= 0 && beat / split < bpmMeasureLines.Count - 1)
            {
                beatTime = bpmMeasureLines[beat / split];
                measureTime = bpmMeasureLines[beat / split + 1] - beatTime;
            }

            for (int i = 1; i < split; i++)
            {
                subLines.transform.GetChild(i - 1).transform.localPosition =
                    new Vector3((beatTime + measureTime / split * i) * gameEvent.speed, 0.5f, 0f);
            }
        }
    }
}
