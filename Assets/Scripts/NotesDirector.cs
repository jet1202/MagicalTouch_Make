using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private NotesController notesController;
    [SerializeField] private Speeds speedsDirector;
    [SerializeField] private Angles anglesDirector;
    [SerializeField] private GameObject noteParent;
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private GameObject slidePrefab;
    [SerializeField] private GameObject slideMaintainPrefab;
    [SerializeField] private GameObject bpmParent;
    [SerializeField] private GameObject bpmPrefab;

    [SerializeField] public GameObject lengthObj;
    [SerializeField] public GameObject laneObj;
    [SerializeField] private GameObject kindObj;
    [SerializeField] private GameObject bpmObj;
    [SerializeField] private GameObject maintainObj;
    [SerializeField] private GameObject fieldObj;

    [SerializeField] private GameObject speedObj;
    [SerializeField] private GameObject angleObj;
    
    public GameObject focusNote = null;
    public int objectKind; // 0 note 1 bpm 2 slide 3 slideMaintain 4 speeds 5 angles

    [SerializeField] private TMP_InputField timeField;
    [SerializeField] private TMP_InputField laneFieldF;
    [SerializeField] private TMP_InputField laneFieldL;
    [SerializeField] private TMP_Dropdown kindDropdown;
    [SerializeField] private TMP_InputField lengthField;
    [SerializeField] private TMP_InputField bpmField;
    [SerializeField] private Toggle isJudgeToggle;
    [SerializeField] private Toggle isVariationToggle;
    [SerializeField] private TMP_Dropdown fieldDropdown;

    [SerializeField] private TMP_InputField speedTimeField;
    [SerializeField] private TMP_InputField speedSpeedField;
    [SerializeField] private Toggle speedIsVariationToggle;
    [SerializeField] private TMP_InputField angleDegreeField;
    [SerializeField] private TMP_InputField angleVariationField;
    
    private int focusTime;
    
    private int noteLaneF;
    private int noteLaneL;
    private int noteKind;
    private int noteLength;
    private int bpmBpm;
    
    private float rayDistance = 30f;
    
    public Dictionary<GameObject, Bpm> bpms = new Dictionary<GameObject, Bpm>();

    public bool isImporting = false;

    private void Update()
    {
        if (gameEvent.isEdit && EventSystem.current.currentSelectedGameObject == null)
        {
            // キーボード入力
            if (gameEvent.tabMode == 0)
            {
                if (Input.GetKeyDown(KeyCode.N)) NewNote();
                if (Input.GetKeyDown(KeyCode.B)) NewBpm();
            }

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
                        objectKind = 0;
                        SetChoose();
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                        gameEvent.FocusBeatSet(focusNote.GetComponent<NotesData>().note.GetTime() / 1000f);
                    }
                    else if (hit.collider.gameObject.CompareTag("Bpm"))
                    {
                        SetDisChoose();
                        focusNote = hit.collider.gameObject;
                        focusNote.GetComponent<BpmData>().Choose();
                        objectKind = 1;
                        SetChoose();
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                    }
                    else if (hit.collider.gameObject.CompareTag("Slide"))
                    {
                        SetDisChoose();
                        focusNote = hit.collider.gameObject;
                        focusNote.GetComponent<SlideData>().Choose();
                        objectKind = 2;
                        SetChoose();
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                        gameEvent.FocusBeatSet(focusNote.GetComponent<SlideData>().note.GetTime() / 1000f);
                    }
                    else if (hit.collider.gameObject.CompareTag("SlideMaintain"))
                    {
                        SetDisChoose();
                        focusNote = hit.collider.gameObject;
                        focusNote.GetComponent<SlideMaintainData>().Choose();
                        objectKind = 3;
                        SetChoose();
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                        gameEvent.FocusBeatSet(focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[focusNote].time / 1000f);
                    }
                    else if (hit.collider.gameObject.CompareTag("SpeedPoint"))
                    {
                        SetDisChoose();
                        focusNote = hit.collider.gameObject;
                        speedsDirector.SetChoose(focusNote);
                        objectKind = 4;
                        SetChoose();
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                        gameEvent.FocusBeatSet(speedsDirector.fieldSpeeds[focusNote].GetTime() / 1000f);
                    }
                    else if (hit.collider.gameObject.CompareTag("AnglePoint"))
                    {
                        SetDisChoose();
                        focusNote = hit.collider.gameObject;
                        anglesDirector.SetChoose(focusNote);
                        objectKind = 5;
                        SetChoose();
                        gameEvent.nowBeatNote = -1;
                        gameEvent.nowBeatLong = -1;
                        gameEvent.FocusBeatSet(anglesDirector.fieldAngles[focusNote].GetTime() / 1000f);
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
                if (focusNote != null && objectKind != 1 && objectKind != 4 && objectKind != 5)
                {
                    if (objectKind == 3)
                    {
                        SlideMaintain data;
                        data = focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[focusNote];
                        
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        {
                            NoteLaneSet(data.startLane - 1, data.endLane);
                        }
                        else
                        {
                            NoteLaneSet(data.startLane - 1, data.endLane - 1);
                        }
                    }
                    else
                    {
                        Note data;
                        if (objectKind == 0)
                            data = focusNote.GetComponent<NotesData>().note;
                        else
                            data = focusNote.GetComponent<SlideData>().note;

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
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (focusNote != null && objectKind != 1 && objectKind != 4 && objectKind != 5)
                {
                    if (objectKind == 3)
                    {
                        SlideMaintain data;
                        data = focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[focusNote];
                        
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        {
                            NoteLaneSet(data.startLane + 1, data.endLane);
                        }
                        else
                        {
                            NoteLaneSet(data.startLane + 1, data.endLane + 1);
                        }
                    }
                    else
                    {
                        Note data;
                        if (objectKind == 0)
                            data = focusNote.GetComponent<NotesData>().note;
                        else
                            data = focusNote.GetComponent<SlideData>().note;

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
            }

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (focusNote != null)
                {
                    if (objectKind == 0)
                    {
                        focusNote.GetComponent<NotesData>().ClearNote();
                        gameEvent.FocusBeatSet(gameEvent.time);
                    }
                    else if (objectKind == 2)
                    {
                        focusNote.GetComponent<SlideData>().ClearNote();
                        gameEvent.FocusBeatSet(gameEvent.time);
                    }
                    else if (objectKind == 3)
                    {
                        focusNote.GetComponent<SlideMaintainData>().Clear();
                    }
                    else if (objectKind == 1)
                    {
                        if (bpms.Count != 1)
                        {
                            focusNote.GetComponent<BpmData>().ClearBpm();
                            bpms.Remove(focusNote);
                            notesController.MeasureLineSet(bpms);
                            gameEvent.FocusBeatSet(gameEvent.time);
                        }
                    }
                    else if (objectKind == 4)
                    {
                        speedsDirector.DeleteSpeeds(focusNote);
                    }
                    else if (objectKind == 5)
                    {
                        anglesDirector.DeleteAngles(focusNote);
                    }

                    focusNote = null;
                }
            }
        }
    }
    
    private void SetChoose()
    {
        if (objectKind == 0)
        {
            // focusNoteのデータを取り出し表示する
            Note n = focusNote.GetComponent<NotesData>().note;
            timeField.text = (n.GetTime() / 1000f).ToString("F3");
            laneFieldF.text = n.GetStartLane().ToString();
            laneFieldL.text = n.GetEndLane().ToString();
            kindDropdown.value = NoteKindToInt(n.GetKind());
            lengthField.text = (n.GetLength() / 1000f).ToString("F3");
            fieldDropdown.value = n.GetField();

            if (n.GetKind() == 'L')
            {
                lengthObj.SetActive(true);
                laneObj.SetActive(true);
                kindObj.SetActive(true);
                bpmObj.SetActive(false);
                maintainObj.SetActive(false);
            }
            else
            {
                lengthObj.SetActive(false);
                laneObj.SetActive(true);
                kindObj.SetActive(true);
                bpmObj.SetActive(false);
                maintainObj.SetActive(false);
            }
            
            fieldObj.SetActive(true);
        }
        else if (objectKind == 2)
        {
            Note n = focusNote.GetComponent<SlideData>().note;
            timeField.text = (n.GetTime() / 1000f).ToString("F3");
            laneFieldF.text = n.GetStartLane().ToString();
            laneFieldL.text = n.GetEndLane().ToString();
            fieldDropdown.value = n.GetField();
            
            lengthObj.SetActive(false);
            laneObj.SetActive(true);
            kindObj.SetActive(false);
            bpmObj.SetActive(false);
            maintainObj.SetActive(false);
            fieldObj.SetActive(true);
        }
        else if (objectKind == 3)
        {
            SlideMaintain s = focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[focusNote];
            timeField.text = ((s.time + focusNote.GetComponent<SlideMaintainData>().parentSc.note.GetTime()) / 1000f).ToString("F3");
            laneFieldF.text = s.startLane.ToString();
            laneFieldL.text = s.endLane.ToString();
            isJudgeToggle.isOn = s.isJudge;
            isVariationToggle.isOn = s.isVariation;
            
            lengthObj.SetActive(false);
            laneObj.SetActive(false);
            kindObj.SetActive(false);
            bpmObj.SetActive(false);
            maintainObj.SetActive(true);
            fieldObj.SetActive(false);
        }
        else if (objectKind == 1)
        {
            Bpm b = bpms[focusNote];
            timeField.text = (b.GetTime() / 1000f).ToString("F3");
            bpmField.text = b.GetBpm().ToString();
            
            lengthObj.SetActive(false);
            laneObj.SetActive(false);
            kindObj.SetActive(false);
            bpmObj.SetActive(true);
            maintainObj.SetActive(false);
            fieldObj.SetActive(false);
        }
        else if (objectKind == 4)
        {
            Speed s = speedsDirector.fieldSpeeds[focusNote];
            speedTimeField.text = (s.GetTime() / 1000f).ToString("F3");
            speedSpeedField.text = (s.GetSpeed100() / 100f).ToString("F2");
            speedIsVariationToggle.isOn = s.GetIsVariation();
            
            speedObj.SetActive(true);
            angleObj.SetActive(false);
        }
        else if (objectKind == 5)
        {
            Angle a = anglesDirector.fieldAngles[focusNote];
            speedTimeField.text = (a.GetTime() / 1000f).ToString("F3");
            angleDegreeField.text = a.GetDegree().ToString();
            angleVariationField.text = (a.GetVariation() / 10f).ToString("F1");
            
            speedObj.SetActive(false);
            angleObj.SetActive(true);
        }
    }

    private void SetDisChoose()
    {
        if (focusNote == null) return;

        if (objectKind == 0)
        {
            focusNote.GetComponent<NotesData>().DisChoose();
        }
        else if (objectKind == 1)
        {
            focusNote.GetComponent<BpmData>().DisChoose();
        }
        else if (objectKind == 2)
        {
            focusNote.GetComponent<SlideData>().DisChoose();
        }
        else if (objectKind == 3)
        {
            focusNote.GetComponent<SlideMaintainData>().DisChoose();
        }
        else if (objectKind == 4)
        {
            speedsDirector.SetDisChoose(focusNote);
        }
        else if (objectKind == 5)
        {
            anglesDirector.SetDisChoose(focusNote);
        }
    }

    public void Deselect()
    {
        SetDisChoose();
        focusNote = null;
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
        gameEvent.FocusBeatSet(gameEvent.time);
    }
    
    public void TimeSet()
    {
        TimeSet(float.Parse(timeField.text));
    }
    
    public void TimeSet(float cTime)
    {
        cTime = Math.Min(gameEvent.GetComponent<AudioSource>().clip.length, Math.Max(cTime, 0f));
        focusTime = (int)(cTime * 1000);
        timeField.text = (focusTime / 1000f).ToString("F3");
        
        if (focusNote != null)
        {
            if (objectKind == 0)
            {
                focusNote.GetComponent<NotesData>().ChangeTime(focusTime);
            }
            else if (objectKind == 1)
            {
                bpms[focusNote].SetTime(focusTime);
                focusNote.GetComponent<BpmData>().ChangeTime(focusTime / 1000f);
                
                notesController.MeasureLineSet(bpms);
            }
            else if (objectKind == 2)
            {
                focusNote.GetComponent<SlideData>().ChangeTime(focusTime);
            }
            else if (objectKind == 3)
            {
                int t = focusTime - focusNote.GetComponent<SlideMaintainData>().parentSc.note.GetTime();
                if (t < 0)
                {
                    focusTime -= t;
                    t = 0;
                }
                focusNote.GetComponent<SlideMaintainData>().SetTime(t);
                timeField.text = (focusTime / 1000f).ToString("F3");
            }
            else if (objectKind == 4)
            {
                speedsDirector.SetTime(focusNote, focusTime);
            }
            else
            {
                anglesDirector.SetTime(focusNote, focusTime);
            }
        }
    }
    
    // Note
    public void NewNote()
    {
        NewNote((int)(gameEvent.time * 1000), 5, 7, 'N', 0, 0);
    }
    
    public void NewNote(int time, int start, int end, char kind, int length, int field)
    {
        GameObject obj = Instantiate(notePrefab, noteParent.transform);
        obj.SetActive(true);
        obj.GetComponent<NotesData>().note = new Note(time, start, end, kind, length, field);
        obj.GetComponent<NotesData>().DefaultSettings(FieldColor(field), gameEvent.isNoteColor);
        
        SetDisChoose();
        focusNote = obj;
        focusNote.GetComponent<NotesData>().Choose();
        objectKind = 0;
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
        if (isImporting) return;
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
            if (objectKind == 0)
                focusNote.GetComponent<NotesData>().ChangeLane(noteLaneF, noteLaneL);
            else if (objectKind == 2)
                focusNote.GetComponent<SlideData>().ChangeLane(noteLaneF, noteLaneL);
            else
                focusNote.GetComponent<SlideMaintainData>().SetLane(noteLaneF, noteLaneL);
        }
    }

    public void NoteKindSet()
    {
        if (isImporting) return;

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
        if (isImporting) return;

        NoteLengthSet(float.Parse(lengthField.text));
    }

    public void NoteLengthSet(float cLength)
    {
        noteLength = (int)(Math.Max(cLength, 0f) * 1000);
        if (focusNote != null && focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
        {
            if ((noteLength + focusTime) / 1000f > gameEvent.GetComponent<AudioSource>().clip.length)
            {
                noteLength = (int)((gameEvent.GetComponent<AudioSource>().clip.length - focusTime / 1000f) * 1000);
            }
            
            lengthField.text = (noteLength / 1000f).ToString();
            focusNote.GetComponent<NotesData>().ChangeLength(noteLength);
        }
    }

    public void NoteFieldSet()
    {
        if (isImporting) return;

        int v = fieldDropdown.value;
        if (objectKind == 0)
        {
            focusNote.GetComponent<NotesData>().ChangeField(v);
            focusNote.transform.GetChild(3).GetComponent<SpriteRenderer>().color = FieldColor(v);
        }
        else if (objectKind == 2)
        {
            focusNote.GetComponent<SlideData>().ChangeField(fieldDropdown.value);
            focusNote.transform.GetChild(3).GetComponent<SpriteRenderer>().color = FieldColor(v);
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
        NewBpm((int)(gameEvent.time * 1000), 120);
    }

    public void NewBpm(int time, int bpm)
    {
        GameObject obj = Instantiate(bpmPrefab, bpmParent.transform);
        obj.GetComponent<BpmData>().DefaultSettings(time / 1000f, bpm);
        obj.SetActive(true);
        bpms.Add(obj, new Bpm(time, bpm));
        
        SetDisChoose();
        focusNote = obj;
        focusNote.GetComponent<BpmData>().Choose();
        objectKind = 1;
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

    public void NewSlide()
    {
        if (focusNote == null || objectKind != 2 && objectKind != 3)
            NewSlide((int)(gameEvent.time * 1000), 5, 7, 0, Array.Empty<SlideMaintain>());
        else if (objectKind == 2)
            NewSlideMaintain((int)(gameEvent.time * 1000) - focusNote.GetComponent<SlideData>().note.GetTime(), 5, 7, true, true);
        else
            NewSlideMaintain((int)(gameEvent.time * 1000) - focusNote.GetComponent<SlideMaintainData>().parentSc.note.GetTime(), 5, 7, true, true);
    }

    public void NewSlide(int time, int start, int end, int sub, SlideMaintain[] maintain)
    {
        GameObject obj = Instantiate(slidePrefab, noteParent.transform);
        obj.GetComponent<SlideData>().note = new Note(time, start, end, 'S', 0, sub);
        obj.GetComponent<SlideData>().DefaultSettings(FieldColor(sub), gameEvent.isNoteColor);
        obj.SetActive(true);
        
        SetDisChoose();
        focusNote = obj;
        objectKind = 2;
        foreach (var data in maintain)
        {
            SlideMaintain a = data;
            NewSlideMaintain(a.time, a.startLane, a.endLane, a.isJudge, a.isVariation);
        }
        SetDisChoose();
        focusNote = obj;
        focusNote.GetComponent<SlideData>().Choose();
        objectKind = 2;
        SetChoose();
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
    }
    
    // SlideMaintain
    public void NewSlideMaintain(int time, int start, int end, bool isJudge, bool isVariation)
    {
        Transform pare;
        if (objectKind == 2)
            pare = focusNote.transform;
        else
            pare = focusNote.GetComponent<SlideMaintainData>().parent.transform;

        GameObject obj = Instantiate(slideMaintainPrefab, noteParent.transform);

        SlideMaintain mt = new SlideMaintain();
        mt.time = Math.Max(0, time);
        mt.startLane = start;
        mt.endLane = end;
        mt.isJudge = isJudge;
        mt.isVariation = isVariation;
        pare.GetComponent<SlideData>().NewMaintain(obj, mt);
        
        obj.SetActive(true);
        
        SetDisChoose();
        focusNote = obj;
        focusNote.GetComponent<SlideMaintainData>().Choose();
        objectKind = 3;
        SetChoose();
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
    }

    public void SetJudge()
    {
        SetJudge(isJudgeToggle.isOn);
    }

    public void SetJudge(bool judge)
    {
        if (objectKind != 3 || focusNote == null)
            return;

        focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[focusNote].isJudge = judge;
    }

    public void SetVariation()
    {
        SetVariation(isVariationToggle.isOn);
    }

    public void SetVariation(bool variation)
    {
        if (objectKind != 3 || focusNote == null)
            return;

        focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[focusNote].isVariation = variation;
    }

    public void NewSpeed()
    {
        GameObject o = speedsDirector.NewSpeeds((int)(gameEvent.time * 1000), 100, false);
        
        SetDisChoose();
        focusNote = o;
        speedsDirector.SetChoose(focusNote);
        objectKind = 4;
        SetChoose();
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
        gameEvent.FocusBeatSet(speedsDirector.fieldSpeeds[focusNote].GetTime() / 1000f);
    }

    public void NewAngle()
    {
        GameObject o = anglesDirector.NewAngles((int)(gameEvent.time * 1000), 0, 0);
        
        SetDisChoose();
        focusNote = o;
        anglesDirector.SetChoose(focusNote);
        objectKind = 5;
        SetChoose();
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
        gameEvent.FocusBeatSet(anglesDirector.fieldAngles[focusNote].GetTime() / 1000f);
    }

    public void SetSpeedTime(string timeS)
    {
        float time = float.Parse(timeS);
        float cTime = Math.Clamp(time, 0f, gameEvent.GetComponent<AudioSource>().clip.length);
        focusTime = (int)(cTime * 1000);
        speedTimeField.text = (focusTime / 1000f).ToString("F3");
        
        if (objectKind == 4)
            speedsDirector.SetTime(focusNote, focusTime);
        else if (objectKind == 5)
            anglesDirector.SetTime(focusNote, focusTime);
    }

    public void SetSpeedSpeed(string speedS)
    {
        float speed = float.Parse(speedS);
        float cSpeed = Math.Clamp(speed, -1000f, 1000f);
        int speed100 = (int)(cSpeed * 100);
        speedSpeedField.text = (speed100 / 100f).ToString("F2");
        
        speedsDirector.SetSpeed(focusNote, speed100);
    }

    public void SetSpeedIsVariation(bool isVariation)
    {
        speedsDirector.SetIsVariation(focusNote, isVariation);
    }

    public void SetAngleDegree(string degreeS)
    {
        int degree = int.Parse(degreeS);
        anglesDirector.SetDegree(focusNote, degree);
    }

    public void SetAngleVariation(string variationS)
    {
        int variation = (int)(float.Parse(variationS) * 10);
        if (Math.Abs(variation) < 10 && variation != 0)
            variation = 0;
        angleVariationField.text = (variation / 10f).ToString("F1");
        anglesDirector.SetVariation(focusNote, variation);
    }

    public void SetSpeedFieldDropDown(int value)
    {
        speedsDirector.ChangeField(value);
        speedsDirector.SetColor(FieldColor(value));
        anglesDirector.SetColor(FieldColor(value));
    }

    public void DeleteFieldNoteChange(int number)
    {
        foreach (Transform t in noteParent.transform)
        {
            if (t.CompareTag("SlideMaintain")) continue;
            
            Note note;
            if (t.CompareTag("Slide"))
                note = t.GetComponent<SlideData>().note;
            else
                note = t.GetComponent<NotesData>().note;

            int n;
            if (note.GetField() == number)
                n = 0;
            else if (note.GetField() > number)
                n = note.GetField() - 1;
            else
                n = note.GetField();

            if (t.CompareTag("Slide"))
                t.GetComponent<SlideData>().note.SetField(n);
            else
                t.GetComponent<NotesData>().note.SetField(n);

            t.GetChild(3).GetComponent<SpriteRenderer>().color = FieldColor(n);
        }
    }

    public void NoteFieldColorOn(bool isColor)
    {
        foreach (Transform t in noteParent.transform)
        {
            if (t.CompareTag("SlideMaintain")) continue;

            t.GetChild(3).gameObject.SetActive(isColor);
        }
    }

    public Color FieldColor(int number)
    {
        if (number == 0)
            return Color.white;

        float c;
        int spl = 2;
        int f = 3;
        if (number <= 3)
            c = number;
        else
        {
            number -= 3;
            while (true)
            {
                if (number <= f)
                {
                    c = (float)(number * 2 - 1) / spl;
                    break;
                }
                else
                {
                    number -= spl;
                    spl *= 2;
                    f *= 2;
                }
            }
        }
        
        float r = (55f + (Math.Abs(c - 2) > 1 ? 0 : 1 - Math.Abs(c - 2)) * 200f) / 255f;
        float g = (55f + (Math.Abs(c) > 1 ? (Math.Abs(c - 3) > 1 ? 0 : 1 - Math.Abs(c - 3)) : 1 - Math.Abs(c)) * 200f) /
                  255f;
        float b = (55f + (Math.Abs(c - 1) > 1 ? 0 : 1 - Math.Abs(c - 1)) * 200f) / 255f;

        Color color = new Color(r, g, b);
        return color;
    }
}
