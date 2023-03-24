using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private GameObject noteParent;
    [SerializeField] private GameObject notePrefab;

    [SerializeField] private GameObject lengthObj;
    public GameObject focusNote = null;

    [SerializeField] private InputField timeField;
    [SerializeField] private InputField laneFieldF;
    [SerializeField] private InputField laneFieldL;
    [SerializeField] private Dropdown kindDropdown;
    [SerializeField] private InputField lengthField;
    private int noteTime100;
    private int noteLaneF;
    private int noteLaneL;
    private int noteKind;
    private int noteLength100;
    private float rayDistance = 30f;

    public void NewNote()
    {
        GameObject obj = Instantiate(notePrefab, noteParent.transform);
        obj.GetComponent<NotesData>().note = new Note((int)(gameEvent.time * 100), 5, 7, 'N', 0);
        obj.GetComponent<NotesData>().DefaultSettings();
        obj.SetActive(true);
        if (focusNote != null) focusNote.GetComponent<NotesData>().DisChoose();
        focusNote = obj;
        focusNote.GetComponent<NotesData>().Choose();
        SetChoose();
    }
    
    public void NewNote(int time100, int start, int end, char kind, int length100)
    {
        GameObject obj = Instantiate(notePrefab, noteParent.transform);
        obj.GetComponent<NotesData>().note = new Note(time100, start, end, kind, length100);
        obj.GetComponent<NotesData>().DefaultSettings();
        obj.SetActive(true);
        if (focusNote != null) focusNote.GetComponent<NotesData>().DisChoose();
        focusNote = obj;
        focusNote.GetComponent<NotesData>().Choose();
        SetChoose();
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
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
                    gameEvent.nowBeatNote = -1;
                    gameEvent.nowBeatLong = -1;
                    gameEvent.FocusBeatSet(focusNote.GetComponent<NotesData>().note.GetTime100() / 100f);
                }
                else if (hit.collider != null)
                {
                    if (hit.collider.gameObject.CompareTag("EditRange"))
                    {
                        if (focusNote != null) focusNote.GetComponent<NotesData>().DisChoose();
                        focusNote = null;
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                        gameEvent.FocusBeatSet(gameEvent.time);
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

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (focusNote != null)
                {
                    focusNote.GetComponent<NotesData>().ClearNote();
                    focusNote = null;
                    gameEvent.FocusBeatSet(gameEvent.time);
                }
            }
        }
    }

    private void SetChoose()
    {
        // focusNoteのデータを取り出し表示する
        Note n = focusNote.GetComponent<NotesData>().note;
        timeField.text = (n.GetTime100() / 100f).ToString("F2");
        laneFieldF.text = n.GetStartLane().ToString();
        laneFieldL.text = n.GetEndLane().ToString();
        kindDropdown.value = NoteKindToInt(n.GetKind());
        lengthField.text = (n.GetLength100() / 100f).ToString("F2");
        
        if (n.GetKind() == 'L')
            lengthObj.SetActive(true);
        else
            lengthObj.SetActive(false);
    }

    private bool IsNote(string _tag)
    {
        if (_tag == "Normal" || _tag == "Hold" || _tag == "Flick" || _tag == "Long")
            return true;
        else
            return false;
    }

    public void NoteTimeSet()
    {
        float inp = Math.Max(float.Parse(timeField.text), 0f);
        inp = Math.Min(gameEvent.GetComponent<AudioSource>().clip.length, inp);
        noteTime100 = (int)(inp * 100);
        timeField.text = (noteTime100 / 100f).ToString("F2");
        if (focusNote != null)
        {
            focusNote.GetComponent<NotesData>().ChangeTime(noteTime100);
        }
    }
    
    public void NoteTimeSet(float cTime)
    {
        noteTime100 = (int)(cTime * 100);
        timeField.text = (noteTime100 / 100f).ToString("F2");
        if (focusNote != null)
        {
            focusNote.GetComponent<NotesData>().ChangeTime(noteTime100);
        }
    }

    public void NoteLaneSet()
    {
        noteLaneF = int.Parse(laneFieldF.text);
        noteLaneL = int.Parse(laneFieldL.text);

        noteLaneF = Math.Min(Math.Max(0, noteLaneF), 11);
        noteLaneL = Math.Max(Math.Min(12, noteLaneL), noteLaneF + 1);
        
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
        
        noteLaneF = Math.Min(Math.Max(0, noteLaneF), 11);
        noteLaneL = Math.Max(Math.Min(12, noteLaneL), noteLaneF + 1);
        
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
        
        if (focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
            lengthObj.SetActive(true);
        else
            lengthObj.SetActive(false);
    }

    public void NoteLengthSet()
    {
        float inp = Math.Max(float.Parse(lengthField.text), 0f);
        noteLength100 = (int)(inp * 100);
        if (focusNote != null && focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
        {
            if ((noteLength100 + noteTime100) / 100f > gameEvent.GetComponent<AudioSource>().clip.length)
            {
                noteLength100 = (int)((gameEvent.GetComponent<AudioSource>().clip.length - noteTime100 / 100f) * 100);
            }
            
            lengthField.text = (noteLength100 / 100f).ToString();
            focusNote.GetComponent<NotesData>().ChangeLength(noteLength100);
        }
    }

    public void NoteLengthSet(float cLength)
    {
        noteLength100 = (int)(Math.Max(cLength, 0f) * 100);
        if (focusNote != null)
        {
            if ((noteLength100 + noteTime100) * 100 > gameEvent.GetComponent<AudioSource>().clip.length)
            {
                noteLength100 = (int)((gameEvent.GetComponent<AudioSource>().clip.length - noteTime100 / 100f) * 100);
            }
            
            lengthField.text = (noteLength100 / 100f).ToString();
            focusNote.GetComponent<NotesData>().ChangeLength(noteLength100);
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
            case 3:
                result = 'L';
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
            case 'L':
                result = 3;
                break;
        }

        return result;
    }
}
