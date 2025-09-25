using System.Collections.Generic;
using UnityEngine;

public enum Sound { BGM, SFX }

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume")]
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;

    private float prevBgmVolume;
    private float prevSfxVolume;

    public event System.Action<Sound, float> OnVolumeChanged;

#if UNITY_EDITOR
    private void OnValidate()
    {
        var bgmList = new List<AudioClip>();
        LoadSound(bgmList, Sound.BGM);
        bgmClips = bgmList.ToArray();

        var sfxList = new List<AudioClip>();
        LoadSound(sfxList, Sound.SFX);
        sfxClips = sfxList.ToArray();
    }

    private static void LoadSound(List<AudioClip> _list, Sound _type)
    {
        _list.Clear();
        string path = "Sounds/" + (_type == Sound.BGM ? "BGMs" : "SFXs");
        var clips = Resources.LoadAll<AudioClip>(path);
        if (clips != null && clips.Length > 0)
            _list.AddRange(clips);
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetAudio();
    }

    private void Start()
    {
        PlayBGM();
    }

    #region 배경음
    public void PlayBGM(AudioClip _clip = null)
    {
        if (_clip != null) bgmSource.clip = _clip;
        bgmSource.Play();
    }
    public void PlayBGM(string _name)
    {
        for (int i = 0; i < bgmClips.Length; i++)
        {
            var c = bgmClips[i];
            if (c != null && c.name == _name)
            {
                PlayBGM(c);
                return;
            }
        }
    }

    public void StopBGM() => bgmSource.Stop();

    public void ToggleBGM()
    {
        if (!IsBGMMuted() && bgmVolume > 0f)
        {
            prevBgmVolume = bgmVolume;
            SetBGMVolume(0f);
        }
        else
        {
            float target = prevBgmVolume > 0.2f ? prevBgmVolume : 0.2f;
            SetBGMVolume(target);
        }
    }
    #endregion

    #region 효과음
    public void PlaySFX(AudioClip _clip) => sfxSource.PlayOneShot(_clip);
    public void PlaySFX(string _name)
    {
        for (int i = 0; i < sfxClips.Length; i++)
        {
            var c = sfxClips[i];
            if (c != null && c.name == _name)
            {
                PlaySFX(c);
                return;
            }
        }
    }

    public void ToggleSFX()
    {
        if (!IsSFXMuted() && sfxVolume > 0f)
        {
            prevSfxVolume = sfxVolume;
            SetSFXVolume(0f);
        }
        else
        {
            float target = prevSfxVolume > 0.2f ? prevSfxVolume : 0.2f;
            SetSFXVolume(target);
        }
    }

    public void GameOver() => PlaySFX("GameOver");

    public void Shoot() => PlaySFX("Shoot");
    public void Merge(int _id = 0) => PlaySFX(_id != SpawnManager.Instance.GetFinal() ? "Merge" : "Flame");

    public void Button() => PlaySFX("Button");
    #endregion

    #region SET
    private void SetAudio()
    {
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;

        bgmSource.loop = true;
        if (bgmClips != null && bgmClips.Length > 0) bgmSource.clip = bgmClips[0];

        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }

    public void SetBGMVolume(float _volume = 1)
    {
        bgmVolume = Mathf.Clamp01(_volume);
        bgmSource.volume = bgmVolume;
        bgmSource.mute = (bgmVolume <= 0f);

        if (bgmVolume > 0f) prevBgmVolume = bgmVolume;

        OnVolumeChanged?.Invoke(Sound.BGM, bgmVolume);
    }

    public void SetSFXVolume(float _volume = 1)
    {
        sfxVolume = Mathf.Clamp01(_volume);
        sfxSource.volume = sfxVolume;
        sfxSource.mute = (sfxVolume <= 0f);

        if (sfxVolume > 0f) prevSfxVolume = sfxVolume;

        OnVolumeChanged?.Invoke(Sound.SFX, sfxVolume);
    }
    #endregion

    #region GET
    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;

    public bool IsBGMMuted() => bgmSource != null && bgmSource.mute;
    public bool IsSFXMuted() => sfxSource != null && sfxSource.mute;
    #endregion
}
