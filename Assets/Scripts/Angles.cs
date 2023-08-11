using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Angles : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private Speeds speedsDirector;
    
    [SerializeField] private GameObject anglePrefab;
    
    public List<List<Angle>> anglesData = new List<List<Angle>>();
    public Dictionary<GameObject, Angle> fieldAngles;

    private Transform anglesTransform;
    private Transform linesTransform;

    private void Start()
    {
        linesTransform = transform.GetChild(0);
        anglesTransform = transform.GetChild(1);
    }

    public void NewField()
    {
        anglesData.Add(new List<Angle>() { new Angle(0, 0, 0) });
    }

    public void DeleteField(int index)
    {
        anglesData.RemoveAt(index);
    }

    public void ChangeField()
    {
        anglesData[speedsDirector.nowField] = new List<Angle>(fieldAngles.Values);
        RenewalAngle();
    }

    // Angles
    public GameObject NewAngles(int time, int degree, int variation)
    {
        GameObject obj = Instantiate(anglePrefab, anglesTransform);
        fieldAngles.Add(obj, new Angle(time, degree, variation));
        obj.transform.localPosition = new Vector3(time / 1000f * gameEvent.speed, DegreeY(degree), 0f);
        obj.transform.GetChild(1).GetComponent<SpriteRenderer>().color = notesDirector.FieldColor(speedsDirector.nowField);
        obj.SetActive(true);
        
        RenewalAngleLine();

        return obj;
    }

    public void DeleteAngles(GameObject obj)
    {
        fieldAngles.Remove(obj);
        Destroy(obj);
        
        RenewalAngleLine();
    }

    public void RenewalAngle()
    {
        foreach (Transform g in anglesTransform.transform)
        {
            Destroy(g.gameObject);
        }
        
        fieldAngles = new Dictionary<GameObject, Angle>();

        foreach (var s in anglesData[speedsDirector.nowField])
        {
            GameObject obj = Instantiate(anglePrefab, anglesTransform);
            obj.transform.localPosition = new Vector3(s.GetTime() / 1000f * gameEvent.speed, DegreeY(s.GetDegree()), 0f);
            obj.transform.GetChild(1).GetComponent<SpriteRenderer>().color = notesDirector.FieldColor(speedsDirector.nowField);
            obj.SetActive(true);
            fieldAngles.Add(obj, s);
        }
        
        RenewalAngleLine();
    }

    private void RenewalAngleLine()
    {
        Angle[] ss = new List<Angle>(fieldAngles.Values).OrderBy(x => x.GetTime()).ToArray();

        if (ss[0].GetTime() != 0)
        {
            NewAngles(0, 0, 0);
            return;
        }
        
        // TODO : angleLineの調整
        // int leng = ss.Length;
        // List<Vector3> positions = new List<Vector3>();
        //
        // for (int i = 0; i < leng; i++)
        // {
        //     positions.Add(new Vector3(ss[i].GetTime() / 1000f * gameEvent.speed,
        //         DegreeY(ss[i].GetDegree()), 0f));
        //     
        //     if (!ss[i].GetIsVariation() && i != leng - 1)
        //         positions.Add(new Vector3(ss[i + 1].GetTime() / 1000f * gameEvent.speed,
        //             DegreeY(ss[i].GetDegree100()), 0f));
        // }
        //
        // positions.Add(new Vector3(gameEvent.GetComponent<AudioSource>().clip.length * gameEvent.speed,
        //     DegreeY(ss[leng - 1].GetDegree100()), 0f));
        //
        // transform.GetComponent<LineRenderer>().positionCount = positions.Count;
        // transform.GetComponent<LineRenderer>().SetPositions(positions.ToArray());
        //
        //
        anglesData[speedsDirector.nowField] = new List<Angle>(fieldAngles.Values);
    }

    public void SetChoose(GameObject angles)
    {
        angles.transform.GetChild(2).gameObject.SetActive(true);
    }
    
    public void SetDisChoose(GameObject angles)
    {
        angles.transform.GetChild(2).gameObject.SetActive(false);
    }

    public void SetTime(GameObject angles, int time)
    {
        Angle s = fieldAngles[angles];
        s.SetTime(time);
        angles.transform.localPosition = new Vector3(s.GetTime() / 1000f * gameEvent.speed, DegreeY(s.GetDegree()), 0f);
        RenewalAngleLine();
    }

    public void SetDegree(GameObject angles, int degree)
    {
        Angle s = fieldAngles[angles];
        s.SetDegree(degree);
        angles.transform.localPosition = new Vector3(s.GetTime() / 1000f * gameEvent.speed, DegreeY(s.GetDegree()), 0f);
        RenewalAngleLine();
    }

    public void SetVariation(GameObject angles, int variation)
    {
        Angle s = fieldAngles[angles];
        s.SetVariation(variation);
        RenewalAngleLine();
    }

    public float DegreeY(int degree)
    {
        degree %= 360;
        float re = -3.2f + (degree / 360f) * 7.2f;
        return re;
    }

    public void SetColor(Color color)
    {
        // transform.GetComponent<LineRenderer>().startColor = color;
        // transform.GetComponent<LineRenderer>().endColor = color;

        foreach (var g in fieldAngles.Keys)
        {
            g.transform.GetChild(1).GetComponent<SpriteRenderer>().color = color;
        }
    }
}
