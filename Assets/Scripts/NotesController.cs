using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class NotesController : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private GameObject measureLines;
    [SerializeField] private GameObject notes;
    [SerializeField] private AudioSource audio;
    [SerializeField] private GameObject MeasureLinePrefab;
    private float measureDistance;
    private float time;
    private float offset;
    private float length;

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
        foreach (Transform g in measureLines.gameObject.transform)
        {
            Destroy(g.gameObject);
        }
        // ラインを生成
        length = time * gameEvent.speed;
        for (float i = offset; i < length; i += measureDistance)
        {
            // GameObject obj = Instantiate(MeasureLinePrefab, new Vector3(i, 0.5f, 0), Quaternion.identity, measureLines.transform);
            GameObject obj = Instantiate(MeasureLinePrefab, measureLines.transform);
            obj.transform.localPosition = new Vector3(i, 0.5f, 0);
            obj.SetActive(true);
        }
        
        // ノーツの位置調整
        foreach (Transform g in notes.gameObject.transform)
        {
            g.GetComponent<NotesData>().DefaultSettings();
        }
    }
}
