using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private UserIO userIO;
    [SerializeField] private NotesController notesController;
    [SerializeField] private FieldSettingController fieldSettingController;
    [SerializeField] private Speeds speedsDirector;
    [SerializeField] private Angles anglesDirector;
    [SerializeField] private Transparencies alphaDirector;
    
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
    [SerializeField] private GameObject transparencyObj;

    [SerializeField] private GameObject selectRectangleObj;
    
    public List<KeyValuePair<int, GameObject>> focusNotes = new List<KeyValuePair<int, GameObject>>();
    // 0 note 1 bpm 2 slide 3 slideMaintain 4 speeds 5 angles 6 transparencies
    
    private int focusTime;
    
    private int noteLaneF;
    private int noteLaneL;
    private int noteKind;
    private int noteLength;
    private int bpmBpm;
    private int longSplit = 8;
    
    private float rayDistance = 30f;
    private Vector2 downMousePos = new Vector2();
    bool isDrag = false;
    private bool isClick = false;
    
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
                RaycastHit2D[] hs = Physics2D.RaycastAll((Vector2)ray.origin, (Vector2)ray.direction, rayDistance);
                foreach (var h in hs)
                {
                    if (h.transform.CompareTag("EditRange"))
                        isClick = true;
                }
                if (isClick)
                {
                    downMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    isDrag = false;
                }
                
            }

            if (Input.GetMouseButton(0))
            {
                if (isClick)
                {
                    Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (isDrag)
                    {
                        Vector2 center = (downMousePos + pos) / 2;
                        float width = Math.Abs(downMousePos.x - pos.x);
                        float height = Math.Abs(downMousePos.y - pos.y);
                        selectRectangleObj.transform.position = new Vector3(center.x, center.y, 0);
                        selectRectangleObj.transform.localScale = new Vector3(width, height, 1);
                    }
                    else
                    {
                        if (Vector2.Distance(downMousePos, pos) > 0.3f)
                        {
                            isDrag = true;
                            selectRectangleObj.SetActive(true);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (isClick)
                {
                    GameObject[] hits;
                    if (isDrag)
                    {
                        selectRectangleObj.SetActive(false);

                        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        Vector2 lu = new Vector2(Math.Min(downMousePos.x, pos.x), Math.Max(downMousePos.y, pos.y));
                        Vector2 rd = new Vector2(Math.Max(downMousePos.x, pos.x), Math.Min(downMousePos.y, pos.y));

                        bool isUp = true;
                        List<RaycastHit2D> hitList = new List<RaycastHit2D>();
                        for (float i = lu.y; isUp; i -= 0.5f)
                        {
                            if (rd.y >= i)
                            {
                                isUp = false;
                                i = rd.y;
                            }

                            foreach (var hit in Physics2D.LinecastAll(new Vector2(lu.x, i), new Vector2(rd.x, i)))
                            {
                                if (!hitList.Contains(hit))
                                    hitList.Add(hit);
                            }
                        }

                        hits = new GameObject[hitList.Count];
                        for (int i = 0; i < hitList.Count; i++)
                            hits[i] = hitList[i].collider.gameObject;
                    }
                    else
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        hits = new GameObject[1]
                            { Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction, rayDistance).collider.gameObject };
                    }

                    int mode = 0;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        mode += 1;
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                        mode += 2;

                    if (mode == 0) ClearFocus();
                    
                    foreach (var hit in hits)
                    {
                        int k = GetObjKind(hit);
                        if (k != -1)
                            ClickResponse(mode, new KeyValuePair<int, GameObject>(k, hit), isDrag);
                    }

                    isDrag = false;
                    isClick = false;
                }
            }

            // 上下キー操作
            int key = 0;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                key--;
            if (Input.GetKeyDown(KeyCode.DownArrow))
                key++;

            if (key != 0)
            {
                bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                int ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 1 : 2;
                int k;
                GameObject obj;
                int start, end;
                foreach (var kv in focusNotes)
                {
                    k = kv.Key;
                    obj = kv.Value;
                    
                    if (k == 0)
                    {
                        var data = obj.GetComponent<NotesData>().note;
                        start = data.GetStartLane();
                        end = data.GetEndLane();
                    }
                    else if (k == 2)
                    {
                        var data = obj.GetComponent<SlideData>().note;
                        start = data.GetStartLane();
                        end = data.GetEndLane();
                    }
                    else if (k == 3)
                    {
                        var data = obj.GetComponent<SlideMaintainData>().parentSc.slideMaintain[obj];
                        start = data.startLane;
                        end = data.endLane;
                    }
                    else
                        return;

                    NoteLaneSet(kv, start + key * ctrl, end + (isShift ? 0 : key) * ctrl);
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                int k;
                GameObject obj;
                foreach (var kv in focusNotes)
                {
                    k = kv.Key;
                    obj = kv.Value;
                    if (k == 0)
                    {
                        obj.GetComponent<NotesData>().ClearNote();
                    }
                    else if (k == 2)
                    {
                        obj.GetComponent<SlideData>().ClearNote();
                    }
                    else if (k == 3)
                    {
                        obj.GetComponent<SlideMaintainData>().Clear();
                    }
                    else if (k == 1)
                    {
                        if (bpms.Count != 1)
                        {
                            obj.GetComponent<BpmData>().ClearBpm();
                            bpms.Remove(obj);
                            notesController.MeasureLineSet(bpms);
                        }
                    }
                    else if (k == 4)
                    {
                        speedsDirector.DeleteSpeeds(obj);
                    }
                    else if (k == 5)
                    {
                        anglesDirector.DeleteAngles(obj);
                    }
                    else if (k == 6)
                    {
                        alphaDirector.DeleteTransparencies(obj);
                    }
                    
                }
                ClearFocus();
            }
            
            if (Input.GetKeyDown(KeyCode.M))
            {
                int k;
                GameObject obj;
                int start, end;
                foreach (var kv in focusNotes)
                {
                    k = kv.Key;
                    obj = kv.Value;
                    
                    if (k == 0)
                    {
                        var data = obj.GetComponent<NotesData>().note;
                        start = data.GetStartLane();
                        end = data.GetEndLane();
                    }
                    else if (k == 2)
                    {
                        var data = obj.GetComponent<SlideData>().note;
                        start = data.GetStartLane();
                        end = data.GetEndLane();
                    }
                    else if (k == 3)
                    {
                        var data = obj.GetComponent<SlideMaintainData>().parentSc.slideMaintain[obj];
                        start = data.startLane;
                        end = data.endLane;
                    }
                    else
                        return;
                    
                    NoteLaneSet(kv, 12-end, 12-start);
                }
            }
        }
    }

    private void ClickResponse(int mode, KeyValuePair<int, GameObject> kv, bool isDrag)
    {
        switch (mode)
        {
            case 0:
                // Normal
                AddObj(kv);
                break;
            case 1:
                // Shift
                if (GetFocusNotesIndex(kv.Value) == -1 || isDrag)
                    AddObj(kv);
                else
                    CoreObj(kv);
                break;
            case 2:
                // Ctrl
                if (GetFocusNotesIndex(kv.Value) == -1 || isDrag)
                    ;
                else
                    DeleteObj(kv);
                break;
            case 3:
                // Shift + Ctrl
                if (GetFocusNotesIndex(kv.Value) == -1 && !isDrag) ;
                else ;
                break;
        }
    }

    public void ClearFocus()
    {
        for (int i = focusNotes.Count - 1; i >= 0; i--)
        {
            DeleteObj(focusNotes[i]);
        }
        gameEvent.FocusBeatSet(gameEvent.time);
    }

    public void CoreObj(KeyValuePair<int, GameObject> kv)
    {
        int index = GetFocusNotesIndex(kv.Value);
        if (index == -1)
        {
            focusNotes.Insert(0, kv);
        }
        else
        {
            focusNotes.RemoveAt(index);
            focusNotes.Insert(0, kv);
        }
        CorePresent();
    }
    
    public void AddObj(KeyValuePair<int, GameObject> kv)
    {
        int index = GetFocusNotesIndex(kv.Value);
        if (index == -1)
        {
            focusNotes.Add(kv);
            SetChoose(kv, false);
            if (focusNotes.Count == 1)
                CorePresent();
        }
    }
    
    public void DeleteObj(KeyValuePair<int, GameObject> kv)
    {
        int index = GetFocusNotesIndex(kv.Value);
        if (index != -1)
        {
            focusNotes.RemoveAt(index);
            SetDisChoose(kv);
            if (index == 0)
                CorePresent();
        }
    }

    private int GetObjKind(GameObject obj)
    {
        int k = -1;
        if (IsNote(obj.tag))
            k = 0;
        else if (obj.CompareTag("Bpm"))
            k = 1;
        else if (obj.CompareTag("Slide"))
            k = 2;
        else if (obj.CompareTag("SlideMaintain"))
            k = 3;
        else if (obj.CompareTag("SpeedPoint"))
            k = 4;
        else if (obj.CompareTag("AnglePoint"))
            k = 5;
        else if (obj.CompareTag("AlphaPoint"))
            k = 6;

        return k;
    }

    private int GetFocusNotesIndex(GameObject obj)
    {
        if (obj == null) return -1;
        
        int index = -1;
        for (int i = 0; i < focusNotes.Count; i++)
        {
            if (focusNotes[i].Value == obj)
            {
                index = i;
                break;
            }
        }

        return index;
    }

    private void SetChoose(KeyValuePair<int, GameObject> kv, bool isCore)
    {
        switch (kv.Key)
        {
            case 0:
                kv.Value.GetComponent<NotesData>().Choose(isCore);
                break;
            case 1:
                kv.Value.GetComponent<BpmData>().Choose(isCore);
                break;
            case 2:
                kv.Value.GetComponent<SlideData>().Choose(isCore);
                break;
            case 3:
                kv.Value.GetComponent<SlideMaintainData>().Choose(isCore);
                break;
            case 4:
                speedsDirector.SetChoose(kv.Value, isCore);
                break;
            case 5:
                anglesDirector.SetChoose(kv.Value, isCore);
                break;
            case 6:
                alphaDirector.SetChoose(kv.Value, isCore);
                break;
        }
    }

    private void SetDisChoose(KeyValuePair<int, GameObject> kv)
    {
        switch (kv.Key)
        {
            case 0:
                kv.Value.GetComponent<NotesData>().DisChoose();
                break;
            case 1:
                kv.Value.GetComponent<BpmData>().DisChoose();
                break;
            case 2:
                kv.Value.GetComponent<SlideData>().DisChoose();
                break;
            case 3:
                kv.Value.GetComponent<SlideMaintainData>().DisChoose();
                break;
            case 4:
                speedsDirector.SetDisChoose(kv.Value);
                break;
            case 5:
                anglesDirector.SetDisChoose(kv.Value);
                break;
            case 6:
                alphaDirector.SetDisChoose(kv.Value);
                break;
        }
    }
    
    private void CorePresent()
    {
        if (focusNotes.Count == 0)
        {
            gameEvent.FocusBeatSet(gameEvent.time);
            return;
        }
        
        SetChoose(focusNotes[0], true);
        if (focusNotes.Count > 1)
            SetChoose(focusNotes[1], false);
        
        int i = focusNotes[0].Key;
        GameObject obj = focusNotes[0].Value;
        
        if (i == 0) // note
        {
            // focusNoteのデータを取り出し表示する
            NotesData data = obj.GetComponent<NotesData>();
            Note n = data.note;
            userIO.NoteTimeOutput(n.GetTime() / 1000f); 
            userIO.NoteLaneFOutput(n.GetStartLane());
            userIO.NoteLaneLOutput(n.GetEndLane());
            userIO.NoteKindDropdownOutput(NoteKindToInt(n.GetKind()));
            userIO.NoteLengthOutput(n.GetLength() / 1000f);
            userIO.NoteFieldDropdownOutput(n.GetField());
            gameEvent.FocusBeatSet(n.GetTime());

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
        else if (i == 2) // slide
        {
            SlideData data = obj.GetComponent<SlideData>();
            Note n = data.note;
            userIO.NoteTimeOutput(n.GetTime() / 1000f);
            userIO.NoteLaneFOutput(n.GetStartLane());
            userIO.NoteLaneLOutput(n.GetEndLane());
            userIO.NoteFieldDropdownOutput(n.GetField());
            userIO.IsDummyToggleOutput(data.isDummy);
            userIO.SlideFieldColorDropdownOutput(data.fieldColor);
            gameEvent.FocusBeatSet(n.GetTime());
            
            lengthObj.SetActive(false);
            laneObj.SetActive(true);
            kindObj.SetActive(false);
            bpmObj.SetActive(false);
            slideObj.SetActive(true);
            maintainObj.SetActive(false);
            fieldObj.SetActive(true);
        }
        else if (i == 3) // slideMaintain
        {
            SlideMaintain s = obj.GetComponent<SlideMaintainData>().parentSc.slideMaintain[obj];
            userIO.NoteTimeOutput((s.time + obj.GetComponent<SlideMaintainData>().parentSc.note.GetTime()) / 1000f);
            userIO.NoteLaneFOutput(s.startLane);
            userIO.NoteLaneLOutput(s.endLane);
            userIO.IsJudgeToggleOutput(s.isJudge);
            userIO.IsVariationToggleOutput(s.isVariation);
            gameEvent.FocusBeatSet(s.time);
            
            lengthObj.SetActive(false);
            laneObj.SetActive(false);
            kindObj.SetActive(false);
            bpmObj.SetActive(false);
            slideObj.SetActive(false);
            maintainObj.SetActive(true);
            fieldObj.SetActive(false);
        }
        else if (i == 1) // bpm
        {
            Bpm b = bpms[obj];
            userIO.NoteTimeOutput(b.GetTime() / 1000f);
            userIO.BpmOutput(b.GetBpm() / 1000f);
            
            lengthObj.SetActive(false);
            laneObj.SetActive(false);
            kindObj.SetActive(false);
            bpmObj.SetActive(true);
            slideObj.SetActive(false);
            maintainObj.SetActive(false);
            fieldObj.SetActive(false);
        }
        else if (i == 4) // speed
        {
            Speed s = speedsDirector.fieldSpeeds[obj];
            userIO.SpeedTimeOutput(s.GetTime() / 1000f);
            userIO.SpeedSpeedOutput(s.GetSpeed100() / 100f);
            userIO.SpeedIsVariationToggleOutput(s.GetIsVariation());
            gameEvent.FocusBeatSet(s.GetTime());
            
            speedObj.SetActive(true);
            angleObj.SetActive(false);
            transparencyObj.SetActive(false);
        }
        else if (i == 5) // angle
        {
            Angle a = anglesDirector.fieldAngles[obj];
            userIO.SpeedTimeOutput(a.GetTime() / 1000f);
            userIO.AngleDegreeOutput(a.GetDegree());
            userIO.AngleVariationOutput(a.GetVariation() / 10f);
            gameEvent.FocusBeatSet(a.GetTime());
            
            speedObj.SetActive(false);
            angleObj.SetActive(true);
            transparencyObj.SetActive(false);
        }
        else if (i == 6) // Transparency
        {
            Transparency t = alphaDirector.fieldTransparencies[obj];
            userIO.SpeedTimeOutput(t.GetTime() / 1000f);
            userIO.TransparencyAlphaOutput(t.GetAlpha());
            userIO.TransparencyIsVariationToggleOutput(t.GetIsVariation());
            gameEvent.FocusBeatSet(t.GetTime());
            
            speedObj.SetActive(false);
            angleObj.SetActive(false);
            transparencyObj.SetActive(true);
        }
    }
    
    public void TimeSetAll(int cTime)
    {
        cTime = Math.Clamp(0, cTime, (int)(gameEvent.GetComponent<AudioSource>().clip.length * 1000));
        focusTime = cTime;

        foreach (var kv in focusNotes)
        {
            TimeSet(kv, focusTime);
        }
    }

    public void TimeSet(KeyValuePair<int, GameObject> kv, int time)
    {
        time = Math.Clamp(time, 0, (int)(gameEvent.GetComponent<AudioSource>().clip.length * 1000));
        
        if (kv.Key == 0)
        {
            kv.Value.GetComponent<NotesData>().ChangeTime(time);
        }
        else if (kv.Key == 1)
        {
            bpms[kv.Value].SetTime(time);
            kv.Value.GetComponent<BpmData>().ChangeTime(time / 1000f);
                
            notesController.MeasureLineSet(bpms);
        }
        else if (kv.Key == 2)
        {
            kv.Value.GetComponent<SlideData>().ChangeTime(time);
        }
        else if (kv.Key == 3)
        {
            int t = time - kv.Value.GetComponent<SlideMaintainData>().parentSc.note.GetTime();
            if (t < 0)
            {
                time -= t;
                t = 0;
            }
            kv.Value.GetComponent<SlideMaintainData>().SetTime(t);
        }
        else if (kv.Key == 4)
        {
            speedsDirector.SetTime(kv.Value, time);
        }
        else if (kv.Key == 5)
        {
            anglesDirector.SetTime(kv.Value, time);
        }
        else if (kv.Key == 6)
        {
            alphaDirector.SetTime(kv.Value, time);
        }

        if (focusNotes[0].Value == kv.Value)
        {
            userIO.NoteTimeOutput(time / 1000f);
            gameEvent.FocusBeatSet(time);
        }
    }
    
    // Note
    public void NewNote()
    {
        NewNote(gameEvent.time, 5, 7, 'N', 0, 0, false);
    }
    
    public void NewNote(int time, int start, int end, char kind, int length, int field, bool isAdd)
    {
        GameObject obj = Instantiate(notePrefab, noteParent.transform);
        obj.SetActive(true);
        obj.GetComponent<NotesData>().note = new Note(time, start, end, kind, length, field);
        obj.GetComponent<NotesData>().DefaultSettings(FieldColor(field), gameEvent.isNoteColor);
        
        if (!isAdd) ClearFocus();
        AddObj(new KeyValuePair<int, GameObject>(0, obj));
    }
    
    private bool IsNote(string _tag)
    {
        if (_tag == "Normal" || _tag == "Hold" || _tag == "Flick" || _tag == "Long")
            return true;
        else
            return false;
    }
    
    public void NoteLaneSetAll(int start, int end)
    {
        noteLaneF = start;
        noteLaneL = end;
        
        noteLaneF = Math.Min(Math.Max(0, noteLaneF), 23);
        noteLaneL = Math.Max(Math.Min(24, noteLaneL), noteLaneF + 1);

        foreach (var kv in focusNotes)
        {
            NoteLaneSet(kv, noteLaneF, noteLaneL);
        }
    }

    public void NoteLaneSet(KeyValuePair<int, GameObject> kv, int start, int end)
    {
        end = Math.Clamp(end, 1, 24);
        start = Math.Clamp(start, 0, end - 1);
        
        if (kv.Key == 0)
            kv.Value.GetComponent<NotesData>().ChangeLane(start, end);
        else if (kv.Key == 2)
            kv.Value.GetComponent<SlideData>().ChangeLane(start, end);
        else if (kv.Key == 3)
            kv.Value.GetComponent<SlideMaintainData>().SetLane(start, end);
        else
            return;

        if (focusNotes[0].Value == kv.Value)
        {
            userIO.NoteLaneFOutput(start);
            userIO.NoteLaneLOutput(end);
        }
    }

    public void NoteKindSetAll(int value)
    {
        noteKind = value;
        foreach (var kv in focusNotes)
        {
            NoteKindSet(kv, noteKind);
        }
    }

    public void NoteKindSet(KeyValuePair<int, GameObject> kv, int kind)
    {
        if (kv.Key != 0) return;
            
        kv.Value.GetComponent<NotesData>().ChangeKind(NoteKindToChar(kind));

        if (kv.Value.GetComponent<NotesData>().note.GetKind() == 'L')
            lengthObj.SetActive(true);
        else
            lengthObj.SetActive(false);
    }
    
    public void NoteLengthSetAll(int cLength)
    {
        noteLength = Math.Max(cLength, 0);
        foreach (var kv in focusNotes)
        {
            NoteLengthSet(kv, noteLength);
        }
    }

    public void NoteLengthSet(KeyValuePair<int, GameObject> kv, int len)
    {
        if (kv.Key != 0) return;
        if (kv.Value.GetComponent<NotesData>().note.GetKind() != 'L') return;
        
        int t = kv.Value.GetComponent<NotesData>().note.GetTime();
        if ((len + t) / 1000f > gameEvent.GetComponent<AudioSource>().clip.length)
        {
            len = (int)((gameEvent.GetComponent<AudioSource>().clip.length - t / 1000f) * 1000);
        }
        kv.Value.GetComponent<NotesData>().ChangeLength(len);

        if (focusNotes[0].Value == kv.Value)
        {
            userIO.NoteLengthOutput(len / 1000f);
            gameEvent.FocusBeatSet(t + len);
        }
    }

    public void NoteFieldSetAll(int value)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key == 0)
            {
                kv.Value.GetComponent<NotesData>().ChangeField(value);
                kv.Value.transform.GetChild(3).GetComponent<SpriteRenderer>().color = FieldColor(value);
                SetNoteColor(kv.Value.transform);
            }
            else if (kv.Key == 2)
            {
                kv.Value.GetComponent<SlideData>().ChangeField(value);
                kv.Value.transform.GetChild(3).GetComponent<SpriteRenderer>().color = FieldColor(value);
                kv.Value.GetComponent<SlideData>().ChangeColor();
            }
            
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
        NewBpm(gameEvent.time, 120f, false);
    }

    public void NewBpm(int time, float bpm, bool isAdd)
    {
        GameObject obj = Instantiate(bpmPrefab, bpmParent.transform);
        obj.GetComponent<BpmData>().DefaultSettings(time / 1000f, bpm);
        obj.SetActive(true);
        bpms.Add(obj, new Bpm(time, (int)(bpm * 1000)));
        
        if (!isAdd) ClearFocus();
        AddObj(new KeyValuePair<int, GameObject>(1, obj));
        
        notesController.MeasureLineSet(bpms);
    }

    public void BpmSetAll(int bpm)
    {
        bpmBpm = Math.Clamp(bpm, 1, 1000);

        foreach(var kv in focusNotes)
        {
            BpmSet(kv, bpmBpm);
        }
    }

    public void BpmSet(KeyValuePair<int, GameObject> kv, int bpm)
    {
        if (kv.Key != 1) return;
        
        bpms[kv.Value].SetBpm(bpm);
        kv.Value.GetComponent<BpmData>().ChangeBpm(bpm / 1000f);
        
        notesController.MeasureLineSet(bpms);
        
        if (focusNotes[0].Value == kv.Value)
            userIO.BpmOutput(bpm / 1000f);
    }

    // slide
    public void NewSlide()
    {
        if (focusNotes.Count == 0)
            NewSlide(gameEvent.time, 5, 7, 0, false, 0, Array.Empty<SlideMaintain>(), false);
        else
        {
            int t;
            if (focusNotes[0].Key == 2) 
                t = gameEvent.time - focusNotes[0].Value.GetComponent<SlideData>().note.GetTime();
            else if (focusNotes[0].Key == 3)
                t = gameEvent.time - focusNotes[0].Value.GetComponent<SlideMaintainData>().parentSc.note.GetTime();
            else
                return;
            
            NewSlideMaintain(t, 5, 7, true, true, focusNotes[0], false);
        }
    }

    public void NewSlide(int time, int start, int end, int field, bool isDummy, int color, SlideMaintain[] maintain, bool isAdd)
    {
        GameObject obj = Instantiate(slidePrefab, noteParent.transform);
        obj.GetComponent<SlideData>().note = new Note(time, start, end, 'S', 0, field);
        obj.GetComponent<SlideData>().isDummy = isDummy;
        obj.GetComponent<SlideData>().fieldColor = color;
        obj.GetComponent<SlideData>().DefaultSettings(FieldColor(field), gameEvent.isNoteColor);
        obj.SetActive(true);

        var kv = new KeyValuePair<int, GameObject>(2, obj);
        if (!isAdd) ClearFocus();
        AddObj(kv);
        
        foreach (var data in maintain)
        {
            SlideMaintain a = data;
            NewSlideMaintain(a.time, a.startLane, a.endLane, a.isJudge, a.isVariation, kv, isAdd);
        }
    }

    public void SetDummyAll(bool dummy)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 2)
                continue;
        
            kv.Value.GetComponent<SlideData>().ChangeDummy(dummy);
        }
    }
    
    public void SetColorAll(int value)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 2)
                continue;
        
            kv.Value.GetComponent<SlideData>().ChangeColor(value);
        }
    }
    
    // SlideMaintain
    public void NewSlideMaintain(int time, int start, int end, bool isJudge, bool isVariation, KeyValuePair<int, GameObject> kv, bool isAdd)
    {
        Transform pare;
        if (kv.Key == 2)
            pare = kv.Value.transform;
        else if (kv.Key == 3)
            pare = kv.Value.GetComponent<SlideMaintainData>().parent.transform;
        else
            return;

        GameObject obj = Instantiate(slideMaintainPrefab, noteParent.transform);

        SlideMaintain mt = new SlideMaintain();
        mt.time = Math.Max(0, time);
        mt.startLane = start;
        mt.endLane = end;
        mt.isJudge = isJudge;
        mt.isVariation = isVariation;
        pare.GetComponent<SlideData>().NewMaintain(obj, mt);
        
        obj.SetActive(true);
        
        if (!isAdd) ClearFocus();
        AddObj(new KeyValuePair<int, GameObject>(3, obj));
    }

    public void SetJudgeAll(bool judge)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 3)
                continue;

            kv.Value.GetComponent<SlideMaintainData>().parentSc.slideMaintain[kv.Value].isJudge = judge;
        }
    }

    public void SetVariationAll(bool variation)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 3)
                continue;

            kv.Value.GetComponent<SlideMaintainData>().parentSc.slideMaintain[kv.Value].isVariation = variation;
        }
    }

    public void NewSpeed()
    {
        int t = gameEvent.time;
        NewSpeed(t, speedsDirector.GetTimeToLastSpeed(t), false, false);
    }

    public void NewSpeed(int time, int speed100, bool isVariation, bool isAdd)
    {
        GameObject o = speedsDirector.NewSpeeds(time, speed100, isVariation);
        
        if (!isAdd) ClearFocus();
        AddObj(new KeyValuePair<int, GameObject>(4, o));
    }
    
    public void NewAngle()
    {
        int t = gameEvent.time;
        NewAngle(t, anglesDirector.GetTimeToLastAngle(t), 0, false);
    }

    public void NewAngle(int time, int degree, int variation, bool isAdd)
    {
        GameObject o = anglesDirector.NewAngles(time, degree, variation);
        
        if (!isAdd) ClearFocus();
        AddObj(new KeyValuePair<int, GameObject>(5, o));
    }
    
    public void NewTransparency()
    {
        int t = gameEvent.time;
        NewTransparency(t, alphaDirector.GetTimeToLastAlpha(t), false, false);
    }

    public void NewTransparency(int time, int alpha, bool isVariation, bool isAdd)
    {
        GameObject o = alphaDirector.NewTransparencies(time, alpha, isVariation);
        
        if (!isAdd) ClearFocus();
        AddObj(new KeyValuePair<int, GameObject>(6, o));
    }

    public void SetSpeedSpeedAll(float speed)
    {
        float cSpeed = Math.Clamp(speed, -10000f, 10000f);
        int speed100 = (int)(cSpeed * 100);
        userIO.SpeedSpeedOutput(speed100 / 100f);

        foreach (var kv in focusNotes)
        {
            if (kv.Key != 4)
                continue;
            
            speedsDirector.SetSpeed(kv.Value, speed100);
        }
    }

    public void SetSpeedIsVariationAll(bool isVariation)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 4)
                continue;
            
            speedsDirector.SetIsVariation(kv.Value, isVariation);
        }
    }

    public void SetAngleDegreeAll(int degree)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 5)
                continue;
            
            anglesDirector.SetDegree(kv.Value, degree);
        }
    }

    public void SetAngleVariationAll(int variation)
    {
        if (Math.Abs(variation) < 10)
            variation = 0;
        userIO.AngleVariationOutput(variation / 10f);
        
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 5)
                continue;
            
            anglesDirector.SetVariation(kv.Value, variation);
        }
    }
    
    public void SetTransparencyAlphaAll(int alpha)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 6)
                continue;
            
            alphaDirector.SetAlpha(kv.Value, alpha);
        }
    }
    
    public void SetTransparencyIsVariationAll(bool isVariation)
    {
        foreach (var kv in focusNotes)
        {
            if (kv.Key != 6)
                continue;
            
            alphaDirector.SetIsVariation(kv.Value, isVariation);
        }
    }

    public void SetSpeedFieldDropdown(int value)
    {
        speedsDirector.ChangeField(value);
        speedsDirector.SetColor(FieldColor(value));
        anglesDirector.SetColor(FieldColor(value));
        alphaDirector.SetColor(FieldColor(value));
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

    public void NoteColorSetting()
    {
        foreach (Transform t in noteParent.transform)
        {
            SetNoteColor(t);
        }
    }

    public void SetNoteColor(Transform t)
    {
        float alpha = gameEvent.noteAlpha / 100f;
        Note n;
        
        int type = 0;
        switch (t.tag)
        {
            case "Normal":
            case "Hold":
            case "Flick":
            case "Long":
                n = t.GetComponent<NotesData>().note;
                type = 1;
                break;
            case "Slide":
                n = t.GetComponent<SlideData>().note;
                type = 2;
                break;
            case "SlideMaintain":
                n = t.GetComponent<SlideMaintainData>().parentSc.note;
                type = 3;
                break;
            default:
                n = null;
                Debug.Log("null");
                break;
        }

        if (n == null) return;
        
        if (gameEvent.isDummyHide)
            t.gameObject.SetActive(!fieldSettingController.FieldIsDummy(n.GetField()));
        else
            t.gameObject.SetActive(true);
        
        Color cTmp;
        float a = fieldSettingController.FieldIsDummy(n.GetField()) ? alpha / 2f : alpha;
        switch (type)
        {
            case 1:
                t.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, a);
                cTmp = t.GetChild(2).GetComponent<SpriteRenderer>().color;
                cTmp = new Color(cTmp.r, cTmp.g, cTmp.b, a);
                t.GetChild(2).GetComponent<SpriteRenderer>().color = cTmp;
                break;
            case 2:
                t.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, a);
                cTmp = t.GetChild(2).GetComponent<LineRenderer>().startColor; 
                cTmp = new Color(cTmp.r, cTmp.g, cTmp.b, a); 
                t.GetChild(2).GetComponent<LineRenderer>().startColor = cTmp; 
                t.GetChild(2).GetComponent<LineRenderer>().endColor = cTmp; 
                break;
            case 3: 
                cTmp = t.GetChild(0).GetComponent<SpriteRenderer>().color; 
                cTmp = new Color(cTmp.r, cTmp.g, cTmp.b, a); 
                t.GetChild(0).GetComponent<SpriteRenderer>().color = cTmp; 
                break;
        }
    }

    public void LongNoteInsideSetting(bool isInside)
    {
        foreach (Transform t in noteParent.transform)
        {
            if (t.CompareTag("Long"))
            {
                t.GetChild(4).gameObject.SetActive(isInside);
            }
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

    public float[] GetLongInsideTime(int startTime, int length)
    {
        List<float> insideTime = new List<float>();
        if (length <= 100) return insideTime.ToArray();
        
        int split = longSplit;

        int leng = notesController.bpmLines.Count;
        for (int i = 0; i < leng - 1; i++)
        {
            int s = notesController.bpmLines[i + 1] - notesController.bpmLines[i];
            for (int j = 0; j < longSplit; j++)
            {
                float t = (notesController.bpmLines[i] + (float)s / split * j) / 1000f;
                if (startTime / 1000f < t && t < (startTime + length - 100) / 1000f)
                    insideTime.Add(t);
            }
        }
        insideTime.Add((startTime + length - 100) / 1000f);

        return insideTime.ToArray();
    }

    public int CountNotes()
    {
        int type = -1;
        int count = 0;
        int f = 0;
        foreach (Transform t in noteParent.transform)
        {
            switch (t.tag)
            {
                case "Normal":
                case "Hold":
                case "Flick":
                    type = 0;
                    f = t.GetComponent<NotesData>().note.GetField();
                    break;
                case "Long":
                    type = 1;
                    f = t.GetComponent<NotesData>().note.GetField();
                    break;
                case "Slide":
                    type = 2;
                    f = t.GetComponent<SlideData>().note.GetField();
                    break;
                case "SlideMaintain":
                    type = 3;
                    f = t.GetComponent<SlideMaintainData>().parentSc.note.GetField();
                    break;
            }
            
            if (fieldSettingController.FieldIsDummy(f))
                continue;
            
            if (type == 0)
                count++;
            else if (type == 1)
            {
                count++;
                count += t.GetChild(4).childCount;
            }
            else if (type == 2)
            {
                if (!t.GetComponent<SlideData>().isDummy)
                {
                    count++;

                    var sm = t.GetComponent<SlideData>().slideMaintain.Values;
                    foreach (var m in sm)
                    {
                        if (m.isJudge)
                            count++;
                    }
                }
            }
            else if (type == 3)
                ;
        }

        return count;
    }
}
