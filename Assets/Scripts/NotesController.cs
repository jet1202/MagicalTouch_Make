using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class NotesController : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private Speeds speeds;
    [SerializeField] private Angles angles;
    [SerializeField] private Transparencies transparencies;
    
    [SerializeField] private GameObject measureLines;
    [SerializeField] private GameObject subLines;
    [SerializeField] private GameObject notes;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject measureLinePrefab;
    [SerializeField] private GameObject subLinePrefab;
    private float time;
    private int leng;

    public List<int> bpmLines = new List<int>();
        
    private void Start()
    {
        gameEvent
            .ObserveEveryValueChanged(x => x.focusBeat)
            .Subscribe(x => BeatLineSet(x, gameEvent.split));
    }

    void Update()
    {
        transform.position = new Vector3(gameEvent.time * -gameEvent.speed / 1000f, 0, 0);
    }

    public void MeasureLineSet(Dictionary<GameObject, Bpm> bpmData)
    {
        if (audioSource.clip == null) return;
        
        // 既存のラインを削除
        foreach (Transform g in measureLines.transform)
        {
            Destroy(g.gameObject);
        }

        List<Bpm> bpms = new List<Bpm>(new List<Bpm>(bpmData.Values).OrderBy(x => x.GetTime()));
        
        // ラインを生成
        time = audioSource.clip.length;
        leng = bpms.Count;
        Bpm bpm, nextBpm;
        bpmLines = new List<int>();
        for (int i = 0; i < leng; i++)
        {
            bpm = bpms[i];
            if (i == leng - 1)
                nextBpm = new Bpm((int)(time * 1000), 0);
            else
                nextBpm = bpms[i + 1];

            for (float j = bpm.GetTime() / 1000f; j < nextBpm.GetTime() / 1000f; j += 60f / (bpm.GetBpm() / 1000f) * 4)
            {
                int t = (int)Math.Round(j * 1000);
                bpmLines.Add(t);
            }
        }

        foreach (var t in bpmLines)
        {
            GameObject obj = Instantiate(measureLinePrefab, measureLines.transform);
            obj.transform.localPosition = new Vector3(t * gameEvent.speed / 1000f, 0.5f, 0);
            obj.SetActive(true);
        }

        // ノーツの位置調整
        foreach (Transform g in notes.gameObject.transform)
        {
            if (g.CompareTag("Slide"))
                g.GetComponent<SlideData>().Change();
            else if (g.CompareTag("SlideMaintain"))
                g.GetComponent<SlideMaintainData>().Change();
            else　if (g.CompareTag("Long"))
            {
                g.GetComponent<NotesData>().ChangeTimeBySpeed();
                g.GetComponent<NotesData>().ChangeLongInside();
            }
            else
                g.GetComponent<NotesData>().ChangeTimeBySpeed();
        }
        
        // Bpmの位置調整
        foreach (var g in bpmData)
        {
            g.Key.GetComponent<BpmData>().ChangeTime(g.Value.GetTime() / 1000f);
        }
        
        // Speed変更による位置調整
        speeds.RenewalSpeed();
        angles.RenewalAngle();
        transparencies.RenewalAlpha();
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
            if (beat / split >= 0 && beat / split < bpmLines.Count - 1)
            {
                int beatTime = 0;
                int measureTime = 0;
                
                beatTime = bpmLines[beat / split];
                measureTime = bpmLines[beat / split + 1] - beatTime;

                for (int i = 1; i < split; i++)
                {
                    subLines.transform.GetChild(i - 1).transform.localPosition =
                        new Vector3((beatTime + (float)measureTime / split * i) * gameEvent.speed / 1000f, 0.5f, 0f);
                }
            }
        }
    }
}
