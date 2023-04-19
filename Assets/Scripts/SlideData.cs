using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void DefaultSettings()
    {
        noteBody = transform.GetChild(0).gameObject;
        noteFlame = transform.GetChild(1).gameObject;
        noteLine = transform.GetChild(2).gameObject;
        lineRenderer = noteLine.GetComponent<LineRenderer>();
        
        int leng = centerDirector.NotesData.Count;
        for (int i = 0; i <= leng; i++)
        {
            if (!centerDirector.NotesData.ContainsKey(i))
            {
                Number = i;
                centerDirector.NotesData.Add(Number, new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime100(), new KeyValuePair<char, int>(note.GetKind(), note.GetLength100())));
                break;
            }
        }

        Change();
    }
    
    private void Change()
    {
        int dis = note.GetEndLane() - note.GetStartLane();
        Vector3 pos = new Vector3(note.GetTime100() / 100f * gameEvent.speed, startLanePosy - laneDif * (note.GetStartLane() + note.GetEndLane()) / 2f, 0f);
        // noteBody
        transform.localPosition = pos;
        noteBody.GetComponent<SpriteRenderer>().size = new Vector2(dis * 0.5f, 1f);
        // noteFlame
        noteFlame.transform.localPosition =
            new Vector3(Mathf.Max(gameEvent.speed * note.GetLength100() / 200 - 0.075f, 0f), 0f, pos.z);
        noteFlame.transform.localScale = 
            new Vector3(Mathf.Max(gameEvent.speed * note.GetLength100() / 100f - 0.15f, 0f) + 0.4f, dis * 0.6f + 0.1f, 1f);
        // collider2D
        GetComponent<BoxCollider2D>().offset =
            new Vector2(Mathf.Max(gameEvent.speed * note.GetLength100() / 200 - 0.075f, 0f), 0f);
        GetComponent<BoxCollider2D>().size = 
            new Vector2(Mathf.Max(gameEvent.speed * note.GetLength100() / 100f - 0.15f, 0f) + 0.3f, 0.6f * dis);

        centerDirector.NotesData[Number] = new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime100(),
            new KeyValuePair<char, int>(note.GetKind(), note.GetLength100()));
        LineChange();
    }

    public void LineChange()
    {
        var d = slideMaintain.OrderBy(x => x.Value.time100);
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
            float t = data.Value.time100 / 100f;
            float l = (data.Value.endLine + data.Value.startLine - note.GetStartLane() - note.GetEndLane()) / -2f;

            positions[i + 1] = new Vector3(t * gameEvent.speed, (laneDif * l), 0f);

            i++;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
    
    private void CenterNotesDataUpdate()
    {
        centerDirector.NotesData[Number] = new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime100(), new KeyValuePair<char, int>(note.GetKind(), note.GetLength100()));
    }
    
    public void Choose()
    {
        noteFlame.SetActive(true);
    }

    public void DisChoose()
    {
        noteFlame.SetActive(false);
    }

    public void ChangeTime(int time100)
    {
        note.SetTime100(time100);
        transform.localPosition = new Vector3(time100 / 100f * gameEvent.speed, transform.localPosition.y, 0f);
        CenterNotesDataUpdate();
        LineChange();
    }

    public void ChangeLane(int startLane, int endLane)
    {
        note.SetLane(startLane, endLane);
        float start = startLanePosy - (laneDif * startLane);
        float end = startLanePosy - (laneDif * endLane);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        Change();
        LineChange();
    }
    
    public void ClearNote()
    {
        centerDirector.NotesData.Remove(Number);
        Destroy(this.gameObject);
    }

    public void NewMaintain(GameObject obj, SlideMaintain data)
    {
        slideMaintain.Add(obj, data);
        obj.GetComponent<SlideMaintainData>().DefaultSettings(this.gameObject);
        obj.GetComponent<SlideMaintainData>().SetTime(data.time100);
        obj.GetComponent<SlideMaintainData>().SetLane(data.startLine, data.endLine);
        LineChange();
    }
}
