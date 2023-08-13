using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePreview : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private NotesDirector notesDirector;

    public List<List<Angle>> angles;
    private bool isPreview = false;
    private Transform linesParent;
    private float time;

    void Start()
    {
        this.gameObject.SetActive(false);
        linesParent = this.transform.GetChild(2);
    }

    void Update()
    {
        if (time == gameEvent.time || !isPreview) return;
        time = gameEvent.time;

        int leng = angles.Count;
        for (int i = 0; i < leng; i++)
        {
            float degree = -TimeToAngle(time, angles[i]);
            linesParent.GetChild(i).transform.eulerAngles = new Vector3(0f, 0f, degree);
        }
    }

    public void ChangeField(int fieldCount)
    {
        if (linesParent == null) linesParent = this.gameObject.transform.GetChild(2);
        
        foreach (Transform t in linesParent)
        {
            Destroy(t.gameObject);
        }

        for (int i = 0; i < fieldCount; i++)
        {
            GameObject obj = Instantiate(linePrefab, linesParent);

            obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = notesDirector.FieldColor(i);
        }
    }

    public void SetPreview(bool preview)
    {
        this.gameObject.SetActive(preview);
        isPreview = preview;
    }

    private float TimeToAngle(float time, List<Angle> angleWork)
    {
        int leng = angleWork.Count;
        int index = leng - 1;
        for (int i = 0; i < leng; i++)
        {
            if (angleWork[i].GetTime() > time * 1000)
            {
                index = i - 1;
                break;
            }
        }

        if (index == leng - 1)
        {
            int a = angleWork[index].GetDegree() % 360;
            return a;
        }
        else if (index == -1)
        {
            int a = angleWork[0].GetDegree() % 360;
            return a;
        }
        else
        {
            Angle before = angleWork[index];
            Angle after = angleWork[index + 1];
            
            float T = time - before.GetTime() / 1000f;
            float t1 = (after.GetTime() - before.GetTime()) / 1000f;
            float a1 = after.GetDegree() - before.GetDegree();
            int v = before.GetVariation();

            float a;
            if (v > 0)
                a = a1 * (float)Math.Pow(T / t1, v);
            else if (v < 0)
                a = a1 * (float)Math.Pow(T / t1, -1.0 / v);
            else
                a = 0;

            float angle = (before.GetDegree() + a) % 360;

            return angle;
        }
    }
}
