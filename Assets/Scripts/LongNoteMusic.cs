using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongNoteMusic : MonoBehaviour
{
    private AudioSource audioSource;
    public int longNumber = 0;
    public bool playing = false;
    private bool isPlayingMusic = false;

    private void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (playing)
        {
            // 再生処理(Long)
            if (isPlayingMusic)
            {
                if (longNumber == 0)
                {
                    audioSource.Stop();
                    isPlayingMusic = false;
                }
            }
            else
            {
                if (longNumber != 0)
                {
                    audioSource.Play();
                    isPlayingMusic = true;
                }
            }
        }
        else
        {
            if (isPlayingMusic)
            {
                audioSource.Stop();
                isPlayingMusic = false;
                longNumber = 0;
            }
        }
    }
}
