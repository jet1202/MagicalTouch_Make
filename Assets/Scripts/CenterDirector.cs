using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class CenterDirector : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private AudioClip normalAudio;
    [SerializeField] private AudioClip holdAudio;
    [SerializeField] private AudioClip flickAudio;
    private AudioSource audioSource;
    private bool playing = false;
    private KeyValuePair<float, char> nextTiming;

    public SortedDictionary<int, KeyValuePair<float, char>> NotesData =
        new SortedDictionary<int, KeyValuePair<float, char>>();

    private int preliminaryNum = 0;
    private List<KeyValuePair<float, char>> notesTiming;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartPlayButton()
    {
        preliminaryNum = 0;
        notesTiming = new List<KeyValuePair<float, char>>();
        var hoge = new List<KeyValuePair<float, char>>(NotesData.Values).OrderBy(x => x.Key);
        foreach (var p in hoge)
        {
            notesTiming.Add(p);
        }

        preliminaryNum = 0;
        foreach (var pair in notesTiming)
        {
            if (pair.Key < gameEvent.time) preliminaryNum++;
        }

        if (preliminaryNum < notesTiming.Count)
        {
            nextTiming = notesTiming[preliminaryNum];
            playing = true;
        }
    }

    private void Update()
    {
        if (playing)
        {
            if (!gameEvent.isPlaying) playing = false;

            if (nextTiming.Key <= gameEvent.time)
            {
                _play(nextTiming.Value);
                preliminaryNum++;
                if (preliminaryNum >= notesTiming.Count)
                    playing = false;
                else
                {
                    nextTiming = notesTiming[preliminaryNum];
                }
            }
        }
    }

    private void _play(char tag)
    {
        if (tag == 'N')
        {
            audioSource.PlayOneShot(normalAudio);
        }
        else if (tag == 'H') 
        {
            audioSource.PlayOneShot(holdAudio);
        }
        else if (tag == 'F')
        {
            audioSource.PlayOneShot(flickAudio);
        }
    }
}
