using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSound : MonoBehaviour
{
    AudioSource m_ambient;
    AudioInfo m_info;
    AudioClip[] m_clips = new AudioClip[1];
    internal AmbientTypes m_currentType = AmbientTypes.SURFACE;

    void Start() { m_ambient = GetComponent<AudioSource>(); m_info = AudioManager.Instance.m_AudioInfo; }

    ///Different Heigths play different Ambient Sounds
    ///Main Music lowers till mute of Volume when underground
    void Update()
    {
        if (!GameManager.Instance) return;
        if (!GameManager.Instance.m_MainCamera) return;
        float pos = GameManager.Instance.m_MainCamera.transform.position.y;

        if (pos > 38)
            SetSound(AmbientTypes.SKY);
        else if (pos > 0.3f)
            SetSound(AmbientTypes.SURFACE);
        else if (pos > -15)
            SetSound(AmbientTypes.WATER);
        else if (pos > -19.7)
            SetSound(AmbientTypes.LAVA);
        else if (pos > -45)
            SetSound(AmbientTypes.UNDERGROUND);
        else
            SetSound(AmbientTypes.CAVE);


        if (!AudioManager.Instance) return;
        AudioManager.Instance.SetMainMusicVolumeOffset(GameManager.Map(pos, -40, -20, 0, 100));
    }

///Helper that get the according Clio from an Enum and plays it
    void SetSound(AmbientTypes _types)
    {
        switch (_types)
        {
            case AmbientTypes.SKY: m_clips = m_info.Ambient[0].clips; break;
            case AmbientTypes.SURFACE: m_clips = m_info.Ambient[1].clips; break;
            case AmbientTypes.WATER: m_clips = m_info.Ambient[2].clips; break;
            case AmbientTypes.LAVA: m_clips = m_info.Ambient[3].clips; break;
            case AmbientTypes.UNDERGROUND: m_clips = m_info.Ambient[4].clips; break;
            case AmbientTypes.CAVE: m_clips = m_info.Ambient[5].clips; break;
            default: break;
        }

        if (!m_ambient.isPlaying || m_currentType != _types)
        {
            m_ambient = AudioManager.Play(m_ambient, AudioManager.PlayRandomFromList(ref m_clips));
            m_ambient.outputAudioMixerGroup = AudioManager.Instance.m_AmbientMixer;
        }

        m_currentType = _types;
    }
}
