using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticeController : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private Image panel;
    [SerializeField] private Text title;
    [SerializeField] private Text notice;
    
    public void OpenNotice(int types, string name)
    {
        switch (types)
        {
            case 0:
                title.text = "Notice";
                panel.color = new Color(209 / 255f, 255 / 255f, 209 / 255f);
                break;
            case 1:
                title.text = "Error";
                panel.color = new Color(255 / 255f, 209 / 255f, 209 / 255f);
                break;
        }

        notice.text = name;
        gameEvent.isOpenTab = true;
        this.gameObject.SetActive(true);
    }

    public void CloseNotice()
    {
        this.gameObject.SetActive(false);
        gameEvent.isOpenTab = false;
    }
}
