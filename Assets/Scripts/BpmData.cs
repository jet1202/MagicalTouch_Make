using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BpmData : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;

    private GameObject flame;
    private GameObject bpmText;

    public void DefaultSettings(float time, float bpm)
    {
        flame = transform.GetChild(0).gameObject;
        bpmText = transform.GetChild(1).gameObject;
        
        bpmText.transform.GetComponent<MeshRenderer>().sortingLayerName = "Important";
        bpmText.transform.GetComponent<MeshRenderer>().sortingOrder = 0;

        transform.localPosition = new Vector3(gameEvent.speed * time, 0f, 0f);
        bpmText.GetComponent<TextMeshPro>().text = bpm.ToString("F");
    }

    public void Choose()
    {
        flame.SetActive(true);
    }

    public void DisChoose()
    {
        flame.SetActive(false);
    }

    public void ChangeTime(float time)
    {
        transform.localPosition = new Vector3(gameEvent.speed * time, 0f, 0f);
    }

    public void ChangeBpm(float bpm)
    {
        bpmText.GetComponent<TextMeshPro>().text = bpm.ToString("F4");
    }

    public void ClearBpm()
    {
        Destroy(this.gameObject);
    }
}
