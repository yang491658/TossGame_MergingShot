using UnityEngine;

public enum Sound { BGM, SFX }

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private float prevBgmVolume = 1f;
    private float prevSfxVolume = 1f;

    public event System.Action<Sound, float> OnVolumeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        bgmSource.loop = true;

        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }

    #region 배경음
    public void PlayBGM(AudioClip _clip = null)
    {
        if (_clip != null) bgmSource.clip = _clip;
        if (bgmSource == null || bgmSource.clip == null) return;
        bgmSource.Play();
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
    public void PlaySFX(AudioClip _clip, float _volume = 1f)
    {
        if (sfxSource == null || _clip == null) return;
        sfxSource.PlayOneShot(_clip, Mathf.Clamp01(_volume));
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
    #endregion

    #region SET
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
