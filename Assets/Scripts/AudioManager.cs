using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public AudioClip track1, track2;
    public bool StopPlaying = true;
    public bool loop = true;
    public bool playTrack1 = false;
    public bool playTrack2 = false;
    [Range(0, 1f)]
    public float volume = .4f;
    public bool mute = true;

    AudioSource audio;
	// Use this for initialization
	void Start () {
        audio = GetComponent<AudioSource>();
        audio.clip = track1;
        audio.loop = true;
        audio.Play();
	}
	
	// Update is called once per frame
	void Update () {
        if (mute)
        {
            mute = false;
            volume = 0;
        }
        audio.volume = volume * volume;
        audio.loop = loop;
        if (playTrack1)
        {
            audio.clip = track1;
            audio.Play();
        }
        else if (playTrack2)
        {
            audio.clip = track2;
            audio.Play();
        }
        if (StopPlaying)
        {
            audio.Stop();
        } else
        {
            audio.Play();
        }
        StopPlaying = false;
        playTrack1 = false;
        playTrack2 = false;
	}
}
