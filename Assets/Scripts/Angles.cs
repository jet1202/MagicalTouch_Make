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
    [SerializeField] private LinePreview linePreview;
    
    [SerializeField] private GameObject anglePrefab;
    [SerializeField] private GameObject angleLinePrefab;
    
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
        linePreview.angles = anglesData;
    }

    public void DeleteField(int index)
    {
        anglesData.RemoveAt(index);
        linePreview.angles = anglesData;
    }

    public void ChangeField()
    {
        anglesData[speedsDirector.nowField] = new List<Angle>(fieldAngles.Values);
    }

    // Angles
    public GameObject NewAngles(int time, int degree, int variation)
    {
        GameObject obj = Instantiate(anglePrefab, anglesTransform);
        fieldAngles.Add(obj, new Angle(time, degree, variation));
        obj.transform.localPosition = new Vector3(time / 1000f * gameEvent.speed, DegreeY(degree, true), 0f);
        obj.transform.GetChild(1).GetComponent<SpriteRenderer>().color =
            notesDirector.FieldColor(speedsDirector.nowField);
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
            obj.transform.localPosition =
                new Vector3(s.GetTime() / 1000f * gameEvent.speed, DegreeY(s.GetDegree(), true), 0f);
            obj.transform.GetChild(1).GetComponent<SpriteRenderer>().color =
                notesDirector.FieldColor(speedsDirector.nowField);
            obj.SetActive(true);
            fieldAngles.Add(obj, s);
        }
        
        RenewalAngleLine();
    }

    private void RenewalAngleLine()
    {
        Angle[] aa = new List<Angle>(fieldAngles.Values).OrderBy(x => x.GetTime()).ToArray();

        if (aa[0].GetTime() != 0)
        {
            NewAngles(0, 0, 0);
            return;
        }

        foreach (Transform l in linesTransform)
        {
            Destroy(l.gameObject);
        }

        int leng = aa.Length;
        int zone = aa[0].GetDegree() / 360;
        GameObject obj = Instantiate(angleLinePrefab, linesTransform);
        obj.transform.localPosition = new Vector3(0f, -7.2f * zone, 0f);
        List<Vector3> positions = new List<Vector3>();
        
        for (int i = 0; i < leng; i++)
        {
            positions.Add(new Vector3(aa[i].GetTime() / 1000f * gameEvent.speed,
                DegreeY(aa[i].GetDegree(), false), 0f));

            if (aa[i].GetDegree() / 360 != zone)
            {
                int z = zone;
                int Z = aa[i].GetDegree() / 360;
                int sZ = Z + (z > Z ? -1 : 1);
                
                for (z = AngleLoop(zone, sZ); z != sZ; z = AngleLoop(z, sZ))
                {
                    obj.GetComponent<LineRenderer>().positionCount = positions.Count;
                    obj.GetComponent<LineRenderer>().SetPositions(positions.ToArray());
                    obj.SetActive(true);

                    positions = positions.GetRange(positions.Count - 2, 2);

                    obj = Instantiate(angleLinePrefab, linesTransform);
                    obj.transform.localPosition = new Vector3(0f, -7.2f * z, 0f);
                }

                zone = Z;
            }

            if (i != leng - 1)
            {
                if (aa[i].GetVariation() == 0)
                {
                    positions.Add(new Vector3(aa[i + 1].GetTime() / 1000f * gameEvent.speed,
                        DegreeY(aa[i].GetDegree(), false), 0f));
                }
                else if (aa[i].GetVariation() == 1)
                {

                }
                else
                {
                    float T, nT, A, sT, aT, V;
                    int t1, t2, t, a1, a2, a;
                    for (float j = aa[i].GetTime(); j < aa[i + 1].GetTime(); j += 200 / gameEvent.speed)
                    {
                        t1 = aa[i].GetTime();
                        t2 = aa[i + 1].GetTime();
                        a1 = aa[i].GetDegree();
                        a2 = aa[i + 1].GetDegree();
                        T = j;
                        if (aa[i].GetVariation() > 0) V = aa[i].GetVariation();
                        else if (aa[i].GetVariation() < 0) V = -1.0f / aa[i].GetVariation();
                        else V = 0f;

                        t = t2 - t1;
                        nT = T - t1;
                        a = a2 - a1;
                        sT = nT / t;
                        aT = (float)Math.Pow(sT, V);
                        A = aT * a + a1;

                        positions.Add(new Vector3(T / 1000f * gameEvent.speed, DegreeY(A, false), 0f));
                        
                        if ((int)(A / 360) != zone)
                        {
                            int z = zone;
                            int Z = (int)(A / 360);
                            int sZ = Z + (z > Z ? -1 : 1);
                            
                            for (z = AngleLoop(zone, sZ); z != sZ; z = AngleLoop(z, sZ))
                            {
                                obj.GetComponent<LineRenderer>().positionCount = positions.Count;
                                obj.GetComponent<LineRenderer>().SetPositions(positions.ToArray());
                                obj.SetActive(true);
                        
                                positions = positions.GetRange(positions.Count - 2, 2);
                        
                                obj = Instantiate(angleLinePrefab, linesTransform);
                                obj.transform.localPosition = new Vector3(0f, -7.2f * z, 0f);
                            }

                            zone = Z;
                        }
                    }
                }
            }
        }
        
        positions.Add(new Vector3(gameEvent.GetComponent<AudioSource>().clip.length * gameEvent.speed,
            DegreeY(aa[leng - 1].GetDegree(), false), 0f));

        obj.GetComponent<LineRenderer>().positionCount = positions.Count;
        obj.GetComponent<LineRenderer>().SetPositions(positions.ToArray());
        obj.SetActive(true);
        
        anglesData[speedsDirector.nowField] = new List<Angle>(fieldAngles.Values);
        linePreview.angles = anglesData;
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
        angles.transform.localPosition =
            new Vector3(s.GetTime() / 1000f * gameEvent.speed, DegreeY(s.GetDegree(), true), 0f);
        RenewalAngleLine();
    }

    public void SetDegree(GameObject angles, int degree)
    {
        Angle s = fieldAngles[angles];
        s.SetDegree(degree);
        angles.transform.localPosition =
            new Vector3(s.GetTime() / 1000f * gameEvent.speed, DegreeY(s.GetDegree(), true), 0f);
        RenewalAngleLine();
    }

    public void SetVariation(GameObject angles, int variation)
    {
        Angle s = fieldAngles[angles];
        s.SetVariation(variation);
        RenewalAngleLine();
    }

    public float DegreeY(float degree, bool isMod)
    {
        if (isMod)
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

    private int AngleLoop(int a, int b)
    {
        if (a > b) a--;
        else if (a < b) a++;

        return a;
    }
}