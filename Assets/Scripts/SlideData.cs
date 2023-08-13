using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class SlideData : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private CenterDirector centerDirector;
    
    public Dictionary<GameObject, SlideMaintain> slideMaintain = new Dictionary<GameObject, SlideMaintain>();
    private LineRenderer lineRenderer;
    private GameObject noteBody;
    private GameObject noteFlame;
    private GameObject noteLine;

    private float startLanePosy = 4f;
    private float laneDif = 0.6f;
    public Note note;
    public int Number;

    public void DefaultSettings(Color color, bool isColor)
    {
        noteBody = transform.GetChild(0).gameObject;
        noteFlame = transform.GetChild(1).gameObject;
        noteLine = transform.GetChild(2).gameObject;
        lineRenderer = noteLine.GetComponent<LineRenderer>();

        transform.GetChild(3).GetComponent<SpriteRenderer>().color = color;
        transform.GetChild(3).gameObject.SetActive(isColor);
        
        int leng = centerDirector.NotesData.Count;
        for (int i = 0; i <= leng; i++)
        {
            if (!centerDirector.NotesData.ContainsKey(i))
            {
                Number = i;
                centerDirector.NotesData.Add(Number, new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime(), new KeyValuePair<char, int>(note.GetKind(), note.GetLength())));
                break;
            }
        }

        Change();
    }
    
    public void Change()
    {
        int dis = note.GetEndLane() - note.GetStartLane();
        Vector3 pos = new Vector3(note.GetTime() / 1000f * gameEvent.speed, startLanePosy - laneDif * (note.GetStartLane() + note.GetEndLane()) / 2f, 0f);
        // noteBody
        transform.localPosition = pos;
        noteBody.GetComponent<SpriteRenderer>().size = new Vector2(dis * 0.5f, 1f);
        // noteFlame
        noteFlame.transform.localPosition =
            new Vector3(Mathf.Max(gameEvent.speed * note.GetLength() / 2000 - 0.075f, 0f), 0f, pos.z);
        noteFlame.transform.localScale = 
            new Vector3(Mathf.Max(gameEvent.speed * note.GetLength() / 1000f - 0.15f, 0f) + 0.4f, dis * 0.6f + 0.1f, 1f);
        // collider2D
        GetComponent<BoxCollider2D>().offset =
            new Vector2(Mathf.Max(gameEvent.speed * note.GetLength() / 2000 - 0.075f, 0f), 0f);
        GetComponent<BoxCollider2D>().size = 
            new Vector2(Mathf.Max(gameEvent.speed * note.GetLength() / 1000f - 0.15f, 0f) + 0.3f, 0.6f * dis);

        centerDirector.NotesData[Number] = new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime(),
            new KeyValuePair<char, int>(note.GetKind(), note.GetLength()));
        LineChange();
    }

    public void LineChange()
    {
        var d = slideMaintain.OrderBy(x => x.Value.time);
        slideMaintain = new Dictionary<GameObject, SlideMaintain>();
        foreach (var data in d)
        {
            slideMaintain.Add(data.Key, data.Value);
        }

        Vector3[] positions = new Vector3[slideMaintain.Count + 1];
        positions[0] = new Vector3(0, 0, 0);
        int i = 0;
        foreach (var data in slideMaintain)
        {
            float t = data.Value.time / 1000f;
            float l = (data.Value.endLane + data.Value.startLane - note.GetStartLane() - note.GetEndLane()) / -2f;

            positions[i + 1] = new Vector3(t * gameEvent.speed, (laneDif * l), 0f);

            i++;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
    
    private void CenterNotesDataUpdate()
    {
        centerDirector.NotesData[Number] = new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime(), new KeyValuePair<char, int>(note.GetKind(), note.GetLength()));
    }
    
    public void Choose()
    {
        noteFlame.SetActive(true);
    }

    public void DisChoose()
    {
        noteFlame.SetActive(false);
    }

    public void ChangeTime(int time)
    {
        note.SetTime(time);
        transform.localPosition = new Vector3(time / 1000f * gameEvent.speed, transform.localPosition.y, 0f);
        CenterNotesDataUpdate();
        LineChange();

        foreach (var s in slideMaintain)
        {
            s.Key.GetComponent<SlideMaintainData>().Change();
        }
    }

    public void ChangeLane(int startLane, int endLane)
    {
        note.SetLane(startLane, endLane);
        float start = startLanePosy - (laneDif * startLane);
        float end = startLanePosy - (laneDif * endLane);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        Change();
        LineChange();
        
        foreach (var s in slideMaintain)
        {
            s.Key.GetComponent<SlideMaintainData>().Change();
        }
    }

    public void ChangeField(int field)
    {
        note.SetField(field);
    }
    
    public void ClearNote()
    {
        centerDirector.NotesData.Remove(Number);

        foreach (var s in slideMaintain)
        {
            Destroy(s.Key);
            // s.Key.GetComponent<SlideMaintainData>().Clear();
        }
        Destroy(this.gameObject);
    }

    public void NewMaintain(GameObject obj, SlideMaintain data)
    {
        slideMaintain.Add(obj, data);
        obj.GetComponent<SlideMaintainData>().DefaultSettings(this.gameObject);
        obj.GetComponent<SlideMaintainData>().SetTime(data.time);
        obj.GetComponent<SlideMaintainData>().SetLane(data.startLane, data.endLane);
        LineChange();
    }
}
