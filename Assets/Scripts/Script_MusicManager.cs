using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class Script_MusicManager : MonoBehaviour
{
    public static Script_MusicManager instance; // Singleton pattern

    [SerializeField] private AudioSource SFXObject;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private AudioMixerGroup audioMixer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Prevent destruction on scene load
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
        PlayAudio();
    }

    public void PlayAudio()
    {
        AudioSource audioSource = Instantiate(SFXObject, transform.position, Quaternion.identity);
        audioSource.transform.parent = transform;

        int rand = Random.Range(0, audioClips.Length);
        audioSource.outputAudioMixerGroup = audioMixer;
        audioSource.clip = audioClips[rand];

        audioSource.loop = true;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource, clipLength);
    }

    public void StopAudio()
    {

    }
}
