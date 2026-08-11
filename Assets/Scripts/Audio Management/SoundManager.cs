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
    HORSE_SNORT,
    LANCE_HIT,
    SHIELD_RAISE,
    SHIELD_LOWER,
    SHIELD_HIT,
    SHIELD_BREAK,
    APPLAUSE, 
    CAMERA_180TURN,
    INTERACT_SOUND,
    MENU_BG_MUSIC,
    MATCH_BG_MUSIC,
    TRIUMPH,
    GASP,
    HORSE_BRAKE
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
    [SerializeField] private float globalVolume;
    [Header("")]
    [SerializeField] private SoundList[] soundList;

    public static SoundManager Instance;
    private AudioSource audioSource;
    private AudioSource player1AS;
    private AudioSource player2AS;

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
    /// Only used as a workaround since this file has the ExecuteInEditMode property.
    /// </summary>
    /// <param name="player1"></param>
    /// <param name="player2"></param>
    public void Setup(GameObject player1, GameObject player2)
    {
        player1AS = player1.GetComponent<AudioSource>();
        player2AS = player2.GetComponent<AudioSource>();
    }

    #region Public functions

    /// <summary>
    /// Sets new volume of all audio sources.
    /// </summary>
    /// <param name="newValue">The new value [0; 1]</param>
    public void SetVolume(float newValue)
    {
        globalVolume = newValue;
        audioSource.volume = globalVolume;
        player1AS.volume = globalVolume;
        player2AS.volume = globalVolume;
    }

    /// <summary>
    /// Getter function for the global volume.
    /// </summary>
    /// <returns>The overall volume of all soundtracks and effects.</returns>
    public float GetVolume()
    {
        return globalVolume;
    }

    /// <summary>
    /// Plays a one-shot of a random sound from the pool of sounds associated with that name.
    /// </summary>
    /// <param name="sound">The specific sound to be played.</param>
    /// <param name="volume">The volume of the played sound. 1 (max) by default.</param>
    /// <param name="specificIndex">Which exact clip from the group of clips associated with given sound should be chosen.</param>
    public void PlaySound(SoundType sound, float volume = 1, int specificIndex = -1)
    { 
        AudioClip clip = SelectClip(sound, specificIndex);
        Instance.audioSource.PlayOneShot(clip, volume * globalVolume);
    }

    /// <summary>
    /// Plays a random sound from the pool of sounds associated with that name. Can be interrupted.
    /// </summary>
    /// <param name="sound">The specific sound to be played.</param>
    /// <param name="volume">The volume of the played sound. 1 (max) by default.</param>
    /// <param name="sourceIndex">The audio source which should play the sound.</param>
    /// <param name="specificIndex">Which exact clip from the group of clips associated with given sound should be chosen.</param>
    public void PlayLongSound(SoundType sound, float volume = 1, int sourceIndex = 0, int specificIndex = -1)
    {
        AudioClip chosenClip = SelectClip(sound, specificIndex);
        AudioSource source = Instance.audioSource;
        if (sourceIndex == 1) { source = Instance.player1AS; }
        if (sourceIndex == 2) { source = Instance.player2AS; }

        source.clip = chosenClip;
        source.volume = volume * globalVolume;
        source.Play();
    }

    /// <summary>
    /// This method instructs the sound manager to stop playing the currently played sound.
    /// </summary>
    /// <param name="sourceIndex">The audio source which should be interrupted.</param>
    public void InterruptPlayingSound(int sourceIndex = 0)
    {
        AudioSource source = Instance.audioSource;
        if (sourceIndex == 1) { source = Instance.player1AS; }
        if (sourceIndex == 2) { source = Instance.player2AS; }

        if (!source.isPlaying) { return; }

        source.Stop();
        source.clip = null;
    }
    #endregion

    /// <summary>
    /// This function selects the clip of a given sound type, using either the given index, or randomly.
    /// </summary>
    /// <param name="sound">Sound type of the clip to be selected.</param>
    /// <param name="specificIndex">Index of a specific audioclip. -1 if random.</param>
    /// <returns>The chosen audioclip.</returns>
    AudioClip SelectClip(SoundType sound, int specificIndex)
    {
        AudioClip randomClip;
        AudioClip[] clips = Instance.soundList[(int)sound].Sounds;

        if (clips.Length == 0)
        {
            Debug.LogWarning("The sound " + sound.ToString() + " cannot be played as it has no audio clips assigned.");
            return null;
        }

        if (specificIndex < 0)
        {
            randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        }
        else
        {
            randomClip = clips[specificIndex];
        }

        if (randomClip == null)
        {
            Debug.LogWarning("The sound " + sound.ToString() + " cannot be played as it has no audio clips assigned.");
            return null;
        }
        return randomClip;
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

