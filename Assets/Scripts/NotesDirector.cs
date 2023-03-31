using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private NotesController notesController;
    [SerializeField] private GameObject noteParent;
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private GameObject bpmParent;
    [SerializeField] private GameObject bpmPrefab;

    [SerializeField] public GameObject lengthObj;
    [SerializeField] public GameObject laneAndKindObj;
    [SerializeField] private GameObject bpmObj;
    public GameObject focusNote = null;
    public bool noteOrBpm;

    [SerializeField] private TMP_InputField timeField;
    [SerializeField] private TMP_InputField laneFieldF;
    [SerializeField] private TMP_InputField laneFieldL;
    [SerializeField] private TMP_Dropdown kindDropdown;
    [SerializeField] private TMP_InputField lengthField;
    [SerializeField] private TMP_InputField bpmField;
    
    private int focusTime100;
    
    private int noteLaneF;
    private int noteLaneL;
    private int noteKind;
    private int noteLength100;
    private int bpmBpm;
    
    private float rayDistance = 30f;
    
    public Dictionary<GameObject, Bpms> bpms = new Dictionary<GameObject, Bpms>();

    private void Update()
    {
        if (gameEvent.isEdit && EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction, rayDistance);
                
                if (hit.collider != null)
                {
                    if (IsNote(hit.collider.gameObject.tag))
                    {
                        SetDisChoose();
                        focusNote = hit.collider.gameObject;
                        focusNote.GetComponent<NotesData>().Choose();
                        noteOrBpm = true;
                        SetChoose();
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                        gameEvent.FocusBeatSet(focusNote.GetComponent<NotesData>().note.GetTime100() / 100f);
                    }
                    else if (hit.collider.gameObject.CompareTag("Bpm"))
                    {
                        SetDisChoose();
                        focusNote = hit.collider.gameObject;
                        focusNote.GetComponent<BpmData>().Choose();
                        noteOrBpm = false;
                        SetChoose();
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                    }
                    else if (hit.collider.gameObject.CompareTag("EditRange"))
                    {
                        SetDisChoose();
                        focusNote = null;
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                        gameEvent.FocusBeatSet(gameEvent.time);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (focusNote != null && noteOrBpm)
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
                if (focusNote != null && noteOrBpm)
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
                    if (noteOrBpm)
                    {
                        focusNote.GetComponent<NotesData>().ClearNote();
                        gameEvent.FocusBeatSet(gameEvent.time);
                    }
                    else
                    {
                        if (bpms.Count != 1)
                        {
                            focusNote.GetComponent<BpmData>().ClearBpm();
                            bpms.Remove(focusNote);
                            notesController.MeasureLineSet(bpms);
                        }
                    }

                    focusNote = null;
                }
            }
        }
    }
    
    private void SetChoose()
    {
        if (noteOrBpm)
        {
            // focusNoteのデータを取り出し表示する
            Note n = focusNote.GetComponent<NotesData>().note;
            timeField.text = (n.GetTime100() / 100f).ToString("F2");
            laneFieldF.text = n.GetStartLane().ToString();
            laneFieldL.text = n.GetEndLane().ToString();
            kindDropdown.value = NoteKindToInt(n.GetKind());
            lengthField.text = (n.GetLength100() / 100f).ToString("F2");

            if (n.GetKind() == 'L')
            {
                lengthObj.SetActive(true);
                laneAndKindObj.SetActive(true);
                bpmObj.SetActive(false);
            }
            else
            {
                lengthObj.SetActive(false);
                laneAndKindObj.SetActive(true);
                bpmObj.SetActive(false);
            }
        }
        else
        {
            Bpms b = bpms[focusNote];
            timeField.text = (b.GetTime100() / 100f).ToString("F2");
            bpmField.text = b.GetBpm().ToString();
            
            lengthObj.SetActive(false);
            laneAndKindObj.SetActive(false);
            bpmObj.SetActive(true);
        }
    }

    private void SetDisChoose()
    {
        if (focusNote == null) return;

        if (noteOrBpm)
        {
            focusNote.GetComponent<NotesData>().DisChoose();
        }
        else
        {
            focusNote.GetComponent<BpmData>().DisChoose();
        }
    }
    
    public void TimeSet()
    {
        TimeSet(float.Parse(timeField.text));
    }
    
    public void TimeSet(float cTime)
    {
        cTime = Math.Min(gameEvent.GetComponent<AudioSource>().clip.length, Math.Max(cTime, 0f));
        focusTime100 = (int)(cTime * 100);
        timeField.text = (focusTime100 / 100f).ToString("F2");
        
        if (focusNote != null)
        {
            if (noteOrBpm)
                focusNote.GetComponent<NotesData>().ChangeTime(focusTime100);
            else
            {
                bpms[focusNote].SetTime100(focusTime100);
                focusNote.GetComponent<BpmData>().ChangeTime(focusTime100 / 100f);
                
                notesController.MeasureLineSet(bpms);
            }
        }
    }
    
    // Note
    public void NewNote()
    {
        NewNote((int)(gameEvent.time * 100), 5, 7, 'N', 0);
    }
    
    public void NewNote(int time100, int start, int end, char kind, int length100)
    {
        GameObject obj = Instantiate(notePrefab, noteParent.transform);
        obj.GetComponent<NotesData>().note = new Note(time100, start, end, kind, length100);
        obj.GetComponent<NotesData>().DefaultSettings();
        obj.SetActive(true);
        
        SetDisChoose();
        focusNote = obj;
        focusNote.GetComponent<NotesData>().Choose();
        noteOrBpm = true;
        SetChoose();
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
    }
    
    private bool IsNote(string _tag)
    {
        if (_tag == "Normal" || _tag == "Hold" || _tag == "Flick" || _tag == "Long")
            return true;
        else
            return false;
    }

    public void NoteLaneSet()
    { 
        NoteLaneSet(int.Parse(laneFieldF.text), int.Parse(laneFieldL.text));
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
        NoteLengthSet(float.Parse(lengthField.text));
    }

    public void NoteLengthSet(float cLength)
    {
        noteLength100 = (int)(Math.Max(cLength, 0f) * 100);
        if (focusNote != null && focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
        {
            if ((noteLength100 + focusTime100) / 100f > gameEvent.GetComponent<AudioSource>().clip.length)
            {
                noteLength100 = (int)((gameEvent.GetComponent<AudioSource>().clip.length - focusTime100 / 100f) * 100);
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
    
    // Bpm
    public void NewBpm()
    {
        NewBpm((int)(gameEvent.time * 100), 120);
    }

    public void NewBpm(int time100, int bpm)
    {
        GameObject obj = Instantiate(bpmPrefab, bpmParent.transform);
        obj.GetComponent<BpmData>().DefaultSettings(time100 / 100f, bpm);
        obj.SetActive(true);
        bpms.Add(obj, new Bpms(time100, bpm));
        
        SetDisChoose();
        focusNote = obj;
        focusNote.GetComponent<BpmData>().Choose();
        noteOrBpm = false;
        SetChoose();
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
        
        notesController.MeasureLineSet(bpms);
    }

    public void BpmSet()
    {
        BpmSet(int.Parse(bpmField.text));
    }

    public void BpmSet(int bpm)
    {
        bpmBpm = Math.Max(0, Math.Min(900, bpm));
        bpmField.text = bpmBpm.ToString();

        if (focusNote != null)
        {
            bpms[focusNote].SetBpm(bpmBpm);
            focusNote.GetComponent<BpmData>().ChangeBpm(bpmBpm);
            
            notesController.MeasureLineSet(bpms);
        }
    }
}
