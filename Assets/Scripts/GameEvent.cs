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
    
    public int time;
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
    public int nowBeat = -1;
    private bool isD = false;
    public int timeSignature = 4;

    private void Start()
    {
        time = 0;
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
            time = (int)Math.Round(audioSource.time * 1000);
            userIO.TimeOutput(time / 1000f);
            timeSlider.value = time;
            
            FocusBeatSet(time);
        }
        
        isEdit = !isPlaying && isFileSet && !isOpenTab;

        if (isEdit && EventSystem.current.currentSelectedGameObject == null)
        {
            // キーボード入力
            if (Input.GetKeyDown(KeyCode.Space)) PlayClick();

            int key = 0;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                key--;
            if (Input.GetKeyDown(KeyCode.RightArrow))
                key++;
            
            if (key != 0)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    foreach (var kv in notesDirector.focusNotes)
                    {
                        if (kv.Key != 0)
                            continue;
                        if (kv.Value.GetComponent<NotesData>().note.GetKind() != 'L')
                            continue;

                        int bLen = kv.Value.GetComponent<NotesData>().note.GetLength();
                        int tim = kv.Value.GetComponent<NotesData>().note.GetTime();

                        nowBeat = NextBeat(key == 1, tim + bLen);
                        
                        int aLen = BeatToTime(nowBeat) - tim;
                        if (aLen < 0) nowBeat++;
                        else if ((tim + aLen) / 1000f > audioSource.clip.length) nowBeat--;
                        
                        notesDirector.NoteLengthSet(kv, aLen);
                    }
                }
                else
                {
                    if (notesDirector.focusNotes.Count == 0)
                    {
                        nowBeat = NextBeat(key == 1, time);
                        
                        int wantTime = BeatToTime(nowBeat);
                        if (wantTime < 0) nowBeat++;
                        else if (wantTime / 1000f > audioSource.clip.length) nowBeat--;
                        
                        ChangeTime(BeatToTime(nowBeat));
                    }
                    else
                    {
                        int t;
                        foreach (var kv in notesDirector.focusNotes)
                        {
                            if (kv.Key == 3) // SlideMaintain
                            {
                                if (notesDirector.focusNotes.Contains(
                                        new KeyValuePair<int, GameObject>(2, kv.Value.GetComponent<SlideMaintainData>().parent)))
                                    continue;
                            }

                            t = GetFocusTime(kv);
                            nowBeat = NextBeat(key == 1, t);
                            
                            int wantTime = BeatToTime(nowBeat);
                            if (wantTime < 0) nowBeat++;
                            else if (wantTime / 1000f > audioSource.clip.length) nowBeat--;
                            
                            notesDirector.TimeSet(kv, BeatToTime(nowBeat));
                        }
                    }
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
                            ChangeTime(time - (int)(9000 / speed));
                        }
                        else if (mouseVal < 0)
                        {
                            ChangeTime(time + (int)(9000 / speed));
                        }
                    }
                    else
                    {
                        // Shiftを押していなければBpmとSplitで移動
                        if (mouseVal > 0)
                        {
                            nowBeat = NextBeat(false, time);
                            ChangeTime(BeatToTime(nowBeat));
                        }
                        else if (mouseVal < 0)
                        {
                            nowBeat = NextBeat(true, time);
                            ChangeTime(BeatToTime(nowBeat));
                        }
                    }
                }
            }
            

            // 複製機能の実装
            if (Input.GetKeyDown(KeyCode.C))
            {
                var bFocusNotes = new List<KeyValuePair<int, GameObject>>(notesDirector.focusNotes);
                notesDirector.ClearFocus();

                foreach (var kv in bFocusNotes)
                {
                    int time = GetFocusTime(kv);
                    nowBeat = NextBeat(true, time);
                    
                    if (kv.Key == 0) // Notes
                    {
                        Note data = kv.Value.GetComponent<NotesData>().note;
                        notesDirector.NewNote(BeatToTime(nowBeat),
                            data.GetStartLane(), data.GetEndLane(), data.GetKind(), data.GetLength(), data.GetField(), true);
                    }
                    else if (kv.Key == 2) // Slides
                    {
                        Note data = kv.Value.GetComponent<SlideData>().note;
                        bool dummy = kv.Value.GetComponent<SlideData>().isDummy;
                        int color = kv.Value.GetComponent<SlideData>().fieldColor;
                        notesDirector.NewSlide(
                            BeatToTime(nowBeat),
                            data.GetStartLane(), data.GetEndLane(), data.GetField(), dummy, color,
                            kv.Value.GetComponent<SlideData>().slideMaintain.Values.ToArray(), true);
                    }
                    else if (kv.Key == 3) // SlideMaintains
                    {
                        var parent = kv.Value.GetComponent<SlideMaintainData>().parent;
                        if (bFocusNotes.Contains(new KeyValuePair<int, GameObject>(2, parent)))
                            continue;

                        SlideMaintain data = kv.Value.GetComponent<SlideMaintainData>().parentSc.slideMaintain[kv.Value];
                        var pt = parent.GetComponent<SlideData>().note.GetTime();
                        notesDirector.NewSlideMaintain(BeatToTime(nowBeat) - pt,
                            data.startLane, data.endLane, data.isJudge, data.isVariation, kv, true);
                    }
                    else if (kv.Key == 4) // Speeds
                    {
                        Speed data = speedsDirector.fieldSpeeds[kv.Value];
                        notesDirector.NewSpeed(BeatToTime(nowBeat),
                            data.GetSpeed100(), data.GetIsVariation(), true);
                    }
                    else if (kv.Key == 5) // Angles
                    {
                        Angle data = anglesDirector.fieldAngles[kv.Value];
                        notesDirector.NewAngle(BeatToTime(nowBeat),
                            data.GetDegree(), data.GetVariation(), true);
                    }
                    else if (kv.Key == 6) // Transparencies
                    {
                        Transparency data = alphaDirector.fieldTransparencies[kv.Value];
                        notesDirector.NewTransparency(BeatToTime(nowBeat),
                            data.GetAlpha(), data.GetIsVariation(), true);
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
                ChangeTime((int)Math.Round(timeSlider.value));
            }
        }
        
        
        // キーボード入力
        
        // フルスクリーンモード切り替え
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightAlt))
        {
            // open
            if (Input.GetKeyDown(KeyCode.O))
            {
                userIO.DataImportPathOpenFileButton();
                userIO.DataImportButton();
            }
            
            // save
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
                    userIO.DataExportPathInput() == "")
                {
                    userIO.DataExportPathOpenFileButton();
                }
                
                userIO.DataExportButton();
            }
        }
    }

    public int GetFocusTime(KeyValuePair<int, GameObject> kv)
    {
        int t = 0;
        switch (kv.Key)
        {
            case 0:
                // Notes
                t = kv.Value.GetComponent<NotesData>().note.GetTime();
                break;
            case 1:
                // Bpms
                t = notesDirector.bpms[kv.Value].GetTime();
                break;
            case 2:
                // Slides
                t = kv.Value.GetComponent<SlideData>().note.GetTime();
                break;
            case 3:
                // SlideMaintains
                t = kv.Value.GetComponent<SlideMaintainData>().parentSc
                    .slideMaintain[kv.Value].time + kv.Value.GetComponent<SlideMaintainData>().parentSc.note.GetTime();
                break;
            case 4:
                // Speeds
                t = speedsDirector.fieldSpeeds[kv.Value].GetTime();
                break;
            case 5:
                // angles
                t = anglesDirector.fieldAngles[kv.Value].GetTime();
                break;
            case 6:
                // Transparencies
                t = alphaDirector.fieldTransparencies[kv.Value].GetTime();
                break;
        }

        return t;
    }

    private int TimeToBeat(int nowTime)
    {
        List<int> lines = notesController.bpmLines;
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
        
        float beatTimeA = (float)(lines[measure + 1] - lines[measure]) / split;

        isD = false;
        int spBeat = 0;
        b = split - 1;
        for (int i = 0; i < split; i++)
        {
            spBeat = lines[measure] + (int)Math.Round(beatTimeA * i);
            if (spBeat > nowTime)
            {
                isD = false;
                b = i - 1;
                break;
            }
            else if (spBeat == nowTime)
            {
                isD = true;
                b = i;
                break;
            }
        }

        return measure * split + b;
    }

    private int BeatToTime(int beat)
    {
        List<int> lines = notesController.bpmLines;

        int measure = beat / split;
        int b = beat % split;

        if (measure < 0) return 0;
        
        float beatTimeA = (float)(lines[measure + 1] - lines[measure]) / split;
        
        int i = lines[measure] + (int)Math.Round(beatTimeA * b);
        i = Math.Min(i, (int)(audioSource.clip.length * 1000));

        return i;
    }

    private int NextBeat(bool arrow, int nowTime)
    {
        int beat;
        if (arrow)
        {
            beat = TimeToBeat(nowTime) + 1;
        }
        else
        {
            beat = TimeToBeat(nowTime);
            if (isD) beat--;
        }

        beat = Math.Max(0, beat);

        return beat;
    }

    public void FocusBeatSet(int basis)
    {
        focusBeat = TimeToBeat(basis);
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
        
        if (notesDirector.focusNotes.Count == 0)
            FocusBeatSet(time);
        else
            FocusBeatSet(GetFocusTime(notesDirector.focusNotes[0]));
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
                audioSource.time = time / 1000f;
                centerDirector.StartPlayButton();
                if (audioSource.clip.length > time / 1000f)
                    audioSource.Play();
            }
            else
            {
                audioSource.Stop();
                time = (int)Math.Round(audioSource.time * 1000);
            }
        }
        
    }

    public void TimeSet(int inTime)
    {
        // TimeFieldの変更
        ChangeTime(inTime);
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
        notesDirector.ClearFocus();

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
        notesDirector.ClearFocus();

        tabMode = 1;
        
        speedsDirector.RenewalSpeed();
        anglesDirector.RenewalAngle();
        alphaDirector.RenewalAlpha();
    }

    public void SpeedSet(float inSpeed)
    {
        speed = inSpeed;
        if (speed < 1) speed = 1.0f;
        if (speed > 20) speed = 20.0f;
        userIO.SpeedOutput(speed);
        notesController.MeasureLineSet(notesDirector.bpms);
        
        if (notesDirector.focusNotes.Count == 0)
            FocusBeatSet(time);
        else
        {
            FocusBeatSet(GetFocusTime(notesDirector.focusNotes[0]));
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

    public void DataImport(string notePath, string bpmPath, string fieldPath, bool isOld)
    {

        if (!File.Exists(bpmPath))
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Import: ファイルが見つかりません");
            return;
        }

        // try
        // {
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
                    notesDirector.NewBpm(b.time, b.bpm / 1000f, false);
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
                    else
                    {
                        transData[i].Add(new Transparency(0, 100, false));
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
                    if (isOld)
                    {
                        int start = Math.Clamp(n.Value.GetStartLane() * 2, 0, 24);
                        int end = Math.Clamp(n.Value.GetEndLane() * 2, 0, 24);
                        n.Value.SetLane(start, end);

                        if (n.Value.GetKind() == 'S')
                        {
                            foreach (var item in slidesData[n.Key].item)
                            {
                                int s = Math.Clamp(item.startLane * 2, 0, 24);
                                int e = Math.Clamp(item.endLane * 2, 0, 24);
                                item.startLane = s;
                                item.endLane = e;
                            }
                        }
                    }
                    
                    if (n.Value.GetKind() == 'S')
                        notesDirector.NewSlide(n.Value.GetTime(), n.Value.GetStartLane(), n.Value.GetEndLane(),
                            n.Value.GetField(), slidesData[n.Key].isDummy, slidesData[n.Key].color,
                            slidesData[n.Key].item, false);
                    else
                        notesDirector.NewNote(n.Value.GetTime(), n.Value.GetStartLane(), n.Value.GetEndLane(),
                            n.Value.GetKind(), n.Value.GetLength(), n.Value.GetField(), false);
                }
            }
        // }
        // catch (Exception e)
        // {
        //      TabClose();
        //      noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, $"Import: {e}");
        //      return;
        // }
        
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
            ExportJson.ExportingSheet(notes, path + "\\Data.json");
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

    private void ChangeTime(int cTime)
    {
        time = cTime;
        timeSlider.value = time;
        userIO.TimeOutput(time / 1000f);
        FocusBeatSet(time);
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
        timeSlider.maxValue = audioSource.clip.length * 1000;
        title.text = file;

        fileAddress = fileName;
        noticeCanvas.GetComponent<NoticeController>().OpenNotice(0, "AudioLoad Finished.");
        
        notesDirector.NewBpm(0, 120f, false);
        notesController.MeasureLineSet(notesDirector.bpms);

        audioSource.pitch = 1.0f;
        musicSpeedField.text = "1.0";
        
        TitleBarSetter.Instance.SetTitleBar(file);
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
    
    public void DataImportOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Open Folder", "", false);
        userIO.DataImportPathOutput(paths.First());
    }
    
    public void DataExportOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Open Folder", "", false);
        userIO.DataExportPathOutput(paths.First());
    }

    public void BgmVolumeSet(float value)
    {
        audioSource.volume = value;
    }

    public void SeVolumeSet(float value)
    {
        centerDirector.GetComponent<AudioSource>().volume = value;
    }
}
