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
public class Settings_Container : ScriptableObject
{
    public PlayerInfo Player;

    [Space]
    public int FOV;
    public int RenderDistance;
    public int RenderScale;
    public int CurrentPipelineAssetIndex;
    [Space]
    public Font BlockFont;
}
