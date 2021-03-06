using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;

public class SettingsManager : MonoBehaviour
{
    [Header("Quality Settings")]
    [SerializeField] private ForwardRendererData m_rendererData;
    [SerializeField] private UniversalRenderPipelineAsset[] m_graphicsTier;
    [SerializeField] private string[] m_graphicsTierString = new string[4] { "Low", "Medium", "High", "Epic" };
    [SerializeField] private Button m_currentGraphicsTier;

    [Header("Other Settings")]
    [SerializeField] private VolumeProfile m_postprocessingData;
    [SerializeField] private GameObject m_quit;
    [SerializeField] private SettingsContainer m_settings;
    [SerializeField] private RenderTexture m_renderTexture;
    [Header("Music")]
    [SerializeField] private Slider m_masterVolume;
    [SerializeField] private Slider m_musicVolume, m_ambientVolume, m_effectsVolume;
    [Header("Video")]
    [SerializeField] private Slider m_renderDistance;
    [SerializeField] private Slider m_renderScale;
    [Header("Gameplay")]
    [SerializeField] private Slider m_fieldOfView;
    [SerializeField] private Slider m_mouseSensitivity;

    [Space]
    [Header("Text Mesh Pro's")]
    [SerializeField]
    private TextMeshProUGUI m_masterVolumeTMPro;
    [SerializeField]
    private TextMeshProUGUI
    m_musicVolumeTMPro, m_ambientVolumeTMPro, m_effectsVolumeTMPro,
    m_fieldOfViewTMPro, m_mouseSensitivityTMPro,
    m_renderDistanceTMPro, m_lodTMPro, m_renderScaleTMPro, m_currentGraphicsTierTMPro,
    m_ssaoTMPro, m_aaTMPro, m_bloomTMPro, m_depthOfFieldTMPro;


    void Start()
    {
        if (GameManager.Instance is null)
            m_quit.SetActive(false);

        ResetAll();
    }

    void ResetAll()
    {
        m_masterVolume.value = AudioListener.volume * 100;
        SetMasterVolume();
        m_musicVolume.value = AudioManager.Instance.GetMainMusicVolume() * 100;
        SetMusicVolume();
        m_ambientVolume.value = GameManager.Map(m_settings.AmbientVolume, -35, 20, 0, 100);
        SetAmbientVolume();

        m_effectsVolume.value = GameManager.Map(m_settings.EffectsVolume, -35, 20, 0, 100);
        SetEffectsVolume();

        m_fieldOfView.value = m_settings.FOV;
        SetFOV();
        m_mouseSensitivity.value = m_settings.MouseSensitivity;
        SetMouseSensitivity();
        m_renderDistance.value = m_settings.RenderDistance;
        SetRenderDistance();
        m_settings.LOD--;
        SetLOD();
        m_renderScale.value = m_settings.RenderScale;
        SetRenderScale();
        m_settings.AA--;
        SetAA();

        m_currentGraphicsTierTMPro.SetText($"Graphics: {m_graphicsTierString[m_settings.CurrentPipelineAssetIndex].ToString()}");
        m_ssaoTMPro.SetText($"SSAO: {(m_settings.SSAO ? "ON" : "OFF")}");
        m_bloomTMPro.SetText($"Bloom: {(m_settings.Bloom ? "ON" : "OFF")}");
        m_depthOfFieldTMPro.SetText($"Depth of Field: {(m_settings.DepthOfField ? "ON" : "OFF")}");
    }

    public void SetFOV()
    {
        m_settings.FOV = (int)m_fieldOfView.value;
        m_fieldOfViewTMPro.text = "FOV: " + m_settings.FOV.ToString();
    }

    public void SetMouseSensitivity()
    {
        m_settings.MouseSensitivity = (float)Math.Round((double)m_mouseSensitivity.value, 2);
        m_mouseSensitivityTMPro.text = "MouseSensitivity: " + m_settings.MouseSensitivity.ToString() + "x";
    }

    public void SetMasterVolume()
    {
        AudioListener.volume = m_masterVolume.value * 0.01f;
        m_masterVolumeTMPro.SetText($"Master Volume: {m_masterVolume.value.ToString()}%");
    }
    public void SetMusicVolume()
    {
        AudioManager.Instance.SetMainMusicVolume(m_musicVolume.value * 0.01f);
        m_musicVolumeTMPro.SetText($"Music: {m_musicVolume.value.ToString()}%");
    }
    public void SetAmbientVolume()
    {
        AudioManager.Instance.m_AmbientMixer.audioMixer.SetFloat("AmbientVolume", m_settings.AmbientVolume = GameManager.Map(m_ambientVolume.value, 0, 100, -35, 20));
        m_ambientVolumeTMPro.SetText($"Ambient: {m_ambientVolume.value.ToString()}%");
    }
    public void SetEffectsVolume()
    {
        AudioManager.Instance.m_FXMixer.audioMixer.SetFloat("EffectsVolume", m_settings.EffectsVolume = GameManager.Map(m_effectsVolume.value, 0, 100, -35, 20));
        m_effectsVolumeTMPro.SetText($"Effects: {m_effectsVolume.value.ToString()}%");
    }

    public void SetRenderDistance()
    {
        m_settings.RenderDistance = (int)m_renderDistance.value;
        m_renderDistanceTMPro.SetText($"Render: {(1 << (6 + (int)m_settings.RenderDistance)) / Chunk.SIZE} Chunks");

        VoxelGeneration.UpdateNow = true;
    }
    public void SetLOD()
    {
        m_settings.LOD++;

        if (m_settings.LOD > 5)
            m_settings.LOD = 0;

        if (m_settings.LOD == 0)
            m_lodTMPro.SetText($"NO LOD");
        else
            m_lodTMPro.SetText($"LOD: {m_settings.LOD }");

        VoxelGeneration.UpdateNow = true;
    }
    public void SetRenderScale()
    {
        if (GameManager.Instance)
            if (GameManager.Instance.m_MainCamera)
                if (GameManager.Instance.m_MainCamera.targetTexture)
                    GameManager.Instance.m_MainCamera.targetTexture.Release();

        Resolution r = Screen.currentResolution;
        m_renderTexture.width = Mathf.RoundToInt(r.width * 0.01f * m_renderScale.value);
        m_renderTexture.height = Mathf.RoundToInt(r.height * 0.01f * m_renderScale.value);

        m_settings.RenderScale = Mathf.RoundToInt(m_renderScale.value);

        if (GameManager.Instance)
            if (GameManager.Instance.m_MainCamera)
                GameManager.Instance.m_MainCamera.targetTexture = m_renderTexture;

        m_renderScaleTMPro.SetText($"Render Scale: {m_renderScale.value.ToString()}%");
    }
    public void SetAA()
    {
        m_settings.AA++;

        if (m_settings.AA > 3)
            m_settings.AA = 0;

        if (m_settings.AA == 0)
            m_aaTMPro.SetText($"Anti Aliasing: NONE");
        else
            m_aaTMPro.SetText($"Anti Aliasing: {1 << m_settings.AA}x");

        // RenderTexture.active = null;
        // if (GameManager.Instance)
        //     if (GameManager.Instance.m_MainCamera)
        //         GameManager.Instance.m_MainCamera.targetTexture = null;
        // // m_renderTexture.antiAliasing = 1 << m_settings.AA;
        // RenderTexture.active = m_renderTexture;
        // if (GameManager.Instance)
        //     if (GameManager.Instance.m_MainCamera)
        //         GameManager.Instance.m_MainCamera.targetTexture = m_renderTexture;
    }
    public void SetGraphicsQuality()
    {
        int currentIndex = 0;
        for (int i = 0; i < m_graphicsTier.Length; i++)
            if (m_settings.CurrentPipelineAssetIndex == i)
            {
                currentIndex = i;
                break;
            }

        currentIndex++;
        if (currentIndex == m_graphicsTier.Length)
            currentIndex = 0;

        m_settings.CurrentPipelineAssetIndex = currentIndex;
        GraphicsSettings.renderPipelineAsset = m_graphicsTier[currentIndex];
        QualitySettings.renderPipeline = m_graphicsTier[currentIndex];

        m_currentGraphicsTierTMPro.SetText($"Graphics: {m_graphicsTierString[currentIndex].ToString()}");
    }

    public void SetSSAO()
    {
        m_settings.SSAO = !m_settings.SSAO;
        m_rendererData.rendererFeatures[0].SetActive(m_settings.SSAO);
        m_ssaoTMPro.SetText($"SSAO: {(m_settings.SSAO ? "ON" : "OFF")}");
    }

    public void SetBloom()
    {
        m_settings.Bloom = !m_settings.Bloom;
        Bloom bloom;
        m_postprocessingData.TryGet(out bloom);
        bloom.active = m_settings.Bloom;
        m_bloomTMPro.SetText($"Bloom: {(m_settings.Bloom ? "ON" : "OFF")}");
    }

    public void SetDepthOfField()
    {
        m_settings.DepthOfField = !m_settings.DepthOfField;
        DepthOfField depth;
        m_postprocessingData.TryGet(out depth);
        depth.active = m_settings.DepthOfField;
        m_depthOfFieldTMPro.SetText($"Depth of Field: {(m_settings.DepthOfField ? "ON" : "OFF")}");
    }
}
