using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// basically a list of all sounds
public enum SoundType
{
    HORSE_JOG,
    HORSE_GALLOP,
    HORSE_JUMP,
    HORSE_LAND,
    LANCE_HIT,
    SHIELD_RAISE,
    SHIELD_LOWER,
    SHIELD_HIT,
    SHIELD_BREAK,
    APPLAUSE, 
    CAMERA_180TURN,
    INTERACT_SOUND,
    MENU_BG_MUSIC,
    MATCH_BG_MUSIC
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    public float globalVolume;
    [Header("")]
    [SerializeField] private SoundList[] soundList;

    public static SoundManager Instance;
    private AudioSource audioSource;
    private void Awake()
    {
        if (Instance == null) // only one instance of this script permitted
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays random sound from the pool of sounds associated with that name.
    /// </summary>
    /// <param name="sound">The specific sound to be played.</param>
    /// <param name="volume">The volume of the played sound. 1 (max) by default.</param>
    /// <param name="specificIndex">Which exact clip from the group of clips associated with given sound should be chosen.</param>
    public void PlaySound(SoundType sound, float volume = 1, int specificIndex = -1)
    { 
        AudioClip randomClip;
        AudioClip[] clips = Instance.soundList[(int)sound].Sounds;

        if (clips.Length == 0)
        {
            Debug.LogWarning("The sound " + sound.ToString() + " cannot be played as it has no audio clips assigned.");
            return;
        }

        if (specificIndex < 0)
        {
            randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        }
        else
        { 
            randomClip = clips[specificIndex];
        }

        if(randomClip == null)
        {
            Debug.LogWarning("The sound " + sound.ToString() + " cannot be played as it has no audio clips assigned.");
            return;
        }

        Instance.audioSource.PlayOneShot(randomClip, volume * globalVolume);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        // names all serialized fields accordingly

        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}

