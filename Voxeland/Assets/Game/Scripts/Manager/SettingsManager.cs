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
    [SerializeField] private Settings_Container m_settings;
    [SerializeField] private RenderTexture m_renderTexture;
    [Header("Music")]
    [SerializeField] private Slider m_masterVolume;
    [SerializeField] private Slider m_musicVolume, m_ambientVolume;
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
    m_musicVolumeTMPro, m_ambientVolumeTMPro,
    m_fieldOfViewTMPro, m_mouseSensitivityTMPro, m_renderDistanceTMPro, m_renderScaleTMPro, m_currentGraphicsTierTMPro,
    m_ssaoTMPro, m_fogTMPro, m_bloomTMPro, m_depthOfFieldTMPro;


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
        SetAmbientVolume();

        m_fieldOfView.value = m_settings.FOV;
        SetFOV();
        m_mouseSensitivity.value = m_settings.MouseSensitivity;
        SetMouseSensitivity();
        m_renderDistance.value = m_settings.RenderDistance;
        SetRenderDistance();
        m_renderScale.value = m_settings.RenderScale;
        SetRenderScale();

        m_currentGraphicsTierTMPro.SetText($"Graphics: {m_graphicsTierString[m_settings.CurrentPipelineAssetIndex].ToString()}");
        m_ssaoTMPro.SetText($"SSAO: {(m_settings.SSAO ? "ON" : "OFF")}");
        m_fogTMPro.SetText($"Fog: {(m_settings.Fog ? "ON" : "OFF")}");
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
        m_ambientVolumeTMPro.SetText($"Ambient: {m_ambientVolume.value.ToString()}%");
    }

    public void SetRenderDistance()
    {
        m_settings.RenderDistance = (int)m_renderDistance.value;
        m_renderDistanceTMPro.SetText($"Render Distance: {m_settings.RenderDistance.ToString()}m");
    }
    public void SetRenderScale()
    {
        if (GameManager.Instance)
            if (GameManager.Instance.m_MainCamera)
                if (GameManager.Instance.m_MainCamera.targetTexture)
                    GameManager.Instance.m_MainCamera.targetTexture.Release();

        m_settings.RenderScale = Mathf.RoundToInt(m_renderScale.value);

        Resolution r = Screen.currentResolution;
        m_renderTexture.width = Mathf.RoundToInt(r.width * 0.01f * m_renderScale.value);
        m_renderTexture.height = Mathf.RoundToInt(r.height * 0.01f * m_renderScale.value);

        if (GameManager.Instance)
            if (GameManager.Instance.m_MainCamera)
                GameManager.Instance.m_MainCamera.targetTexture = m_renderTexture;

        m_renderScaleTMPro.SetText($"Render Scale: {m_renderScale.value.ToString()}%");
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

    public void SetFog()
    {
        m_settings.Fog = !m_settings.Fog;
        m_rendererData.rendererFeatures[1].SetActive(m_settings.Fog);
        m_fogTMPro.SetText($"Fog: {(m_settings.Fog ? "ON" : "OFF")}");
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
