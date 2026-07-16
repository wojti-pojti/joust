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
    LANCE_HIT,
    SHIELD_RAISE,
    SHIELD_LOWER,
    SHIELD_HIT,
    SHIELD_BREAK,
    MATCH_BEGINNING,
    MATCH_END,
    NEW_TURN, // applause
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
    [SerializeField] AudioClip[] sounds;
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] SoundList[] soundList;

    public static SoundManager Instance;
    AudioSource audioSource;
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

    public void PlaySound(SoundType sound, float volume = 1, int specificIndex = -1)
    { // plays random sound from the pool of sounds associated with that name

        AudioClip randomClip;
        AudioClip[] clips = Instance.soundList[(int)sound].Sounds;

        if (specificIndex < 0) randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        else randomClip = clips[specificIndex];

        Instance.audioSource.PlayOneShot(randomClip, volume);
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

