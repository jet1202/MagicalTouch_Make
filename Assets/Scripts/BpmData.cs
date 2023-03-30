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

    private void Start()
    {
        flame = transform.GetChild(1).gameObject;
        bpmText = transform.GetChild(2).gameObject;
    }

    public void DefaultSettings(float time, int bpm)
    {
        // bpmText.transform.GetComponent<MeshRenderer>().sortingLayerName = "Important";
        // bpmText.transform.GetComponent<MeshRenderer>().sortingOrder = 0;

        transform.localPosition = new Vector3(gameEvent.speed * time, 0f, 0f);
        bpmText.GetComponent<TextMeshProUGUI>().text = bpm.ToString();
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

    public void ChangeBpm(int bpm)
    {
        bpmText.GetComponent<TextMeshProUGUI>().text = bpm.ToString();
    }

    public void ClearBpm()
    {
        Destroy(this.gameObject);
    }
}
