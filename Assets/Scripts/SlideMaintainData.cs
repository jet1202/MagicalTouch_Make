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

    public void DefaultSettings()
    {
        body = this.transform.GetChild(0).gameObject;
        flame = this.transform.GetChild(1).gameObject;
        parent = this.transform.parent.gameObject;
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

    public void SetTime(int time100)
    {
        transform.localPosition = new Vector3(time100 / 100f * gameEvent.speed, transform.localPosition.y, 0f);

        parentSc.slideMaintain[this.gameObject].time100 = time100;
    }

    public void SetLane(int startLane, int endLane)
    {
        float start = startLanePosy - (laneDif * startLane);
        float end = startLanePosy - (laneDif * endLane);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        int dis = endLane - startLane;
        body.GetComponent<SpriteRenderer>().size = new Vector2(dis * 0.5f, 1f);
        this.GetComponent<BoxCollider2D>().size = new Vector2(0.3f, laneDif * dis);

        parentSc.slideMaintain[this.gameObject].startLine = startLane;
        parentSc.slideMaintain[this.gameObject].endLine = endLane;
    }

    public void Clear()
    {
        parentSc.slideMaintain.Remove(gameObject);
        Destroy(this.gameObject);
    }
}
