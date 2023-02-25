using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CenterDirector : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private LongNoteMusic longNoteMusic;
    [SerializeField] private AudioClip normalAudio;
    [SerializeField] private AudioClip holdAudio;
    [SerializeField] private AudioClip flickAudio;
    private AudioSource audioSource;
    private bool playing = false;
    private KeyValuePair<float, char> nextTiming;

    public SortedDictionary<int, KeyValuePair<float, KeyValuePair<char, float>>> NotesData =
        new SortedDictionary<int, KeyValuePair<float, KeyValuePair<char, float>>>();

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
        var n = new List<KeyValuePair<float, char>>();
        var hoge = new List<KeyValuePair<float, KeyValuePair<char, float>>>(NotesData.Values);
        foreach (var p in hoge)
        {
            n.Add(new KeyValuePair<float, char>(p.Key, p.Value.Key));
            if (p.Value.Key == 'L')
                n.Add(new KeyValuePair<float, char>(p.Key + p.Value.Value, 'E'));
            Debug.Log($"{p.Key}, {p.Value.Key}, {p.Value.Value}");
        }

        var h = n.OrderBy(x => x.Key);
        foreach (var p in h)
        {
            notesTiming.Add(p);
            Debug.Log($"{p.Key}, {p.Value}");
        }

        int longNumber = 0;
        preliminaryNum = 0;
        foreach (var pair in notesTiming)
        {
            if (pair.Key < gameEvent.time) preliminaryNum++;
            if (pair.Value == 'L') longNumber++;
            else if (pair.Value == 'E') longNumber--;
        }

        longNoteMusic.longNumber = longNumber;

        if (preliminaryNum < notesTiming.Count)
        {
            nextTiming = notesTiming[preliminaryNum];
            playing = true;
            longNoteMusic.playing = true;
        }
    }

    private void Update()
    {
        if (playing)
        {
            if (!gameEvent.isPlaying)
            {
                playing = false;
                longNoteMusic.playing = false;
                longNoteMusic.GetComponent<AudioSource>().Stop();
                longNoteMusic.longNumber = 0;
            }

            if (nextTiming.Key <= gameEvent.time)
            {
                char val = nextTiming.Value;
                _play(val);
                if (val == 'L')
                    longNoteMusic.longNumber++;
                else if (val == 'E')
                    longNoteMusic.longNumber--;
                preliminaryNum++;
                
                // ノーツの種類に応じてLongNoteMusicを再生
                
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
        if (tag == 'N' || tag == 'L')
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
