using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Screen = UnityEngine.Screen;

public class GameEvent : MonoBehaviour
{
    [SerializeField] private Text title;
    [SerializeField] private Button playButton;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TMP_InputField splitField;
    [SerializeField] private TMP_InputField timeField;
    [SerializeField] private TMP_InputField musicImportField;
    [SerializeField] private TMP_InputField dataImportFieldSheet;
    [SerializeField] private TMP_InputField dataImportFieldAddition;
    [SerializeField] private TMP_InputField dataExportField;
    [SerializeField] private TMP_InputField dataExportNameField;
    [SerializeField] private TMP_InputField speedField;

    [SerializeField] private Canvas settingCanvas;
    [SerializeField] private Canvas dataImportCanvas;
    [SerializeField] private Canvas dataExportCanvas;
    [SerializeField] private Canvas noticeCanvas;

    [SerializeField] private GameObject notes;
    [SerializeField] private GameObject bpms;
    
    [SerializeField] private CenterDirector centerDirector;
    [SerializeField] private NotesController notesController;
    [SerializeField] private NotesDirector notesDirector;
    
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

    private float beatTime;
    private float mouseVal;
    public int focusBeat = 0;
    public int nowBeatTime = -1;
    public int nowBeatNote = -1;
    public int nowBeatLong = -1;
    public int timeSignature = 4;

    public SpeedItem[] speedItems = new SpeedItem[] { };

    private void Start()
    {
        time = 0f;
        isPlaying = false;
        audioSource = GetComponent<AudioSource>();
        speed = 1.0f;
        speedField.text = speed.ToString("F1");
        split = 1;
        splitField.text = split.ToString();
    }
    
    private void Update()
    {
        if (isPlaying)
        {
            time = audioSource.time;
            timeField.text = time.ToString("F2");
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
                    notesDirector.focusNote != null && notesDirector.noteOrBpm)
                {
                    if (notesDirector.focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
                    {
                        nowBeatLong = NextBeat(false,
                            (notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() +
                            notesDirector.focusNote.GetComponent<NotesData>().note.GetLength100()) / 100f, nowBeatLong);

                        nowBeatLong = Mathf.Max(0, nowBeatLong);
                        float len = beatToTime(nowBeatLong) -
                                    notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() / 100f;
                        if (len < 0f) nowBeatLong++;
                        notesDirector.NoteLengthSet(len);
                        
                        nowBeatTime = -1;
                        nowBeatNote = -1;
                        FocusBeatSet((notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() +
                                     notesDirector.focusNote.GetComponent<NotesData>().note.GetLength100()) / 100f);
                    }
                }
                else
                {
                    if (notesDirector.focusNote == null)
                    {
                        nowBeatTime = NextBeat(false, time, nowBeatTime);
                        nowBeatTime = Mathf.Max(0, nowBeatTime);
                        ChangeTime(beatToTime(nowBeatTime));
                        nowBeatNote = -1;
                        FocusBeatSet(time);
                    }
                    else
                    {
                        float time;
                        if (notesDirector.noteOrBpm)
                            time = notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() / 100f;
                        else
                            time = notesDirector.bpms[notesDirector.focusNote].GetTime100() / 100f;
                        nowBeatNote = NextBeat(false, time, nowBeatNote);
                        nowBeatNote = Mathf.Max(0, nowBeatNote);
                        notesDirector.TimeSet(beatToTime(nowBeatNote));
                        nowBeatTime = -1;
                        FocusBeatSet(beatToTime(nowBeatNote));
                    }
                        
                    nowBeatLong = -1;
                }
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                    notesDirector.focusNote != null && notesDirector.noteOrBpm)
                {
                    if (notesDirector.focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
                    {
                        nowBeatLong = NextBeat(true,
                            (notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() +
                            notesDirector.focusNote.GetComponent<NotesData>().note.GetLength100()) / 100f, nowBeatLong);
                        
                        float len = beatToTime(nowBeatLong) -
                                    notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() / 100f;
                        if (len + notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() / 100f >
                            audioSource.clip.length)
                            nowBeatLong--;
                        notesDirector.NoteLengthSet(len);
                        
                        nowBeatTime = -1;
                        nowBeatNote = -1;
                        FocusBeatSet((notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() +
                                     notesDirector.focusNote.GetComponent<NotesData>().note.GetLength100()) / 100f);
                    }
                }
                else
                {
                    if (notesDirector.focusNote == null)
                    {
                        nowBeatTime = NextBeat(true, time, nowBeatTime);
                        ChangeTime(Mathf.Min(beatToTime(nowBeatTime), audioSource.clip.length));
                        nowBeatNote = -1;
                        FocusBeatSet(time);
                    }
                    else
                    {
                        float time;
                        if (notesDirector.noteOrBpm)
                            time = notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() / 100f;
                        else
                            time = notesDirector.bpms[notesDirector.focusNote].GetTime100() / 100f;
                        nowBeatNote = NextBeat(true, time, nowBeatNote);
                        notesDirector.TimeSet(Mathf.Min(beatToTime(nowBeatNote), audioSource.clip.length));
                        nowBeatTime = -1;
                        FocusBeatSet(Mathf.Min(beatToTime(nowBeatNote), audioSource.clip.length));
                    }

                    nowBeatLong = -1;
                }
            }
            
            // スクロールで時間移動できる機能
            mouseVal = Input.GetAxis("Mouse ScrollWheel");
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
                    ChangeTime(beatToTime(nowBeatTime));
                    FocusBeatSet(time);
                }
                else if (mouseVal < 0)
                {
                    nowBeatTime = NextBeat(true, time, nowBeatTime);
                    ChangeTime(Mathf.Min(beatToTime(nowBeatTime), audioSource.clip.length));
                    FocusBeatSet(time);
                }
            }


            // 複製機能の実装
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (notesDirector.focusNote != null && notesDirector.noteOrBpm)
                {
                    Note data = notesDirector.focusNote.GetComponent<NotesData>().note;
                    
                    float time;
                    time = data.GetTime100() / 100f;
                    nowBeatNote = NextBeat(true, time, nowBeatNote);
                    int n = nowBeatNote;
                    notesDirector.NewNote((int)(Mathf.Min(beatToTime(nowBeatNote), audioSource.clip.length) * 100), data.GetStartLane(), data.GetEndLane(), data.GetKind(), data.GetLength100());
                    nowBeatNote = n;
                    nowBeatTime = -1;
                    FocusBeatSet(beatToTime(nowBeatNote));
                    nowBeatLong = -1;
                }
            }
        }

        if (isEdit)
        {
            // sliderの監視
            if (timeSlider.value.ToString("F2") != time.ToString("F2"))
            {
                ChangeTime(timeSlider.value);
            }
        }
        
        // フルスクリーンモード切り替え
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }

    private int timeToBeat(float nowTime)
    {
        List<float> lines = notesController.bpmMeasureLines;
        lines.Sort();

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

    private float beatToTime(int beat)
    {
        List<float> lines = notesController.bpmMeasureLines;

        int measure = beat / split;
        int b = beat % split;

        float beatTime = (lines[measure + 1] - lines[measure]) / split;

        return lines[measure] + beatTime * b;
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

    public void SplitSet()
    {
        // SplitFieldの変更
        split = Mathf.Max(1, int.Parse(splitField.text));
        splitField.text = split.ToString();
        nowBeatTime = -1;
        nowBeatNote = -1;
        nowBeatLong = -1;
        
        if (notesDirector.focusNote == null)
            FocusBeatSet(time);
        else
        {
            if (notesDirector.noteOrBpm)
                FocusBeatSet(notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() / 100f);
            else
                FocusBeatSet(notesDirector.bpms[notesDirector.focusNote].GetTime100() / 100f);
        }
    }

    public void PlayClick()
    {
        // Playのクリック
        if (isFileSet)
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                playButton.GetComponent<Image>().color = new Color(87f / 255, 120f / 255, 171f / 255, 1f);
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
                playButton.GetComponent<Image>().color = new Color(106f / 255, 106f / 255, 106f / 255, 1f);
                audioSource.Stop();
                time = audioSource.time;
            }
        }
        
    }

    public void TimeSet()
    {
        // TimeFieldの変更
        ChangeTime(float.Parse(timeField.text));
        nowBeatTime = -1;
        nowBeatLong = -1;
    }

    public void SpeedSet()
    {
        speed = (float)Math.Round(float.Parse(speedField.text), 1, MidpointRounding.AwayFromZero);
        if (speed < 1) speed = 1.0f;
        if (speed > 10) speed = 10.0f;
        speedField.text = speed.ToString();
        notesController.MeasureLineSet(notesDirector.bpms);
        
        if (notesDirector.focusNote == null)
            FocusBeatSet(time);
        else
        {
            if (notesDirector.noteOrBpm)
                FocusBeatSet(notesDirector.focusNote.GetComponent<NotesData>().note.GetTime100() / 100f);
            else
                FocusBeatSet(notesDirector.bpms[notesDirector.focusNote].GetTime100() / 100f);
        }
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
                         $"The number of notes : {notes.transform.childCount}\n";
        noticeCanvas.GetComponent<NoticeController>().OpenNotice(2, massage);
    }

    public void MusicImport()
    {
        string address = musicImportField.text.Trim(' ', '"', '\n');

        StartCoroutine(StreamLoadAudioFile(address));
        TabClose();
    }

    public void DataImport()
    {
        string additionPath = dataImportFieldAddition.text.Trim(' ', '"', '\n');
        string sheetPath = dataImportFieldSheet.text.Trim(' ', '"', '\n');

        if (!File.Exists(additionPath))
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Import: ファイルが見つかりません");
            return;
        }

        try
        {
            if (additionPath != "")
            {
                // Additionの入手
                NoteAddition additionData = new NoteAddition();
                
                additionData = ExportJson.ImportingAddition(additionPath);

                speedItems = additionData.speedItem;

                foreach (Transform b in bpms.transform)
                {
                    b.GetComponent<BpmData>().ClearBpm();
                }
                notesDirector.bpms.Clear();

                foreach (BpmItem b in additionData.bpmItem)
                {
                    notesDirector.NewBpm(b.time100, b.bpm);
                }
            }
            
            if (sheetPath != "")
            {
                // Sheetの入手
                List<Note> notesData;
                
                notesData = ExportJson.ImportingSheet(sheetPath);

                foreach (Transform t in notes.transform)
                {
                    t.GetComponent<NotesData>().ClearNote();
                }

                foreach (Note n in notesData)
                {
                    notesDirector.NewNote(n.GetTime100(), n.GetStartLane(), n.GetEndLane(), n.GetKind(), n.GetLength100());
                }
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

    public void DataExport()
    {
        string path = dataExportField.text.Trim(' ', '"', '\n').TrimEnd('\\', '/');
        string name = dataExportNameField.text.Trim(' ', '"', '\n', '\\', '/');
        name = name == "" ? file : name;

        if (!Directory.Exists(path))
        {
            TabClose();
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Export: ディレクトリを見つけられませんでした");
            return;
        }

        try
        {
            ExportJson.ExportingSheet(notes, path + $"\\{name}.json");
            ExportJson.ExportingAddition(path + $"\\{name}Addition.json", new List<SpeedItem>(), new List<Bpms>(notesDirector.bpms.Values));
            
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
        time = Mathf.Floor(cTime * 100) / 100f;
        timeSlider.value = time;
        timeField.text = time.ToString();
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

        string Extension = Path.GetExtension(fileName);
        if (!(Extension == ".ogg" || Extension == ".mp3" || Extension == ".wav"))
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Audio: This file format is not supported");
            yield break;
        }
        
        using (WWW www = new WWW("file://" + fileName))
        {
            AudioClip audioClip;
            
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, $"Audio : {www.error}");
                yield break;
            }
            
            audioClip = www.GetAudioClip();
            audioClip.name = "Audio";
            
            audioSource.clip = audioClip;
            isFileSet = true;
            timeSlider.maxValue = audioSource.clip.length;

            file = Path.GetFileNameWithoutExtension(fileName);
            title.text = file;
        }

        fileAddress = fileName;
        noticeCanvas.GetComponent<NoticeController>().OpenNotice(0, "AudioLoad Finished.");
        
        notesDirector.NewBpm(0, 120);
        notesController.MeasureLineSet(notesDirector.bpms);
    }

    public void SettingOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", new []{new ExtensionFilter("Sound file", "wav", "ogg", "mp3")}, false);
        musicImportField.text = paths.First();
    }
    
    public void ExportOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Open Folder", "", false);
        dataExportField.text = paths.First();
    }
    
    public void ImportAdditionOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", new []{new ExtensionFilter("json file", "json")}, false);
        dataImportFieldAddition.text = paths.First();
    }
    
    public void ImportSheetOpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", new []{new ExtensionFilter("json file", "json")}, false);
        dataImportFieldSheet.text = paths.First();
    }
}
