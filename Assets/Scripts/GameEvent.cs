using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameEvent : MonoBehaviour
{
    [SerializeField] private Text title;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TMP_InputField musicSpeedField;

    [SerializeField] private GameObject noteButton;
    [SerializeField] private GameObject speedButton;
    [SerializeField] private GameObject noteTab;
    [SerializeField] private GameObject speedTab;

    [SerializeField] private Canvas settingCanvas;
    [SerializeField] private Canvas musicImportCanvas;
    [SerializeField] private Canvas dataImportCanvas;
    [SerializeField] private Canvas dataExportCanvas;
    [SerializeField] private Canvas noticeCanvas;

    [SerializeField] private GameObject notes;
    [SerializeField] private GameObject bpms;
    [SerializeField] private GameObject speeds;
    [SerializeField] private GameObject angles;
    [SerializeField] private GameObject transparencies;

    [SerializeField] private UserIO userIO;
    [SerializeField] private CenterDirector centerDirector;
    [SerializeField] private NotesController notesController;
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private Speeds speedsDirector;
    [SerializeField] private Angles anglesDirector;
    [SerializeField] private Transparencies alphaDirector;
    [SerializeField] private FieldSettingController fieldSettingController;
    [SerializeField] private LinePreview linePreview;
    
    public float time;
    public bool isPlaying;
    public float speed;
    public int split;
    private AudioSource audioSource;
    private string file;
    private string fileAddress = "";
    public bool isFileSet = false;
    public bool isOpenTab = false;
    public bool isEdit;
    public bool isNoteColor = false;
    public bool isDummyHide = false;
    public int noteAlpha = 100;
    public bool isLongInside = false;
    public int tabMode = 0; // 0 Note 1 Speed

    private float beatTime;
    private float mouseVal;
    public int focusBeat = 0;
    public int nowBeatTime = -1;
    public int nowBeatNote = -1;
    public int nowBeatLong = -1;
    public int timeSignature = 4;

    private void Start()
    {
        time = 0f;
        isPlaying = false;
        audioSource = GetComponent<AudioSource>();
        speed = 1.0f;
        userIO.SpeedOutput(speed);
        userIO.MusicSpeedOutput(1.0f);
        split = 1;
        userIO.SplitOutput(split);
        
        NoteTabSet();
    }
    
    private void Update()
    {
        if (isPlaying)
        {
            time = audioSource.time;
            userIO.TimeOutput(time);
            timeSlider.value = time;
            
            FocusBeatSet(time);
        }
        
        isEdit = !isPlaying && isFileSet && !isOpenTab;

        if (isEdit && EventSystem.current.currentSelectedGameObject == null)
        {
            // キーボード入力
            if (Input.GetKeyDown(KeyCode.Space)) PlayClick();

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                    notesDirector.focusNote != null && notesDirector.objectKind == 0)
                {
                    if (notesDirector.focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
                    {
                        nowBeatLong = NextBeat(false,
                            (notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() +
                            notesDirector.focusNote.GetComponent<NotesData>().note.GetLength()) / 1000f, nowBeatLong);

                        nowBeatLong = Mathf.Max(0, nowBeatLong);
                        float len = BeatToTime(nowBeatLong) -
                                    notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() / 1000f;
                        if (len < 0f) nowBeatLong++;
                        notesDirector.NoteLengthSet(len);
                        
                        nowBeatTime = -1;
                        nowBeatNote = -1;
                        FocusBeatSet((notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() +
                                     notesDirector.focusNote.GetComponent<NotesData>().note.GetLength()) / 1000f);
                    }
                }
                else
                {
                    if (notesDirector.focusNote == null)
                    {
                        nowBeatTime = NextBeat(false, time, nowBeatTime);
                        nowBeatTime = Mathf.Max(0, nowBeatTime);
                        ChangeTime(BeatToTime(nowBeatTime));
                        nowBeatNote = -1;
                        FocusBeatSet(time);
                    }
                    else
                    {
                        float t;
                        t = GetFocusTime();
                        
                        nowBeatNote = NextBeat(false, t, nowBeatNote);
                        nowBeatNote = Mathf.Max(0, nowBeatNote);
                        if (notesDirector.objectKind == 4)
                            notesDirector.SetSpeedTime(BeatToTime(nowBeatNote));
                        else
                            notesDirector.TimeSet(BeatToTime(nowBeatNote));
                        nowBeatTime = -1;
                        FocusBeatSet(BeatToTime(nowBeatNote));
                    }
                        
                    nowBeatLong = -1;
                }
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                    notesDirector.focusNote != null && notesDirector.objectKind == 0)
                {
                    if (notesDirector.focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
                    {
                        nowBeatLong = NextBeat(true,
                            (notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() +
                            notesDirector.focusNote.GetComponent<NotesData>().note.GetLength()) / 1000f, nowBeatLong);
                        
                        float len = BeatToTime(nowBeatLong) -
                                    notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() / 1000f;
                        if (len + notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() / 1000f >
                            audioSource.clip.length)
                            nowBeatLong--;
                        notesDirector.NoteLengthSet(len);
                        
                        nowBeatTime = -1;
                        nowBeatNote = -1;
                        FocusBeatSet((notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() +
                                     notesDirector.focusNote.GetComponent<NotesData>().note.GetLength()) / 1000f);
                    }
                }
                else
                {
                    if (notesDirector.focusNote == null)
                    {
                        nowBeatTime = NextBeat(true, time, nowBeatTime);
                        ChangeTime(Mathf.Min(BeatToTime(nowBeatTime), audioSource.clip.length));
                        nowBeatNote = -1;
                        FocusBeatSet(time);
                    }
                    else
                    {
                        float t;
                        t = GetFocusTime();

                        nowBeatNote = NextBeat(true, t, nowBeatNote);
                        if (notesDirector.objectKind == 4)
                            notesDirector.SetSpeedTime(
                                Mathf.Min(BeatToTime(nowBeatNote), audioSource.clip.length));
                        else
                            notesDirector.TimeSet(Mathf.Min(BeatToTime(nowBeatNote), audioSource.clip.length));
                        nowBeatTime = -1;
                        FocusBeatSet(Mathf.Min(BeatToTime(nowBeatNote), audioSource.clip.length));
                    }

                    nowBeatLong = -1;
                }
            }
            
            // スクロールで時間移動できる機能
            mouseVal = Input.GetAxis("Mouse ScrollWheel");
            if (mouseVal != 0 && !isOpenTab)
            {
                // スクロールしたときに、EditRange内にポインタがいれば移動
                bool isIn = false;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                foreach (RaycastHit2D hit in Physics2D.RaycastAll(ray.origin, ray.direction, 10f))
                {
                    if (hit.collider.gameObject.CompareTag("EditRange"))
                    {
                        isIn = true;
                        break;
                    }
                }
                if (isIn)
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        // Shiftを押していたらTimeとSpeedで移動
                        if (mouseVal > 0)
                        {
                            ChangeTime(time - 9f / speed);
                            nowBeatTime = -1;
                            FocusBeatSet(time);
                        }
                        else if (mouseVal < 0)
                        {
                            ChangeTime(time + 9f / speed);
                            nowBeatTime = -1;
                            FocusBeatSet(time);
                        }
                    }
                    else
                    {
                        // Shiftを押していなければBpmとSplitで移動
                        if (mouseVal > 0)
                        {
                            nowBeatTime = NextBeat(false, time, nowBeatTime);
                            nowBeatTime = Mathf.Max(0, nowBeatTime);
                            ChangeTime(BeatToTime(nowBeatTime));
                            FocusBeatSet(time);
                        }
                        else if (mouseVal < 0)
                        {
                            nowBeatTime = NextBeat(true, time, nowBeatTime);
                            ChangeTime(Mathf.Min(BeatToTime(nowBeatTime), audioSource.clip.length));
                            FocusBeatSet(time);
                        }
                    }
                }
            }
            


            // 複製機能の実装
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (notesDirector.focusNote != null)
                {
                    if (notesDirector.objectKind == 0)
                    {
                        Note data = notesDirector.focusNote.GetComponent<NotesData>().note;

                        float time;
                        time = data.GetTime() / 1000f;
                        nowBeatNote = NextBeat(true, time, nowBeatNote);
                        int n = nowBeatNote;
                        notesDirector.NewNote((int)(Mathf.Min(BeatToTime(nowBeatNote), audioSource.clip.length) * 1000),
                            data.GetStartLane(), data.GetEndLane(), data.GetKind(), data.GetLength(), data.GetField());
                        nowBeatNote = n;
                        nowBeatTime = -1;
                        FocusBeatSet(BeatToTime(nowBeatNote));
                        nowBeatLong = -1;
                    }
                    else if (notesDirector.objectKind == 2)
                    {
                        Note data = notesDirector.focusNote.GetComponent<SlideData>().note;
                        bool dummy = notesDirector.focusNote.GetComponent<SlideData>().isDummy;
                        int color = notesDirector.focusNote.GetComponent<SlideData>().fieldColor;

                        float time;
                        time = data.GetTime() / 1000f;
                        nowBeatNote = NextBeat(true, time, nowBeatNote);
                        int n = nowBeatNote;
                        notesDirector.NewSlide(
                            (int)(Mathf.Min(BeatToTime(nowBeatNote), audioSource.clip.length) * 1000),
                            data.GetStartLane(), data.GetEndLane(), data.GetField(), dummy, color,
                            notesDirector.focusNote.GetComponent<SlideData>().slideMaintain.Values.ToArray());
                        nowBeatNote = n;
                        nowBeatTime = -1;
                        FocusBeatSet(BeatToTime(nowBeatNote));
                        nowBeatLong = -1;
                    }
                }
            }
        }
        else if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (Input.GetKeyDown(KeyCode.Delete) && EventSystem.current.currentSelectedGameObject.CompareTag("FieldElement"))
            {
                int n = int.Parse(EventSystem.current.currentSelectedGameObject.name);
                if (n != 0)
                    fieldSettingController.DeleteField(n);
            }
        }

        if (isEdit)
        {
            // sliderの監視
            if (timeSlider.value.ToString("F2") != time.ToString("F2"))
            {
                ChangeTime(timeSlider.value);
                FocusBeatSet(time);
                nowBeatTime = -1;
            }
        }
        
        // フルスクリーンモード切り替え
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }

    private float GetFocusTime()
    {
        float t = 0;
        switch (notesDirector.objectKind)
        {
            case 0:
                t = notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() / 1000f;
                break;
            case 1:
                t = notesDirector.bpms[notesDirector.focusNote].GetTime() / 1000f;
                break;
            case 2:
                t = notesDirector.focusNote.GetComponent<SlideData>().note.GetTime() / 1000f;
                break;
            case 3:
                t = (notesDirector.focusNote.GetComponent<SlideMaintainData>().parentSc
                    .slideMaintain[notesDirector.focusNote].time + notesDirector.focusNote
                    .GetComponent<SlideMaintainData>().parentSc.note.GetTime()) / 1000f;
                break;
            case 4:
                // Speeds
                t = speedsDirector.fieldSpeeds[notesDirector.focusNote].GetTime() / 1000f;
                break;
            case 5:
                // angles
                t = anglesDirector.fieldAngles[notesDirector.focusNote].GetTime() / 1000f;
                break;
            case 6:
                // Transparencies
                t = alphaDirector.fieldTransparencies[notesDirector.focusNote].GetTime() / 1000f;
                break;
        }

        return t;
    }

    private int timeToBeat(float nowTime)
    {
        List<float> lines = notesController.bpmMeasureLines;
        lines.Sort();

        if (lines.Count < 3) return 0;

        int measure = 0;
        int b;
        for (int i = 1; i < lines.Count; i++)
        {
            measure = i - 1;
            
            if (lines[i] > nowTime)
            {
                break;
            }
        }
        
        float beatTime = (lines[measure + 1] - lines[measure]) / split;
        b = (int)((nowTime - lines[measure]) / beatTime);

        return measure * split + b;
    }

    private float BeatToTime(int beat)
    {
        List<float> lines = notesController.bpmMeasureLines;

        int measure = beat / split;
        int b = beat % split;

        if (measure < 0) return 0f;
        
        float beatTimeA = (lines[measure + 1] - lines[measure]) / split;

        return lines[measure] + beatTimeA * b;
    }

    private int NextBeat(bool arrow, float nowTime, int beat)
    {
        if (arrow)
        {
            if (beat == -1)
            {
                beat = timeToBeat(nowTime) + 1;
            }
            else
            {
                beat++;
            }
        }
        else
        {
            if (beat == -1)
            {
                beat = timeToBeat(nowTime);
            }
            else
            {
                beat--;
            }
        }

        return beat;
    }

    public void FocusBeatSet(float basis)
    {
        focusBeat = timeToBeat(basis);
    }
    

    public void SettingsClick()
    {
        // Settingsボタン
        settingCanvas.gameObject.SetActive(true);
        isOpenTab = true;
    }

    public void SplitSet(int value)
    {
        // SplitFieldの変更
        split = value;
        userIO.SplitOutput(split);
        nowBeatTime = -1;
        nowBeatNote = -1;
        nowBeatLong = -1;
        
        if (notesDirector.focusNote == null)
            FocusBeatSet(time);
        else
        {
            switch (notesDirector.objectKind)
            {
                case 0:
                    FocusBeatSet(notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() / 1000f);
                    break;
                case 1:
                    FocusBeatSet(notesDirector.bpms[notesDirector.focusNote].GetTime() / 1000f);
                    break;
                case 2:
                    FocusBeatSet(notesDirector.focusNote.GetComponent<SlideData>().note.GetTime() / 1000f);
                    break;
                case 3:
                    FocusBeatSet(notesDirector.focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[notesDirector.focusNote].time / 1000f);
                    break;
            }
        }
    }

    public void PlayClick()
    {
        // Playのクリック
        if (isFileSet)
        {
            isPlaying = !isPlaying;
            userIO.PlayButtonColor(isPlaying);
            if (isPlaying)
            {
                audioSource.time = time;
                centerDirector.StartPlayButton();
                if (audioSource.clip.length > time)
                    audioSource.Play();
                nowBeatTime = -1;
                nowBeatNote = -1;
                nowBeatLong = -1;
            }
            else
            {
                audioSource.Stop();
                time = audioSource.time;
            }
        }
        
    }

    public void TimeSet(float inTime)
    {
        // TimeFieldの変更
        ChangeTime(inTime);
        nowBeatTime = -1;
        nowBeatLong = -1;
    }

    public void NoteTabSet()
    {
        notes.SetActive(true);
        speeds.SetActive(false);
        angles.SetActive(false);
        transparencies.SetActive(false);
        noteTab.SetActive(true);
        speedTab.SetActive(false);
        noteButton.GetComponent<Image>().color = new Color(150f / 255f, 150f / 255f, 150f / 255f, 1f);
        speedButton.GetComponent<Image>().color = Color.white;
        notesDirector.Deselect();

        tabMode = 0;
    }

    public void SpeedTabSet()
    {
        notes.SetActive(false);
        speeds.SetActive(true);
        angles.SetActive(true);
        transparencies.SetActive(true);
        noteTab.SetActive(false);
        speedTab.SetActive(true);
        noteButton.GetComponent<Image>().color = Color.white;
        speedButton.GetComponent<Image>().color = new Color(150f / 255f, 150f / 255f, 150f / 255f, 1f);
        notesDirector.Deselect();

        tabMode = 1;
        
        speedsDirector.RenewalSpeed();
        anglesDirector.RenewalAngle();
        alphaDirector.RenewalAlpha();
    }

    public void SpeedSet(float inSpeed)
    {
        speed = inSpeed;
        if (speed < 1) speed = 1.0f;
        if (speed > 10) speed = 10.0f;
        userIO.SpeedOutput(speed);
        notesController.MeasureLineSet(notesDirector.bpms);
        
        if (notesDirector.focusNote == null)
            FocusBeatSet(time);
        else
        {
            switch (notesDirector.objectKind)
            {
                case 0:
                    FocusBeatSet(notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() / 100f);
                    break;
                case 1:
                    FocusBeatSet(notesDirector.bpms[notesDirector.focusNote].GetTime() / 1000f);
                    break;
                case 2:
                    FocusBeatSet(notesDirector.focusNote.GetComponent<SlideData>().note.GetTime() / 1000f);
                    break;
                case 3:
                    FocusBeatSet(notesDirector.focusNote.GetComponent<SlideMaintainData>().parentSc.slideMaintain[notesDirector.focusNote].time / 1000f);
                    break;
            }
        }
    }
    
    public void MusicSpeedSet(float musicSpeed)
    {
        // 曲の速度変更
        if (musicSpeed < 0.1f) musicSpeed = 0.1f;
        if (musicSpeed > 2.0f) musicSpeed = 2.0f; 
        userIO.MusicSpeedOutput(musicSpeed);
        audioSource.pitch = musicSpeed;
    }

    public void FieldColorSet(bool isColor)
    {
        notesDirector.NoteFieldColorOn(isColor);
        isNoteColor = isColor;
    }

    public void LinePreviewSet(bool isPreview)
    {
        linePreview.SetPreview(isPreview);
    }

    public void DummyHideSet(bool isHide)
    {
        isDummyHide = isHide;
        notesDirector.NoteColorSetting();
    }

    public void LongInsideSet(bool isInside)
    {
        isLongInside = isInside;
        notesDirector.LongNoteInsideSetting(isInside);
    }

    public void PreviewAlphaSet(bool isAlpha)
    {
        linePreview.isAlpha = isAlpha;
    }

    public void NoteAlphaSet(float alpha)
    {
        noteAlpha = (int)(alpha * 100);
        notesDirector.NoteColorSetting();
    }

    public void ImportMusicClick()
    {
        musicImportCanvas.gameObject.SetActive(true);
        isOpenTab = true;
    }

    public void ExportClick()
    {
        dataExportCanvas.gameObject.SetActive(true);
        isOpenTab = true;
    }

    public void ImportClick()
    {
        dataImportCanvas.gameObject.SetActive(true);
        isOpenTab = true;
    }

    public void InfoClick()
    {
        string len = audioSource.clip == null ? "Null" : $"{(int)audioSource.clip.length / 60}:{(int)audioSource.clip.length % 60}";
        string massage = $"Version : {UnityEngine.Device.Application.version}\n" +
                         $"URL : https://github.com/jet1202/MagicalTouch_Make/\n\n" +
                         $"MusicLength : {len}\n" +
                         $"The number of notes : {notesDirector.CountNotes()}\n";
        noticeCanvas.GetComponent<NoticeController>().OpenNotice(2, massage);
    }

    public void MusicImport(string path)
    {
        StartCoroutine(StreamLoadAudioFile(path));
        TabClose();
    }

    public void DataImport(string notePath, string bpmPath, string fieldPath)
    {

        if (!File.Exists(bpmPath))
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Import: ファイルが見つかりません");
            return;
        }

        try
        {
            if (bpmPath != "")
            {
                // bpmの入手
                BpmSave bpmData = new BpmSave();
                
                bpmData = ExportJson.ImportingBpm(bpmPath);

                foreach (Transform b in bpms.transform)
                {
                    b.GetComponent<BpmData>().ClearBpm();
                }
                notesDirector.bpms.Clear();

                foreach (BpmItem b in bpmData.bpmItem)
                {
                    notesDirector.NewBpm(b.time, b.bpm / 1000f);
                }
            }
            
            if (notePath != "")
            {
                // Sheetの入手
                Dictionary<int, Note> notesData;
                Dictionary<int, SlideSave> slidesData;
                
                notesData = ExportJson.ImportingSheet(notePath);
                slidesData = ExportJson.ImportingSlide();

                foreach (Transform t in notes.transform)
                {
                    t.GetComponent<NotesData>().ClearNote();
                }
                
                foreach (var n in notesData)
                {
                    if (n.Value.GetKind() == 'S')
                        notesDirector.NewSlide(n.Value.GetTime(), n.Value.GetStartLane(), n.Value.GetEndLane(),
                            n.Value.GetField(), slidesData[n.Key].isDummy, slidesData[n.Key].color,
                            slidesData[n.Key].item);
                    else
                        notesDirector.NewNote(n.Value.GetTime(), n.Value.GetStartLane(), n.Value.GetEndLane(),
                            n.Value.GetKind(), n.Value.GetLength(), n.Value.GetField());
                }
            }

            if (fieldPath != "")
            {
                // fieldの入手
                Field[] fieldData = ExportJson.ImportingField(fieldPath);

                List<List<Speed>> speedData = new List<List<Speed>>();
                List<List<Angle>> angleData = new List<List<Angle>>();
                List<List<Transparency>> transData = new List<List<Transparency>>();

                List<bool> isDummyData = new List<bool>();

                int i = 0;
                foreach (var f in fieldData)
                {
                    speedData.Add(new List<Speed>());
                    foreach (var s in f.speedItem)
                    {
                        speedData[i].Add(new Speed(s.time, s.speed, s.isVariation));
                    }
                    
                    angleData.Add(new List<Angle>());
                    foreach (var a in f.angleWork)
                    {
                        angleData[i].Add(new Angle(a.time, a.angle, a.variation));
                    }
                    
                    transData.Add(new List<Transparency>());
                    if (f.transparencyItem != null)
                    {
                        foreach (var t in f.transparencyItem)
                        {
                            transData[i].Add(new Transparency(t.time, t.alpha, t.isVariation));
                        }
                    }

                    isDummyData.Add(f.isDummy);
                    
                    i++;
                }

                speedsDirector.speedsData = speedData;
                speedsDirector.nowField = 0;
                speedsDirector.RenewalSpeed();

                anglesDirector.anglesData = angleData; 
                anglesDirector.RenewalAngle();
                
                alphaDirector.alphaData = transData;
                alphaDirector.RenewalAlpha();

                fieldSettingController.fieldsCount = i;
                fieldSettingController.fieldsIsDummy = isDummyData;
                fieldSettingController.RenewalField();
            }
        }
        catch (Exception e)
        {
             TabClose();
             noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, $"Import: {e}");
             return;
        }
        
        TabClose();
        noticeCanvas.GetComponent<NoticeController>().OpenNotice(0, "Import Finished.");
    }

    public void DataExport(string path)
    {
        if (!Directory.Exists(path))
        {
            TabClose();
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Export: ディレクトリを見つけられませんでした");
            return;
        }

        try
        {
            ExportJson.ExportingSheet(notes, path + "\\Note.json");
            ExportJson.ExportingBpm(path + "\\Bpm.json", new List<Bpm>(notesDirector.bpms.Values));
            ExportJson.ExportingField(path + "\\Field.json", fieldSettingController.fieldsCount,
                fieldSettingController.fieldsIsDummy, speedsDirector.speedsData, anglesDirector.anglesData,
                alphaDirector.alphaData);
            
            TabClose();
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(0, "Finish Export.");
        }
        catch (Exception e)
        {
            TabClose();
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, $"Export: {e}");
        }
    }

    public void TabClose()
    {
        settingCanvas.gameObject.SetActive(false);
        musicImportCanvas.gameObject.SetActive(false);
        dataExportCanvas.gameObject.SetActive(false);
        dataImportCanvas.gameObject.SetActive(false);
        isOpenTab = false;
    }

    public void NoticeClose()
    {
        noticeCanvas.GetComponent<NoticeController>().CloseNotice();
        isOpenTab = false;
    }

    private void ChangeTime(float cTime)
    {
        time = Mathf.Floor(cTime * 1000) / 1000f;
        timeSlider.value = time;
        userIO.TimeOutput(time);
    }

    IEnumerator StreamLoadAudioFile(string fileName)
    {
        audioSource.clip = null;
        isFileSet = false;
        
        if (audioSource == null)
            yield break;

        if (!File.Exists(fileName))
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Audio: File not found.");
            yield break;
        }

        string Extension = Path.GetExtension(fileName).ToLower();
        if (!(Extension == ".ogg" || Extension == ".mp3" || Extension == ".wav"))
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Audio: This file format is not supported");
            yield break;
        }
        
        string url = "file://" + fileName;
        var request = UnityWebRequestMultimedia.GetAudioClip(url, GetAudioType(url));

        ((DownloadHandlerAudioClip)request.downloadHandler).compressed = false;
        ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = false;

        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, $"Audio : {request.error}");
            yield break;
        }
        
        audioSource.clip = DownloadHandlerAudioClip.GetContent(request);

        file = Path.GetFileNameWithoutExtension(fileName);
        isFileSet = true;
        timeSlider.maxValue = audioSource.clip.length;
        title.text = file;

        fileAddress = fileName;
        noticeCanvas.GetComponent<NoticeController>().OpenNotice(0, "AudioLoad Finished.");
        
        notesDirector.NewBpm(0, 120f);
        notesController.MeasureLineSet(notesDirector.bpms);

        audioSource.pitch = 1.0f;
        musicSpeedField.text = "1.0";
    }

    public AudioType GetAudioType(string url)
    {
        switch (Path.GetExtension(url).ToLower())
        {
            case ".ogg":
                return AudioType.OGGVORBIS;
            case ".mp3":
                return AudioType.MPEG;
            case ".wav":
                return AudioType.WAV;
            default:
                return AudioType.UNKNOWN;
        }
    }

    public void MusicPathOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", new []{new ExtensionFilter("Sound file", "wav", "ogg", "mp3")}, false);
        if (paths.Length != 0)
            userIO.MusicImportPathOutput(paths.First());
    }
    
    public void DataImportNoteOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "",
            new[] { new ExtensionFilter("json file", "json") }, false);
        userIO.DataImportNotePathOutput(paths.First());
    }
    
    public void DataImportBpmOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "",
            new[] { new ExtensionFilter("json file", "json") }, false);
        userIO.DataImportBpmPathOutput(paths.First());
    }

    public void DataImportFieldOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "",
            new[] { new ExtensionFilter("json file", "json") }, false);
        userIO.DataImportFieldPathOutput(paths.First());
    }
    
    public void DataExportOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Open Folder", "", false);
        userIO.DataExportPathOutput(paths.First());
    }
}
