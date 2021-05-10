using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject m_quit;
    [SerializeField] private Settings_Container m_settings;
    [SerializeField] private RenderTexture m_renderTexture;
    [SerializeField] private UniversalRenderPipelineAsset[] m_graphicsTier;
    [SerializeField] private string[] m_graphicsTierString = new string[4] { "Low", "Medium", "High", "Epic" };
    [SerializeField] private Slider m_masterVolume, m_musicVolume, m_ambientVolume;
    [SerializeField] private Slider m_fieldOfView, m_renderDistance, m_renderScale;
    [SerializeField]
    private TextMeshProUGUI
    m_masterVolumeTMPro, m_musicVolumeTMPro, m_ambientVolumeTMPro,
    m_fieldOfViewTMPro, m_renderDistanceTMPro, m_renderScaleTMPro, m_currentGraphicsTierTMPro;
    [SerializeField] private Button m_currentGraphicsTier;


    void Start()
    {
        if (GameManager.Instance is null)
            m_quit.SetActive(false);

        ResetAll();
    }

    void ResetAll()
    {
        m_fieldOfView.value = m_settings.FOV;

        m_renderDistance.value = m_settings.RenderDistance;
        m_renderScale.value = m_settings.RenderScale;
        m_currentGraphicsTierTMPro.SetText($"Graphics: {m_graphicsTierString[m_settings.CurrentPipelineAssetIndex].ToString()}");

        m_masterVolume.value = AudioListener.volume * 100;


        SetFOV();
        SetMasterVolume();
        SetMusicVolume();
        SetAmbientVolume();
        SetRenderDistance();
    }

    public void SetFOV()
    {
        m_settings.FOV = (int)m_fieldOfView.value;
        m_fieldOfViewTMPro.text = "FOV: " + m_settings.FOV.ToString();
    }

    public void SetMasterVolume()
    {
        AudioListener.volume = m_masterVolume.value * 0.01f;
        m_masterVolumeTMPro.SetText($"Master Volume: {m_masterVolume.value.ToString()}%");
    }
    public void SetMusicVolume()
    {
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
            if (GameManager.Instance.m_MainCamera != null)
                if (GameManager.Instance.m_MainCamera.targetTexture != null)
                    GameManager.Instance.m_MainCamera.targetTexture.Release();

        m_settings.RenderScale = Mathf.RoundToInt(m_renderScale.value);

        Resolution r = Screen.currentResolution;
        m_renderTexture.width = Mathf.RoundToInt(r.width * 0.01f * m_renderScale.value);
        m_renderTexture.height = Mathf.RoundToInt(r.height * 0.01f * m_renderScale.value);

        if (GameManager.Instance)
            if (GameManager.Instance.m_MainCamera != null)
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

        m_currentGraphicsTierTMPro.SetText($"Graphics: {m_graphicsTierString[currentIndex].ToString()}");
    }
}
