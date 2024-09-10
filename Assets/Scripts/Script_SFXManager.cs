using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_SFXManager : MonoBehaviour
{
    public static Script_SFXManager instance;

    [SerializeField] private GameObject SFXObject;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void PlaySFXClip(AudioClip sfx, Transform spawnTransform, float volume)
    {
        GameObject audioSourceObject = Instantiate(SFXObject, spawnTransform.position, Quaternion.identity);

        AudioSource audioSource = audioSourceObject.GetComponent<AudioSource>();

        audioSource.clip = sfx;

        audioSource.volume = volume;

        audioSource.transform.parent = transform;
        audioSource.Play();
        
        float clipLength = audioSource.clip.length;
        
        Destroy(audioSourceObject, clipLength);
    }

    public void PlayRandomSFXClip(AudioClip[] sfx, Transform spawnTransform, float volume)
    {
        int rand = Random.Range(0, sfx.Length);

        GameObject audioSourceObject = Instantiate(SFXObject, spawnTransform.position, Quaternion.identity);

        AudioSource audioSource = audioSourceObject.GetComponent<AudioSource>();

        audioSource.clip = sfx[rand];

        audioSource.volume = volume;

        audioSource.transform.parent = transform;
        audioSource.Play();

        float clipLength = audioSource.clip.length;
  
        Destroy(audioSourceObject, clipLength);
    }
}
