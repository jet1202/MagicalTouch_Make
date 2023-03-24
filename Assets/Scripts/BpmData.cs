using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BpmData : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    
    public void DefaultSettings(float time, int bpm)
    {
        
    }

    public void Change(float time, int bpm)
    {
        
    }

    public void Choose()
    {
        
    }

    public void DisChoose()
    {
        
    }

    public void ChangeTime(float time)
    {
        
    }

    public void ChangeBpm(int bpm)
    {
        
    }

    public void ClearBpm()
    {
        Destroy(this.gameObject);
    }
}
