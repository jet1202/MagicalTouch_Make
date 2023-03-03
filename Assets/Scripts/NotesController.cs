using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEditor;

public class NotesController : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private GameObject measureLines;
    [SerializeField] private GameObject subLines;
    [SerializeField] private GameObject notes;
    [SerializeField] private AudioSource audio;
    [SerializeField] private GameObject measureLinePrefab;
    [SerializeField] private GameObject subLinePrefab;
    private float measureDistance;
    private float time;
    private float offset;
    private float length;

    private void Start()
    {
        gameEvent
            .ObserveEveryValueChanged(x => x.focusBeat)
            .Subscribe(x => BeatLineSet(x, gameEvent.split, (60f / gameEvent.bpm * gameEvent.timeSignature)));
    }

    void Update()
    {
        transform.position = new Vector3(gameEvent.time * -gameEvent.speed, 0, 0);
    }

    public void MeasureLineSet(float bpm, float speed)
    {
        if (audio.clip == null) return;
        
        measureDistance = 60 / bpm * speed * gameEvent.timeSignature;
        time = audio.clip.length;
        offset = gameEvent.offset * gameEvent.speed;
        // 既存のラインを削除
        foreach (Transform g in measureLines.transform)
        {
            Destroy(g.gameObject);
        }
        // ラインを生成
        length = time * gameEvent.speed;
        for (float i = offset; i < length; i += measureDistance)
        {
            // GameObject obj = Instantiate(MeasureLinePrefab, new Vector3(i, 0.5f, 0), Quaternion.identity, measureLines.transform);
            GameObject obj = Instantiate(measureLinePrefab, measureLines.transform);
            obj.transform.localPosition = new Vector3(i, 0.5f, 0);
            obj.SetActive(true);
        }
        
        // ノーツの位置調整
        foreach (Transform g in notes.gameObject.transform)
        {
            g.GetComponent<NotesData>().ChangeTimeBySpeed();
        }
    }

    public void BeatLineSet(int beat, int split, float measureTime)
    {
        if (gameEvent.isFileSet)
        {
            Debug.Log($":{beat}");

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
            float beatTime = measureTime * beat + gameEvent.offset;
            for (int i = 1; i < split; i++)
            {
                subLines.transform.GetChild(i - 1).transform.localPosition =
                    new Vector3((beatTime + measureTime / split * i) * gameEvent.speed, 0.5f, 0f);
            }
        }
    }
}
