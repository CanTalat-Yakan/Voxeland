using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public Audio_Info m_AudioInfo;
    AudioSource m_mainMusicSource;
    float m_tmpVolume;


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

        if (m_AudioInfo.MainMusic is null)
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
    public AudioSource Play(AudioClip _clip, bool _loop = false, float _volume = 1)
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

    public AudioSource[] PlaySequence(params AudioClip[] _clips)
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

    public void PlayMainMusic(float _volume = 1)
    {
        if (m_mainMusicSource || m_AudioInfo.MainMusic is null)
            return;

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.clip = PlayRandomFromList(ref m_AudioInfo.MainMusic);
        audioSource.volume = m_tmpVolume = _volume;
        audioSource.pitch = 1;
        audioSource.loop = true;
        audioSource.Play();
        m_mainMusicSource = audioSource;
    }
    public void StopMainMusic()
    {
        Destroy(m_mainMusicSource);
    }
    public void SetMainMusicVolume(float _percentage)
    {
        if (m_mainMusicSource)
            m_mainMusicSource.volume = _percentage;
    }
    public float GetMainMusicVolume()
    {
        return m_mainMusicSource ? m_mainMusicSource.volume : 0.69f;
    }
    public void ResetMainMusicVolume()
    {
        m_mainMusicSource.volume = m_tmpVolume;
    }

    T PlayRandomFromList<T>(ref T[] _list) { return _list[Random.Range(0, _list.Length)]; }
}