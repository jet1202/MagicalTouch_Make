using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserIO : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private FieldSettingController fieldSettingController;
    
    // Userの入力・出力を管理する関数群
    
    // Main, Setting, Import, Export
    [SerializeField] private Button playButton;
    [SerializeField] private TMP_InputField timeField;
    [SerializeField] private TMP_InputField splitField;
    [SerializeField] private TMP_InputField speedField;
    [SerializeField] private TMP_InputField musicSpeedField;
    [SerializeField] private Toggle fieldIsDummyToggle;
    [SerializeField] private TMP_InputField musicImportField;
    [SerializeField] private TMP_InputField dataImportFieldNote;
    [SerializeField] private TMP_InputField dataImportFieldBpm;
    [SerializeField] private TMP_InputField dataImportFieldField;
    [SerializeField] private TMP_InputField dataExportPathField;
    
    // Button ========================================
    
    // MainButton
    public void SettingButton()
    {
        // 設定ボタン
        gameEvent.SettingsClick();
    }

    public void PlayButton()
    {
        // 再生ボタン
        gameEvent.PlayClick();
    }
    public void PlayButtonColor(bool isOn)
    {
        if (isOn)
            playButton.GetComponent<Image>().color = new Color(87f / 255, 120f / 255, 171f / 255, 1f);
        else
            playButton.GetComponent<Image>().color = new Color(106f / 255, 106f / 255, 106f / 255, 1f);
    }

    public void NoteTabButton()
    {
        // noteタブボタン
        gameEvent.NoteTabSet();
    }
    
    public void SpeedTabButton()
    {
        // speedタブボタン
        gameEvent.SpeedTabSet();
    }

    public void ImportMusicButton()
    {
        // 音楽インポートボタン
        gameEvent.ImportMusicClick();
    }

    public void ExportButton()
    {
        // データエクスポートボタン
        gameEvent.ExportClick();
    }

    public void ImportButton()
    {
        // データインポートボタン
        gameEvent.ImportClick();
    }

    public void InfoButton()
    {
        // Infoボタン
        gameEvent.InfoClick();
    }
    
    public void TabCloseButton()
    {
        // タブ閉じるボタン
        gameEvent.TabClose();
    }

    public void NoticeCloseButton()
    {
        // Notice閉じるボタン
        gameEvent.NoticeClose();
    }
    
    // Setting, Import, ExportButton
    public void MusicImportButton()
    {
        // 音楽インポート
        string path = musicImportField.text.Trim(' ', '"', '\n');
        gameEvent.MusicImport(path);
    }

    public void DataImportButton()
    {
        string notePath = dataImportFieldNote.text.Trim(' ', '"', '\n');
        string bpmPath = dataImportFieldBpm.text.Trim(' ', '"', '\n');
        string fieldPath = dataImportFieldField.text.Trim(' ', '"', '\n');
        
        gameEvent.DataImport(notePath, bpmPath, fieldPath);
    }

    public void DataExportButton()
    {
        string path = dataExportPathField.text.Trim(' ', '"', '\n');
        gameEvent.DataExport(path);
    }
    
    public void MusicPathOpenFileButton()
    {
        // 音楽パス選択ファイルを開く
        gameEvent.MusicPathOpenFile();
    }
    
    public void DataImportNotePathOpenFileButton()
    {
        // データパス選択ファイルを開く
        gameEvent.DataImportNoteOpenFile();
    }
    
    public void DataImportBpmPathOpenFileButton()
    {
        // データパス選択ファイルを開く
        gameEvent.DataImportBpmOpenFile();
    }
    
    public void DataImportFieldPathOpenFileButton()
    {
        // データパス選択ファイルを開く
        gameEvent.DataImportFieldOpenFile();
    }
    
    public void DataExportPathOpenFileButton()
    {
        // データパス選択ファイルを開く
        gameEvent.DataExportOpenFile();
    }
    
    // Toggle, Slider ========================================
    
    // SettingToggle
    public void FieldColorToggle(bool isOn)
    {
        // fieldColorToggle
        gameEvent.FieldColorSet(isOn);
    }
    
    public void LinePreviewToggle(bool isOn)
    {
        // linePreviewToggle
        gameEvent.LinePreviewSet(isOn);
    }

    public void IsDummyHideToggle(bool isOn)
    {
        // isDummyHideToggle
        gameEvent.DummyHideSet(isOn);
    }

    public void NoteAlphaSlider(float value)
    {
        // noteAlphaSlider
        gameEvent.NoteAlphaSet(value);
    }
    
    // InputField ====================================

    // MainField
    public void SplitInput(string text)
    {
        // splitの入力
        int split = Mathf.Max(1, int.Parse(text));
        gameEvent.SplitSet(split);
    }
    public void SplitOutput(int value)
    {
        // splitの出力
        splitField.text = value.ToString();
    }

    public void TimeInput(string text)
    {
        float time = float.Parse(text);
        gameEvent.TimeSet(time);
    }

    public void TimeOutput(float value)
    {
        // timeの出力
        timeField.text = value.ToString("F3");
    }

    // SettingField
    public void SpeedInput(string text)
    {
        float speed = float.Parse(text);
        gameEvent.SpeedSet(speed);
    }
    public void SpeedOutput(float value)
    {
        speedField.text = value.ToString("F1");
    }
    
    public void MusicSpeedInput(string text)
    {
        float musicSpeed = float.Parse(text);
        gameEvent.MusicSpeedSet(musicSpeed);
    }
    public void MusicSpeedOutput(float value)
    {
        musicSpeedField.text = value.ToString();
    }
    
    // Import, ExportField
    public void MusicImportPathOutput(string path)
    {
        musicImportField.text = path;
    }
    
    public void DataImportNotePathOutput(string path)
    {
        dataImportFieldNote.text = path;
    }
    
    public void DataImportBpmPathOutput(string path)
    {
        dataImportFieldBpm.text = path;
    }
    
    public void DataImportFieldPathOutput(string path)
    {
        dataImportFieldField.text = path;
    }
    
    public void DataExportPathOutput(string path)
    {
        dataExportPathField.text = path;
    }
    
    // SettingのScrollView関係

    public void FieldScrollViewSelect(int value)
    {
        fieldSettingController.SelectField(value);
    }
    
    public void AddFieldButton()
    {
        // フィールド追加ボタン
        fieldSettingController.AddField();
    }
    
    public void FieldIsDummyToggleInput(bool isOn)
    {
        fieldSettingController.SetIsDummy(isOn);
    }
    public void FieldIsDummyToggleOutput(bool isOn)
    {
        fieldIsDummyToggle.isOn = isOn;
    }
    
    // Bottom
    [Header("note関係")]
    [SerializeField] private TMP_InputField noteTimeField;
    [SerializeField] private TMP_InputField noteLaneFieldF;
    [SerializeField] private TMP_InputField noteLaneFieldL;
    [SerializeField] private TMP_Dropdown noteKindDropdown;
    [SerializeField] private TMP_InputField noteLengthField;
    [SerializeField] private TMP_InputField bpmField;
    [SerializeField] private Toggle isDummyToggle;
    [SerializeField] private Toggle isJudgeToggle;
    [SerializeField] private Toggle isVariationToggle;
    [SerializeField] private TMP_Dropdown noteFieldDropdown;
    [SerializeField] private TMP_Dropdown slideFieldColorDropdown;

    [SerializeField] private TMP_InputField speedTimeField;
    [SerializeField] private TMP_InputField speedSpeedField;
    [SerializeField] private Toggle speedIsVariationToggle;
    [SerializeField] private TMP_InputField angleDegreeField;
    [SerializeField] private TMP_InputField angleVariationField;
    [SerializeField] private TMP_InputField transparencyAlphaField;
    [SerializeField] private Toggle transparencyIsVariationToggle;
    [SerializeField] private TMP_Dropdown speedFieldDropdown;
    
    // Button ========================================
    
    // NoteTab
    public void NoteAddButton()
    {
        notesDirector.NewNote();
    }

    public void BpmAddButton()
    {
        notesDirector.NewBpm();
    }

    public void SlideAddButton()
    {
        notesDirector.NewSlide();
    }
    
    // speedTab
    public void SpeedAddButton()
    {
        notesDirector.NewSpeed();
    }
    
    public void AngleAddButton()
    {
        notesDirector.NewAngle();
    }
    
    public void TransparencyAddButton()
    {
        notesDirector.NewTransparency();
    }
    
    // InputField, Dropdown, Toggle ==================
    
    // NoteTab
    public void NoteTimeInput(string text)
    {
        float time = float.Parse(text);
        notesDirector.TimeSet(time);
    }
    public void NoteTimeOutput(float value)
    {
        noteTimeField.text = value.ToString("F3");
    }
    
    public void NoteLaneFInput(string text)
    {
        int laneF = int.Parse(text);
        int laneL = int.Parse(noteLaneFieldL.text);
        notesDirector.NoteLaneSet(laneF, laneL);
    }
    public void NoteLaneFOutput(int value)
    {
        noteLaneFieldF.text = value.ToString();
    }
    
    public void NoteLaneLInput(string text)
    {
        int laneF = int.Parse(noteLaneFieldF.text);
        int laneL = int.Parse(text);
        notesDirector.NoteLaneSet(laneF, laneL);
    }
    public void NoteLaneLOutput(int value)
    {
        noteLaneFieldL.text = value.ToString();
    }
    
    public void NoteKindDropdownInput(int value)
    {
        notesDirector.NoteKindSet(value);
    }
    public void NoteKindDropdownOutput(int value)
    {
        noteKindDropdown.value = value;
    }
    
    public void NoteLengthInput(string text)
    {
        float length = float.Parse(text);
        notesDirector.NoteLengthSet(length);
    }
    public void NoteLengthOutput(float value)
    {
        noteLengthField.text = value.ToString("F3");
    }
    
    public void NoteFieldDropdownInput(int value)
    {
        notesDirector.NoteFieldSet(value);
    }
    public void NoteFieldDropdownOutput(int value)
    {
        noteFieldDropdown.value = value;
    }
    
    public void BpmInput(string text)
    {
        float bpm = float.Parse(text);
        notesDirector.BpmSet(bpm);
    }
    public void BpmOutput(float value)
    {
        bpmField.text = value.ToString("F3");
    }

    public void IsDummyToggleInput(bool isOn)
    {
        notesDirector.SetDummy(isOn);
    }
    public void IsDummyToggleOutput(bool isOn)
    {
        isDummyToggle.isOn = isOn;
    }
    
    public void SlideFieldColorDropdownInput(int value)
    {
        notesDirector.SetColor(value);
    }
    public void SlideFieldColorDropdownOutput(int value)
    {
        slideFieldColorDropdown.value = value;
    }
    
    public void IsJudgeToggleInput(bool isOn)
    {
        notesDirector.SetJudge(isOn);
    }
    public void IsJudgeToggleOutput(bool isOn)
    {
        isJudgeToggle.isOn = isOn;
    }
    
    public void IsVariationToggleInput(bool isOn)
    {
        notesDirector.SetVariation(isOn);
    }
    public void IsVariationToggleOutput(bool isOn)
    {
        isVariationToggle.isOn = isOn;
    }
    
    // SpeedTab
    public void SpeedTimeInput(string text)
    {
        float time = float.Parse(text);
        notesDirector.SetSpeedTime(time);
    }
    public void SpeedTimeOutput(float value)
    {
        speedTimeField.text = value.ToString("F3");
    }
    
    public void SpeedSpeedInput(string text)
    {
        float speed = float.Parse(text);
        notesDirector.SetSpeedSpeed(speed);
    }
    public void SpeedSpeedOutput(float value)
    {
        speedSpeedField.text = value.ToString("F2");
    }
    
    public void SpeedIsVariationToggleInput(bool isOn)
    {
        notesDirector.SetSpeedIsVariation(isOn);
    }
    public void SpeedIsVariationToggleOutput(bool isOn)
    {
        speedIsVariationToggle.isOn = isOn;
    }
    
    public void AngleDegreeInput(string text)
    {
        int degree = int.Parse(text);
        notesDirector.SetAngleDegree(degree);
    }
    public void AngleDegreeOutput(int value)
    {
        angleDegreeField.text = value.ToString();
    }
    
    public void AngleVariationInput(string text)
    {
        int variation = (int)(float.Parse(text) * 10);
        notesDirector.SetAngleVariation(variation);
    }
    public void AngleVariationOutput(float value)
    {
        angleVariationField.text = value.ToString("F1");
    }
    
    public void TransparencyAlphaInput(string text)
    {
        int alpha = int.Parse(text);
        notesDirector.SetTransparencyAlpha(alpha);
    }
    public void TransparencyAlphaOutput(float value)
    {
        transparencyAlphaField.text = value.ToString("F2");
    }
    
    public void TransparencyIsVariationToggleInput(bool isOn)
    {
        notesDirector.SetTransparencyIsVariation(isOn);
    }
    public void TransparencyIsVariationToggleOutput(bool isOn)
    {
        transparencyIsVariationToggle.isOn = isOn;
    }
    
    public void SpeedFieldDropdownInput(int value)
    {
        notesDirector.SetSpeedFieldDropdown(value);
    }
    public void SpeedFieldDropdownOutput(int value)
    {
        speedFieldDropdown.value = value;
    }
    
    public void UpdateFieldDropdown(int value)
    {
        noteFieldDropdown.ClearOptions();
        speedFieldDropdown.ClearOptions();
        
        List<string> list = new List<string>();
        for (int i = 0; i < value; i++)
            list.Add(i.ToString());
        
        noteFieldDropdown.AddOptions(list);
        speedFieldDropdown.AddOptions(list);
    }
}
