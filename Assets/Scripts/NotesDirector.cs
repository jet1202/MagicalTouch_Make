using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private UserIO userIO;
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
    [SerializeField] private GameObject slideObj;
    [SerializeField] private GameObject maintainObj;
    [SerializeField] private GameObject fieldObj;

    [SerializeField] private GameObject speedObj;
    [SerializeField] private GameObject angleObj;
    
    public GameObject focusNote = null;
    public int objectKind; // 0 note 1 bpm 2 slide 3 slideMaintain 4 speeds 5 angles
    
    private int focusTime;
    
    private int noteLaneF;
    private int noteLaneL;
    private int noteKind;
    private int noteLength;
    private float bpmBpm;
    
    private float rayDistance = 30f;
    
    public Dictionary<GameObject, Bpm> bpms = new Dictionary<GameObject, Bpm>();

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
        if (objectKind == 0) // note
        {
            // focusNoteのデータを取り出し表示する
            Note n = focusNote.GetComponent<NotesData>().note;
            userIO.NoteTimeOutput(n.GetTime() / 1000f); 
            userIO.NoteLaneFOutput(n.GetStartLane());
            userIO.NoteLaneLOutput(n.GetEndLane());
            userIO.NoteKindDropdownOutput(NoteKindToInt(n.GetKind()));
            userIO.NoteLengthOutput(n.GetLength() / 1000f);
            userIO.NoteFieldDropdownOutput(n.GetField());

            if (n.GetKind() == 'L')
            {
                lengthObj.SetActive(true);
            }
            else
            {
                lengthObj.SetActive(false);
            }
            
            laneObj.SetActive(true);
            kindObj.SetActive(true);
            bpmObj.SetActive(false);
            slideObj.SetActive(false);
            maintainObj.SetActive(false);
            
            fieldObj.SetActive(true);
        }
        else if (objectKind == 2) // slide
        {
            SlideData data = focusNote.GetComponent<SlideData>();
            Note n = data.note;
            userIO.NoteTimeOutput(n.GetTime() / 1000f);
            userIO.NoteLaneFOutput(n.GetStartLane());
            userIO.NoteLaneLOutput(n.GetEndLane());
            userIO.NoteFieldDropdownOutput(n.GetField());
            userIO.IsDummyToggleOutput(data.isDummy);
            userIO.SlideFieldColorDropdownOutput(data.fieldColor);
            
            lengthObj.SetActive(false);
            laneObj.SetActive(true);
            kindObj.SetActive(false);
            bpmObj.SetActive(false);
            slideObj.SetActive(true);
            maintainObj.SetActive(false);
            fieldObj.SetActive(true);
        }
        else if (objectKind == 3) // slideMaintain
        {
            SlideMaintain s = focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[focusNote];
            userIO.NoteTimeOutput((s.time + focusNote.GetComponent<SlideMaintainData>().parentSc.note.GetTime()) / 1000f);
            userIO.NoteLaneFOutput(s.startLane);
            userIO.NoteLaneLOutput(s.endLane);
            userIO.IsJudgeToggleOutput(s.isJudge);
            userIO.IsVariationToggleOutput(s.isVariation);
            
            lengthObj.SetActive(false);
            laneObj.SetActive(false);
            kindObj.SetActive(false);
            bpmObj.SetActive(false);
            slideObj.SetActive(false);
            maintainObj.SetActive(true);
            fieldObj.SetActive(false);
        }
        else if (objectKind == 1) // bpm
        {
            Bpm b = bpms[focusNote];
            userIO.NoteTimeOutput(b.GetTime() / 1000f);
            userIO.BpmOutput(b.GetBpm());
            
            lengthObj.SetActive(false);
            laneObj.SetActive(false);
            kindObj.SetActive(false);
            bpmObj.SetActive(true);
            slideObj.SetActive(false);
            maintainObj.SetActive(false);
            fieldObj.SetActive(false);
        }
        else if (objectKind == 4) // speed
        {
            Speed s = speedsDirector.fieldSpeeds[focusNote];
            userIO.SpeedTimeOutput(s.GetTime() / 1000f);
            userIO.SpeedSpeedOutput(s.GetSpeed100() / 100f);
            userIO.SpeedIsVariationToggleOutput(s.GetIsVariation());
            
            speedObj.SetActive(true);
            angleObj.SetActive(false);
        }
        else if (objectKind == 5) // angle
        {
            Angle a = anglesDirector.fieldAngles[focusNote];
            userIO.SpeedTimeOutput(a.GetTime() / 1000f);
            userIO.AngleDegreeOutput(a.GetDegree());
            userIO.AngleVariationOutput(a.GetVariation() / 10f);
            
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
    
    public void TimeSet(float cTime)
    {
        cTime = Math.Min(gameEvent.GetComponent<AudioSource>().clip.length, Math.Max(cTime, 0f));
        focusTime = (int)(cTime * 1000);
        userIO.NoteTimeOutput(focusTime / 1000f);
        
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
                userIO.NoteTimeOutput(focusTime / 1000f);
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
    
    public void NoteLaneSet(int start, int end)
    {
        noteLaneF = start;
        noteLaneL = end;
        
        noteLaneF = Math.Min(Math.Max(0, noteLaneF), 11);
        noteLaneL = Math.Max(Math.Min(12, noteLaneL), noteLaneF + 1);
        
        userIO.NoteLaneFOutput(noteLaneF);
        userIO.NoteLaneLOutput(noteLaneL);
        
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

    public void NoteKindSet(int value)
    {
        noteKind = value;
        if (focusNote != null)
        {
            focusNote.GetComponent<NotesData>().ChangeKind(NoteKindToChar(noteKind));
        }
        
        if (focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
            lengthObj.SetActive(true);
        else
            lengthObj.SetActive(false);
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
            
            userIO.NoteLengthOutput(noteLength / 1000f);
            focusNote.GetComponent<NotesData>().ChangeLength(noteLength);
        }
    }

    public void NoteFieldSet(int value)
    {
        if (objectKind == 0)
        {
            focusNote.GetComponent<NotesData>().ChangeField(value);
            focusNote.transform.GetChild(3).GetComponent<SpriteRenderer>().color = FieldColor(value);
        }
        else if (objectKind == 2)
        {
            focusNote.GetComponent<SlideData>().ChangeField(value);
            focusNote.transform.GetChild(3).GetComponent<SpriteRenderer>().color = FieldColor(value);
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
        NewBpm((int)(gameEvent.time * 1000), 120f);
    }

    public void NewBpm(int time, float bpm)
    {
        GameObject obj = Instantiate(bpmPrefab, bpmParent.transform);
        obj.GetComponent<BpmData>().DefaultSettings(time / 1000f, bpm);
        obj.SetActive(true);
        bpms.Add(obj, new Bpm(time, (int)(bpm * 1000)));
        
        SetDisChoose();
        focusNote = obj;
        focusNote.GetComponent<BpmData>().Choose();
        objectKind = 1;
        SetChoose();
        gameEvent.nowBeatNote = -1;
        gameEvent.nowBeatLong = -1;
        
        notesController.MeasureLineSet(bpms);
    }

    public void BpmSet(float bpm)
    {
        bpmBpm = Math.Max(0f, Math.Min(1000f, bpm));
        userIO.BpmOutput(bpmBpm);

        if (focusNote != null)
        {
            bpms[focusNote].SetBpm((int)(bpmBpm * 1000f));
            focusNote.GetComponent<BpmData>().ChangeBpm((int)(bpmBpm * 1000f));
            
            notesController.MeasureLineSet(bpms);
        }
    }

    // slide
    public void NewSlide()
    {
        if (focusNote == null || objectKind != 2 && objectKind != 3)
            NewSlide((int)(gameEvent.time * 1000), 5, 7, 0, false, 0, Array.Empty<SlideMaintain>());
        else if (objectKind == 2)
            NewSlideMaintain((int)(gameEvent.time * 1000) - focusNote.GetComponent<SlideData>().note.GetTime(), 5, 7, true, true);
        else
            NewSlideMaintain((int)(gameEvent.time * 1000) - focusNote.GetComponent<SlideMaintainData>().parentSc.note.GetTime(), 5, 7, true, true);
    }

    public void NewSlide(int time, int start, int end, int field, bool isDummy, int color, SlideMaintain[] maintain)
    {
        GameObject obj = Instantiate(slidePrefab, noteParent.transform);
        obj.GetComponent<SlideData>().note = new Note(time, start, end, 'S', 0, field);
        obj.GetComponent<SlideData>().isDummy = isDummy;
        obj.GetComponent<SlideData>().fieldColor = color;
        obj.GetComponent<SlideData>().DefaultSettings(FieldColor(field), gameEvent.isNoteColor);
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

    public void SetDummy(bool dummy)
    {
        if (objectKind != 2 || focusNote == null)
            return;
        
        focusNote.GetComponent<SlideData>().ChangeDummy(dummy);
    }
    
    public void SetColor(int value)
    {
        if (objectKind != 2 || focusNote == null)
            return;
        
        focusNote.GetComponent<SlideData>().ChangeColor(value);
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

    public void SetJudge(bool judge)
    {
        if (objectKind != 3 || focusNote == null)
            return;

        focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[focusNote].isJudge = judge;
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

    public void SetSpeedTime(float time)
    {
        float cTime = Math.Clamp(time, 0f, gameEvent.GetComponent<AudioSource>().clip.length);
        focusTime = (int)(cTime * 1000);
        userIO.SpeedTimeOutput(focusTime / 1000f);
        
        if (objectKind == 4)
            speedsDirector.SetTime(focusNote, focusTime);
        else if (objectKind == 5)
            anglesDirector.SetTime(focusNote, focusTime);
    }

    public void SetSpeedSpeed(float speed)
    {
        float cSpeed = Math.Clamp(speed, -1000f, 1000f);
        int speed100 = (int)(cSpeed * 100);
        userIO.SpeedSpeedOutput(speed100 / 100f);
        
        speedsDirector.SetSpeed(focusNote, speed100);
    }

    public void SetSpeedIsVariation(bool isVariation)
    {
        speedsDirector.SetIsVariation(focusNote, isVariation);
    }

    public void SetAngleDegree(int degree)
    {
        anglesDirector.SetDegree(focusNote, degree);
    }

    public void SetAngleVariation(int variation)
    {
        if (Math.Abs(variation) < 10 && variation != 0)
            variation = 0;
        userIO.AngleVariationOutput(variation / 10f);
        anglesDirector.SetVariation(focusNote, variation);
    }

    public void SetSpeedFieldDropdown(int value)
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
