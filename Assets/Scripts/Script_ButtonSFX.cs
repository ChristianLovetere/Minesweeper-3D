using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_ButtonSFX : MonoBehaviour
{
    [SerializeField] private AudioClip[] _audioClips;
    public void PlayButtonSound()
    {
        Script_SFXManager.instance.PlayRandomSFXClip(_audioClips, transform, 1f);
    }
}
