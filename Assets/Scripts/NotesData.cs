using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesData : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private CenterDirector centerDirector;
    [SerializeField] private FieldSettingController fieldSettingController;
    
    [SerializeField] private Sprite normalNote;
    [SerializeField] private Sprite holdNote;
    [SerializeField] private Sprite flickNote;
    [SerializeField] private Sprite longNote;

    [SerializeField] private GameObject longInsidePrefab;
    
    private float startLanePosy = 4f;
    private float laneDif = 0.3f;
    private GameObject noteBody;
    private GameObject noteFlame;
    private GameObject noteLength;
    private GameObject noteFieldColor;
    private GameObject longInsideParent;
    public Note note;
    public int Number;

    public void DefaultSettings(Color color, bool isColor)
    {
        noteBody = transform.GetChild(0).gameObject;
        noteFlame = transform.GetChild(1).gameObject;
        noteLength = transform.GetChild(2).gameObject;
        noteFieldColor = transform.GetChild(3).gameObject;
        longInsideParent = transform.GetChild(4).gameObject;
        
        noteFieldColor.GetComponent<SpriteRenderer>().color = color;
        noteFieldColor.SetActive(isColor);
        
        int leng = centerDirector.NotesData.Count;
        char k = IsDummyNote() ? 'D' : note.GetKind();
        for (int i = 0; i <= leng; i++)
        {
            if (!centerDirector.NotesData.ContainsKey(i))
            {
                Number = i;
                centerDirector.NotesData.Add(Number, new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime(), new KeyValuePair<char, int>(k, note.GetLength())));
                break;
            }
        }
        
        Change();
        
        ChangeKind(note.GetKind());
        notesDirector.SetNoteColor(transform);
    }

    private void Change()
    {
        int dis = note.GetEndLane() - note.GetStartLane();
        Vector3 pos = new Vector3(note.GetTime() / 1000f * gameEvent.speed, startLanePosy - laneDif * (note.GetStartLane() + note.GetEndLane()) / 2f, 0f);
        // noteBody
        transform.localPosition = pos;
        noteBody.GetComponent<SpriteRenderer>().size = new Vector2(dis * 0.25f, 1f);
        // noteFlame
        noteFlame.transform.localPosition =
            new Vector3(Mathf.Max(gameEvent.speed * note.GetLength() / 2000 - 0.075f, 0f), 0f, pos.z);
        noteFlame.transform.localScale = 
            new Vector3(Mathf.Max(gameEvent.speed * note.GetLength() / 1000f - 0.15f, 0f) + 0.4f, dis * laneDif + 0.1f, 1f);
        // noteLength
        noteLength.transform.localPosition = 
            new Vector3(gameEvent.speed * note.GetLength() / 2000, 0f, 0f);
        noteLength.transform.localScale = 
            new Vector3(gameEvent.speed * note.GetLength() / 1000f, dis * laneDif - 0.1f, 1f);
        // noteFieldColor
        noteFieldColor.transform.localScale =
            new Vector3(0.05f, dis * laneDif - 0.3f, 1f);
        // collider2D
        GetComponent<BoxCollider2D>().offset =
            new Vector2(Mathf.Max(gameEvent.speed * note.GetLength() / 2000 - 0.075f, 0f), 0f);
        GetComponent<BoxCollider2D>().size = 
            new Vector2(Mathf.Max(gameEvent.speed * note.GetLength() / 1000f - 0.15f, 0f) + 0.3f, dis * laneDif);

        CenterNotesDataUpdate();
        
        ChangeLongInside();
    }

    public void ChangeTimeBySpeed()
    {
        Change();
    }

    public void Choose(bool isCore)
    {
        if (isCore)
            noteFlame.GetComponent<SpriteRenderer>().color = new Color(1f, 0.3f, 0.3f, 1f);
        else
            noteFlame.GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0f, 0.7f);
        
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
        ChangeLongInside();
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
                ChangeLength(0);
                break;
            case 'H':
                noteBody.GetComponent<SpriteRenderer>().sprite = holdNote;
                transform.gameObject.tag = "Hold";
                ChangeLength(0);
                break;
            case 'F':
                noteBody.GetComponent<SpriteRenderer>().sprite = flickNote;
                transform.gameObject.tag = "Flick";
                ChangeLength(0);
                break;
            case 'L':
                noteBody.GetComponent<SpriteRenderer>().sprite = longNote;
                transform.gameObject.tag = "Long";
                break;
        }
        CenterNotesDataUpdate();
    }

    public void ChangeLength(int length)
    {
        note.SetLength(length);
        Change();
    }

    public void ChangeField(int field)
    {
        note.SetField(field);
        CenterNotesDataUpdate();
    }

    private void CenterNotesDataUpdate()
    {
        char k = IsDummyNote() ? 'D' : note.GetKind();
        centerDirector.NotesData[Number] = new KeyValuePair<int, KeyValuePair<char, int>>(note.GetTime(),
            new KeyValuePair<char, int>(k, note.GetLength()));
    }

    public void ClearNote()
    {
        centerDirector.NotesData.Remove(Number);
        Destroy(this.gameObject);
    }

    public void ChangeLongInside()
    {
        float[] t = notesDirector.GetLongInsideTime(note.GetTime(), note.GetLength());
        longInsideParent.SetActive(gameEvent.isLongInside);

        foreach (Transform g in longInsideParent.transform)
            Destroy(g.gameObject);

        foreach (float time in t)
        {
            float x = (time - note.GetTime() / 1000f) * gameEvent.speed;
            GameObject obj = Instantiate(longInsidePrefab, longInsideParent.transform);
            obj.transform.localPosition = new Vector3(x, 0f, 0f);
            obj.SetActive(true);
        }
    }

    private bool IsDummyNote()
    {
        return fieldSettingController.fieldsIsDummy[note.GetField()];
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
