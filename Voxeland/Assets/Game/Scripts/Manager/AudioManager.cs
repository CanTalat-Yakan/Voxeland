using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] internal AudioInfo m_AudioInfo;
    [SerializeField] internal AudioMixer m_AudioMixer;
    AudioSource m_mainMusicSource;
    float m_volumeOffset = 100, m_tmpVolume = 14;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance is null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (m_AudioInfo.Music is null)
            StartCoroutine(StreamMainMusic("http://voxeland.xyz/bgmusic/minecraft-background-music.mp3", AudioType.MPEG)); //alternative: .ogg")
        else
            PlayMainMusic(0.14f);
    }

    IEnumerator StreamMainMusic(string _link, AudioType _type = AudioType.OGGVORBIS)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(_link, _type))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                if (Mirror.ChatWindow.Instance)
                    Mirror.ChatWindow.Instance.OnServerMessage("Music downloaded failed!");
                Debug.Log(www.error);
            }
            else
                m_mainMusicSource = Play(DownloadHandlerAudioClip.GetContent(www), true, 0.69f);
        }
    }
    internal AudioSource Play(AudioClip _clip, bool _loop = false, float _volume = 1)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.clip = _clip;
        audioSource.loop = _loop;
        audioSource.volume = _volume;
        audioSource.spatialBlend = 0.1f;
        audioSource.maxDistance = 130;
        audioSource.spread = 1;
        audioSource.dopplerLevel = 0;
        audioSource.reverbZoneMix = 1;
        audioSource.pitch = 1;
        audioSource.Play();
        Destroy(audioSource, _clip.length);

        return audioSource;
    }
    internal static AudioSource Play(AudioSource _source, AudioClip _clip, AudioMixer _mixer = null, bool? _loop = null, float? _volume = null, float? _pitch = null)
    {
        _source.clip = _clip;
        if (_mixer) _source.outputAudioMixerGroup = _mixer.outputAudioMixerGroup;
        if (_loop != null) _source.loop = _loop.Value;
        if (_volume != null) _source.volume = _volume.Value;
        if (_pitch != null) _source.pitch = _pitch.Value;
        _source.Play();

        return _source;
    }

    internal AudioSource[] PlaySequence(params AudioClip[] _clips)
    {
        AudioSource[] audiosources = new AudioSource[_clips.Length];

        for (int i = 0; i < _clips.Length; i++)
        {
            audiosources[i] = gameObject.AddComponent<AudioSource>();
            audiosources[i].clip = _clips[i];

            audiosources[i].volume = 1;
            audiosources[i].rolloffMode = AudioRolloffMode.Custom;
            audiosources[i].spatialBlend = 0.1f;
            audiosources[i].maxDistance = 130;
            audiosources[i].spread = 1;
            audiosources[i].dopplerLevel = 0;
            audiosources[i].reverbZoneMix = 1;
            audiosources[i].pitch = 1;

            ulong delay = 0;
            for (int j = 0; j < i; j++)
                delay += (ulong)_clips[j].length;

            audiosources[i].Play(delay);
            Destroy(audiosources[i], _clips[i].length);
        }

        return audiosources;
    }

    internal void PlayMainMusic(float _volume = 1)
    {
        if (m_mainMusicSource || m_AudioInfo.Music is null)
            return;

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.clip = PlayRandomFromList(ref m_AudioInfo.Music);
        audioSource.volume = m_tmpVolume = _volume;
        audioSource.pitch = 1;
        audioSource.loop = true;
        audioSource.Play();
        m_mainMusicSource = audioSource;
    }
    internal void StopMainMusic() { Destroy(m_mainMusicSource); }
    internal void SetMainMusicVolume(float _percentage) { m_tmpVolume = _percentage; if (m_mainMusicSource) m_mainMusicSource.volume = _percentage * m_volumeOffset * 0.01f; }
    internal float GetMainMusicVolume() { return m_mainMusicSource ? m_mainMusicSource.volume : 0.14f; }
    internal void SetMainMusicVolumeOffset(float _percentage) { m_volumeOffset = _percentage; SetMainMusicVolume(m_tmpVolume); }

    internal static T PlayRandomFromList<T>(ref T[] _list) { return _list[Random.Range(0, _list.Length)]; }
}