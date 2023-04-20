using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideMaintainData : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private CenterDirector centerDirector;
    
    private float startLanePosy = 4f;
    private float laneDif = 0.6f;

    private GameObject body;
    private GameObject flame;
    public GameObject parent;
    public SlideData parentSc;

    public void DefaultSettings(GameObject paren)
    {
        body = this.transform.GetChild(0).gameObject;
        flame = this.transform.GetChild(1).gameObject;
        parent = paren.gameObject;
        parentSc = parent.GetComponent<SlideData>();
    }

    public void Choose()
    {
        this.flame.SetActive(true);
    }

    public void DisChoose()
    {
        this.flame.SetActive(false);
    }

    public void Change()
    {
        transform.localPosition = new Vector3((parentSc.slideMaintain[this.gameObject].time100 + parentSc.note.GetTime100()) / 100f * gameEvent.speed, transform.localPosition.y, 0f);
        
        float start = startLanePosy - (laneDif * parentSc.slideMaintain[gameObject].startLine);
        float end = startLanePosy - (laneDif * parentSc.slideMaintain[gameObject].endLine);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        int dis = parentSc.slideMaintain[gameObject].endLine - parentSc.slideMaintain[gameObject].startLine;
        body.GetComponent<SpriteRenderer>().size = new Vector2(dis, 0.1f);
        flame.GetComponent<SpriteRenderer>().size = new Vector2(dis + 0.2f, 0.2f);
        this.GetComponent<BoxCollider2D>().size = new Vector2(0.3f, laneDif * dis);
    }

    public void SetTime(int time100)
    {
        transform.localPosition = new Vector3((time100 + parentSc.note.GetTime100()) / 100f * gameEvent.speed, transform.localPosition.y, 0f);
        parentSc.slideMaintain[this.gameObject].time100 = time100;
        
        parentSc.LineChange();
    }

    public void SetLane(int startLane, int endLane)
    {
        float start = startLanePosy - (laneDif * startLane);
        float end = startLanePosy - (laneDif * endLane);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        int dis = endLane - startLane;
        body.GetComponent<SpriteRenderer>().size = new Vector2(dis, 0.1f);
        flame.GetComponent<SpriteRenderer>().size = new Vector2(dis + 0.2f, 0.2f);
        this.GetComponent<BoxCollider2D>().size = new Vector2(0.3f, laneDif * dis);

        parentSc.slideMaintain[this.gameObject].startLine = startLane;
        parentSc.slideMaintain[this.gameObject].endLine = endLane;
        
        parentSc.LineChange();
    }

    public void Clear()
    {
        parentSc.slideMaintain.Remove(gameObject);
        Destroy(this.gameObject);
    }
}
