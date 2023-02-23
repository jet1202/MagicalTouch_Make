using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameEvent : MonoBehaviour
{
    [SerializeField] private Text title;
    [SerializeField] private Button playButton;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private InputField bpmField;
    [SerializeField] private InputField offsetField;
    [SerializeField] private InputField splitField;
    [SerializeField] private InputField timeField;
    [SerializeField] private InputField musicImportField;
    [SerializeField] private InputField dataImportFieldBase;
    [SerializeField] private InputField dataImportFieldSheet;
    [SerializeField] private InputField dataExportField;
    [SerializeField] private InputField speedField;

    [SerializeField] private Canvas settingCanvas;
    [SerializeField] private Canvas dataImportCanvas;
    [SerializeField] private Canvas dataExportCanvas;
    [SerializeField] private Canvas noticeCanvas;

    [SerializeField] private GameObject notes;
    
    [SerializeField] private CenterDirector centerDirector;
    [SerializeField] private NotesController notesController;
    [SerializeField] private NotesDirector notesDirector;
    public float time;
    public bool isPlaying;
    public float speed;
    public int bpm;
    public float offset;
    public int split;
    private AudioSource audioSource;
    private string file;
    private string fileAddress = "";
    public bool isFileSet = false;
    public bool isOpenTab = false;
    public bool isEdit;

    private float beatTime;
    private float mouseVal;
    public int nowBeat = -1;
    public int nowBeatLong = -1;
    public int timeSignature = 4;


    private void Start()
    {
        time = 0f;
        isPlaying = false;
        audioSource = GetComponent<AudioSource>();
        speed = 1.0f;
        speedField.text = speed.ToString("F1");
        bpm = 120;
        bpmField.text = bpm.ToString();
        offset = 0f;
        offsetField.text = offset.ToString("F2");
        split = 1;
        splitField.text = split.ToString();
    }

    private void FixedUpdate()
    {
        if (isPlaying)
        {
            time = audioSource.time;
            // time += Time.fixedDeltaTime;
            timeField.text = time.ToString("F2");
            timeSlider.value = time;
        }
    }

    private void Update()
    {
        // キーボード入力
        if (Input.GetKeyDown(KeyCode.Space)) PlayClick();
        isEdit = !isPlaying && isFileSet && !isOpenTab;

        if (isEdit)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                    notesDirector.focusNote != null)
                {
                    if (notesDirector.focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
                    {
                        beatTime = (60f / bpm * timeSignature) / split;
                        if (nowBeatLong == -1)
                        {
                            nowBeatLong =
                                (int)((notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() +
                                       notesDirector.focusNote.GetComponent<NotesData>().note.GetLength() - offset) / beatTime);
                        }
                        else
                        {
                            nowBeatLong--;
                        }

                        nowBeatLong = Mathf.Max(0, nowBeatLong);
                        float len = nowBeatLong * beatTime + offset -
                                    notesDirector.focusNote.GetComponent<NotesData>().note.GetTime();
                        if (len < 0f) nowBeatLong++;
                        notesDirector.NoteLengthSet(len);
                        
                        nowBeat = -1;
                    }
                }
                else
                {
                    beatTime = (60f / bpm * timeSignature) / split;
                    if (nowBeat == -1)
                    {
                        if (notesDirector.focusNote == null)
                            nowBeat = (int)((time - offset) / beatTime);
                        else
                            nowBeat = (int)((notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() - offset) / beatTime);
                    }
                    else
                    {
                        nowBeat--;
                    }

                    nowBeat = Mathf.Max(0, nowBeat);
                    if (notesDirector.focusNote == null)
                        ChangeTime(nowBeat * beatTime + offset);
                    else
                        notesDirector.NoteTimeSet(nowBeat * beatTime + offset);
                    nowBeatLong = -1;
                }
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                    notesDirector.focusNote != null)
                {
                    if (notesDirector.focusNote.GetComponent<NotesData>().note.GetKind() == 'L')
                    {
                        beatTime = (60f / bpm * timeSignature) / split;
                        if (nowBeatLong == -1)
                        {
                            nowBeatLong =
                                (int)((notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() +
                                    notesDirector.focusNote.GetComponent<NotesData>().note.GetLength() - offset) / beatTime) + 1;
                        }
                        else
                        {
                            nowBeatLong++;
                        }

                        float len = nowBeatLong * beatTime + offset -
                                    notesDirector.focusNote.GetComponent<NotesData>().note.GetTime();
                        if (len + notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() >
                            audioSource.clip.length)
                            nowBeatLong--;
                        notesDirector.NoteLengthSet(len);
                        
                        nowBeat = -1;
                    }
                }
                else
                {
                    beatTime = (60f / bpm * timeSignature) / split;
                    if (nowBeat == -1)
                    {
                        if (notesDirector.focusNote == null)
                            nowBeat = (int)((time - offset) / beatTime) + 1;
                        else
                            nowBeat = (int)((notesDirector.focusNote.GetComponent<NotesData>().note.GetTime() - offset) / beatTime) + 1;
                    }
                    else
                    {
                        nowBeat++;
                    }

                    if (notesDirector.focusNote == null)
                        ChangeTime(Mathf.Min((nowBeat * beatTime + offset), audioSource.clip.length));
                    else
                        notesDirector.NoteTimeSet(Mathf.Min((nowBeat * beatTime + offset), audioSource.clip.length));

                    nowBeatLong = -1;
                }
            }
            
            // スクロールで時間移動できる機能
            mouseVal = Input.GetAxis("Mouse ScrollWheel");
            if (mouseVal > 0)
            {
                ChangeTime(time - 5f / speed);
            }
            else if (mouseVal < 0)
            {
                ChangeTime(time + 5f / speed);
            }
            
            
            // 複製機能の実装
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (notesDirector.focusNote != null)
                {
                    Note data = notesDirector.focusNote.GetComponent<NotesData>().note;
                    beatTime = (60f / bpm * timeSignature) / split;
                    notesDirector.NewNote(data.GetTime() + beatTime, data.GetStartLane(), data.GetEndLane(), data.GetKind(), data.GetLength());
                }
            }
            
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
    

    public void SettingsClick()
    {
        // Settingsボタン
        settingCanvas.gameObject.SetActive(true);
        isOpenTab = true;
    }

    public void BpmSet()
    {
        // BPMFieldの変更
        bpm = int.Parse(bpmField.text);
        if (bpm < 30) bpm = 30;
        if (bpm > 600) bpm = 600;
        bpmField.text = bpm.ToString();
        notesController.MeasureLineSet(bpm, speed);
    }
    
    public void BpmSet(int b)
    {
        // BPMFieldの変更
        bpm = b;
        if (bpm < 30) bpm = 30;
        if (bpm > 600) bpm = 600;
        bpmField.text = bpm.ToString();
        notesController.MeasureLineSet(bpm, speed);
    }

    public void OffsetSet()
    {
        // Offsetの変更
        offset = (float)Math.Round(float.Parse(offsetField.text), 2, MidpointRounding.AwayFromZero);
        offsetField.text = offset.ToString();
        notesController.MeasureLineSet(bpm, speed);
    }
    
    public void OffsetSet(float o)
    {
        // Offsetの変更
        offset = o;
        offsetField.text = offset.ToString();
        notesController.MeasureLineSet(bpm, speed);
    }

    public void SplitSet()
    {
        // SplitFieldの変更
        split = Mathf.Max(1, int.Parse(splitField.text));
        splitField.text = split.ToString();
        nowBeat = -1;
        nowBeatLong = -1;
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
                nowBeat = -1;
                nowBeatLong = -1;
            }
            else
            {
                playButton.GetComponent<Image>().color = new Color(106f / 255, 106f / 255, 106f / 255, 1f);
                audioSource.Stop();
            }
        }
        
    }

    public void TimeSet()
    {
        // TimeFieldの変更
        ChangeTime(float.Parse(timeField.text));
        nowBeat = -1;
        nowBeatLong = -1;
    }

    public void SpeedSet()
    {
        speed = (float)Math.Round(float.Parse(speedField.text), 1, MidpointRounding.AwayFromZero);
        if (speed < 1) speed = 1.0f;
        if (speed > 10) speed = 10.0f;
        speedField.text = speed.ToString();
        notesController.MeasureLineSet(bpm, speed);
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

    public void MusicImport()
    {
        string address = musicImportField.text.Trim(' ', '"', '\n');

        StartCoroutine(StreamLoadAudioFile(address));
        TabClose();
    }

    public void DataImport()
    {
        string basePath = dataImportFieldBase.text.Trim(' ', '"', '\n');
        string sheetPath = dataImportFieldSheet.text.Trim(' ', '"', '\n');

        if (!File.Exists(sheetPath))
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Import: ファイルが見つかりません");
            return;
        }

        try
        {
            if (sheetPath != "")
            {
                // Sheetの入手
                List<Note> notesData = Export.ImportingSheet(sheetPath);

                foreach (Transform t in notes.transform)
                {
                    t.GetComponent<NotesData>().ClearNote();
                }

                foreach (Note n in notesData)
                {
                    notesDirector.NewNote(n.Time, n.GetStartLane(), n.GetEndLane(), n.GetKind(), n.GetLength());
                }
            }

            if (basePath != "")
            {
                // Baseの入手
                KeyValuePair<string, KeyValuePair<int, float>> baseData = Export.ImportingBase(basePath);
                
                BpmSet(baseData.Value.Key);
                OffsetSet(baseData.Value.Value);
                if (file != "")
                    StartCoroutine(StreamLoadAudioFile(baseData.Key));
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

        if (!Directory.Exists(path))
        {
            TabClose();
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Export: ディレクトリを見つけられませんでした");
            return;
        }

        try
        {
            Export.ExportingSheet(notes, path + $"\\{file}.bin");
            Export.ExportingBase(bpm, offset, fileAddress, path + "\\base.bin");
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
        
        if (audioSource == null || string.IsNullOrEmpty(fileName))
            yield break;

        if (!File.Exists(fileName))
        {
            noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, "Audio: File not found.");
            yield break;
        }
        
        using (WWW www = new WWW("file://" + fileName))
        {
            AudioClip audioClip;
            
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                noticeCanvas.GetComponent<NoticeController>().OpenNotice(1, www.error);
                yield break;
            }

            audioClip = www.GetAudioClip();
            audioClip.name = "Audio";
            
            //AudioClipの圧縮設定
            // Debug.Log(AssetDatabase.GetAssetPath(audioClip.GetInstanceID()));
            // AudioImporter audioImporter = AssetImporter.GetAtPath("File///" + fileName) as AudioImporter;
            // audioImporter.forceToMono = true;
            // audioImporter.loadInBackground = true;
            //
            // AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
            // settings.loadType = AudioClipLoadType.Streaming;
            // settings.compressionFormat = AudioCompressionFormat.Vorbis;
            // settings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
            // ここまで

            audioSource.clip = audioClip;
            isFileSet = true;
            timeSlider.maxValue = audioSource.clip.length;

            file = Path.GetFileNameWithoutExtension(fileName);
            title.text = file;
        }

        fileAddress = fileName;
        noticeCanvas.GetComponent<NoticeController>().OpenNotice(0, "AudioLoad Finished.");
    }
}
