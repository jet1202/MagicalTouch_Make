using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesData : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private CenterDirector centerDirector;
    [SerializeField] private Sprite normalNote;
    [SerializeField] private Sprite holdNote;
    [SerializeField] private Sprite flickNote;
    [SerializeField] private Sprite longNote;
    private float startLanePosy = 4f;
    private float laneDif = 0.6f;
    private GameObject noteBody;
    private GameObject noteFlame;
    private GameObject noteLength;
    public Note note;
    public int Number;

    public void DefaultSettings()
    {
        noteBody = transform.GetChild(0).gameObject;
        noteFlame = transform.GetChild(1).gameObject;
        noteLength = transform.GetChild(2).gameObject;
        
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
        
        noteBody.GetComponent<SpriteRenderer>().sprite = NoteKind(note.GetKind());
        transform.gameObject.tag = "Normal";
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
        // noteLength
        noteLength.transform.localPosition = 
            new Vector3(gameEvent.speed * note.GetLength100() / 200, 0f, 0f);
        noteLength.transform.localScale = 
            new Vector3(gameEvent.speed * note.GetLength100() / 100f, dis * 0.6f - 0.1f, 1f);
        // collider2D
        GetComponent<BoxCollider2D>().offset =
            new Vector2(Mathf.Max(gameEvent.speed * note.GetLength100() / 200 - 0.075f, 0f), 0f);
        GetComponent<BoxCollider2D>().size = 
            new Vector2(Mathf.Max(gameEvent.speed * note.GetLength100() / 100f - 0.15f, 0f) + 0.3f, 0.6f * dis);

        centerDirector.NotesData[Number] = new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime100(),
            new KeyValuePair<char, int>(note.GetKind(), note.GetLength100()));
    }

    public void ChangeTimeBySpeed()
    {
        Change();
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
    }

    public void ChangeLane(int startLane, int endLane)
    {
        note.SetLane(startLane, endLane);
        float start = startLanePosy - (laneDif * startLane);
        float end = startLanePosy - (laneDif * endLane);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        Change();
    }

    public void ChangeKind(char kind)
    {
        note.SetKind(kind);
        switch (kind)
        {
            case 'N':
                noteBody.GetComponent<SpriteRenderer>().sprite = normalNote;
                transform.gameObject.tag = "Normal";
                break;
            case 'H':
                noteBody.GetComponent<SpriteRenderer>().sprite = holdNote;
                transform.gameObject.tag = "Hold";
                break;
            case 'F':
                noteBody.GetComponent<SpriteRenderer>().sprite = flickNote;
                transform.gameObject.tag = "Flick";
                break;
            case 'L':
                noteBody.GetComponent<SpriteRenderer>().sprite = longNote;
                transform.gameObject.tag = "Long";
                break;
        }
        CenterNotesDataUpdate();
    }

    public void ChangeLength(int length100)
    {
        note.SetLength100(length100);
        Change();
    }

    public void ChangeSub(int sub)
    {
        note.SetSub(sub);
    }

    private void CenterNotesDataUpdate()
    {
        centerDirector.NotesData[Number] = new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime100(), new KeyValuePair<char, int>(note.GetKind(), note.GetLength100()));
    }

    public void ClearNote()
    {
        centerDirector.NotesData.Remove(Number);
        Destroy(this.gameObject);
    }
    
    private Sprite NoteKind(char kind)
    {
        Sprite result = null;
        switch (kind)
        {
            case 'N':
                result = normalNote;
                break;
            case 'H':
                result = holdNote;
                break;
            case 'F':
                result = flickNote;
                break;
            case 'L':
                result = longNote;
                break;
        }

        return result;
    }
}
