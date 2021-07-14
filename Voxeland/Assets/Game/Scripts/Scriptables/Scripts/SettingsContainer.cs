using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public struct PlayerInfo
{
    string name;
    int skin;
}
[CreateAssetMenu(menuName = "Scriptables/Settings Container", fileName = "Settings Info", order = 0)]
public class SettingsContainer : ScriptableObject
{
    public PlayerInfo Player;

    [Space]
    public int FOV;
    public float MouseSensitivity;
    public int RenderDistance;
    public int LOD;
    public int RenderScale;
    public int AA;
    public int CurrentPipelineAssetIndex;
    [Space]
    public bool SSAO;
    public bool Fog;
    public bool Bloom;
    public bool DepthOfField;
    [Space]
    public float AmbientVolume;
    public float EffectsVolume;
    [Space]
    public Font BlockFont;
    public Texture2D[] SkinHeadTextures;
    public Texture2D[] SkinTopTextures;
    public Texture2D[] SkinBottomTextures;
}
