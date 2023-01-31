using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesData : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private CenterDirector centerDirector;
    [SerializeField] private Sprite normalNote;
    [SerializeField] private Sprite holdNote;
    [SerializeField] private Sprite flickNote;
    private float startLanePosy = 4f;
    private float laneDif = 0.6f;
    private GameObject noteBody;
    private GameObject noteFlame;
    public Note note;
    public int Number;

    public void DefaultSettings()
    {
        noteBody = transform.GetChild(0).gameObject;
        noteFlame = transform.GetChild(1).gameObject;
        int dis = note.GetEndLane() - note.GetStartLane();
        transform.localPosition = new Vector3(note.GetTime() * gameEvent.speed, startLanePosy - laneDif * (note.GetStartLane() + note.GetEndLane()) / 2f, 0f);
        noteBody.GetComponent<SpriteRenderer>().size = new Vector2(dis * 0.5f, 1f);
        noteFlame.transform.localScale = new Vector3(noteFlame.transform.localScale.x, dis * 0.6f + 0.1f, 1f);
        GetComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.6f * dis);
        noteBody.GetComponent<SpriteRenderer>().sprite = NoteKind(note.GetKind());
        transform.gameObject.tag = "Normal";
        
        Number = centerDirector.NotesData.Count; ;
        centerDirector.NotesData.Add(Number, new KeyValuePair<float, char>(note.GetTime(), note.GetKind()));
    }

    public void Choose()
    {
        noteFlame.SetActive(true);
    }

    public void DisChoose()
    {
        noteFlame.SetActive(false);
    }

    public void ChangeTime(float time)
    {
        note.SetTime(time);
        transform.localPosition = new Vector3(time * gameEvent.speed, transform.localPosition.y, 0f);
        CenterNotesDataUpdate();
    }

    public void ChangeLane(int startLane, int endLane)
    {
        note.SetLane(startLane, endLane);
        float start = startLanePosy - (laneDif * startLane);
        float end = startLanePosy - (laneDif * endLane);
        transform.localPosition = new Vector3(transform.localPosition.x, (start + end) / 2f, 0f);
        noteBody.GetComponent<SpriteRenderer>().size = new Vector2((endLane - startLane) * 0.5f, 1f);
        this.GetComponent<BoxCollider2D>().size = new Vector2(0.3f, (endLane - startLane) * 0.6f);
        noteFlame.transform.localScale = new Vector3(noteFlame.transform.localScale.x, (endLane - startLane) * 0.6f + 0.1f, 1f);
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
        }
        CenterNotesDataUpdate();
    }

    private void CenterNotesDataUpdate()
    {
        centerDirector.NotesData[Number] = new KeyValuePair<float, char>(note.GetTime(), note.GetKind());
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
        }

        return result;
    }
}
