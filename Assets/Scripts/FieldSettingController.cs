using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FieldSettingController : MonoBehaviour
{
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private UserIO userIO;
    [SerializeField] private Speeds speedsDirector;
    [SerializeField] private LinePreview linePreview;
    [SerializeField] private GameObject fieldsContentsPrefab;

    [SerializeField] private GameObject scrollView;
    private GameObject viewport;
    private Transform content;
    
    [NonSerialized] public int fieldsCount = 1;
    [NonSerialized] public List<bool> fieldsIsDummy = new List<bool>();

    private int fieldNumber = 0;

    private void Start()
    {
        content = scrollView.transform.GetChild(0).GetChild(0);
        
        AddIsDummy();
        RenewalField();
    }

    public void RenewalField()
    {
        foreach (Transform obj in content)
        {
            Destroy(obj.gameObject);
        }

        for (int i = 0; i < fieldsCount; i++)
        {
            GameObject contents = Instantiate(fieldsContentsPrefab, content);
            int index = i;
            
            contents.name = i.ToString();
            contents.transform.GetChild(1).GetComponent<Text>().text = i.ToString();
            contents.transform.GetChild(0).GetComponent<Image>().color = notesDirector.FieldColor(i);
            contents.GetComponent<Button>().onClick.AddListener(() => userIO.FieldScrollViewSelect(index));
            contents.SetActive(true);
        }
        ContentsColorSetting();
        
        // DropDownの要素数の変更
        userIO.UpdateFieldDropdown(fieldsCount);
        
        linePreview.ChangeField(fieldsCount);
    }

    public void AddField()
    {
        fieldsCount++;
        AddIsDummy();
        RenewalField();
        
        speedsDirector.NewField();
    }

    public void DeleteField(int number)
    {
        fieldsCount--;
        DeleteIsDummy(number);
        RenewalField();
        
        speedsDirector.DeleteField(number);
        notesDirector.DeleteFieldNoteChange(number);
    }

    public void SelectField(int number)
    {
        fieldNumber = number;
        userIO.FieldIsDummyToggleOutput(fieldsIsDummy[number]);
        ContentsColorSetting();
    }

    public void SetIsDummy(bool isOn)
    {
        fieldsIsDummy[fieldNumber] = isOn;
        ContentsColorSetting();
        notesDirector.NoteColorSetting();
    }

    private void AddIsDummy()
    {
        fieldsIsDummy.Add(false);
    }
    
    private void DeleteIsDummy(int number)
    {
        fieldsIsDummy.RemoveAt(number);
    }

    public bool FieldIsDummy(int number)
    {
        return fieldsIsDummy[number];
    }

    private void ContentsColorSetting()
    {
        for (int i = 0; i < fieldsCount; i++)
        {
            GameObject obj = content.GetChild(i).gameObject;
            Color c = fieldsIsDummy[i]
                ? new Color(100f / 255f, 100f / 255f, 100f / 255f)
                : new Color(50f / 255f, 50f / 255f, 50f / 255f);
            Color c2 = i == fieldNumber
                ? new Color(1f, 1f, 1f, 100f / 255f)
                : new Color(1f, 1f, 1f, 0f);

            obj.transform.GetChild(1).GetComponent<Text>().color = c;
            obj.transform.GetComponent<Image>().color = c2;
        }
    }
}
