using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private GameObject noteParent;
    [SerializeField] private GameObject notePrefab;
    public GameObject focusNote = null;

    [SerializeField] private InputField timeField;
    [SerializeField] private InputField laneFieldF;
    [SerializeField] private InputField laneFieldL;
    [SerializeField] private Dropdown kindDropdown;
    private float noteTime;
    private int noteLaneF;
    private int noteLaneL;
    private int noteKind;
    private float rayDistance = 30f;

    public void NewNote()
    {
        GameObject obj = Instantiate(notePrefab, noteParent.transform);
        obj.GetComponent<NotesData>().note = new Note(gameEvent.time, 5, 7, 'N');
        obj.GetComponent<NotesData>().DefaultSettings();
        obj.SetActive(true);
    }
    
    public void NewNote(float time, int start, int end, char kind)
    {
        GameObject obj = Instantiate(notePrefab, noteParent.transform);
        obj.GetComponent<NotesData>().note = new Note(time, start, end, kind);
        obj.GetComponent<NotesData>().DefaultSettings();
        obj.SetActive(true);
        if (focusNote != null) focusNote.GetComponent<NotesData>().DisChoose();
        focusNote = obj;
        focusNote.GetComponent<NotesData>().Choose();
        SetChoose();
        gameEvent.nowBeat = -1;
    }

    public void ResetNote()
    {
        foreach (Transform t in noteParent.transform)
        {
            Destroy(t);
        }
    }

    private void Update()
    {
        if (gameEvent.isEdit)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction, rayDistance);
                
                if (hit.collider != null && IsNote(hit.collider.gameObject.tag))
                {
                    if (focusNote != null) focusNote.GetComponent<NotesData>().DisChoose();
                    focusNote = hit.collider.gameObject;
                    focusNote.GetComponent<NotesData>().Choose();
                    SetChoose();
                    gameEvent.nowBeat = -1;
                }
                else if (hit.collider != null)
                {
                    if (hit.collider.gameObject.CompareTag("EditRange"))
                    {
                        if (focusNote != null) focusNote.GetComponent<NotesData>().DisChoose();
                        focusNote = null;
                        gameEvent.nowBeat = -1;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (focusNote != null)
                {
                    Note data = focusNote.GetComponent<NotesData>().note;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        NoteLaneSet(data.GetStartLane() - 1, data.GetEndLane());
                    }
                    else
                    {
                        NoteLaneSet(data.GetStartLane() - 1, data.GetEndLane() - 1);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (focusNote != null)
                {
                    Note data = focusNote.GetComponent<NotesData>().note;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        NoteLaneSet(data.GetStartLane() + 1, data.GetEndLane());
                    }
                    else
                    {
                        NoteLaneSet(data.GetStartLane() + 1, data.GetEndLane() + 1);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (focusNote != null)
                {
                    focusNote.GetComponent<NotesData>().ClearNote();
                    focusNote = null;
                }
            }
        }
    }

    private void SetChoose()
    {
        // focusNoteのデータを取り出し表示する
        Note n = focusNote.GetComponent<NotesData>().note;
        timeField.text = n.GetTime().ToString("F2");
        laneFieldF.text = n.GetStartLane().ToString();
        laneFieldL.text = n.GetEndLane().ToString();
        kindDropdown.value = NoteKindToInt(n.GetKind());
    }

    private bool IsNote(string _tag)
    {
        if (_tag == "Normal" || _tag == "Hold" || _tag == "Flick")
            return true;
        else
        {
            return false;
        }
    }

    public void NoteTimeSet()
    {
        noteTime = Mathf.Max(float.Parse(timeField.text), 0f);
        if (noteTime > gameEvent.GetComponent<AudioSource>().clip.length)
            noteTime = 0f;
        timeField.text = noteTime.ToString("F2");
        if (focusNote != null)
        {
            focusNote.GetComponent<NotesData>().ChangeTime(noteTime);
        }
    }
    
    public void NoteTimeSet(float cTime)
    {
        noteTime = cTime;
        timeField.text = noteTime.ToString("F2");
        focusNote.GetComponent<NotesData>().ChangeTime(noteTime);
    }

    public void NoteLaneSet()
    {
        noteLaneF = int.Parse(laneFieldF.text);
        noteLaneL = int.Parse(laneFieldL.text);

        noteLaneF = Mathf.Min(Mathf.Max(0, noteLaneF), 11);
        noteLaneL = Mathf.Max(Mathf.Min(12, noteLaneL), noteLaneF + 1);
        
        laneFieldF.text = noteLaneF.ToString();
        laneFieldL.text = noteLaneL.ToString();
        
        if (focusNote != null)
        {
            focusNote.GetComponent<NotesData>().ChangeLane(noteLaneF, noteLaneL);
        }
    }
    
    public void NoteLaneSet(int start, int end)
    {
        noteLaneF = start;
        noteLaneL = end;
        
        noteLaneF = Mathf.Min(Mathf.Max(0, noteLaneF), 11);
        noteLaneL = Mathf.Max(Mathf.Min(12, noteLaneL), noteLaneF + 1);
        
        laneFieldF.text = noteLaneF.ToString();
        laneFieldL.text = noteLaneL.ToString();
        
        if (focusNote != null)
        {
            focusNote.GetComponent<NotesData>().ChangeLane(noteLaneF, noteLaneL);
        }
    }

    public void NoteKindSet()
    {
        noteKind = kindDropdown.value;
        if (focusNote != null)
        {
            focusNote.GetComponent<NotesData>().ChangeKind(NoteKindToChar(noteKind));
        }
    }

    private char NoteKindToChar(int kind)
    {
        char result = '-';
        switch (kind)
        {
            case 0:
                result = 'N';
                break;
            case 1:
                result = 'H';
                break;
            case 2:
                result = 'F';
                break;
        }

        return result;
    }
    
    private int NoteKindToInt(char kind)
    {
        int result = -1;
        switch (kind)
        {
            case 'N':
                result = 0;
                break;
            case 'H':
                result = 1;
                break;
            case 'F':
                result = 2;
                break;
        }

        return result;
    }
}
