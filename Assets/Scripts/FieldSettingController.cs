using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    
    public int fieldsCount = 1;

    private void Start()
    {
        content = scrollView.transform.GetChild(0).GetChild(0);
        
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

            contents.name = i.ToString();
            contents.transform.GetChild(1).GetComponent<Text>().text = i.ToString();
            contents.transform.GetChild(0).GetComponent<Image>().color = notesDirector.FieldColor(i);
        }
        
        // DropDownの要素数の変更
        userIO.UpdateFieldDropdown(fieldsCount);
        
        linePreview.ChangeField(fieldsCount);
    }

    public void AddField()
    {
        fieldsCount++;
        RenewalField();
        
        speedsDirector.NewField();
    }

    public void DeleteField(int number)
    {
        fieldsCount--;
        RenewalField();
        
        speedsDirector.DeleteField(number);
        notesDirector.DeleteFieldNoteChange(number);
    }
}
