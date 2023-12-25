using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Transparencies : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private Speeds speedsDirector;

    [SerializeField] private GameObject transparencyPrefab;

    public List<List<Transparency>> alphaData = new List<List<Transparency>>();
    public Dictionary<GameObject, Transparency> fieldTransparencies;

    public void NewField()
    {
        alphaData.Add(new List<Transparency>() { new Transparency(0, 100, false) });
        
    }
    
    public void DeleteField(int index)
    {
        alphaData.RemoveAt(index);
    }

    public void ChangeField()
    {
        alphaData[speedsDirector.nowField] = new List<Transparency>(fieldTransparencies.Values);
    }
    
    // Transparencies
    public GameObject NewTransparencies(int time, int alpha, bool isVariation)
    {
        GameObject obj = Instantiate(transparencyPrefab, transform);
        fieldTransparencies.Add(obj, new Transparency(time, alpha, isVariation));
        obj.transform.localPosition = new Vector3(time / 1000f * gameEvent.speed, TransparencyY(alpha), 0f);
        obj.transform.GetChild(1).GetComponent<SpriteRenderer>().color = notesDirector.FieldColor(speedsDirector.nowField);
        obj.SetActive(true);

        RenewalAlphaLine();

        return obj;
    }
    
    public void DeleteTransparencies(GameObject obj)
    {
        fieldTransparencies.Remove(obj);
        Destroy(obj);
        
        RenewalAlphaLine();
    }

    public void RenewalAlpha()
    {
        foreach (Transform g in transform)
        {
            Destroy(g.gameObject);
        }
        
        fieldTransparencies = new Dictionary<GameObject, Transparency>();

        foreach (var a in alphaData[speedsDirector.nowField])
        {
            GameObject obj = Instantiate(transparencyPrefab, transform);
            obj.transform.localPosition =
                new Vector3(a.GetTime() / 1000f * gameEvent.speed, TransparencyY(a.GetAlpha()), 0f);
            obj.transform.GetChild(1).GetComponent<SpriteRenderer>().color =
                notesDirector.FieldColor(speedsDirector.nowField);
            obj.SetActive(true);
            fieldTransparencies.Add(obj, a);
        }
        
        RenewalAlphaLine();
    }
    
    public void RenewalAlphaLine()
    {
        Transparency[] tp = new List<Transparency>(fieldTransparencies.Values).OrderBy(x => x.GetTime()).ToArray();

        if (tp[0].GetTime() != 0)
        {
            NewTransparencies(0, 100, false);
            return;
        }

        int leng = tp.Length;
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < leng; i++)
        {
            positions.Add(new Vector3(tp[i].GetTime() / 1000f * gameEvent.speed,
                TransparencyY(tp[i].GetAlpha()), 0f));
            
            if (!tp[i].GetIsVariation() && i != leng - 1)
                positions.Add(new Vector3(tp[i + 1].GetTime() / 1000f * gameEvent.speed, 
                    TransparencyY(tp[i].GetAlpha()), 0f));
        }
        
        positions.Add(new Vector3(gameEvent.GetComponent<AudioSource>().clip.length * gameEvent.speed,
            TransparencyY(tp[leng - 1].GetAlpha()), 0f));
        
        transform.GetComponent<LineRenderer>().positionCount = positions.Count;
        transform.GetComponent<LineRenderer>().SetPositions(positions.ToArray());
        
        alphaData[speedsDirector.nowField] = new List<Transparency>(fieldTransparencies.Values);
    }

    public void SetChoose(GameObject transparencies)
    {
        transparencies.transform.GetChild(2).gameObject.SetActive(true);
    }

    public void SetDisChoose(GameObject transparencies)
    {
        transparencies.transform.GetChild(2).gameObject.SetActive(false);
    }
    
    public void SetTime(GameObject transparencies, int time)
    {
        Transparency t = fieldTransparencies[transparencies];
        t.SetTime(time);
        transparencies.transform.localPosition = new Vector3(t.GetTime() / 1000f * gameEvent.speed, TransparencyY(t.GetAlpha()), 0f);
        RenewalAlphaLine();
    }
    
    public void SetAlpha(GameObject transparencies, int alpha)
    {
        Transparency t = fieldTransparencies[transparencies];
        t.SetAlpha(alpha);
        transparencies.transform.localPosition = new Vector3(t.GetTime() / 1000f * gameEvent.speed, TransparencyY(t.GetAlpha()), 0f);
        RenewalAlphaLine();
    }
    
    public void SetIsVariation(GameObject transparencies, bool isVariation)
    {
        Transparency t = fieldTransparencies[transparencies];
        t.SetIsVariation(isVariation);
        RenewalAlphaLine();
    }

    public float TransparencyY(int alpha)
    {
        alpha = Math.Clamp(alpha, 0, 100);
        float re = -3.2f + (alpha / 100f) * 7.2f;
        return re;
    }

    public void SetColor(Color color)
    {
        foreach (var g in fieldTransparencies.Keys)
        {
            g.transform.GetChild(1).GetComponent<SpriteRenderer>().color = color;
        }
    }
}
