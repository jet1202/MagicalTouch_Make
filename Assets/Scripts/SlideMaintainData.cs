using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideMaintainData : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private NotesDirector notesDirector;
    
    private float startLanePosy = 4f;
    private float laneDif = 0.3f;

    private GameObject body;
    private GameObject flame;
    public GameObject parent;
    public SlideData parentSc;

    public void DefaultSettings(GameObject paren, int color)
    {
        body = this.transform.GetChild(0).gameObject;
        flame = this.transform.GetChild(1).gameObject;
        parent = paren;
        parentSc = parent.GetComponent<SlideData>();
        
        body.GetComponent<SpriteRenderer>().color = parentSc.SlideColor(color, 1f);
        notesDirector.SetNoteColor(this.transform);
    }

    public void Choose(bool isCore)
    {
        if (isCore)
            flame.GetComponent<SpriteRenderer>().color = new Color(1f, 0.3f, 0.3f, 1f);
        else
            flame.GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0f, 0.7f);
        
        this.flame.SetActive(true);
    }

    public void DisChoose()
    {
        this.flame.SetActive(false);
    }

    public void Change()
    {
        transform.localPosition =
            new Vector3(
                (parentSc.slideMaintain[this.gameObject].time + parentSc.note.GetTime()) / 1000f * gameEvent.speed,
                transform.localPosition.y, 0f);
        
        float start = startLanePosy - (laneDif * parentSc.slideMaintain[gameObject].startLane);
        float end = startLanePosy - (laneDif * parentSc.slideMaintain[gameObject].endLane);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        float dis = parentSc.slideMaintain[gameObject].endLane - parentSc.slideMaintain[gameObject].startLane;
        body.GetComponent<SpriteRenderer>().size = new Vector2(dis / 2f, 0.1f);
        flame.GetComponent<SpriteRenderer>().size = new Vector2(dis / 2f + 0.2f, 0.2f);
        this.GetComponent<BoxCollider2D>().size = new Vector2(0.3f, laneDif * dis);
    }

    public void SetTime(int time)
    {
        transform.localPosition = new Vector3((time + parentSc.note.GetTime()) / 1000f * gameEvent.speed,
            transform.localPosition.y, 0f);
        parentSc.slideMaintain[this.gameObject].time = time;
        
        parentSc.LineChange();
    }

    public void SetLane(int startLane, int endLane)
    {
        float start = startLanePosy - (laneDif * startLane);
        float end = startLanePosy - (laneDif * endLane);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        int dis = endLane - startLane;
        body.GetComponent<SpriteRenderer>().size = new Vector2(dis / 2f, 0.1f);
        flame.GetComponent<SpriteRenderer>().size = new Vector2(dis / 2f + 0.2f, 0.2f);
        this.GetComponent<BoxCollider2D>().size = new Vector2(0.3f, laneDif * dis);

        parentSc.slideMaintain[this.gameObject].startLane = startLane;
        parentSc.slideMaintain[this.gameObject].endLane = endLane;
        
        parentSc.LineChange();
    }

    public void Clear()
    {
        parentSc.slideMaintain.Remove(gameObject);
        parentSc.LineChange();
        Destroy(this.gameObject);
    }
}
