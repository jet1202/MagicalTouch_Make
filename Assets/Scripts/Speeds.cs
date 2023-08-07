using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Speeds : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    
    [SerializeField] private GameObject speedPrefab;
    
    public List<List<Speed>> speedsData = new List<List<Speed>>();
    public Dictionary<GameObject, Speed> fieldSpeeds;
    public int nowField;

    private void Start()
    {
        NewField();
        nowField = 0;
    }

    public void NewField()
    {
        speedsData.Add(new List<Speed>() { new Speed(0, 100, false) });
    }

    public void DeleteField(int index)
    {
        speedsData.RemoveAt(index);
    }

    public GameObject NewSpeeds(int time, int speed100, bool isVariation)
    {
        GameObject obj = Instantiate(speedPrefab, transform);
        fieldSpeeds.Add(obj, new Speed(time, speed100, isVariation));
        obj.transform.localPosition = new Vector3(time / 1000f * gameEvent.speed, 0.4f + (speed100 / 100f) * 0.6f, 0f);
        obj.SetActive(true);
        
        RenewalSpeedLine();

        return obj;
    }

    public void DeleteSpeeds(GameObject obj)
    {
        fieldSpeeds.Remove(obj);
        Destroy(obj);
        
        RenewalSpeedLine();
    }

    public void RenewalSpeed()
    {
        foreach (Transform g in transform)
        {
            Destroy(g.gameObject);
        }
        
        fieldSpeeds = new Dictionary<GameObject, Speed>();

        foreach (var s in speedsData[nowField])
        {
            GameObject obj = Instantiate(speedPrefab, transform);
            obj.transform.localPosition = new Vector3(s.GetTime() / 1000f * gameEvent.speed, 0.4f + (s.GetSpeed100() / 100f) * 0.6f, 0f);
            obj.SetActive(true);
            fieldSpeeds.Add(obj, s);
        }
        
        RenewalSpeedLine();
    }

    private void RenewalSpeedLine()
    {
        Speed[] ss = new List<Speed>(fieldSpeeds.Values).OrderBy(x => x.GetTime()).ToArray();

        if (ss[0].GetTime() != 0)
        {
            NewSpeeds(0, 100, false);
            return;
        }
        
        int leng = ss.Length;
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < leng; i++)
        {
            positions.Add(new Vector3(ss[i].GetTime() / 1000f * gameEvent.speed,
                0.4f + (ss[i].GetSpeed100() / 100f) * 0.6f, 0f));
            
            if (!ss[i].GetIsVariation() && i != leng - 1)
                positions.Add(new Vector3(ss[i + 1].GetTime() / 1000f * gameEvent.speed,
                    0.4f + (ss[i].GetSpeed100() / 100f) * 0.6f, 0f));
        }

        positions.Add(new Vector3(gameEvent.GetComponent<AudioSource>().clip.length * gameEvent.speed,
            0.4f + (ss[leng - 1].GetSpeed100() / 100f) * 0.6f, 0f));

        transform.GetComponent<LineRenderer>().positionCount = positions.Count;
        transform.GetComponent<LineRenderer>().SetPositions(positions.ToArray());


        speedsData[nowField] = new List<Speed>(fieldSpeeds.Values);
    }

    public void SetChoose(GameObject speeds)
    {
        speeds.transform.GetChild(2).gameObject.SetActive(true);
    }
    
    public void SetDisChoose(GameObject speeds)
    {
        speeds.transform.GetChild(2).gameObject.SetActive(false);
    }

    public void SetTime(GameObject speeds, int time)
    {
        Speed s = fieldSpeeds[speeds];
        s.SetTime(time);
        speeds.transform.localPosition = new Vector3(s.GetTime() / 1000f * gameEvent.speed, 0.4f + (s.GetSpeed100() / 100f) * 0.6f, 0f);
        RenewalSpeedLine();
    }

    public void SetSpeed(GameObject speeds, int speed100)
    {
        Speed s = fieldSpeeds[speeds];
        s.SetSpeed100(speed100);
        speeds.transform.localPosition = new Vector3(s.GetTime() / 1000f * gameEvent.speed, 0.4f + (s.GetSpeed100() / 100f) * 0.6f, 0f);
        RenewalSpeedLine();
    }

    public void SetIsVariation(GameObject speeds, bool isVariation)
    {
        Speed s = fieldSpeeds[speeds];
        s.SetIsVariation(isVariation);
        RenewalSpeedLine();
    }
}
